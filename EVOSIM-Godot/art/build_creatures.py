"""Original articulated animal components. No third-party meshes or textures."""
from pathlib import Path
# Reuse the existing original geometry/material helpers, not its export loop.
source=Path(__file__).with_name('build_assets.py').read_text()
exec(compile(source.split('manifest={}')[0],str(Path(__file__).with_name('build_assets.py')),'exec'))
mats['herb'].diffuse_color=(.26,.20,.105,1)
mats['cream'].diffuse_color=(.48,.42,.26,1)
mats['predator'].diffuse_color=(.18,.095,.048,1)
for n in ['herb','cream','predator']:mats[n].node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=mats[n].diffuse_color

def body(pred):
 # Continuous torso rings replace the visibly intersecting ellipsoids. Coordinates
 # match the existing four hip anchors; no biological dimensions are changed.
 mat='predator' if pred else 'herb'
 sections=[(-.69,.06,.67,.09),(-.54,.22,.68,.26),(-.36,.255,.70,.27),
           (-.12,.23,.71,.25),(.12,.245,.74,.28),(.34,.26,.76,.32),(.52,.16,.78,.24),(.60,.055,.79,.10)]
 verts=[];faces=[];n=12
 for x,width,z,height in sections:
  if not pred:width*=.87;height*=.92
  for k in range(n):
   a=k*math.tau/n;verts.append((x,math.cos(a)*width,z+math.sin(a)*height))
 for i in range(len(sections)-1):
  for k in range(n):faces.append((i*n+k,i*n+(k+1)%n,(i+1)*n+(k+1)%n,(i+1)*n+k))
 faces += [tuple(reversed(range(n))),tuple((len(sections)-1)*n+k for k in range(n))]
 mesh=bpy.data.meshes.new('continuous_torso');mesh.from_pydata(verts,[],faces);mesh.update()
 obj=bpy.data.objects.new('torso',mesh);bpy.context.collection.objects.link(obj);parts.append(obj)
 for key in [mat,'cream','dark']:mesh.materials.append(mats[key])
 for poly in mesh.polygons:
  center=sum((mesh.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
  poly.material_index=2 if pred and center.z>.86 and center.x<.25 else (1 if not pred and center.z<.54 else 0)
  poly.use_smooth=True
 modifier=obj.modifiers.new('soft anatomical transitions','SUBSURF');modifier.levels=1
 bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=modifier.name)
def head(pred):
 mat='predator' if pred else 'herb'
 # Relative to neck hinge at (0.43, 0, 0.86).
 branch((0,0,0),(.18,0,.12 if pred else .29),.15,mat)
 ico('skull',(.25,0,.14 if pred else .33),(.23,.135,.165),mat,2)
 ico('snout',(.47,0,.06 if pred else .24),(.24,.105,.10) if pred else (.19,.085,.085),mat if pred else 'cream',2)
 ico('nose',(.66,0,.055 if pred else .24),(.055,.09,.06),'dark',1)
 for side in [-1,1]:
  y=side*.14;z=.19 if pred else .38
  ico('eye',(.36,y,z),(.036,.026,.038),'dark',2)
  if pred:
   # Closed triangular ears overlap the skull; oval detached ears read as rodents.
   vertices=[(.06,side*.10,.22),(.24,side*.11,.23),(.10,side*.19,.48),(.04,side*.18,.23),(.21,side*.18,.23)]
   ear_mesh=bpy.data.meshes.new('pointed_ear');ear_mesh.from_pydata(vertices,[],[(0,1,2),(1,4,2),(4,3,2),(3,0,2),(0,3,4,1)]);ear_mesh.update()
   ear=bpy.data.objects.new('ear',ear_mesh);bpy.context.collection.objects.link(ear);parts.append(ear)
   ear_mesh.materials.append(mats[mat]);ear_mesh.materials.append(mats['dark']);ear_mesh.polygons[0].material_index=1
  else:
   ear=ico('ear',(.12,side*.20,z+.21),(.09,.045,.21),mat,2);ear.rotation_euler.x=side*-.55
   inner=ico('inner_ear',(.155,side*.20,z+.22),(.045,.033,.145),'cream',1);inner.rotation_euler.x=ear.rotation_euler.x
  if not pred:
   branch((.12,y*.7,z+.10),(-.01,y*1.3,z+.43),.025,'cream',tip=.35)
   branch((.04,y*1.1,z+.30),(.17,y*1.6,z+.43),.016,'cream',tip=.15)
def leg(pred):
 mat='predator' if pred else 'herb'
 branch((0,0,0),(-.035,0,-.28),.075,mat,tip=.7)
 branch((-.035,0,-.28),(.02,0,-.55),.047,mat,tip=.75)
 ico('paw' if pred else 'hoof',(.07,0,-.56),(.12 if pred else .085,.075,.065),'dark',2)
def tail(pred):
 branch((0,0,0),(-.27,0,-.08 if pred else .16),.09,'predator' if pred else 'cream',tip=.7)
 branch((-.27,0,-.08 if pred else .16),(-.57 if pred else -.34,0,-.18 if pred else .19),.075,'dark' if pred else 'cream',tip=.08)
manifest={}
for pred in [False,True]:
 for part,fn in [('body',body),('head',head),('leg',leg),('tail',tail)]:
  parts=[];fn(pred);bpy.ops.object.select_all(action='DESELECT')
  for obj in parts:obj.select_set(True)
  bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();obj=bpy.context.object
  name=('wolf_' if pred else 'deer_')+part;obj.name=name
  bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
  bpy.ops.export_scene.gltf(filepath=str(root/(name+'.glb')),export_format='GLB',use_selection=True,export_yup=True)
  manifest[name]={'vertices':len(obj.data.vertices),'polygons':len(obj.data.polygons)}
  obj.hide_render=True;obj.hide_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(root.parent/'art/Creature-library.blend'))
(root.parent/'art/creature-manifest.json').write_text(json.dumps(manifest,indent=2))
