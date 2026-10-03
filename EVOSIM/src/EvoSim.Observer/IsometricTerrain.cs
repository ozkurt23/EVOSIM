using EvoSim.Core;
namespace EvoSim.Observer;
internal sealed partial class WorldView
{
 World? boundWorld;
 const float CameraSin=.72f,Diagonal=.70710678f,LandHeight=1.05f;
 Vertex[] terrainVertices=Array.Empty<Vertex>(),shoreVertices=Array.Empty<Vertex>();
 Pt Project(float x,float y,float z=0)=>new(View.X+View.W/2f+(x-CenterX-y+CenterY)*Diagonal*Scale,View.Y+View.H/2f+(x-CenterX+y-CenterY)*Diagonal*CameraSin*Scale-z*Scale);
 float Elevation(float x,float y){int cell=boundWorld?.Map.Cell(x,y)??-1;return cell>=0&&boundWorld!.Map.Tiles[cell]!=1?LandHeight:0;}
 Pt Unproject(float x,float y,float z=0){float dx=(x-View.X-View.W/2f)/(Diagonal*Scale),dy=(y-View.Y-View.H/2f+z*Scale)/(Diagonal*CameraSin*Scale);return new(CenterX+(dy+dx)*.5f,CenterY+(dy-dx)*.5f);}
 public Pt Screen(float x,float y)=>Project(x,y,Elevation(x,y));
 public Pt WorldAt(float x,float y){var land=Unproject(x,y,LandHeight);return Elevation(land.X,land.Y)>0?land:Unproject(x,y);}
 public void Pan(int dx,int dy){var delta=Unproject(View.X+View.W/2f+dx,View.Y+View.H/2f+dy);CenterX=Math.Clamp(CenterX-(delta.X-CenterX),0,mapSize);CenterY=Math.Clamp(CenterY-(delta.Y-CenterY),0,mapSize);}
 (int X0,int Y0,int X1,int Y1) VisibleCells(float margin=12){var corners=new[]{Unproject(View.X-margin,View.Y-margin),Unproject(View.X+View.W+margin,View.Y-margin),Unproject(View.X-margin,View.Y+View.H+margin),Unproject(View.X+View.W+margin,View.Y+View.H+margin)};return(Math.Clamp((int)MathF.Floor(corners.Min(c=>c.X))-4,0,mapSize-1),Math.Clamp((int)MathF.Floor(corners.Min(c=>c.Y))-4,0,mapSize-1),Math.Clamp((int)MathF.Ceiling(corners.Max(c=>c.X))+4,0,mapSize-1),Math.Clamp((int)MathF.Ceiling(corners.Max(c=>c.Y))+4,0,mapSize-1));}
 void DrawRaisedTerrain(World world){
  int needed=mapSize*mapSize*6;if(terrainVertices.Length<needed)terrainVertices=new Vertex[needed];if(shoreVertices.Length<needed*2)shoreVertices=new Vertex[needed*2];
  var area=VisibleCells();int count=0,sides=0;
  Vertex V(Pt pos,float u,float v,Col color)=>new(pos.X,pos.Y,color){UV=new(u/mapSize,v/mapSize)};
  void Quad(int x,int y,float height){var a=V(Project(x,y,height),x,y,new Col(255,255,255));var b=V(Project(x+1,y,height),x+1,y,new Col(255,255,255));var c=V(Project(x+1,y+1,height),x+1,y+1,new Col(255,255,255));var d=V(Project(x,y+1,height),x,y+1,new Col(255,255,255));terrainVertices[count++]=a;terrainVertices[count++]=b;terrainVertices[count++]=c;terrainVertices[count++]=a;terrainVertices[count++]=c;terrainVertices[count++]=d;}
  // Water first. The vertical banks and raised land then explain the same physical coast.
  for(int y=area.Y0;y<=area.Y1;y++)for(int x=area.X0;x<=area.X1;x++)if(world.Map.Tiles[x+y*mapSize]==1)Quad(x,y,0);
  if(count>0)SDL.SDL_RenderGeometry(p.Renderer,ground,terrainVertices,count,null,0);count=0;
  void Wall(float ax,float ay,float bx,float by,bool east){Pt a=Project(ax,ay,LandHeight),b=Project(bx,by,LandHeight),c=Project(bx,by),d=Project(ax,ay);float variant=Noise((int)ax,(int)ay,world.Seed);var top=Col.Mix(Col.Hex(0x6B6244),Col.Hex(0x877458),variant*.45f);var bottom=Col.Hex(east?0x403C36u:0x514639u);shoreVertices[sides++]=new(a.X,a.Y,top);shoreVertices[sides++]=new(b.X,b.Y,top);shoreVertices[sides++]=new(c.X,c.Y,bottom);shoreVertices[sides++]=new(a.X,a.Y,top);shoreVertices[sides++]=new(c.X,c.Y,bottom);shoreVertices[sides++]=new(d.X,d.Y,bottom);}
  for(int y=area.Y0;y<=area.Y1;y++)for(int x=area.X0;x<=area.X1;x++)if(world.Map.Tiles[x+y*mapSize]!=1){Quad(x,y,LandHeight);if(x==mapSize-1||world.Map.Tiles[x+1+y*mapSize]==1)Wall(x+1,y,x+1,y+1,true);if(y==mapSize-1||world.Map.Tiles[x+(y+1)*mapSize]==1)Wall(x,y+1,x+1,y+1,false);}
  if(sides>0)SDL.SDL_RenderGeometry(p.Renderer,IntPtr.Zero,shoreVertices,sides,null,0);
  if(count>0)SDL.SDL_RenderGeometry(p.Renderer,ground,terrainVertices,count,null,0);
 }
}
