using System.Security.Cryptography;
using System.Text.Json;
using EvoSim.Core;
using EvoSim.Observer;

// Separate process: never register v1 types/model fingerprints in the future v2 host.
internal static class Program
{
    const string PinnedHash="50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033";
    static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    static int Main(string[] args)
    {
        try
        {
            if(Hash(File.ReadAllBytes(typeof(World).Assembly.Location))!=PinnedHash)
                throw new InvalidDataException("Korunan v1 modelinin dosya özeti eşleşmiyor.");
            TimelineArchive.ConfigureCoreHash(PinnedHash);
            if(args.Length==1&&args[0]=="selftest"){SelfTest();return 0;}
            if(args.Length<2||args[0] is not ("inspect" or "extract"))
                throw new ArgumentException("inspect WORLD [TICK] veya extract WORLD NEW_DIRECTORY [TICK]");
            string source=Path.GetFullPath(args[1]);
            if(!File.Exists(source))throw new FileNotFoundException("Dünya bulunamadı.",source);
            string? output=args[0]=="extract"?Path.GetFullPath(args.Length>=3?args[2]:throw new ArgumentException("Yeni çıktı dizini gerekli.")):null;
            int tickIndex=output==null?2:3;
            long? tick=args.Length>tickIndex?long.Parse(args[tickIndex]):null;
            Console.WriteLine(JsonSerializer.Serialize(Read(source,output,tick),new JsonSerializerOptions{WriteIndented=true}));
            return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex.Message);return 1;}
    }

    static object Read(string source,string? output,long? tick)
    {
        string originalHash=Hash(File.ReadAllBytes(source));
        var store=new WorldStore(Path.GetDirectoryName(source)!,PinnedHash);
        var slot=store.List().Single(s=>s.Id==Path.GetFileNameWithoutExtension(source));
        var head=store.Load(slot);var selected=head;
        if(tick.HasValue&&tick.Value!=head.Tick)
        {
            var timeline=TimelineSession.Open(head,store.ReadTimeline(slot));
            if(tick<timeline.EarliestTick||tick>timeline.HeadTick)throw new ArgumentOutOfRangeException(nameof(tick),"Kayıtlı zaman aralığı dışında.");
            timeline.BeginSeek(tick.Value);
            while(timeline.IsSeeking)timeline.PumpSeek(10,2000);
            if(timeline.Progress.Error!=null)throw new InvalidDataException(timeline.Progress.Error);
            selected=timeline.DisplayWorld;
        }
        using var memory=new MemoryStream();selected.Save(memory);byte[] checkpoint=memory.ToArray();
        using var verify=new MemoryStream(checkpoint,false);
        if(World.Load(verify).StateHash()!=selected.StateHash())throw new InvalidDataException("Çıkarılan köken doğrulanamadı.");
        if(Hash(File.ReadAllBytes(source))!=originalHash)throw new IOException("Kaynak dosya okunurken değişti; işlem iptal.");
        var metadata=new {format="EVOSIM-V1-ORIGIN-1",sourceId=slot.Id,sourceName=slot.Name,sourceSha256=originalHash,
            coreSha256=PinnedHash,checkpointFormat=World.CurrentCheckpointFormat,checkpointSha256=Hash(checkpoint),
            headTick=head.Tick,selectedTick=selected.Tick,stateHash=selected.StateHash(),living=selected.Living,
            note="Bu v1 kökenidir; v2 dünya veya v2 geçmiş değildir. Kaynak arşiv korunmuştur."};
        if(output!=null)
        {
            if(Directory.Exists(output)||File.Exists(output))throw new IOException("Çıktı zaten var; üzerine yazılmadı.");
            string parent=Path.GetDirectoryName(output)!;
            if(!Directory.Exists(parent))throw new DirectoryNotFoundException("Çıktının üst dizini mevcut olmalı.");
            string staging=Path.Combine(parent,".evosim-origin-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                File.WriteAllBytes(Path.Combine(staging,"origin.world"),checkpoint);
                File.WriteAllText(Path.Combine(staging,"provenance.json"),JsonSerializer.Serialize(metadata,new JsonSerializerOptions{WriteIndented=true}));
                Directory.Move(staging,output);
            }
            finally{if(Directory.Exists(staging))Directory.Delete(staging,true);}
        }
        return metadata;
    }

    static void SelfTest()
    {
        string dir=Path.Combine(Path.GetTempPath(),"evosim-v1-bridge-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        var checks=new Dictionary<string,bool>();
        try
        {
            var world=new World(42,new WorldConfig{Herbivores=8,Carnivores=2,TreeCount=10});
            var timeline=TimelineSession.Create(world);string origin=world.StateHash();
            for(int i=0;i<40;i++)timeline.AdvancePlaybackOneTick();
            var store=new WorldStore(dir,PinnedHash);
            var slot=store.Save(world,"V1 koruma denemesi",null,1,64,64,1,timelineArchive:timeline.SerializeArchive());
            string source=Path.Combine(dir,slot.Id+".evoworld");string before=Hash(File.ReadAllBytes(source));
            string output=Path.Combine(dir,"extracted");Read(source,output,0);
            using(var input=File.OpenRead(Path.Combine(output,"origin.world")))checks["historical_origin_exact"]=World.Load(input).StateHash()==origin;
            checks["source_unchanged"]=before==Hash(File.ReadAllBytes(source));
            checks["head_still_exact"]=store.Load(slot).StateHash()==world.StateHash();
            bool rejected=false;try{Read(source,output,0);}catch(IOException){rejected=true;}
            checks["overwrite_rejected"]=rejected;
            rejected=false;try{Read(source,null,41);}catch(ArgumentOutOfRangeException){rejected=true;}
            checks["future_rejected"]=rejected;
            rejected=false;try{Read(source,null,-1);}catch(ArgumentOutOfRangeException){rejected=true;}
            checks["before_origin_rejected"]=rejected;
            checks["manifest_written"]=File.Exists(Path.Combine(output,"provenance.json"));
            Console.WriteLine(JsonSerializer.Serialize(new {passed=checks.Values.All(v=>v),checks}));
            if(checks.Values.Any(v=>!v))throw new Exception("V1 koruma testi başarısız.");
        }
        finally{Directory.Delete(dir,true);}
    }
}
