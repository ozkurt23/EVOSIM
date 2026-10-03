import bpy,json
from pathlib import Path
root=Path('outputs/EVOSIM-3D');results=[]
for path in sorted((root/'Unity/Assets/Art').glob('*.fbx')):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(path.resolve()))
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
 assert len(objects)==1,(path,len(objects))
 o=objects[0];assert len(o.data.materials)>0
 assert all(p.material_index<len(o.data.materials) for p in o.data.polygons)
 assert o.location.length<.001
 results.append(dict(asset=path.name,meshes=len(objects),vertices=len(o.data.vertices),triangles=sum(len(p.vertices)-2 for p in o.data.polygons),materials=[m.name for m in o.data.materials],groundOrigin=True))
(root/'validation/fbx-roundtrip.json').write_text(json.dumps(results,indent=2))
print('FBX_ROUNDTRIP_PASSED',len(results))
