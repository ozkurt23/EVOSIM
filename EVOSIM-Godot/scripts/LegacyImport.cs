using Godot;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EvoSim.Core;
using EvoSim.Observer;

public partial class SimulationBridge
{
    private Task<string> legacyImport;
    public bool ImportBusy => legacyImport != null;
    public bool BeginLegacyImport(string source) => Attempt(() => {
#if EVOSIM_FOREST
        if(ImportBusy) throw new InvalidOperationException("Aktarım zaten sürüyor.");
        EnsureStore();
        source=Path.GetFullPath(source);
        if(!File.Exists(source) || Path.GetExtension(source)!=".evoworld") throw new IOException("Bir .evoworld kaydı seçin.");
        string helper=Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"),"..","EVOSIM-Models","LegacyBridge","bin","Release","net10.0","LegacyBridge.dll"));
        string packaged=Path.Combine(OS.GetExecutablePath().GetBaseDir(),"legacy","LegacyBridge");
        string runtime=System.Environment.GetEnvironmentVariable("DOTNET_ROOT") is string root ? Path.Combine(root,"dotnet") : "dotnet";
        bool portable=File.Exists(packaged);
        if(!portable && !File.Exists(helper)) throw new FileNotFoundException("Aktarım aracı yok; yasayan-orman.sh ile başlatın.");
        string output=Path.Combine(Path.GetTempPath(),"evosim-import-"+Guid.NewGuid().ToString("N"));
        Paused=true;
        legacyImport=Task.Run(async () => {
            try {
                var info=new ProcessStartInfo(portable ? packaged : runtime) {UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
                if(!portable)info.ArgumentList.Add(helper);
                foreach(string arg in new[]{"extract",source,output})info.ArgumentList.Add(arg);
                using var process=Process.Start(info)??throw new IOException("Aktarım aracı açılamadı.");
                var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();await stdout;
                string error=await stderr;
                if(process.ExitCode!=0)throw new InvalidDataException(error);
                return output;
            } catch {if(Directory.Exists(output))Directory.Delete(output,true);throw;}
        });
#else
        throw new InvalidOperationException("Bu aktarım yaşayan orman sürümünde kullanılabilir.");
#endif
    });
    // Called by the UI on the main thread: simulation and scene ownership never cross threads.
    // 0 = waiting, 1 = success, -1 = failure.
    public int PollLegacyImport()
    {
        if(legacyImport==null || !legacyImport.IsCompleted)return 0;
        string output=null;
        bool success=Attempt(() => {
#if EVOSIM_FOREST
            output=legacyImport.GetAwaiter().GetResult();
            byte[] checkpoint=File.ReadAllBytes(Path.Combine(output,"origin.world"));
            string provenance=File.ReadAllText(Path.Combine(output,"provenance.json"));
            using var document=JsonDocument.Parse(provenance);var meta=document.RootElement;
            if(meta.GetProperty("format").GetString()!="EVOSIM-V1-ORIGIN-1" ||
                !Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(checkpoint)).Equals(meta.GetProperty("checkpointSha256").GetString(),StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Aktarım kökeni doğrulanamadı.");
            using var input=new MemoryStream(checkpoint,false);var imported=World.ImportV1Origin(input);
            var session=TimelineSession.Create(imported);
            string name=meta.GetProperty("sourceName").GetString()+" · yaşayan orman";
            // The archive embeds provenance in SourceFormat; no separate file can get orphaned.
            var slot=store.Save(imported,name,null,1,64,64,1,meta.GetProperty("sourceId").GetString(),provenance,session.SerializeArchive(),false);
            timeline=session;world=imported;currentSlot=slot;currentDeleted=false;WorldName=name;seed=world.Seed;
            ClimateCycle=false;Paused=true;Speed=1;debt=0;Scenario="normal";climateCommands.Clear();
#endif
        });
        legacyImport=null;
        if(output!=null && Directory.Exists(output))try{Directory.Delete(output,true);}catch(IOException){}
        return success ? 1 : -1;
    }
}
