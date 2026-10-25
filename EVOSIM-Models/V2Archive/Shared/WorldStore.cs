using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EvoSim.Core;
namespace EvoSim.Observer;
internal sealed class WorldSlot {
 public string Id {get;set;}=""; public string Name {get;set;}="";public string SavedUtc {get;set;}="";public int Seed {get;set;} public long Tick {get;set;} public int Living{get;set;} public int MapSize{get;set;}=128;
 public float Zoom{get;set;}=1.25f;public float X{get;set;}=64;public float Y{get;set;}=64;public int Speed{get;set;}=1;public string SourceWorldId{get;set;}="";public string SourceFormat{get;set;}="";public string Sha256{get;set;}="";public string Preview{get;set;}="";
 public bool AutoClimate {get;set;}=true;
 public long TimelineEarliestTick{get;set;} public long TimelineHeadTick{get;set;} public string TimelineSha256{get;set;}="";
}
internal sealed class WorldStore {
 const string LegacyFormat="EVOSIM-LIBRARY-1",CurrentFormat="EVOSIM-LIBRARY-2";
 const int MaxPayloadBytes=512*1024*1024,MaxMetadataBytes=1024*1024;
 static readonly Lazy<string> CoreHash=new(()=>EvoSim.ForestArchive.ModelIdentity.Value);
 readonly Dictionary<bool,(long Stamp,List<WorldSlot> Slots)> listingCache=new();
 readonly string dir;
 sealed record StoredRecord(string Format,WorldSlot Slot,byte[] Head,byte[]? Timeline);
 public WorldStore(string directory,string? coreFingerprint=null){TimelineArchive.ConfigureCoreHash(coreFingerprint??CoreHash.Value);dir=directory;Directory.CreateDirectory(dir);}
 string PathFor(string id){if(!Guid.TryParseExact(id,"N",out _))throw new InvalidDataException("Geçersiz kayıt kimliği.");return Path.Combine(dir,id+".evoworld");}
 public List<WorldSlot> List(bool trash=false){long now=System.Diagnostics.Stopwatch.GetTimestamp();if(listingCache.TryGetValue(trash,out var hit)&&now-hit.Stamp<System.Diagnostics.Stopwatch.Frequency)return hit.Slots;var list=ReadSlots(trash);listingCache[trash]=(now,list);return list;}
 List<WorldSlot> ReadSlots(bool trash){
  var slots=new List<WorldSlot>();string source=trash?Path.Combine(dir,"trash"):dir;if(!Directory.Exists(source))return slots;
  foreach(var path in Directory.EnumerateFiles(source,"*.evoworld",trash?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly)){
   try{using var r=new BinaryReader(File.OpenRead(path));string format=ReadText(r,64);if(format!=LegacyFormat&&format!=CurrentFormat)continue;var meta=ReadMetadata(r,Path.GetFileNameWithoutExtension(path));slots.Add(meta);}
   catch(IOException){}catch(InvalidDataException){}catch(JsonException){}catch(FormatException){}catch(DecoderFallbackException){}
  }
  return slots.OrderByDescending(s=>s.SavedUtc,StringComparer.Ordinal).ToList();
 }
 public WorldSlot Save(World world,string name,WorldSlot? existing,float zoom,float x,float y,int speed,string? sourceWorldId=null,string? sourceFormat=null,byte[]? timelineArchive=null,bool? autoClimate=null){
  if(existing!=null&&Directory.Exists(Path.Combine(dir,"trash",existing.Id)))throw new InvalidOperationException("Bu dünya silinmiş. Yeni kayıt olarak kaydet veya silinenlerden geri al.");
  using var memory=new MemoryStream();world.Save(memory);var data=memory.ToArray();string headHash=Hash(data);StoredRecord? previous=null;
  if(existing!=null){
   previous=ReadRecord(existing);
   if(previous.Format==LegacyFormat)throw new InvalidOperationException("Eski kayıt korunuyor; önce yeni sürüm için ayrı dünya olarak açılmalı.");
   TimelineArchive.ValidateModel(previous.Timeline!);
   if(timelineArchive==null){
    if(!SameHash(headHash,previous.Slot.Sha256))throw new InvalidOperationException("Zaman çizelgesi olmadan ilerlemiş bir dünya kaydedilemez; mevcut geçmiş korundu.");
    timelineArchive=previous.Timeline;
   }
  }
  timelineArchive??=TimelineSession.Create(world).SerializeArchive();
  // Open verifies the archive's head checkpoint, tick, model fingerprint and command log.
  // A past preview paired with the actual head archive must fail before touching any file.
  var timeline=TimelineSession.Open(world,timelineArchive);
  if(timeline.HeadTick!=world.Tick)throw new InvalidDataException("Zaman çizelgesinin son anı dünya kaydıyla eşleşmiyor.");
  if(previous!=null&&(timeline.HeadTick<previous.Slot.TimelineHeadTick||timeline.EarliestTick!=previous.Slot.TimelineEarliestTick))throw new InvalidOperationException("Mevcut dünyanın geçmişi değiştirilemez; bu andan yeni bir dünya oluştur.");
  var slot=new WorldSlot{AutoClimate=autoClimate??existing?.AutoClimate??true,SourceWorldId=sourceWorldId??existing?.SourceWorldId??"",SourceFormat=sourceFormat??existing?.SourceFormat??"",Id=existing?.Id??Guid.NewGuid().ToString("N"),Name=string.IsNullOrWhiteSpace(name)?"İsimsiz vadi":name.Trim(),SavedUtc=DateTime.UtcNow.ToString("O"),Seed=world.Seed,Tick=world.Tick,Living=world.Living,MapSize=world.Map.Size,Zoom=zoom,X=x,Y=y,Speed=speed,Preview=BuildPreview(world),Sha256=headHash,TimelineEarliestTick=timeline.EarliestTick,TimelineHeadTick=timeline.HeadTick,TimelineSha256=Hash(timelineArchive)};
  if(previous!=null&&SameSavedState(previous.Slot,slot))return previous.Slot;
  Write(CurrentFormat,slot,data,timelineArchive);return slot;
 }
 static bool SameSavedState(WorldSlot before,WorldSlot after)=>
  SameHash(before.Sha256,after.Sha256)&&SameHash(before.TimelineSha256,after.TimelineSha256)
  &&before.AutoClimate==after.AutoClimate&&before.Name==after.Name&&before.Zoom==after.Zoom&&before.X==after.X&&before.Y==after.Y&&before.Speed==after.Speed
  &&before.SourceWorldId==after.SourceWorldId&&before.SourceFormat==after.SourceFormat
  &&before.Tick==after.Tick&&before.TimelineHeadTick==after.TimelineHeadTick&&before.TimelineEarliestTick==after.TimelineEarliestTick
  &&before.Seed==after.Seed&&before.MapSize==after.MapSize&&before.Living==after.Living&&before.Preview==after.Preview;
 static string BuildPreview(World world){const int side=64;var pixels=new byte[side*side];for(int y=0;y<side;y++)for(int x=0;x<side;x++){int cell=(y*world.Map.Size/side)*world.Map.Size+x*world.Map.Size/side;pixels[y*side+x]=world.Map.Tiles[cell]==1?(byte)1:world.Map.Tiles[cell]==2?(byte)2:world.Map.Food[cell]>.3f?(byte)3:(byte)0;}
  void Dot(float x,float y,byte color){int xx=Math.Clamp((int)(x/world.Map.Size*side),0,side-1),yy=Math.Clamp((int)(y/world.Map.Size*side),0,side-1);pixels[yy*side+xx]=color;}
  foreach(var tree in world.Map.Trees)Dot(tree.X,tree.Y,4);foreach(var c in world.Creatures)if(c.Alive)Dot(c.X,c.Y,c.Genes[2]<.5f?(byte)5:(byte)6);return Convert.ToBase64String(pixels);
 }
 void Write(string format,WorldSlot slot,byte[] data,byte[]? timeline){
  if(data.Length==0||data.Length>MaxPayloadBytes||(format==CurrentFormat&&(timeline==null||timeline.Length==0||timeline.Length>MaxPayloadBytes)))throw new InvalidDataException("Kayıt boyutu geçersiz.");
  string metadata=JsonSerializer.Serialize(slot);if(Encoding.UTF8.GetByteCount(metadata)>MaxMetadataBytes)throw new InvalidDataException("Kayıt bilgisi çok büyük.");
  string path=PathFor(slot.Id),temp=path+".tmp";
  try{
   using(var file=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){using var w=new BinaryWriter(file,Encoding.UTF8,true);w.Write(format);w.Write(metadata);w.Write(data.Length);w.Write(data);if(format==CurrentFormat){w.Write(timeline!.Length);w.Write(timeline);}w.Flush();file.Flush(true);}
   if(File.Exists(path))File.Copy(path,path+".bak",true);File.Move(temp,path,true);listingCache.Clear();
  }catch{if(File.Exists(temp))File.Delete(temp);throw;}
 }
 static string ReadText(BinaryReader reader,int maxBytes){int count=reader.Read7BitEncodedInt();if(count<0||count>maxBytes)throw new InvalidDataException("Kayıt metni boyutu geçersiz.");var data=reader.ReadBytes(count);if(data.Length!=count)throw new EndOfStreamException("Kayıt metni eksik.");return new UTF8Encoding(false,true).GetString(data);}
 static WorldSlot ReadMetadata(BinaryReader reader,string expectedId){
  var slot=JsonSerializer.Deserialize<WorldSlot>(ReadText(reader,MaxMetadataBytes))??throw new InvalidDataException("Kayıt bilgisi yok.");
  if(!Guid.TryParseExact(slot.Id,"N",out _)||slot.Id!=expectedId||string.IsNullOrWhiteSpace(slot.Name)||string.IsNullOrEmpty(slot.SavedUtc)||slot.SavedUtc.Length<10)throw new InvalidDataException("Kayıt bilgisi geçersiz.");return slot;
 }
 static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
 static bool SameHash(string a,string b)=>string.Equals(a,b,StringComparison.OrdinalIgnoreCase);
 static byte[] ReadPayload(BinaryReader reader,string expectedHash){
  if(expectedHash==null||expectedHash.Length!=64||expectedHash.Any(c=>!Uri.IsHexDigit(c)))throw new InvalidDataException("Kayıt özeti geçersiz.");
  int count=reader.ReadInt32();if(count<=0||count>MaxPayloadBytes||count>reader.BaseStream.Length-reader.BaseStream.Position)throw new InvalidDataException("Kayıt boyutu geçersiz veya kayıt eksik.");
  var data=reader.ReadBytes(count);if(data.Length!=count||!SameHash(Hash(data),expectedHash))throw new InvalidDataException("Dünya kaydı eksik veya bozulmuş; .bak yedeği korunuyor.");return data;
 }
 StoredRecord ReadRecord(WorldSlot slot){
  using var r=new BinaryReader(File.OpenRead(PathFor(slot.Id)));string format=ReadText(r,64);if(format!=LegacyFormat&&format!=CurrentFormat)throw new InvalidDataException("Dünya biçimi tanınmadı.");
  var meta=ReadMetadata(r,slot.Id);var data=ReadPayload(r,meta.Sha256);byte[]? timeline=format==CurrentFormat?ReadPayload(r,meta.TimelineSha256):null;
  if(r.BaseStream.Position!=r.BaseStream.Length)throw new InvalidDataException("Dünya kaydının sonunda beklenmeyen veri var.");
  if(format==CurrentFormat&&(meta.Tick<0||meta.TimelineEarliestTick<0||meta.TimelineEarliestTick>meta.TimelineHeadTick||meta.TimelineHeadTick!=meta.Tick))throw new InvalidDataException("Zaman çizelgesi kayıt bilgisi geçersiz.");
  return new StoredRecord(format,meta,data,timeline);
 }
 static World ReadWorld(StoredRecord record){using var stream=new MemoryStream(record.Head,false);return World.Load(stream);}
 static void ValidateTimeline(StoredRecord record,World world){
  if(record.Timeline==null)throw new InvalidDataException("Dünyanın zaman çizelgesi yok.");
  var timeline=TimelineSession.Open(world,record.Timeline);
  if(world.Tick!=record.Slot.Tick||timeline.HeadTick!=record.Slot.TimelineHeadTick||timeline.EarliestTick!=record.Slot.TimelineEarliestTick)throw new InvalidDataException("Dünya ve zaman çizelgesi kayıt bilgileri eşleşmiyor.");
 }
 public void Trash(WorldSlot slot){
  string path=PathFor(slot.Id),targetDir=Path.Combine(dir,"trash",slot.Id),target=Path.Combine(targetDir,slot.Id+".evoworld");
  if(!File.Exists(path))throw new FileNotFoundException("Dünya kaydı bulunamadı.");if(Directory.Exists(targetDir))throw new IOException("Bu dünya zaten silinenlerde.");
  Directory.CreateDirectory(targetDir);
  try{File.Move(path,target);if(File.Exists(path+".bak"))File.Move(path+".bak",target+".bak");listingCache.Clear();}
  catch{if(File.Exists(target)&&!File.Exists(path))File.Move(target,path);if(!Directory.EnumerateFileSystemEntries(targetDir).Any())Directory.Delete(targetDir);throw;}
 }
 public void Restore(WorldSlot slot){
  string path=PathFor(slot.Id),sourceDir=Path.Combine(dir,"trash",slot.Id),source=Path.Combine(sourceDir,slot.Id+".evoworld");
  if(File.Exists(path))throw new IOException("Aynı kimlikle açık bir kayıt var; üzerine yazılmadı.");
  if(!File.Exists(source))throw new FileNotFoundException("Silinen kayıt bulunamadı.");
  if(File.Exists(source+".bak")){if(File.Exists(path+".bak"))throw new IOException("Mevcut yedek korunuyor; geri alma durduruldu.");File.Move(source+".bak",path+".bak");}
  try{File.Move(source,path);}catch{if(File.Exists(path+".bak")&&!File.Exists(source+".bak"))File.Move(path+".bak",source+".bak");throw;}
  if(!Directory.EnumerateFileSystemEntries(sourceDir).Any())Directory.Delete(sourceDir);listingCache.Clear();
 }
 public World Load(WorldSlot slot){var record=ReadRecord(slot);var world=ReadWorld(record);if(record.Format==CurrentFormat)ValidateTimeline(record,world);return world;}
 public byte[] ReadTimeline(WorldSlot slot){var record=ReadRecord(slot);if(record.Format!=CurrentFormat)throw new InvalidOperationException("Eski dünyanın geçmişi içe aktarıldığı anda başlayacak; önce yeni sürüm için ayrı dünya olarak açılmalı.");ValidateTimeline(record,ReadWorld(record));return record.Timeline!;}
 public (World World,WorldSlot Slot,bool Migrated,byte[] TimelineArchive) OpenForCurrentVersion(WorldSlot slot){
  var record=ReadRecord(slot);var world=ReadWorld(record);
  if(record.Format==CurrentFormat){ValidateTimeline(record,world);return(world,record.Slot,false,record.Timeline!);}
  // Even an old library container carrying the current Core format has no replay log.
  // Import as a new origin at the saved tick; source bytes and identity stay untouched.
  var archive=TimelineSession.Create(world).SerializeArchive();var source=record.Slot;
  var branch=Save(world,source.Name+" · v0.5",null,source.Zoom,source.X,source.Y,source.Speed,source.Id,world.LoadedCheckpointFormat,archive);
  return(world,branch,true,archive);
 }
 public WorldSlot Rename(WorldSlot slot,string name){var record=ReadRecord(slot);var updated=record.Slot;updated.Name=string.IsNullOrWhiteSpace(name)?updated.Name:name.Trim();Write(record.Format,updated,record.Head,record.Timeline);slot.Name=updated.Name;return updated;}
}
