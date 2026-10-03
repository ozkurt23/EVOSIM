"""EVOSIM art validation scene. Original editable meshes, common camera/light.
Run: blender --background --factory-startup --python build_scene.py -- OUTPUT_ROOT
"""
import bpy,math,random,sys,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(sys.argv[sys.argv.index('--')+1]).resolve();rng=random.Random(42)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.world.use_nodes=True;scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.55,.66,.75,1);scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.55
scene.view_settings.view_transform='AgX'
M={}
def mat(name,color,rough=.88):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=rough;M[name]=m;return m
for n,c in {'grass':(.26,.43,.11),'grass_light':(.38,.51,.19),'grass_dark':(.13,.30,.13),'earth':(.15,.105,.067),'earth_light':(.23,.17,.105),'leaf':(.19,.39,.11),'leaf_light':(.40,.55,.16),'leaf_dark':(.075,.23,.12),'pine':(.09,.28,.17),'pine_light':(.23,.43,.22),'bark':(.27,.16,.078),'bark_light':(.39,.26,.12),'rock':(.39,.42,.39),'rock_light':(.51,.52,.46),'rock_dark':(.24,.28,.28),'water':(.045,.28,.39),'water_light':(.095,.42,.52),'foam':(.57,.80,.78),'flower':(.89,.80,.51),'blue':(.16,.49,.57),'red':(.62,.25,.16),'eye':(.018,.036,.027)}.items():mat(n,c,.22 if 'water' in n else .85)
# Shared meshes, instanced in editable scene; FBX export expands object transforms.
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1);sphere=bpy.context.object.data.copy();bpy.data.objects.remove(bpy.context.object,do_unlink=True)
for poly in sphere.polygons:poly.use_smooth=True
bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=1,radius2=.72,depth=1);trunkmesh=bpy.context.object.data.copy();bpy.data.objects.remove(bpy.context.object,do_unlink=True)
active=scene.collection
leafmesh=bpy.data.meshes.new('sculpted_leaf')
leafmesh.from_pydata([(-1,0,0),(-.5,-.48,-.05),(.25,-.52,-.03),(1,0,.08),(.25,.52,-.03),(-.5,.48,-.05),(0,0,.18)],[],[(6,i,(i+1)%6) for i in range(6)])
meshes={}
def obj(name,mesh,loc=(0,0,0),scale=(1,1,1),material='leaf'):
 key=(mesh.name,material)
 if key not in meshes:
  copy=mesh.copy();copy.materials.clear();copy.materials.append(M[material]);meshes[key]=copy
 o=bpy.data.objects.new(name,meshes[key]);active.objects.link(o);o.location=loc;o.scale=scale;return o

def ell(name,loc,scale,material):return obj(name,sphere,loc,scale,material)
def branch(a,b,r,material='bark'):
 a,b=Vector(a),Vector(b);o=obj('branch',trunkmesh,(a+b)*.5,(r,r,(b-a).length),material);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o

def leafmass(center,scale,material='leaf',count=35):
 x,y,z=center;sx,sy,sz=scale
 ell('inner_crown',center,(sx*.84,sy*.84,sz*.82),'leaf_dark' if material=='leaf' else 'pine')
 for k in range(count*3):
  theta=rng.uniform(0,math.tau);u=rng.uniform(-1,1);rr=math.sqrt(1-u*u)
  q=(x+math.cos(theta)*rr*sx*.78,y+math.sin(theta)*rr*sy*.78,z+u*sz*.75)
  color=rng.choices([material,material+'_light' if material=='pine' else 'leaf_light','leaf_dark'],[6,3,1])[0]
  leaf=obj('foliage',leafmesh,q,(sx*.34*rng.uniform(.7,1.1),sy*.32*rng.uniform(.7,1.1),sz*.30),color);leaf.rotation_euler=(rng.uniform(-1.1,1.1),rng.uniform(-1.1,1.1),theta)

def tree(kind):
 if kind=='conifer':
  branch((0,0,0),(.10,.04,5.2),.13)
  for j in range(7):
   z=1.2+j*.56;radius=1.13-j*.13
   for a in range(5):
    t=a*math.tau/5+j*.55;end=(math.cos(t)*radius*.62,math.sin(t)*radius*.62,z-.08)
    branch((0,0,z+.2),end,.035);leafmass(end,(radius*.65,radius*.59,.48),'pine',16)
  leafmass((.05,0,5.08),(.29,.28,.59),'pine',20)
 else:
  h=4.1 if kind=='oak' else 5.25;width=1.0 if kind=='oak' else .72
  branch((0,0,0),(.12,-.03,h*.86),.19 if kind=='oak' else .14)
  for j in range(8):
   a=j*2.4;z=h*(.39+j*.07);spread=width*(.8 if j<5 else .5)
   end=(math.cos(a)*spread,math.sin(a)*spread,z)
   branch((.05,0,z-.5),end,.055);leafmass(end,(width*.88,width*.82,.75 if kind=='oak' else .9),count=38)
  leafmass((.10,0,h),(width*.79,width*.76,.83),count=40)

def rock():
 o=ell('boulder',(0,0,.34),(.75,.57,.57),'rock');o.data=o.data.copy()
 for v in o.data.vertices:v.co*=rng.uniform(.90,1.08)
 for p in o.data.polygons:p.use_smooth=False
 ell('moss',(-.18,.12,.73),(.22,.20,.045),'grass_dark')

def animal(predator=False):
 color='red' if predator else 'blue'
 ell('body',(0,0,.58),(.54,.26,.30),color);ell('head',(.53,0,.81 if not predator else .64),(.22,.19,.23),color)
 for x in [-.33,.31]:
  for y in [-.19,.19]:branch((x,y,.12),(x+.02,y,.53),.046,color)
 for y in [-.14,.14]:ell('ear',(.45,y,.99 if not predator else .85),(.085,.055,.18),color)
 if predator:branch((-.40,0,.64),(-.99,0,.56),.09,color)
 for y in [-.18,.18]:ell('eye',(.65,y,.84 if not predator else .7),(.031,.025,.032),'eye')

def shrub():leafmass((0,0,.45),(.70,.55,.48),count=45)
def reeds():
 for j in range(13):
  x,y=rng.uniform(-.3,.3),rng.uniform(-.3,.3);h=rng.uniform(.5,1.1);branch((x,y,0),(x+.06,y,h),.012,'grass_dark');o=ell('reed_leaf',(x+.10,y,h*.7),(.025,.06,h*.36),'grass_light');o.rotation_euler[1]=-.30
assets={}
for name,fn in [('Tree_Oak_A',lambda:tree('oak')),('Tree_Poplar_A',lambda:tree('poplar')),('Tree_Conifer_A',lambda:tree('conifer')),('Rock_A',rock),('Shrub_A',shrub),('Reeds_A',reeds),('Herbivore_A',lambda:animal(False)),('Carnivore_A',lambda:animal(True))]:
 active=bpy.data.collections.new(name);fn();assets[name]=active
 scene.collection.children.link(active);bpy.ops.object.select_all(action='DESELECT')
 # Export one combined mesh per asset; retain editable component meshes in Blender.
 copies=[]
 for original in list(active.objects):
  clone=original.copy();clone.data=original.data.copy();scene.collection.objects.link(clone);clone.select_set(True);copies.append(clone)
  for polygon in clone.data.polygons:polygon.material_index=0
 bpy.context.view_layer.update();bpy.context.view_layer.objects.active=copies[0]
 bpy.ops.object.join();combined=bpy.context.object;combined.name=name
 # Root origin must be on the ground, not on the first branch's center.
 scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Unity/Assets/Art'/f'{name}.fbx'),use_selection=True,add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,object_types={'MESH'},path_mode='AUTO')
 bpy.data.objects.remove(combined,do_unlink=True)
 scene.collection.children.unlink(active)
active=scene.collection

def instance(name,x,y,z=0,scale=1,angle=0):
 o=bpy.data.objects.new(name,None);o.instance_type='COLLECTION';o.instance_collection=assets[name];active.objects.link(o);o.location=(x,y,z);o.scale=(scale,)*3;o.rotation_euler[2]=angle;return o
# Grid-based raised land pieces with a dark vertical soil cut. One genuine height level.
N=26;HALF=N/2
water=lambda x,y:abs(x-(math.sin(y*.23)*2.0+.18*y))<2.1
land={}
for y in range(N):
 for x in range(N):land[(x,y)]=not water(x-HALF+.5,y-HALF+.5)
# Continuous shore slope: water-adjacent vertices are only 8 cm above water.
def vertex_height(x,y):
 adjacent=[land.get((x+dx,y+dy),True) for dx,dy in [(-1,-1),(-1,0),(0,-1),(0,0)]]
 return .06 if not all(adjacent) else .65
def ground_z(x,y):
 gx,gy=x+HALF,y+HALF;ix,iy=math.floor(gx),math.floor(gy);u,v=gx-ix,gy-iy
 h00,h10,h11,h01=[vertex_height(ix+dx,iy+dy) for dx,dy in [(0,0),(1,0),(1,1),(0,1)]]
 return h00+(h10-h00)*u+(h11-h10)*v if v<=u else h00+(h11-h01)*u+(h01-h00)*v
def safe_land(x,y,radius):
 return all(land.get((math.floor(x+dx+HALF),math.floor(y+dy+HALF)),False) for dx,dy in [(0,0)]+[(math.cos(a*math.tau/16)*radius,math.sin(a*math.tau/16)*radius) for a in range(16)])
verts=[];faces=[];mats=[]
for (x,y),island in land.items():
 if not island:continue
 x0,y0=x-HALF,y-HALF;z=.65
 k=len(verts);verts.extend([(x0+dx,y0+dy,vertex_height(x+dx,y+dy)) for dx,dy in [(0,0),(1,0),(1,1),(0,1)]]);faces.extend([(k,k+1,k+2),(k,k+2,k+3)]);mats.extend([0,0])
 for dx,dy,aa,bb in [(1,0,(x0+1,y0),(x0+1,y0+1)),(-1,0,(x0,y0+1),(x0,y0)),(0,1,(x0+1,y0+1),(x0,y0+1)),(0,-1,(x0,y0),(x0+1,y0))]:
  if land.get((x+dx,y+dy),False):continue
  k=len(verts);verts.extend([(*aa,vertex_height(round(aa[0]+HALF),round(aa[1]+HALF))),(*bb,vertex_height(round(bb[0]+HALF),round(bb[1]+HALF))),(*bb,-.35),(*aa,-.35)]);faces.append((k,k+1,k+2,k+3));mats.append(2 if dx else 3)
mesh=bpy.data.meshes.new('raised_terrain');mesh.from_pydata(verts,[],faces);mesh.materials.clear()
for m in ['grass','grass_light','earth','earth_light']:mesh.materials.append(M[m])
terrain=bpy.data.objects.new('Raised_banks',mesh);active.objects.link(terrain)
for f,i in zip(mesh.polygons,mats):f.material_index=i
# Fine surface shader without giant blobs.
for key in ['grass','grass_light']:
 m=M[key];nt=m.node_tree;tex=nt.nodes.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=28;tex.inputs['Detail'].default_value=2
 bump=nt.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.16;bump.inputs['Distance'].default_value=.08;nt.links.new(tex.outputs['Fac'],bump.inputs['Height']);nt.links.new(bump.outputs['Normal'],nt.nodes.get('Principled BSDF').inputs['Normal']);ramp=nt.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.2;ramp.color_ramp.elements[0].color=(.20,.36,.08,1);ramp.color_ramp.elements[1].position=.8;ramp.color_ramp.elements[1].color=(.32,.46,.14,1);nt.links.new(tex.outputs['Fac'],ramp.inputs['Fac']);nt.links.new(ramp.outputs['Color'],nt.nodes.get('Principled BSDF').inputs['Base Color'])
bpy.ops.mesh.primitive_plane_add(size=N+.4,location=(0,0,-.02));pool=bpy.context.object;pool.name='River_surface';pool.data.materials.append(M['water'])
nt=M['water'].node_tree;noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=3.8;noise.inputs['Detail'].default_value=2;bum=nt.nodes.new('ShaderNodeBump');bum.inputs['Strength'].default_value=.20;bum.inputs['Distance'].default_value=.12;nt.links.new(noise.outputs['Fac'],bum.inputs['Height']);nt.links.new(bum.outputs['Normal'],nt.nodes.get('Principled BSDF').inputs['Normal'])
# Forest masses plus open animal clearing, not uniform scatter.
for j in range(28):
 side=-1 if j<15 else 1;yy=rng.uniform(-10,10);xx=side*rng.uniform(5.3,10.8)
 if xx>0 and yy<1:continue
 name=['Tree_Oak_A','Tree_Poplar_A','Tree_Conifer_A'][j%3];instance(name,xx,yy,.65,rng.uniform(.73,1.07),rng.uniform(0,math.tau))
 if j%2==0:instance('Shrub_A',xx+.7,yy-.8,.65,.65)
for j in range(11):
 yy=rng.uniform(-10,10);xx=math.sin(yy*.23)*2+.18*yy+(2.8 if j%2 else -2.8)
 if safe_land(xx,yy,.4):instance('Reeds_A',xx,yy,ground_z(xx,yy),rng.uniform(.7,1.2))
rock_checks=[]
for x,y,s in [(-4,-7,1.2),(-5,-7.8,.75),(5,5,1.1),(4.8,6,.65),(8,-7,1.3),(8.7,-6.8,.65)]:
 side=-1 if x<math.sin(y*.23)*2+.18*y else 1
 while not safe_land(x,y,s*.85+.6):x+=side*.25
 instance('Rock_A',x,y,ground_z(x,y),s);rock_checks.append({'x':x,'y':y,'radius':s*.85,'fullyOnLand':True})
(ROOT/'validation/shore-check.json').write_text(json.dumps({'shoreHeight':.06,'waterHeight':-.02,'heightGap':.08,'rocks':rock_checks},indent=2))
instance('Herbivore_A',6,-3,.65,1.05,.4);instance('Carnivore_A',8,-4.7,.65,1,-.4)
# Low cover and small flowers only in patches; clear space around animals.
for j in range(620):
 x,y=rng.uniform(-12,12),rng.uniform(-12,12)
 if water(x,y) or (x>4 and -6<y<0):continue
 if math.sin(x*.7)+math.cos(y*.55)<.15:continue
 o=ell('grass_blade',(x,y,ground_z(x,y)+.07),(.022,.065,rng.uniform(.09,.20)),rng.choice(['grass_light','grass_dark']));o.rotation_euler[1]=rng.uniform(-.4,.4)
 if j%6==0:ell('tiny_flower',(x,y,ground_z(x,y)+.23),(.045,.045,.025),'flower')
for j in range(85):
 y=rng.uniform(-12.8,12.8);x=math.sin(y*.23)*2+.18*y+rng.uniform(-1.7,1.7)
 if water(x,y):o=ell('water_glint',(x,y,.001),(.10,.022,.003),'foam');o.rotation_euler[2]=.2
# Presentation ground, coherent sun, orthographic camera.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.42));bpy.context.object.data.materials.append(mat('backdrop',(.17,.23,.24)))
bpy.ops.object.light_add(type='SUN',location=(-10,-12,20));sun=bpy.context.object;sun.rotation_euler=(math.radians(26),math.radians(-22),math.radians(-35));sun.data.energy=2.8;sun.data.angle=math.radians(12)
bpy.ops.object.light_add(type='AREA',location=(0,0,18));bpy.context.object.data.energy=1100;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=22
bpy.ops.object.camera_add(location=(29,-34,33));cam=bpy.context.object;target=Vector((0,0,.5));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=39;scene.camera=cam
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(ROOT/'previews/validation-scene.png')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'blender/EVOSIM-validation.blend'))
(ROOT/'blender/asset-manifest.json').write_text(json.dumps({'assets':{k:len(v.objects) for k,v in assets.items()},'seed':42,'cameraElevationDegrees':math.degrees(math.atan2(32.5,math.hypot(29,34))),'orthographic':True,'note':'Art validation scene, not a running Core simulation'},indent=2))
bpy.ops.render.render(write_still=True)
print('EVOSIM_BLENDER_SCENE_COMPLETE')
