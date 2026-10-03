using EvoSim.Core;
namespace EvoSim.Observer;

internal sealed partial class WorldView : IDisposable {
 readonly Painter p; IntPtr ground; World? groundWorld; Terrain? groundMap; float groundWaterLevel=-1; long groundTick; bool groundDrought;
 readonly HashSet<int> treeCells=new(); readonly List<int> rockCells=new();
 readonly List<RockVisual> rocks=new(); Vertex[] rockBedVertices=new Vertex[2048];
 readonly Dictionary<Tree,CanopyFade> canopyFades=new(); long renderClock; double renderTime; float renderDelta;
 sealed class CanopyFade {public float Alpha;public double ClearAfter;}
 readonly record struct RockVisual(float X,float Y,float Width,float Height,float Variation);
 Vertex[] foodVertices=new Vertex[32768]; int foodVertexCount;
 readonly Vertex[] rockVertices=new Vertex[24]; readonly Vertex[] shapeVertices=new Vertex[192];
 readonly List<VisibleBody> visible=new(); int[] cellHeads=Array.Empty<int>(),bodyNext=new int[256]; int gridColumns,gridRows; float largestBody;
 const int OcclusionCell=64;
 sealed class VisibleBody {public Creature C=null!; public Pt Position; public float Radius; public bool Obscured;}
 readonly List<VisibleBody> bodyPool=new();
 static readonly Pt[] rockOutline={new(-.47f,-.07f),new(-.27f,-.39f),new(.08f,-.48f),new(.38f,-.27f),new(.49f,.05f),new(.26f,.37f),new(-.12f,.45f),new(-.39f,.28f)};
 static readonly Pt[] predatorOutline={new(1.65f,0),new(.55f,-.54f),new(-.10f,-.90f),new(-.94f,-.62f),new(-1.50f,-1.05f),new(-1.13f,0),new(-1.50f,1.05f),new(-.94f,.62f),new(-.10f,.90f),new(.55f,.54f)};
 int mapSize=Terrain.DefaultSize;
 public float Zoom=1.25f,CenterX=Terrain.DefaultSize*.5f,CenterY=Terrain.DefaultSize*.5f; public Rect View; public bool ShowSenses;
 public readonly Col Calm=Col.Hex(0x54A9CD),Hot=Col.Hex(0xDE7263),Text=Col.Hex(0x334D43);
 float Scale=>Math.Min((View.W-40)/(mapSize*1.414214f),(View.H-60)/(mapSize*1.414214f*CameraSin))*Zoom;
 // Minimum screen size is a visual aid only; collision and energy retain the true body radius.
 float BodyRadius(Creature creature)=>Math.Max(3.6f,creature.Radius*Scale*1.35f);
 public WorldView(Painter painter){p=painter;}
 public float MaximumZoom=>5f*mapSize/Terrain.DefaultSize;
 public void Bind(World world){
  if(world==null)throw new ArgumentNullException(nameof(world));mapSize=world.Map.Size;boundWorld=world;
  Zoom=Math.Clamp(float.IsFinite(Zoom)?Zoom:1.25f,.8f,MaximumZoom);
  CenterX=Math.Clamp(float.IsFinite(CenterX)?CenterX:mapSize*.5f,0,mapSize);
  CenterY=Math.Clamp(float.IsFinite(CenterY)?CenterY:mapSize*.5f,0,mapSize);
 }
 public void ZoomAt(float amount,int mx,int my){var before=WorldAt(mx,my);Zoom=Math.Clamp(Zoom*MathF.Pow(1.15f,amount),.8f,MaximumZoom);var after=WorldAt(mx,my);CenterX+=before.X-after.X;CenterY+=before.Y-after.Y;}
 static float Smooth(float t)=>t*t*(3-2*t);
 static float Noise(int x,int y,int seed){uint n=unchecked((uint)(x*374761393+y*668265263+seed*31));n=(n^(n>>13))*1274126177;return((n^(n>>16))&65535)/65535f;}
 static float Field(float x,float y,int seed){int ix=(int)MathF.Floor(x),iy=(int)MathF.Floor(y);float tx=Smooth(x-ix),ty=Smooth(y-iy);float a=Noise(ix,iy,seed)*(1-tx)+Noise(ix+1,iy,seed)*tx,b=Noise(ix,iy+1,seed)*(1-tx)+Noise(ix+1,iy+1,seed)*tx;return a*(1-ty)+b*ty;}
 bool Onscreen(Pt s,float margin)=>s.X>=View.X-margin&&s.Y>=View.Y-margin&&s.X<=View.X+View.W+margin&&s.Y<=View.Y+View.H+margin;
 void CacheObjects(World world){
  treeCells.Clear();rockCells.Clear();sortedTrees.Clear();sortedTrees.AddRange(world.Map.Trees.OrderBy(t=>t.X+t.Y));
  foreach(var tree in world.Map.Trees){int cell=world.Map.Cell(tree.X,tree.Y);if(cell>=0)treeCells.Add(cell);}
  for(int cell=0;cell<world.Map.Tiles.Length;cell++)if(world.Map.Tiles[cell]==2&&!treeCells.Contains(cell))rockCells.Add(cell);
  rocks.Clear();var occupied=new HashSet<int>(rockCells);var drawn=new HashSet<int>();
  foreach(int cell in rockCells){
   if(!drawn.Add(cell))continue;var members=new List<int>{cell};
   for(int head=0;head<members.Count&&members.Count<7;head++){
    int at=members[head],xx=at%mapSize,yy=at/mapSize;
    foreach(int next in new[]{xx+1<mapSize?at+1:-1,xx>0?at-1:-1,yy+1<mapSize?at+mapSize:-1,yy>0?at-mapSize:-1}){
     if(members.Count>=7)break;if(next>=0&&occupied.Contains(next)&&drawn.Add(next))members.Add(next);
    }
   }
   float minX=members.Min(v=>v%mapSize),maxX=members.Max(v=>v%mapSize),minY=members.Min(v=>v/mapSize),maxY=members.Max(v=>v/mapSize);
   float size=MathF.Sqrt(members.Count)*.85f+.4f;
   rocks.Add(new RockVisual((minX+maxX+1)*.5f,(minY+maxY+1)*.5f,size,size*.82f,Noise(cell,37,world.Seed)));
  }

 }
 void Leaf(float x,float y,float angle,float length,float width,Col dark,Col light){
  if(foodVertexCount+6>foodVertices.Length)Array.Resize(ref foodVertices,foodVertices.Length*2);
  float dx=MathF.Cos(angle),dy=MathF.Sin(angle);var root=new Vertex(x,y,dark);
  var a=new Vertex(x+dx*length*.48f-dy*width,y+dy*length*.48f+dx*width,Col.Mix(dark,light,.72f));
  var tip=new Vertex(x+dx*length,y+dy*length,light);
  var b=new Vertex(x+dx*length*.48f+dy*width,y+dy*length*.48f-dx*width,Col.Mix(dark,light,.4f));
  foodVertices[foodVertexCount++]=root;foodVertices[foodVertexCount++]=a;foodVertices[foodVertexCount++]=tip;
  foodVertices[foodVertexCount++]=root;foodVertices[foodVertexCount++]=tip;foodVertices[foodVertexCount++]=b;
 }
 void DrawFood(World world){
  foodVertexCount=0;int size=mapSize;
  var bounds=VisibleCells();int x0=bounds.X0,x1=bounds.X1,y0=bounds.Y0,y1=bounds.Y1;
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){
   int cell=x+y*size;float food=world.Map.Food[cell];if(world.Map.Tiles[cell]!=0||food<.15f)continue;
   var s=Screen(x+.5f,y+.5f);float stock=Math.Clamp(food/world.Config.GrassCapacity,0,1),length=(.22f+.29f*MathF.Sqrt(stock))*Scale;
   float angle=Noise(cell,23,world.Seed)*MathF.Tau;
   var dark=Col.Mix(Col.Hex(0x658B4F),Col.Hex(0x709C53),stock);var light=Col.Mix(Col.Hex(0xA8BE6D),Col.Hex(0xD0DB8C),stock);
   Leaf(s.X,s.Y,angle,length,length*.26f,dark,light);Leaf(s.X,s.Y,angle+2.2f,length*.83f,length*.24f,dark,light);
   if(stock>.2f)Leaf(s.X,s.Y,angle+4.3f,length*.91f,length*.25f,dark,light);
  }
  if(foodVertexCount>0)SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,foodVertices,foodVertexCount,null,0);
 }
 void RockPolygon(float x,float y,float width,float height,float variation){
  float flip=variation>.5f?-1f:1f,angle=(variation-.5f)*.28f,ca=MathF.Cos(angle),sa=MathF.Sin(angle);
  for(int i=0;i<8;i++){
   var a=rockOutline[i];var b=rockOutline[(i+1)%8];var face=Col.Mix(Col.Hex(0xC1CAB3),Col.Hex(0xB4C0AD),variation);var edge=Col.Mix(Col.Hex(0x899D89),Col.Hex(0xD2D7BD),Math.Clamp(.45f-a.X*.55f-a.Y*.65f,0,1));
   rockVertices[i*3]=new Vertex(x-width*.10f,y-height*.15f,face);rockVertices[i*3+1]=new Vertex(x+(a.X*flip*ca-a.Y*sa)*width,y+(a.X*flip*sa+a.Y*ca)*height,edge);rockVertices[i*3+2]=new Vertex(x+(b.X*flip*ca-b.Y*sa)*width,y+(b.X*flip*sa+b.Y*ca)*height,edge);
  }
  SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,rockVertices,24,null,0);
 }
 void DrawRocks(World world){
  LoadTreeSprite();
  // A joined mineral bed preserves the blocked footprint; several adjacent cells share a boulder.
  int count=0;foreach(int cell in rockCells){var s=Screen(cell%mapSize,cell/mapSize);if(!Onscreen(s,Scale+5))continue;if(count+6>rockBedVertices.Length)Array.Resize(ref rockBedVertices,rockBedVertices.Length*2);var color=new Col(114,136,108,32);var q0=Project(cell%mapSize,cell/mapSize,LandHeight);var q1=Project(cell%mapSize+1,cell/mapSize,LandHeight);var q2=Project(cell%mapSize+1,cell/mapSize+1,LandHeight);var q3=Project(cell%mapSize,cell/mapSize+1,LandHeight);var a=new Vertex(q0.X,q0.Y,color);var b=new Vertex(q1.X,q1.Y,color);var c=new Vertex(q2.X,q2.Y,color);var d=new Vertex(q3.X,q3.Y,color);rockBedVertices[count++]=a;rockBedVertices[count++]=b;rockBedVertices[count++]=c;rockBedVertices[count++]=a;rockBedVertices[count++]=c;rockBedVertices[count++]=d;}
  if(count>0)SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,rockBedVertices,count,null,0);
  foreach(var rock in rocks){var s=Screen(rock.X,rock.Y);float w=rock.Width*Scale,h=rock.Height*Scale;if(!Onscreen(s,Math.Max(w,h)+5))continue;
   p.Ellipse(s.X+Scale*.22f,s.Y+Scale*.29f,w*.66f,h*.56f,-.12f,new Col(42,69,51,45),new Col(42,69,51,0),12);if(rockSprite!=IntPtr.Zero){SDL.SDL_SetTextureColorMod(rockSprite,218,224,217);SDL.SDL_RenderCopy(p.Renderer,rockSprite,IntPtr.Zero,new Rect((int)(s.X-w*.78f),(int)(s.Y-h*1.08f),(int)(w*1.55f),(int)(h*1.55f)));}else{RockPolygon(s.X,s.Y+Scale*.22f,w,h,rock.Variation);RockPolygon(s.X,s.Y-Scale*.08f,w*.96f,h*.90f,rock.Variation);}
   if(Scale>8&&rock.Variation>.67f)p.Ellipse(s.X-w*.19f,s.Y+h*.22f,w*.18f,h*.11f,-.6f,Col.Hex(0x8DA66D),Col.Hex(0x809C68),10);
  }
 }
 void TerrainTexture(World world){
  if(ground!=IntPtr.Zero)SDL.SDL_DestroyTexture(ground);ground=IntPtr.Zero;const int size=1024;float samples=(float)size/mapSize;byte[] pixels=new byte[size*size*4];
  float dryness=Math.Clamp((1-world.Map.WaterLevel)/.88f,0,1);
  var waterMask=new float[world.Map.Tiles.Length];for(int ty=0;ty<mapSize;ty++)for(int tx=0;tx<mapSize;tx++){float sample=world.Map.Tiles[tx+ty*mapSize]==1?.72f:0;for(int d=0;d<4;d++){int xx=Math.Clamp(tx+(d==0?1:d==1?-1:0),0,mapSize-1),yy=Math.Clamp(ty+(d==2?1:d==3?-1:0),0,mapSize-1);if(world.Map.Tiles[xx+yy*mapSize]==1)sample+=.07f;}waterMask[tx+ty*mapSize]=sample;}
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float wx=(x+.5f)/samples-.5f,wy=(y+.5f)/samples-.5f;int ix=(int)MathF.Floor(wx),iy=(int)MathF.Floor(wy);float tx=Smooth(wx-ix),ty=Smooth(wy-iy);float water=0,wet=0,rock=0;
   for(int j=0;j<2;j++)for(int i=0;i<2;i++){int cell=Math.Clamp(iy+j,0,mapSize-1)*mapSize+Math.Clamp(ix+i,0,mapSize-1);float w=(i==0?1-tx:tx)*(j==0?1-ty:ty);water+=waterMask[cell]*w;rock+=(world.Map.Tiles[cell]==2&&!treeCells.Contains(cell)?1:0)*w;wet+=world.Map.Moisture[cell]*w;}
   float grain=(Noise(x,y,world.Seed)-.5f)*.023f,patch=Field(wx*.24f,wy*.24f,world.Seed+411),fine=Field(wx*.65f,wy*.65f,world.Seed+813);
   var grass=Col.Mix(Col.Hex(0x80A64E),Col.Hex(0x4E874D),Math.Clamp(wet*.32f+patch*.13f+fine*.13f+grain,0,1));
   grass=Col.Mix(grass,Col.Hex(0xCAB889),dryness*.46f);
   var c=Col.Mix(grass,Col.Hex(0xC3C8A0),rock*.28f);
   int tile=world.Map.Tiles[Math.Min(mapSize-1,(int)((x+.5f)/samples))+Math.Min(mapSize-1,(int)((y+.5f)/samples))*mapSize];
   if(tile==1){float ripples=Field(wx*.9f,wy*.9f,world.Seed+761);c=Col.Mix(Col.Hex(0x4CAFCB),Col.Hex(0x245A7D),Math.Clamp(.22f+water*.34f+ripples*.27f,0,1));}
   else{c=Col.Mix(grass,Col.Hex(0xB7A875),water*.16f);float blade=Noise(x/2,y/3,world.Seed+151);if(blade>.88f)c=Col.Mix(c,blade>.965f?Col.Hex(0xACC076):Col.Hex(0x557B44),.17f);}
   int k=(y*size+x)*4;pixels[k]=c.B;pixels[k+1]=c.G;pixels[k+2]=c.R;pixels[k+3]=255;
  }
  ground=SDL.SDL_CreateTexture(p.Renderer,372645892,0,size,size);if(ground==IntPtr.Zero)throw new Exception(SDL.Error);if(SDL.SDL_UpdateTexture(ground,IntPtr.Zero,pixels,size*4)!=0)throw new Exception(SDL.Error);SDL.SDL_SetTextureScaleMode(ground,1);
  groundWorld=world;groundMap=world.Map;groundWaterLevel=world.Map.WaterLevel;groundTick=world.Tick;groundDrought=world.Drought;
 }
 void DrawWater(World world){
  // Sparse, world-anchored glints; decorative motion uses simulation time, never biological RNG.
  float time=(float)(world.Tick*World.Dt);
  var bounds=VisibleCells();int x0=Math.Max(1,bounds.X0),y0=Math.Max(1,bounds.Y0),x1=Math.Min(mapSize-2,bounds.X1),y1=Math.Min(mapSize-2,bounds.Y1);
  for(int y=y0;y<=y1;y+=3)for(int x=x0;x<=x1;x+=3){int cell=x+y*mapSize;if(world.Map.Tiles[cell]!=1||Noise(x,y,world.Seed+302)>.25f)continue;var s=Screen(x+.5f,y+.5f);if(!Onscreen(s,10))continue;
   float phase=Noise(x,y,world.Seed+411)*6.28f,fade=.5f+.5f*MathF.Sin(time*.55f+phase);byte alpha=(byte)(38+54*fade);float width=Scale*(.24f+.31f*fade);p.Line(s.X-width,s.Y,s.X+width,s.Y,new Col(217,240,212,alpha));
  }
 }
 void IndexBodies(World world,float interpolation){
  gridColumns=(View.W+OcclusionCell-1)/OcclusionCell+2;gridRows=(View.H+OcclusionCell-1)/OcclusionCell+2;int count=gridColumns*gridRows;if(cellHeads.Length<count)cellHeads=new int[count];Array.Fill(cellHeads,-1,0,count);visible.Clear();largestBody=0;
  foreach(var c in world.Creatures){if(!c.Alive)continue;var s=Screen(c.X-c.Vx*.05f*(1-interpolation),c.Y-c.Vy*.05f*(1-interpolation));float r=BodyRadius(c);if(!Onscreen(s,Math.Max(64,r*2)))continue;int index=visible.Count;if(index==bodyPool.Count)bodyPool.Add(new VisibleBody());var body=bodyPool[index];body.C=c;body.Position=s;body.Radius=r;body.Obscured=false;visible.Add(body);largestBody=Math.Max(largestBody,r*1.8f);if(index>=bodyNext.Length)Array.Resize(ref bodyNext,bodyNext.Length*2);
   int gx=Math.Clamp((int)MathF.Floor((s.X-View.X)/OcclusionCell)+1,0,gridColumns-1),gy=Math.Clamp((int)MathF.Floor((s.Y-View.Y)/OcclusionCell)+1,0,gridRows-1),cell=gx+gy*gridColumns;bodyNext[index]=cellHeads[cell];cellHeads[cell]=index;
  }
 }
 bool MarkCanopy(Pt s,float radius){
  bool overlaps=false;float reach=radius+largestBody;int x0=Math.Clamp((int)MathF.Floor((s.X-reach-View.X)/OcclusionCell)+1,0,gridColumns-1),x1=Math.Clamp((int)MathF.Floor((s.X+reach-View.X)/OcclusionCell)+1,0,gridColumns-1),y0=Math.Clamp((int)MathF.Floor((s.Y-reach-View.Y)/OcclusionCell)+1,0,gridRows-1),y1=Math.Clamp((int)MathF.Floor((s.Y+reach-View.Y)/OcclusionCell)+1,0,gridRows-1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)for(int i=cellHeads[x+y*gridColumns];i>=0;i=bodyNext[i]){var b=visible[i];float dx=b.Position.X-s.X,dy=b.Position.Y-s.Y,r=radius+b.Radius*1.8f;if(dx*dx+dy*dy<r*r){b.Obscured=true;overlaps=true;}}
  return overlaps;
 }
 Col BodyColor(World world,Creature c){var hue=c.Genes[4]<.5f?Col.Mix(Calm,Col.Hex(0xAA8CC7),c.Genes[4]*2):Col.Mix(Col.Hex(0xAA8CC7),Hot,(c.Genes[4]-.5f)*2);byte gray=(byte)(hue.R*.2126f+hue.G*.7152f+hue.B*.0722f);return Col.Mix(hue,new Col(gray,gray,gray),.85f*world.Aging(c));}
 void DrawBody(World world,VisibleBody b,int selected,bool aboveCanopy){
  var c=b.C;var s=b.Position;float r=b.Radius,angle=MathF.Atan2((MathF.Cos(c.Angle)+MathF.Sin(c.Angle))*CameraSin,MathF.Cos(c.Angle)-MathF.Sin(c.Angle)),ca=MathF.Cos(angle),sa=MathF.Sin(angle);var hue=BodyColor(world,c);var rim=Col.Mix(hue,Col.Hex(0x294F48),.46f);var light=Col.Mix(hue,Col.Hex(0xF1EDC8),.27f);
  if(!aboveCanopy)p.Ellipse(s.X+1.2f,s.Y+2,r*1.26f,r*.92f,angle,new Col(29,68,53,65),new Col(29,68,53,0),16);
  if(c.Genes[2]<.5f){
   float ear=r*.49f;
   if(aboveCanopy){p.Disk(s.X-ca*r*.60f-sa*r*.66f,s.Y-sa*r*.60f+ca*r*.66f,ear+1.1f,Col.Hex(0xF0EED4),12);p.Disk(s.X-ca*r*.60f+sa*r*.66f,s.Y-sa*r*.60f-ca*r*.66f,ear+1.1f,Col.Hex(0xF0EED4),12);p.Ellipse(s.X,s.Y,r+1.4f,r+1.2f,angle,Col.Hex(0xF0EED4),Col.Hex(0xF0EED4),18);}
   p.Disk(s.X-ca*r*.60f-sa*r*.66f,s.Y-sa*r*.60f+ca*r*.66f,ear,rim,12);p.Disk(s.X-ca*r*.60f+sa*r*.66f,s.Y-sa*r*.60f-ca*r*.66f,ear,rim,12);
   p.Disk(s.X-ca*r*.59f-sa*r*.66f,s.Y-sa*r*.59f+ca*r*.66f,ear*.61f,light,10);p.Disk(s.X-ca*r*.59f+sa*r*.66f,s.Y-sa*r*.59f-ca*r*.66f,ear*.61f,light,10);
   p.Ellipse(s.X,s.Y,r+.4f,r,angle,rim,rim,20);p.Ellipse(s.X-ca*.25f,s.Y-sa*.25f,Math.Max(1,r-1.1f),Math.Max(1,r-1.4f),angle,light,hue,20);
  }else{
   if(aboveCanopy)Shape(s.X,s.Y,r+1.1f,angle,predatorOutline,Col.Hex(0xF0EED4),Col.Hex(0xF0EED4));
   Shape(s.X,s.Y,r,angle,predatorOutline,rim,rim);Shape(s.X+ca*.35f,s.Y+sa*.35f,r*.74f,angle,predatorOutline,light,hue);
   if(r>5)p.Line(s.X-ca*r*.62f,s.Y-sa*r*.62f,s.X+ca*r*.43f,s.Y+sa*r*.43f,Col.Mix(hue,Col.Hex(0xF5E9C7),.44f));
  }
  float eye=Math.Max(.75f,r*.12f);p.Disk(s.X+ca*r*.49f-sa*r*.29f,s.Y+sa*r*.49f+ca*r*.29f,eye,Col.Hex(0x24443A),8);p.Disk(s.X+ca*r*.49f+sa*r*.29f,s.Y+sa*r*.49f-ca*r*.29f,eye,Col.Hex(0x24443A),8);
  if(c.Genes[3]>=.85f)p.Ring(s.X,s.Y,r+2.4f,new Col(80,178,177,130));
  if(c.Id==selected){p.Ring(s.X,s.Y,r+6,Col.Hex(0xFAF2CF));p.Ring(s.X,s.Y,r+7.6f,Col.Hex(0x4C7760));}
 }
 void CrownLobe(float x,float y,float r,float height,float twist,Col shade,Col foliage,Col highlight){
  const int n=9;int count=0;
  for(int i=0;i<n;i++){
   float a=i*MathF.Tau/n,b=(i+1)*MathF.Tau/n;
   float ra=r*(.91f+.09f*MathF.Sin(i*2.37f+twist)),rb=r*(.91f+.09f*MathF.Sin((i+1)*2.37f+twist));
   var pa=new Pt(x+MathF.Cos(a)*ra,y+MathF.Sin(a)*ra*.82f);
   var pb=new Pt(x+MathF.Cos(b)*rb,y+MathF.Sin(b)*rb*.82f);
   var side=Col.Mix(shade,foliage,.17f+.28f*(1-MathF.Cos(a)));
   shapeVertices[count++]=new(pa.X,pa.Y,side);shapeVertices[count++]=new(pb.X,pb.Y,side);shapeVertices[count++]=new(pb.X,pb.Y-height,side);
   shapeVertices[count++]=new(pa.X,pa.Y,side);shapeVertices[count++]=new(pb.X,pb.Y-height,side);shapeVertices[count++]=new(pa.X,pa.Y-height,side);
   var face=Col.Mix(foliage,highlight,Math.Clamp(.45f-.30f*MathF.Sin(a)-.20f*MathF.Cos(a)+.08f*MathF.Sin(i*3.1f+twist),0,1));
   shapeVertices[count++]=new(x-r*.12f,y-height-r*.10f,face);shapeVertices[count++]=new(pa.X,pa.Y-height,face);shapeVertices[count++]=new(pb.X,pb.Y-height,face);
  }
  SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,shapeVertices,count,null,0);
 }
 void Canopy(float x,float y,float r,float twist,Col shade,Col foliage,Col highlight){
  // Layered, asymmetrical polygonal volumes with a consistent upper-left light.
  // At distant zoom keep the main silhouette; close zoom reveals smaller crown masses.
  CrownLobe(x,y+r*.12f,r*.97f,r*.22f,twist,shade,foliage,highlight);
  for(int row=0;row<2;row++)for(int col=0;col<3;col++){
   int i=row*3+col;float xx=x+(col-1)*r*.48f+MathF.Sin(twist+i)*r*.10f;
   float yy=y+(row-.5f)*r*.58f;float rr=r*(.43f+.07f*MathF.Sin(twist+i*1.7f));
   CrownLobe(xx,yy,rr,r*(.18f+.06f*MathF.Sin(twist+i)),twist+i,shade,Col.Mix(foliage,highlight,.08f*(i%3)),highlight);
  }
  if(r>18)CrownLobe(x-r*.12f,y-r*.22f,r*.37f,r*.24f,twist+7,shade,foliage,highlight);
 }
 byte CanopyAlpha(Tree tree,bool occupied){
  float desired=occupied?86:255;
  if(!canopyFades.TryGetValue(tree,out var state)){state=new CanopyFade{Alpha=desired};canopyFades.Add(tree,state);}
  if(occupied)state.ClearAfter=renderTime+.16;
  if(renderTime<state.ClearAfter)desired=86;
  float duration=desired<state.Alpha?.15f:.30f;state.Alpha+=(desired-state.Alpha)*(1-MathF.Exp(-renderDelta/duration));
  return(byte)Math.Clamp(MathF.Round(state.Alpha),86,255);
 }
 void DrawTrees(World world){
  LoadTreeSprite();if(treeSprite!=IntPtr.Zero){DrawTallTrees(world);return;}
  foreach(var tree in world.Map.Trees){var s=Screen(tree.X,tree.Y);float r=tree.Radius*Scale;if(!Onscreen(s,r*1.5f))continue;bool overlaps=MarkCanopy(s,r*1.08f);byte alpha=CanopyAlpha(tree,overlaps);
   p.Ellipse(s.X+r*.32f,s.Y+r*.38f,r*1.17f,r*.91f,-.12f,new Col(31,75,52,(byte)(24+(alpha-86)*32/169)),new Col(31,75,52,0),20);
   float twist=Noise((int)(tree.X*10),(int)(tree.Y*10),world.Seed)*6,dry=Math.Clamp((1-world.Map.WaterLevel)/.88f,0,1),variation=Noise((int)tree.X,(int)tree.Y,world.Seed+33);
   p.Disk(s.X,s.Y,Math.Max(2,Scale*.20f),Col.Hex(0x6E7550),10);p.Disk(s.X-Scale*.025f,s.Y-Scale*.035f,Math.Max(1,Scale*.11f),Col.Hex(0xA99569),8);
   Col shade=Col.Mix(Col.Hex(0x28684F),Col.Hex(0x667C4D),dry).Alpha(alpha),foliage=Col.Mix(Col.Mix(Col.Hex(0x659A49),Col.Hex(0x478354),variation),Col.Hex(0xA4AC69),dry).Alpha(alpha),highlight=Col.Mix(Col.Mix(Col.Hex(0xA5C970),Col.Hex(0x87B960),variation),Col.Hex(0xC2BF81),dry).Alpha(alpha);
   p.Box(s.X-Scale*.13f,s.Y-r*.20f,Scale*.26f,r*.52f,Col.Hex(0x796E4C).Alpha(alpha));
   p.Line(s.X-Scale*.07f,s.Y-r*.19f,s.X-Scale*.07f,s.Y+r*.30f,Col.Hex(0xC2A875).Alpha(alpha));
   Canopy(s.X,s.Y-r*.08f,r,twist,shade,foliage,highlight);
  }
  foreach(var tree in world.Map.Trees){var s=Screen(tree.X,tree.Y);if(!Onscreen(s,tree.Radius*Scale+5))continue;for(int j=0;j<tree.Fruit.Length;j++){
   if(tree.Fruit[j]<=0)continue;var f=tree.FruitPosition(j);var q=Screen(f.X,f.Y);float ar=Math.Max(Math.Clamp(Scale*.38f,1.2f,2.3f),Scale*.25f)*(0.72f+.28f*MathF.Sqrt(tree.Fruit[j]/world.Config.FruitEnergy));
   p.Disk(q.X+.5f,q.Y+.9f,ar+.7f,Col.Hex(0x56633E),10);p.Disk(q.X,q.Y,ar,Col.Hex(0xE74C3D),12);p.Disk(q.X-ar*.26f,q.Y-ar*.26f,ar*.31f,Col.Hex(0xFFD09A),8);p.Line(q.X,q.Y-ar,q.X+.4f,q.Y-ar-1.8f,Col.Hex(0x517B43));
  }}
 }
 public int Pick(World world,int x,int y){Bind(world);int best=0;float dist=float.PositiveInfinity;foreach(var c in world.Creatures){if(!c.Alive)continue;var q=Screen(c.X,c.Y);float d=(q.X-x)*(q.X-x)+(q.Y-y)*(q.Y-y);float hitRadius=Math.Max(8,BodyRadius(c)*(c.Genes[2]>.55f?1.3f:1.05f)+4);if(d<dist&&d<hitRadius*hitRadius){dist=d;best=c.Id;}}return best;}
 public void Draw(World world,int selected,float interpolation){
  Bind(world);
  long now=System.Diagnostics.Stopwatch.GetTimestamp();renderTime=(double)now/System.Diagnostics.Stopwatch.Frequency;renderDelta=renderClock==0?0:Math.Clamp((float)((double)(now-renderClock)/System.Diagnostics.Stopwatch.Frequency),0,.10f);renderClock=now;
  bool newWorld=!ReferenceEquals(groundWorld,world)||!ReferenceEquals(groundMap,world.Map);if(newWorld){CacheObjects(world);canopyFades.Clear();}bool wetnessChanged=Math.Abs(groundWaterLevel-world.Map.WaterLevel)>=.04f&&world.Tick-groundTick>=100;
  if(newWorld||ground==IntPtr.Zero||groundDrought!=world.Drought||wetnessChanged)TerrainTexture(world);
  p.Clip(View);p.Box(View.X,View.Y,View.W,View.H,Col.Hex(0xD9E2D7));DrawRaisedTerrain(world);
  DrawWater(world);DrawMapLayer(world);DrawFood(world);DrawRocks(world);
  foreach(var shelter in world.Map.Shelters){var s=Screen(shelter.X,shelter.Y);if(!Onscreen(s,Scale*4))continue;p.Ellipse(s.X+Scale*.2f,s.Y+Scale*.3f,Scale*2.5f,Scale*1.5f,-.3f,new Col(67,88,55,65),new Col(67,88,55,0));p.Ellipse(s.X,s.Y,Scale*2.2f,Scale*1.4f,-.3f,Col.Hex(0x8B956C),Col.Hex(0xABB589));p.Ellipse(s.X,s.Y-Scale*.45f,Scale*1.6f,Scale*1.05f,-.3f,Col.Hex(0xD2CEAA),Col.Hex(0xADB58D));}
  foreach(var corpse in world.Corpses){if(corpse.Food<=0)continue;var s=Screen(corpse.X,corpse.Y);if(!Onscreen(s,Scale))continue;float age=(float)((world.Tick-corpse.CreatedTick)*World.Dt);byte alpha=(byte)(180*Math.Clamp((world.Config.CorpseLifetime-age)/Math.Min(25,world.Config.CorpseLifetime),0,1));p.Ellipse(s.X,s.Y,Scale*.43f,Scale*.27f,.4f,Col.Hex(0x957D61).Alpha(alpha),Col.Hex(0x8C7D65).Alpha(alpha),8);}
  var chosen=world.Find(selected);
  if(chosen!=null&&chosen.Alive&&ShowSenses){var start=Screen(chosen.X,chosen.Y);if(float.IsFinite(chosen.TargetX)&&float.IsFinite(chosen.TargetY)){var target=Screen(chosen.TargetX,chosen.TargetY);p.Line(start.X,start.Y,target.X,target.Y,new Col(50,92,74,130));p.Ring(target.X,target.Y,4,Col.Hex(0x396E53));}}
  IndexBodies(world,interpolation);foreach(var b in visible)DrawBody(world,b,selected,false);DrawTrees(world);
  // Every occluded creature is readable, including unselected animals in overlapping crowns.
  foreach(var b in visible)if(b.Obscured&&b.C.Id!=selected)DrawBody(world,b,selected,true);
  foreach(var b in visible)if(b.C.Id==selected){DrawBody(world,b,selected,b.Obscured);break;}
  p.Unclip();
 }
 void Shape(float x,float y,float radius,float angle,Pt[] contour,Col center,Col edge){
  float ca=MathF.Cos(angle),sa=MathF.Sin(angle);for(int i=0;i<contour.Length;i++){var a=contour[i];var b=contour[(i+1)%contour.Length];shapeVertices[i*3]=new(x,y,center);shapeVertices[i*3+1]=new(x+(a.X*ca-a.Y*sa)*radius,y+(a.X*sa+a.Y*ca)*radius,edge);shapeVertices[i*3+2]=new(x+(b.X*ca-b.Y*sa)*radius,y+(b.X*sa+b.Y*ca)*radius,edge);}SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,shapeVertices,contour.Length*3,null,0);
 }
 public void Dispose(){if(rockSprite!=IntPtr.Zero)SDL.SDL_DestroyTexture(rockSprite);if(treeSprite!=IntPtr.Zero)SDL.SDL_DestroyTexture(treeSprite);if(ground!=IntPtr.Zero)SDL.SDL_DestroyTexture(ground);}
}
