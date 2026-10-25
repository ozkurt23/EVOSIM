"""Build a separate v2 host from shared renderer sources; never copy user worlds/caches.
The v1 host remains launchable. Generated files are refreshed on every forest launch.
"""
from pathlib import Path
import shutil
src=Path(__file__).resolve().parents[1]
dst=src.with_name('EVOSIM-Godot-Forest')
dst.mkdir(exist_ok=True)
for folder in ('scripts','shaders','assets','validation'):
    source=src/folder
    for path in source.rglob('*'):
        if not path.is_file() or path.suffix in ('.json','.log','.png') and folder=='validation': continue
        target=dst/path.relative_to(src);target.parent.mkdir(parents=True,exist_ok=True)
        if not target.exists() or path.read_bytes()!=target.read_bytes(): shutil.copy2(path,target)
for name in ('main.tscn','NuGet.Config','export_presets.cfg','EvoSimGodot.sln'):
    shutil.copy2(src/name,dst/name)
project=(src/'project.godot').read_text().replace('EVOSIM · Mosswater Valley','EVOSIM · Yaşayan Orman')
project=project.replace('[application]','[application]\nconfig/use_custom_user_dir=true\nconfig/custom_user_dir_name="EVOSIM-Forest-v2"')
(dst/'project.godot').write_text(project)
cs=(src/'EvoSimGodot.csproj').read_text().replace('<Optimize>true</Optimize>','<Optimize>true</Optimize><DefineConstants>$(DefineConstants);EVOSIM_FOREST</DefineConstants>')
cs=cs.replace('<ProjectReference Include="../EVOSIM/src/EvoSim.Core/EvoSim.Core.csproj" AdditionalProperties="Configuration=Release" />', ''.join(
    f'<Reference Include="{name}"><HintPath>../EVOSIM-Models/{folder}/bin/Release/net10.0/{name}.dll</HintPath><Private>true</Private></Reference>'
    for folder,name in [('CoreV2','EvoSim.Core.V2'),('ForestEcology','ForestEcology'),('V2Archive','V2Archive')]))
cs=cs.replace('</Project>', '<Target Name="BuildForestModel" BeforeTargets="ResolveAssemblyReferences"><MSBuild Projects="../EVOSIM-Models/V2Archive/V2Archive.csproj" Targets="Restore;Build" RemoveProperties="RuntimeIdentifier;RuntimeIdentifiers;SelfContained;PublishDir;PublishTrimmed;PublishReadyToRun;DebugType;DebugSymbols;GodotTargetPlatform;PublishAot" Properties="Configuration=Release" /></Target></Project>')
cs='\n'.join(line for line in cs.splitlines() if not any(token in line for token in ('Observer/WorldStore.cs','Observer/TimelineSession.cs','Observer/TimelineArchive.cs','EmbeddedResource')))+'\n'
cs=cs.replace('</Project>', '<ItemGroup>' + ''.join(
    f'<EmbeddedResource Include="../EVOSIM-Models/{folder}/bin/Release/net10.0/{name}.dll" LogicalName="{name}.fingerprint.dll" />'
    for folder,name in [('CoreV2','EvoSim.Core.V2'),('ForestEcology','ForestEcology'),('V2Archive','V2Archive')]) + '</ItemGroup></Project>')
(dst/'EvoSimGodot.csproj').write_text(cs)
print(dst)
