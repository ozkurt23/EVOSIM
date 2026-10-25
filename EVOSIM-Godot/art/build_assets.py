"""Original deterministic EvoSim grove models, metres, +Y forward / Z up in Blender.
Exported glTF uses Godot's standard Y up. No external textures or reference assets.
"""
import bpy, math, random, sys, json
from pathlib import Path
from mathutils import Vector
root=Path(sys.argv[sys.argv.index('--')+1]).resolve();root.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
rng=random.Random(2407);mats={}
for name,col in {'bark':(.085,.052,.030),'leaf':(.045,.12,.019),'leaf_light':(.085,.19,.028),'pine':(.025,.075,.040),'stone':(.085,.10,.105),'moss':(.28,.36,.13),'herb':(.52,.43,.26),'cream':(.78,.70,.47),'predator':(.45,.25,.13),'dark':(.035,.045,.03)}.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*col,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.9;mats[name]=m
for name,col in {'leaf_deep':(.03,.085,.015),'leaf_tip':(.11,.21,.033),'pine_deep':(.012,.036,.025),'pine_light':(.042,.11,.058),'pine_tip':(.075,.16,.065)}.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*col,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.95;mats[name]=m
parts=[]
def ico(name,p,s,mat,sub=1):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(mats[mat]);parts.append(o)
 for v in o.data.vertices:v.co*=rng.uniform(.91,1.07)
 for f in o.data.polygons:f.use_smooth=True
 return o
def branch(a,b,r,mat='bark',tip=.5):
 d=Vector(b)-Vector(a);bpy.ops.mesh.primitive_cone_add(vertices=7,radius1=r,radius2=r*tip,depth=d.length,location=(Vector(a)+Vector(b))*.5);o=bpy.context.object;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(mats[mat]);parts.append(o);return o
def leaf_spray(center, radii, count, evergreen=False):
 # R09/R14: readable crown masses with fine perimeter breakup, not isolated leaf confetti.
 # The solid irregular interior creates broad light/shadow regions; leaves finish its edge.
 if not evergreen:
  ico('crown_mass',center,tuple(r*.62 for r in radii),'leaf',2)
  count=max(45,int(count*.82))
 verts=[];faces=[];material_ids=[]
 for i in range(count):
  u=rng.uniform(-1,1);a=rng.random()*math.tau;r=rng.random()**.28
  radial=math.sqrt(1-u*u)
  offset=Vector((math.cos(a)*radial*radii[0],math.sin(a)*radial*radii[1],u*radii[2]))*r
  c=Vector(center)+offset
  angle=rng.random()*math.tau
  axis=Vector((math.cos(angle),math.sin(angle),rng.uniform(-.4,.55))).normalized()
  side=Vector((-axis.y,axis.x,rng.uniform(-.12,.12))).normalized()
  length=rng.uniform(.075,.15) if not evergreen else rng.uniform(.13,.27)
  width=length*(.48 if not evergreen else .20)
  n=len(verts)
  verts.extend([c-axis*length,c-side*width,c+Vector((0,0,.025)),c+side*width,c+axis*length])
  faces.extend([(n,n+1,n+2),(n+1,n+4,n+2),(n+4,n+3,n+2),(n+3,n,n+2)])
  mid=rng.choices([0,1,2,3],[1,6,3,0.5])[0]
  material_ids.extend([mid]*4)
 mesh=bpy.data.meshes.new('leaf_spray');mesh.from_pydata(verts,[],faces);mesh.update()
 obj=bpy.data.objects.new('leaf_spray',mesh);bpy.context.collection.objects.link(obj);parts.append(obj)
 names=['leaf_deep','leaf','leaf_light','leaf_tip'] if not evergreen else ['pine_deep','pine','pine_light','pine_tip']
 palette_name='pine_palette' if evergreen else 'leaf_palette'
 if palette_name not in mats:
  m=bpy.data.materials.new(palette_name);m.use_nodes=True
  bsdf=m.node_tree.nodes['Principled BSDF'];bsdf.inputs['Base Color'].default_value=(1,1,1,1);bsdf.inputs['Roughness'].default_value=.95
  color_node=m.node_tree.nodes.new('ShaderNodeVertexColor');color_node.layer_name='Col'
  m.node_tree.links.new(color_node.outputs['Color'],bsdf.inputs['Base Color'])
  mats[palette_name]=m
 mesh.materials.append(mats[palette_name])
 colors=mesh.color_attributes.new(name='Col',type='FLOAT_COLOR',domain='POINT')
 for i,item in enumerate(colors.data):
  mid=material_ids[(i//5)*4]
  base=mats['pine' if evergreen else 'leaf'].diffuse_color
  chosen=mats[names[mid]].diffuse_color
  item.color=tuple(base[k]*.62+chosen[k]*.38 for k in range(3))+(1,)
 # Broad upward normals keep dense leaves painterly, without harsh triangle glitter.
 normals=[]
 for v in mesh.vertices:
  outward=(v.co-Vector(center)).normalized()
  normals.append((outward*.25+Vector((0,0,.9))).normalized())
 mesh.normals_split_custom_set_from_vertices(normals)
 return obj

def crown(c,s,mat='leaf'):
 leaf_spray(c,s,260,mat=='pine')

def tree(kind):
 h={'oak':5.5,'alder':6.6,'pine':7.2}[kind]*rng.uniform(.90,1.10)
 bend=Vector((rng.uniform(-.25,.25),rng.uniform(-.22,.22),0))
 previous=Vector((0,0,0))
 for segment in range(6):
  z=h*(segment+1)/6;p=Vector((bend.x*(z/h)**1.4,bend.y*(z/h)**1.4,z))
  branch(previous,p,.16*(1-segment/7),tip=.75);previous=p
 for k in range(5):
  a=k*math.tau/5;branch((math.cos(a)*.40,math.sin(a)*.40,.015),(0,0,.55),.065)
 if kind=='pine':
  for layer in range(11):
   z=1.1+layer*.53;radius=1.85*(1-layer/12)**.75
   for k in range(7):
    a=k*math.tau/7+layer*.47+rng.uniform(-.17,.17)
    tip=Vector((math.cos(a)*radius,math.sin(a)*radius,z+rng.uniform(-.1,.2)))
    start=Vector((0,0,z+.25));branch(start,tip,.026,tip=.18)
    for j in range(3):
     f=.38+j*.27;c=start.lerp(tip,f)
     leaf_spray(c,(radius*.29,radius*.29,.17),45,True)
  leaf_spray((bend.x,bend.y,h-.35),(.28,.28,.65),150,True)
 else:
  # Irregular, overlapping branch fans; narrow alder, spreading orchard crown.
  count=13 if kind=='oak' else 17
  for k in range(count):
   a=k*2.399+rng.uniform(-.25,.25)
   z=(2.5+(h-3.2)*k/(count-1)) if kind=='oak' else 1.9+(h-2.5)*k/(count-1)
   spread=(1.5*(.75+.35*math.sin(math.pi*k/count)) if kind=='oak' else 1.0*(1-.45*k/count))*rng.uniform(.85,1.15)
   tip=Vector((math.cos(a)*spread,math.sin(a)*spread,z+.3))
   start=Vector((bend.x*z/h,bend.y*z/h,z-.55))
   branch(start,tip,.038,tip=.22)
   for j in range(3):
    angle=a+(-.6+j*.6)
    end=tip+Vector((math.cos(angle)*.45,math.sin(angle)*.45,rng.uniform(.15,.48)))
    branch(start.lerp(tip,.65),end,.012,tip=.2)
    leaf_spray(end,(.78 if kind=='oak' else .52,.70 if kind=='oak' else .49,.56),220)
  leaf_spray((bend.x,bend.y,h-.2),(.70,.67,.65),450)
def stone():
 verts=[];faces=[]
 heights=[rng.uniform(-.03,.04),rng.uniform(.25,.38),rng.uniform(.52,.64)]
 stretch=rng.uniform(.85,1.2)
 for ring,(r,z) in enumerate(zip([.76,1.,.79],heights)):
  for j in range(7):
   a=j*math.tau/7;rj=r*rng.uniform(.88,1.08)
   verts.append((math.cos(a)*rj*.8*stretch,math.sin(a)*rj*.64/stretch,z+rng.uniform(-.025,.025)))
 faces.append(tuple(range(6,-1,-1)))
 for ring in range(2):
  for j in range(7):faces.append((ring*7+j,ring*7+(j+1)%7,(ring+1)*7+(j+1)%7,(ring+1)*7+j))
 faces.append(tuple(range(14,21)))
 mesh=bpy.data.meshes.new('beveled_stone');mesh.from_pydata(verts,[],faces);mesh.materials.append(mats['stone']);mesh.update()
 obj=bpy.data.objects.new('stone',mesh);bpy.context.collection.objects.link(obj);parts.append(obj)
 bpy.context.view_layer.objects.active=obj;obj.select_set(True)
 bevel=obj.modifiers.new('Weathered edges','BEVEL');bevel.width=.075;bevel.segments=2
 bpy.ops.object.modifier_apply(modifier=bevel.name)
def animal(pred=False):
 mat='predator' if pred else 'herb'
 ico('torso',(0,0,.66),(.72,.30,.36),mat,2);ico('chest',(.4,0,.76),(.34,.32,.40),mat,2)
 branch((.48,0,.75),(.69,0,1.03 if not pred else .80),.20,mat)
 ico('head',(.80,0,1.12 if not pred else .88),(.29,.18,.23),mat,2);ico('muzzle',(1.02,0,1.03 if not pred else .82),(.20,.13,.12),'cream' if not pred else mat,1)
 for x in [-.43,.43]:
  for y in [-.22,.22]:branch((x,y,.08),(x+.04,y,.62),.065,mat);ico('foot',(x+.04,y,.07),(.12,.08,.06),'dark')
 for y in [-.16,.16]:
  branch((.72,y,1.23 if not pred else 1),(.61,y*1.5,1.60 if not pred else 1.25),.09,mat,tip=.15)
  ico('eye',(.90,y,1.16 if not pred else .94),(.035,.028,.035),'dark')
 branch((-.65,0,.65),(-1.13,0,.70 if pred else .9),.075,mat)
 if not pred:
  for y in [-.09,.09]:branch((.7,y,1.3),(.66,y*2,1.54),.025,'cream')
manifest={}
for name,fn in [('oak',lambda:tree('oak')),('alder',lambda:tree('alder')),('pine',lambda:tree('pine')),('shrub',lambda:leaf_spray((0,0,.42),(.72,.65,.42),450)),('rock',stone),('herbivore',lambda:animal(False)),('predator',lambda:animal(True)),('oak_b',lambda:tree('oak')),('alder_b',lambda:tree('alder')),('rock_b',stone)]:
 parts=[];fn();bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.export_scene.gltf(filepath=str(root/(name+'.glb')),export_format='GLB',use_selection=True,export_yup=True)
 manifest[name]={'vertices':len(o.data.vertices),'polygons':len(o.data.polygons)}
 o.hide_render=True;o.hide_set(True)
(root.parent/'art').mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root.parent/'art/Grove-library.blend'))
(root.parent/'art/asset-manifest.json').write_text(json.dumps(manifest,indent=2))
