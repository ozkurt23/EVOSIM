using EvoSim.Core;
namespace EvoSim.Observer;
internal sealed partial class WorldView
{
 IntPtr treeSprite,rockSprite;bool treeSpriteChecked;
 readonly List<Tree> sortedTrees=new();
 void LoadTreeSprite(){
  if(treeSpriteChecked)return;treeSpriteChecked=true;
  treeSprite=ReadSprite("tall-tree.bgra");rockSprite=ReadSprite("granite.bgra");
 }
 IntPtr ReadSprite(string name){
  string path=Path.Combine(AppContext.BaseDirectory,"assets",name);if(!File.Exists(path))return IntPtr.Zero;
  using var reader=new BinaryReader(File.OpenRead(path));int width=reader.ReadInt32(),height=reader.ReadInt32();
  if(width<1||height<1||width>4096||height>4096)throw new InvalidDataException("Ağaç görseli boyutu geçersiz.");
  var pixels=reader.ReadBytes(checked(width*height*4));if(pixels.Length!=width*height*4)throw new InvalidDataException("Ağaç görseli eksik.");
  var texture=SDL.SDL_CreateTexture(p.Renderer,372645892,0,width,height);if(texture==IntPtr.Zero)throw new Exception(SDL.Error);
  SDL.SDL_UpdateTexture(texture,IntPtr.Zero,pixels,width*4);SDL.SDL_SetTextureBlendMode(texture,1);SDL.SDL_SetTextureScaleMode(texture,1);
  return texture;
 }
 public Rect TreeBounds(Tree tree){var s=Screen(tree.X,tree.Y);float r=tree.Radius*Scale;return new Rect((int)(s.X-r*1.18f),(int)(s.Y-r*3.70f),(int)(r*2.36f),(int)(r*3.9f));}
 void DrawTallTrees(World world){
  foreach(var tree in sortedTrees){
   var s=Screen(tree.X,tree.Y);float r=tree.Radius*Scale;var bounds=TreeBounds(tree);
   if(bounds.X>View.X+View.W||bounds.Y>View.Y+View.H||bounds.X+bounds.W<View.X||bounds.Y+bounds.H<View.Y)continue;
   // Cover the full lifted silhouette, not just the ground footprint.
   bool overlaps=MarkCanopy(new Pt(s.X,s.Y-r*1.0f),r*1.20f);
   overlaps=MarkCanopy(new Pt(s.X,s.Y-r*2.6f),r*.98f)||overlaps;
   byte alpha=(byte)(128+(CanopyAlpha(tree,overlaps)-86)*127/169);
   p.Ellipse(s.X+r*.5f,s.Y+r*.40f,r*1.3f,r*.62f,-.48f,new Col(26,65,48,(byte)(alpha*.27f)),new Col(26,65,48,0),20);
   float dry=Math.Clamp((1-world.Map.WaterLevel)/.88f,0,1),variation=Noise((int)tree.X,(int)tree.Y,world.Seed+122);
   SDL.SDL_SetTextureColorMod(treeSprite,(byte)(238+17*variation),(byte)(255-dry*35),(byte)(245-dry*78));SDL.SDL_SetTextureAlphaMod(treeSprite,alpha);
   SDL.SDL_RenderCopy(p.Renderer,treeSprite,IntPtr.Zero,bounds);
   for(int j=0;j<tree.Fruit.Length;j++){
    if(tree.Fruit[j]<=0)continue;
    float a=j*MathF.Tau/tree.Fruit.Length+.25f,spread=.40f+.30f*Noise(j,(int)(tree.X*31),world.Seed);
    float xx=s.X+MathF.Cos(a)*r*spread,yy=s.Y-r*2.15f+MathF.Sin(a)*r*.82f;
    float size=Math.Max(1.7f,Scale*.24f)*(.72f+.28f*MathF.Sqrt(tree.Fruit[j]/world.Config.FruitEnergy));
    p.Disk(xx+.5f,yy+.7f,size+.5f,Col.Hex(0x4A553A),10);p.Disk(xx,yy,size,Col.Hex(0xEC5241),12);p.Disk(xx-size*.27f,yy-size*.26f,size*.29f,Col.Hex(0xFFCB8A),8);
   }
  }
 }
}
