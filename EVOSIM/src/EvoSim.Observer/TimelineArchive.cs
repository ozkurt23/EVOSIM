using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using EvoSim.Core;

namespace EvoSim.Observer;

internal readonly record struct TimelineCommand(long Tick, bool Drought);

internal sealed class TimelineSnapshot
{
    internal long Tick { get; }
    internal int NextCommandIndex { get; }
    internal int RawLength { get; }
    internal string RawHash { get; }
    internal byte[] Compressed { get; }
    internal TimelineSnapshot(long tick, int nextCommandIndex, int rawLength, string rawHash, byte[] compressed)
    { Tick=tick; NextCommandIndex=nextCommandIndex; RawLength=rawLength; RawHash=rawHash; Compressed=compressed; }
    internal static TimelineSnapshot Capture(World world, int nextCommandIndex)
    {
        byte[] raw=TimelineArchive.SaveWorld(world);
        using var memory=new MemoryStream();
        using(var compressor=new GZipStream(memory,CompressionLevel.Fastest,true))compressor.Write(raw);
        return new TimelineSnapshot(world.Tick,nextCommandIndex,raw.Length,TimelineArchive.Hash(raw),memory.ToArray());
    }
    internal byte[] Inflate()
    {
        if(RawLength<1||RawLength>TimelineArchive.MaximumBytes)throw new InvalidDataException("Geçmiş kaydının açılmış boyutu geçersiz.");
        try
        {
            using var input=new MemoryStream(Compressed,false);
            using var gzip=new GZipStream(input,CompressionMode.Decompress);
            using var output=new MemoryStream(Math.Min(RawLength,1024*1024));
            byte[] buffer=new byte[65536];int count;
            while((count=gzip.Read(buffer))>0)
            {
                if(output.Length+count>RawLength)throw new InvalidDataException("Geçmiş kaydı bildirilen boyutu aşıyor.");
                output.Write(buffer,0,count);
            }
            byte[] raw=output.ToArray();
            if(raw.Length!=RawLength||TimelineArchive.Hash(raw)!=RawHash)throw new InvalidDataException("Geçmiş anının boyutu veya bütünlük özeti eşleşmiyor.");
            return raw;
        }
        catch(IOException ex){throw new InvalidDataException("Sıkıştırılmış geçmiş anı açılamadı.",ex);}
    }
    internal World Restore()
    {
        using var memory=new MemoryStream(Inflate(),false);var world=World.Load(memory);
        if(world.Tick!=Tick||world.LoadedCheckpointFormat!=World.CurrentCheckpointFormat)throw new InvalidDataException("Geçmiş anının zamanı veya model kayıt biçimi eşleşmiyor.");
        return world;
    }
}

// No filesystem access: the host embeds these bytes beside the matching head checkpoint
// in one atomic world file. The host also supplies the actual Core DLL fingerprint.
internal sealed class TimelineArchive
{
    internal const string Format="EVOSIM-TIMELINE-1";
    internal const int MaximumBytes=512*1024*1024;
    internal const int MaximumCommands=2_000_000;
    internal const int DefaultCacheBudgetBytes=32*1024*1024;
    internal const int DefaultSnapshotIntervalTicks=600;
    static readonly object fingerprintLock=new();static string? configuredCoreHash;
    static readonly UTF8Encoding encoding=new(false,true);
    internal static string CoreHash=>configuredCoreHash??throw new InvalidOperationException("Geçmiş sistemi için Core dosya özeti ayarlanmadı.");
    public static void ConfigureCoreHash(string sha256)
    {
        string normalized=NormalizeHash(sha256);
        lock(fingerprintLock)
        {
            if(configuredCoreHash!=null&&configuredCoreHash!=normalized)throw new InvalidOperationException("Çalışan geçmiş sisteminin Core özeti değiştirilemez.");
            configuredCoreHash=normalized;
        }
    }
    internal TimelineSnapshot Origin { get; }
    internal List<TimelineCommand> Commands { get; }
    internal List<TimelineSnapshot> Snapshots { get; }
    internal long HeadTick { get; set; }
    internal string HeadHash { get; set; }
    internal int CacheBudgetBytes { get; }
    internal int SnapshotIntervalTicks { get; }
    internal TimelineArchive(TimelineSnapshot origin,List<TimelineCommand> commands,List<TimelineSnapshot> snapshots,long headTick,string headHash,int cacheBudgetBytes,int snapshotIntervalTicks)
    { Origin=origin;Commands=commands;Snapshots=snapshots;HeadTick=headTick;HeadHash=headHash;CacheBudgetBytes=cacheBudgetBytes;SnapshotIntervalTicks=snapshotIntervalTicks; }
    internal static byte[] SaveWorld(World world)
    {
        using var memory=new MemoryStream();world.Save(memory);
        if(memory.Length>MaximumBytes)throw new InvalidDataException("Dünya geçmiş kaydı boyut sınırını aşıyor.");
        return memory.ToArray();
    }
    internal static string Hash(byte[] data)=>Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
    static string NormalizeHash(string value)
    {
        if(value==null||value.Length!=64||value.Any(c=>!Uri.IsHexDigit(c)))throw new InvalidDataException("Geçersiz SHA-256 özeti.");
        return value.ToLowerInvariant();
    }
    internal byte[] Serialize()
    {
        ValidateStructure();
        using var body=new MemoryStream();
        using(var w=new BinaryWriter(body,encoding,true))
        {
            w.Write(CoreHash);w.Write(World.CurrentCheckpointFormat);w.Write(Origin.Tick);w.Write(HeadTick);w.Write(HeadHash);
            w.Write(SnapshotIntervalTicks);w.Write(CacheBudgetBytes);WriteSnapshot(w,Origin);
            w.Write(Commands.Count);foreach(var command in Commands){w.Write((byte)1);w.Write(command.Tick);w.Write(command.Drought);}
            w.Write(Snapshots.Count);foreach(var snapshot in Snapshots){WriteSnapshot(w,snapshot);if(body.Length>MaximumBytes)throw new InvalidDataException("Geçmiş arşivi boyut sınırını aşıyor.");}
        }
        if(body.Length>MaximumBytes)throw new InvalidDataException("Geçmiş arşivi boyut sınırını aşıyor.");
        byte[] payload=body.ToArray();using var file=new MemoryStream();
        using(var w=new BinaryWriter(file,encoding,true)){w.Write(Format);w.Write(payload.Length);w.Write(SHA256.HashData(payload));w.Write(payload);}
        if(file.Length>MaximumBytes)throw new InvalidDataException("Geçmiş arşivi boyut sınırını aşıyor.");
        return file.ToArray();
    }
    // Cheap protection before replacing an existing LIBRARY-2 record: authenticate the
    // archive and its model identity without constructing another World.
    public static void ValidateModel(byte[] bytes)
    {
        if(bytes==null||bytes.Length>MaximumBytes)throw new InvalidDataException("Geçmiş arşivi boyutu geçersiz.");
        try
        {
            using var input=new MemoryStream(bytes,false);using var file=new BinaryReader(input,encoding,true);
            if(ReadText(file,128)!=Format)throw new InvalidDataException("Geçmiş arşivi sürümü desteklenmiyor.");
            int length=ReadCount(file,MaximumBytes,"arşiv");Ensure(file,(long)length+32);byte[] digest=Exact(file,32),payload=Exact(file,length);
            if(input.Position!=input.Length||!digest.SequenceEqual(SHA256.HashData(payload)))throw new InvalidDataException("Geçmiş arşivinin bütünlük kontrolü başarısız.");
            using var body=new MemoryStream(payload,false);using var reader=new BinaryReader(body,encoding,true);
            if(NormalizeHash(ReadText(reader,64))!=CoreHash||ReadText(reader,128)!=World.CurrentCheckpointFormat)throw new InvalidDataException("Mevcut geçmiş başka bir model sürümüne ait; ayrı bir dünya dalı gerekli.");
        }
        catch(EndOfStreamException ex){throw new InvalidDataException("Geçmiş arşivi kesilmiş.",ex);}
        catch(DecoderFallbackException ex){throw new InvalidDataException("Geçmiş metni UTF-8 biçiminde değil.",ex);}
    }
    internal static TimelineArchive Deserialize(byte[] bytes,World head)
    {
        if(bytes==null||bytes.Length>MaximumBytes)throw new InvalidDataException("Geçmiş arşivi boyutu geçersiz.");
        try
        {
            using var input=new MemoryStream(bytes,false);using var file=new BinaryReader(input,encoding,true);
            if(ReadText(file,128)!=Format)throw new InvalidDataException("Geçmiş arşivi sürümü desteklenmiyor.");
            int length=ReadCount(file,MaximumBytes,"arşiv");Ensure(file,(long)length+32);
            byte[] digest=Exact(file,32),payload=Exact(file,length);
            if(input.Position!=input.Length||!digest.SequenceEqual(SHA256.HashData(payload)))throw new InvalidDataException("Geçmiş arşivinin bütünlük kontrolü başarısız.");
            using var body=new MemoryStream(payload,false);using var r=new BinaryReader(body,encoding,true);
            if(NormalizeHash(ReadText(r,64))!=CoreHash)throw new InvalidDataException("Bu geçmiş başka bir Core modeline ait; eski komutlar yeni modelde yürütülemez.");
            if(ReadText(r,128)!=World.CurrentCheckpointFormat)throw new InvalidDataException("Geçmişin dünya kayıt biçimi bu modelle eşleşmiyor.");
            long earliest=r.ReadInt64(),headTick=r.ReadInt64();string headHash=NormalizeHash(ReadText(r,64));
            int interval=r.ReadInt32(),budget=r.ReadInt32();var origin=ReadSnapshot(r);
            int commandCount=ReadCount(r,MaximumCommands,"komut");Ensure(r,(long)commandCount*10);
            var commands=new List<TimelineCommand>(Math.Min(commandCount,4096));
            for(int i=0;i<commandCount;i++)
            {
                if(r.ReadByte()!=1)throw new InvalidDataException("Geçmişte desteklenmeyen bir komut var.");
                long tick=r.ReadInt64();byte enabled=r.ReadByte();if(enabled>1)throw new InvalidDataException("Geçersiz kuraklık komutu.");commands.Add(new TimelineCommand(tick,enabled==1));
            }
            int snapshotCount=ReadCount(r,100000,"geçmiş anı");var snapshots=new List<TimelineSnapshot>(Math.Min(snapshotCount,1024));
            for(int i=0;i<snapshotCount;i++)snapshots.Add(ReadSnapshot(r));
            if(body.Position!=body.Length)throw new InvalidDataException("Geçmiş arşivinde beklenmeyen ek veri var.");
            var archive=new TimelineArchive(origin,commands,snapshots,headTick,headHash,budget,interval);archive.ValidateStructure();
            if(earliest!=origin.Tick||head.Tick!=headTick||Hash(SaveWorld(head))!=headHash)throw new InvalidDataException("Geçmişin son anı kaydedilen dünyayla eşleşmiyor.");
            // Validate the mandatory origin now. Optional accelerator records are integrity-
            // protected by the complete archive and are fully decoded when selected for replay.
            origin.Restore();return archive;
        }
        catch(EndOfStreamException ex){throw new InvalidDataException("Geçmiş arşivi kesilmiş.",ex);}
        catch(DecoderFallbackException ex){throw new InvalidDataException("Geçmiş metni UTF-8 biçiminde değil.",ex);}
        catch(OverflowException ex){throw new InvalidDataException("Geçmiş boyutu sınırı aşıyor.",ex);}
        catch(ArgumentException ex){throw new InvalidDataException("Geçmiş arşivinde geçersiz alan var.",ex);}
    }
    internal void ValidateStructure()
    {
        _=CoreHash;
        if(Origin.Tick<0||HeadTick<Origin.Tick||Origin.NextCommandIndex!=0||SnapshotIntervalTicks<1||CacheBudgetBytes<0||CacheBudgetBytes>DefaultCacheBudgetBytes||Commands.Count>MaximumCommands)throw new InvalidDataException("Geçmiş aralığı veya önbellek ayarları geçersiz.");
        NormalizeHash(HeadHash);long previous=Origin.Tick;
        foreach(var command in Commands){if(command.Tick<previous||command.Tick>HeadTick)throw new InvalidDataException("Geçmiş komutlarının zaman sırası geçersiz.");previous=command.Tick;}
        ValidateSnapshot(Origin);long cacheBytes=0;var keys=new HashSet<(long,int)>();long lastTick=Origin.Tick;
        foreach(var snapshot in Snapshots)
        {
            ValidateSnapshot(snapshot);if(snapshot.Tick<lastTick||!keys.Add((snapshot.Tick,snapshot.NextCommandIndex)))throw new InvalidDataException("Geçmiş anlarının sırası geçersiz.");lastTick=snapshot.Tick;cacheBytes+=snapshot.Compressed.Length;
        }
        if(cacheBytes>CacheBudgetBytes)throw new InvalidDataException("Geçmiş hızlandırıcıları önbellek bütçesini aşıyor.");
    }
    void ValidateSnapshot(TimelineSnapshot snapshot)
    {
        int next=snapshot.NextCommandIndex;
        if(snapshot.Tick<Origin.Tick||snapshot.Tick>HeadTick||next<0||next>Commands.Count||snapshot.RawLength<1||snapshot.RawLength>MaximumBytes||snapshot.Compressed.Length<18||snapshot.Compressed.Length>MaximumBytes)throw new InvalidDataException("Geçmiş anının boyutu, zamanı veya komut imleci geçersiz.");
        NormalizeHash(snapshot.RawHash);
        if(snapshot.Compressed[0]!=0x1f||snapshot.Compressed[1]!=0x8b)throw new InvalidDataException("Geçmiş anının sıkıştırma biçimi geçersiz.");
        if(next>0&&Commands[next-1].Tick>snapshot.Tick||next<Commands.Count&&Commands[next].Tick<snapshot.Tick)throw new InvalidDataException("Geçmiş anının komut imleci zamanla eşleşmiyor.");
    }
    static void WriteSnapshot(BinaryWriter w,TimelineSnapshot snapshot)
    {w.Write(snapshot.Tick);w.Write(snapshot.NextCommandIndex);w.Write(snapshot.RawLength);w.Write(snapshot.RawHash);w.Write(snapshot.Compressed.Length);w.Write(snapshot.Compressed);}
    static TimelineSnapshot ReadSnapshot(BinaryReader r)
    {
        long tick=r.ReadInt64();int next=r.ReadInt32(),rawLength=r.ReadInt32();string hash=NormalizeHash(ReadText(r,64));int length=ReadCount(r,MaximumBytes,"sıkıştırılmış an");Ensure(r,length);
        return new TimelineSnapshot(tick,next,rawLength,hash,Exact(r,length));
    }
    static int ReadCount(BinaryReader r,int max,string label){int count=r.ReadInt32();if(count<0||count>max)throw new InvalidDataException("Geçersiz "+label+" boyutu.");return count;}
    static void Ensure(BinaryReader r,long count){if(count<0||count>r.BaseStream.Length-r.BaseStream.Position)throw new InvalidDataException("Geçmiş alanı kalan veriyi aşıyor.");}
    static byte[] Exact(BinaryReader r,int count){byte[] bytes=r.ReadBytes(count);if(bytes.Length!=count)throw new EndOfStreamException();return bytes;}
    static string ReadText(BinaryReader r,int maximum)
    {
        uint length=0;for(int i=0;i<5;i++){byte b=r.ReadByte();if(i==4&&(b&0xf8)!=0)throw new InvalidDataException("Geçersiz metin uzunluğu.");length|=(uint)(b&0x7f)<<(7*i);if((b&0x80)==0){if(length>maximum)throw new InvalidDataException("Geçmiş metni çok uzun.");Ensure(r,length);return encoding.GetString(Exact(r,(int)length));}}throw new InvalidDataException("Geçersiz metin boyutu.");
    }
}
