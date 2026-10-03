using System.Runtime.InteropServices;
namespace EvoSim.Observer;

[StructLayout(LayoutKind.Sequential)] internal struct Rect { public int X,Y,W,H; public Rect(int x,int y,int w,int h){X=x;Y=y;W=w;H=h;} public bool Contains(int x,int y)=>x>=X&&y>=Y&&x<X+W&&y<Y+H; }
[StructLayout(LayoutKind.Sequential)] internal struct Col {public byte R,G,B,A; public Col(byte r,byte g,byte b,byte a=255){R=r;G=g;B=b;A=a;} public Col Alpha(byte a)=>new(R,G,B,a); public static Col Hex(uint x)=>new((byte)(x>>16),(byte)(x>>8),(byte)x); public static Col Mix(Col a,Col b,float t)=>new((byte)(a.R+(b.R-a.R)*t),(byte)(a.G+(b.G-a.G)*t),(byte)(a.B+(b.B-a.B)*t),(byte)(a.A+(b.A-a.A)*t));}
[StructLayout(LayoutKind.Sequential)] internal struct Pt {public float X,Y;public Pt(float x,float y){X=x;Y=y;}}
[StructLayout(LayoutKind.Sequential)] internal struct Vertex {public Pt Pos;public Col Color; public Pt UV;public Vertex(float x,float y,Col color){Pos=new(x,y);Color=color;UV=new();}}
[StructLayout(LayoutKind.Explicit,Size=56)] internal struct Event { [FieldOffset(0)]public uint Type; [FieldOffset(12)]public byte WindowEvent; [FieldOffset(16)]public int A; [FieldOffset(20)]public int B; [FieldOffset(24)]public int C; [FieldOffset(16)]public byte Button; [FieldOffset(13)]public byte Repeat; [FieldOffset(12)] public unsafe fixed byte Text[32]; public unsafe string TextValue(){fixed(byte* b=Text)return Marshal.PtrToStringUTF8((IntPtr)b)??"";} }
internal static class SDL {
 const string Lib="libSDL2-2.0.so.0",Ttf="libSDL2_ttf-2.0.so.0";
 [DllImport(Lib)]public static extern int SDL_Init(uint flags);
 [DllImport(Lib)]public static extern IntPtr SDL_CreateWindow([MarshalAs(UnmanagedType.LPUTF8Str)]string title,int x,int y,int w,int h,uint flags);
 [DllImport(Lib)]public static extern void SDL_SetWindowMinimumSize(IntPtr window,int w,int h);
 [DllImport(Lib)]public static extern IntPtr SDL_CreateRenderer(IntPtr window,int index,uint flags);
 [DllImport(Lib)]public static extern void SDL_GetWindowSize(IntPtr window,out int w,out int h);
 [DllImport(Lib)]public static extern int SDL_PollEvent(out Event ev);
 [DllImport(Lib)]public static extern int SDL_PushEvent(ref Event ev);
 [DllImport(Lib)]public static extern void SDL_StartTextInput();
 [DllImport(Lib)]public static extern void SDL_StopTextInput();
 [DllImport(Lib)]public static extern uint SDL_GetMouseState(out int x,out int y);
 [DllImport(Lib)]public static extern int SDL_SetRenderDrawColor(IntPtr r,byte red,byte green,byte blue,byte alpha);
 [DllImport(Lib)]public static extern int SDL_SetRenderDrawBlendMode(IntPtr r,int mode);
 [DllImport(Lib)]public static extern int SDL_RenderClear(IntPtr r);
 [DllImport(Lib)]public static extern void SDL_RenderPresent(IntPtr r);
 [DllImport(Lib)]public static extern int SDL_RenderFillRect(IntPtr r,in Rect rect);
 [DllImport(Lib)]public static extern int SDL_RenderDrawLineF(IntPtr r,float x1,float y1,float x2,float y2);
 [DllImport(Lib)]public static extern int SDL_RenderGeometry(IntPtr r,IntPtr texture,Vertex[] vertices,int count,int[]? indices,int indexCount);
 [DllImport(Lib)]public static extern int SDL_RenderSetClipRect(IntPtr r,in Rect rect);
 [DllImport(Lib,EntryPoint="SDL_RenderSetClipRect")]public static extern int ClearClip(IntPtr r,IntPtr rect);
 [DllImport(Lib)]public static extern IntPtr SDL_CreateTexture(IntPtr r,uint format,int access,int w,int h);
 [DllImport(Lib)]public static extern int SDL_UpdateTexture(IntPtr texture,IntPtr rect,byte[] pixels,int pitch);
 [DllImport(Lib)]public static extern int SDL_SetTextureBlendMode(IntPtr texture,int mode);
 [DllImport(Lib)]public static extern int SDL_SetTextureScaleMode(IntPtr texture,int mode);
 [DllImport(Lib)]public static extern int SDL_SetTextureColorMod(IntPtr texture,byte red,byte green,byte blue);
 [DllImport(Lib)]public static extern int SDL_SetTextureAlphaMod(IntPtr texture,byte alpha);
 [DllImport(Lib)]public static extern IntPtr SDL_CreateTextureFromSurface(IntPtr r,IntPtr surface);
 [DllImport(Lib)]public static extern int SDL_QueryTexture(IntPtr t,out uint format,out int access,out int w,out int h);
 [DllImport(Lib)]public static extern int SDL_RenderCopy(IntPtr r,IntPtr texture,IntPtr src,in Rect dst);
 [DllImport(Lib)]public static extern void SDL_DestroyTexture(IntPtr texture);
 [DllImport(Lib)]public static extern void SDL_FreeSurface(IntPtr surface);
 [DllImport(Lib)]public static extern int SDL_RenderReadPixels(IntPtr r,IntPtr rect,uint format,byte[] pixels,int pitch);
 [DllImport(Lib)]public static extern void SDL_DestroyRenderer(IntPtr r);
 [DllImport(Lib)]public static extern void SDL_DestroyWindow(IntPtr w);
 [DllImport(Lib)]public static extern void SDL_Quit();
 [DllImport(Lib)]public static extern IntPtr SDL_GetError();
 [DllImport(Ttf)]public static extern int TTF_Init();
 [DllImport(Ttf)]public static extern IntPtr TTF_OpenFont([MarshalAs(UnmanagedType.LPUTF8Str)]string path,int size);
 [DllImport(Ttf)]public static extern IntPtr TTF_RenderUTF8_Blended(IntPtr font,[MarshalAs(UnmanagedType.LPUTF8Str)]string text,Col color);
 [DllImport(Ttf)]public static extern void TTF_CloseFont(IntPtr font);
 [DllImport(Ttf)]public static extern void TTF_Quit();
 public static string Error=>Marshal.PtrToStringUTF8(SDL_GetError())??"SDL error";
}
internal sealed class Painter : IDisposable {
 public readonly IntPtr Renderer,Window;
 readonly Dictionary<int,IntPtr> fonts=new();
 readonly Dictionary<(string,int),(IntPtr,int,int)> labels=new();
 Vertex[] shapeVertices=new Vertex[384];
 public int W,H;
 public Painter(bool hidden=false){
  if(SDL.SDL_Init(0x20)!=0)throw new Exception(SDL.Error);
  Window=SDL.SDL_CreateWindow("EVOSIM 0.5.10 · Doğa gözlemevi",0x2FFF0000,0x2FFF0000,1480,940,(uint)(0x20|(hidden?8:4)));
  if(Window==IntPtr.Zero)throw new Exception(SDL.Error);
  SDL.SDL_SetWindowMinimumSize(Window,1200,880);
  Renderer=SDL.SDL_CreateRenderer(Window,-1,hidden?1u:6u);
  if(Renderer==IntPtr.Zero)Renderer=SDL.SDL_CreateRenderer(Window,-1,1);
  if(Renderer==IntPtr.Zero)throw new Exception(SDL.Error);
  SDL.SDL_SetRenderDrawBlendMode(Renderer,1);
  if(SDL.TTF_Init()!=0)throw new Exception("Yazı motoru açılamadı: "+SDL.Error);
  Resize();
 }
 public void Resize()=>SDL.SDL_GetWindowSize(Window,out W,out H);
 public void Color(Col c)=>SDL.SDL_SetRenderDrawColor(Renderer,c.R,c.G,c.B,c.A);
 public void Clear(Col c){Color(c);SDL.SDL_RenderClear(Renderer);}
 public void Box(float x,float y,float w,float h,Col c){Color(c);SDL.SDL_RenderFillRect(Renderer,new Rect((int)x,(int)y,(int)w,(int)h));}
 public void RoundBox(float x,float y,float w,float h,Col c,float radius=8)=>RoundBox(x,y,w,h,radius,c);
 public void RoundBox(float x,float y,float w,float h,float radius,Col c){
  float r=Math.Clamp(radius,0,Math.Min(w,h)*.5f);const int steps=7;int count=steps*4;
  EnsureVertices(count*3);
  for(int i=0;i<count;i++){
   var a=Corner(i);var b=Corner((i+1)%count);
   shapeVertices[i*3]=new(x+w*.5f,y+h*.5f,c);shapeVertices[i*3+1]=new(a.X,a.Y,c);shapeVertices[i*3+2]=new(b.X,b.Y,c);
  }
  SDL.SDL_RenderGeometry(Renderer,IntPtr.Zero,shapeVertices,count*3,null,0);
  Pt Corner(int i){int corner=i/steps;float angle=(corner+(float)(i%steps)/(steps-1))*MathF.PI*.5f;float cx=corner==0||corner==3?x+w-r:x+r,cy=corner<2?y+h-r:y+r;return new(cx+MathF.Cos(angle)*r,cy+MathF.Sin(angle)*r);}
 }
 void EnsureVertices(int count){if(shapeVertices.Length<count)Array.Resize(ref shapeVertices,Math.Max(count,shapeVertices.Length*2));}
 public void Line(float x,float y,float a,float b,Col c){Color(c);SDL.SDL_RenderDrawLineF(Renderer,x,y,a,b);}
 public void Disk(float x,float y,float r,Col c,int n=20){Ellipse(x,y,r,r,0,c,c,n);}
 public void Ellipse(float x,float y,float rx,float ry,float angle,Col center,Col rim,int n=24){
  EnsureVertices(n*3);float ca=MathF.Cos(angle),sa=MathF.Sin(angle);
  for(int i=0;i<n;i++){float a=i*MathF.Tau/n,b=(i+1)*MathF.Tau/n;float ax=MathF.Cos(a)*rx,ay=MathF.Sin(a)*ry,bx=MathF.Cos(b)*rx,by=MathF.Sin(b)*ry;
   shapeVertices[i*3]=new(x,y,center);shapeVertices[i*3+1]=new(x+ax*ca-ay*sa,y+ax*sa+ay*ca,rim);shapeVertices[i*3+2]=new(x+bx*ca-by*sa,y+bx*sa+by*ca,rim);
  } SDL.SDL_RenderGeometry(Renderer,IntPtr.Zero,shapeVertices,n*3,null,0);
 }
 public void Ring(float x,float y,float r,Col c){const int n=40;EnsureVertices(n*6);float inner=Math.Max(0,r-.6f),outer=r+.6f;for(int i=0;i<n;i++){float a=i*MathF.Tau/n,b=(i+1)*MathF.Tau/n;var ai=new Vertex(x+MathF.Cos(a)*inner,y+MathF.Sin(a)*inner,c);var ao=new Vertex(x+MathF.Cos(a)*outer,y+MathF.Sin(a)*outer,c);var bi=new Vertex(x+MathF.Cos(b)*inner,y+MathF.Sin(b)*inner,c);var bo=new Vertex(x+MathF.Cos(b)*outer,y+MathF.Sin(b)*outer,c);shapeVertices[i*6]=ai;shapeVertices[i*6+1]=ao;shapeVertices[i*6+2]=bo;shapeVertices[i*6+3]=ai;shapeVertices[i*6+4]=bo;shapeVertices[i*6+5]=bi;}SDL.SDL_RenderGeometry(Renderer,IntPtr.Zero,shapeVertices,n*6,null,0);}
 public void Text(string text,float x,float y,int size,Col c){
  if(string.IsNullOrEmpty(text))return;
  var key=(text,size);
  if(!labels.TryGetValue(key,out var t)){
   if(labels.Count>1024){foreach(var l in labels.Values)SDL.SDL_DestroyTexture(l.Item1);labels.Clear();}
   if(!fonts.TryGetValue(size,out IntPtr font)){string path=Path.Combine(AppContext.BaseDirectory,"assets","DejaVuSans.ttf");if(!File.Exists(path))path="/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";font=SDL.TTF_OpenFont(path,size);if(font==IntPtr.Zero)throw new Exception(SDL.Error);fonts[size]=font;}
   var surf=SDL.TTF_RenderUTF8_Blended(font,text,new Col(255,255,255));if(surf==IntPtr.Zero)return;
   var texture=SDL.SDL_CreateTextureFromSurface(Renderer,surf);SDL.SDL_FreeSurface(surf);SDL.SDL_QueryTexture(texture,out _,out _,out int tw,out int th);t=(texture,tw,th);labels[key]=t;
  }
  SDL.SDL_SetTextureColorMod(t.Item1,c.R,c.G,c.B);SDL.SDL_SetTextureAlphaMod(t.Item1,c.A);SDL.SDL_RenderCopy(Renderer,t.Item1,IntPtr.Zero,new Rect((int)x,(int)y,t.Item2,t.Item3));
 }
 public void Clip(Rect rect)=>SDL.SDL_RenderSetClipRect(Renderer,rect);
 public void Unclip()=>SDL.ClearClip(Renderer,IntPtr.Zero);
 public void Screenshot(string path){
  byte[] pixels=new byte[W*H*4];if(SDL.SDL_RenderReadPixels(Renderer,IntPtr.Zero,372645892,pixels,W*4)!=0)throw new Exception(SDL.Error);
  using var bw=new BinaryWriter(File.Create(path));bw.Write((ushort)0x4D42);bw.Write(54+pixels.Length);bw.Write(0);bw.Write(54);bw.Write(40);bw.Write(W);bw.Write(-H);bw.Write((ushort)1);bw.Write((ushort)32);bw.Write(0);bw.Write(pixels.Length);bw.Write(2835);bw.Write(2835);bw.Write(0);bw.Write(0);bw.Write(pixels);
 }
 public void Dispose(){foreach(var t in labels.Values)SDL.SDL_DestroyTexture(t.Item1);foreach(var f in fonts.Values)SDL.TTF_CloseFont(f);SDL.TTF_Quit();SDL.SDL_DestroyRenderer(Renderer);SDL.SDL_DestroyWindow(Window);SDL.SDL_Quit();}
}
