using EvoSim.Observer;
internal sealed partial class ObserverApp {
 readonly Dictionary<string,(string Data,IntPtr Texture)> previews=new();
 void DrawWorldPreview(WorldSlot slot,Rect area){
  if(string.IsNullOrEmpty(slot.Preview)){p.RoundBox(area.X,area.Y,area.W,area.H,7,line);p.Text("◌",area.X+12,area.Y+9,21,muted);return;}
  if(!previews.TryGetValue(slot.Id,out var cached)||cached.Data!=slot.Preview){
   byte[] cells;try{cells=Convert.FromBase64String(slot.Preview);}catch(FormatException){return;}if(cells.Length!=4096)return;
   if(cached.Texture!=IntPtr.Zero){SDL.SDL_DestroyTexture(cached.Texture);previews.Remove(slot.Id);}if(previews.Count>64)DisposePreviews();
   Col[] colors={Col.Hex(0x91AA75),Col.Hex(0x6BADB3),Col.Hex(0xB4B49D),Col.Hex(0xC6CD83),Col.Hex(0x548063),Col.Hex(0x82C6E1),Col.Hex(0xD67560)};var pixels=new byte[4096*4];
   for(int i=0;i<cells.Length;i++){var color=colors[Math.Min(cells[i],(byte)6)];pixels[i*4]=color.B;pixels[i*4+1]=color.G;pixels[i*4+2]=color.R;pixels[i*4+3]=255;}
   var texture=SDL.SDL_CreateTexture(p.Renderer,372645892,0,64,64);if(texture==IntPtr.Zero)return;SDL.SDL_UpdateTexture(texture,IntPtr.Zero,pixels,256);SDL.SDL_SetTextureScaleMode(texture,1);
   cached=(slot.Preview,texture);previews[slot.Id]=cached;
  }
  SDL.SDL_RenderCopy(p.Renderer,cached.Texture,IntPtr.Zero,area);
 }
 void DisposePreviews(){foreach(var entry in previews.Values)SDL.SDL_DestroyTexture(entry.Texture);previews.Clear();}
}
