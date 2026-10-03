using System;
using System.Collections;
using System.IO;
using EvoSim.Core;
using UnityEngine;

// Opt-in validation in a fresh preview world; never opens production saves.
public sealed class LiveCapture:MonoBehaviour
{
    IEnumerator Start(){
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--capture");if(i<0||i+1>=args.Length)yield break;
        yield return new WaitForSecondsRealtime(5);
        var bridge=FindFirstObjectByType<EvoSimBridge>();bridge.paused=true;
        yield return new WaitForEndOfFrame();
        try{
            var reference=new World(bridge.seed,EvoSimBridge.DemoConfig());for(long tick=0;tick<bridge.World.Tick;tick++)reference.Step();
            Check(reference.StateHash()==bridge.StateHash(),"Standalone Core hash mismatch");
            Check(bridge.ViewCount==bridge.World.Living,"View count mismatch");
            foreach(var c in bridge.World.Creatures){if(!c.Alive)continue;var root=bridge.transform.Find("Individual_"+c.Id);Check(root!=null,"Missing individual");Check(Math.Abs(root.localPosition.y-CoreTerrainView.HeightAt(bridge.World.Map,c.X,c.Y))<.001f,"Floating individual");var bounds=root.GetComponentInChildren<Renderer>().bounds;Check(bounds.size.magnitude>.15f&&bounds.size.magnitude<4,"Wrong imported model scale");}
            var resources=bridge.GetComponent<CoreResourcesView>();Check(resources.FruitViews>0,"Missing fruit views");
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();int green=0;foreach(var pixel in image.GetPixels32())if(pixel.g>70&&pixel.g>pixel.r*1.1f&&pixel.g>pixel.b*1.1f)green++;
            Check(green>Screen.width*Screen.height/20,"Scene rendering is dark or missing");
            File.WriteAllBytes(args[i+1],image.EncodeToPNG());Destroy(image);
            File.WriteAllText(args[i+1]+".json","{\"passed\":true,\"coreHashEqual\":true,\"tick\":"+bridge.World.Tick+",\"liveViews\":"+bridge.ViewCount+",\"fruitViews\":"+resources.FruitViews+",\"modelScaleAndGroundHeight\":true,\"renderer\":\""+SystemInfo.graphicsDeviceName+"\"}");Application.Quit(0);
        }catch(Exception e){Debug.LogException(e);File.WriteAllText(args[i+1]+".error.txt",e.ToString());Application.Quit(1);}
    }
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
}
