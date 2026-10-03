using System;
using System.IO;
using EvoSim.Core;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EvoSimValidation
{
    static int frames;
    static double started;
    static EvoSimValidation(){EditorApplication.update+=Poll;}
    public static void Run(){
        try{
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            EvoSimSceneBuilder.Build();
            var reference=new World(42,EvoSimBridge.DemoConfig());for(int i=0;i<400;i++)reference.Step();
            foreach(int fps in new[]{30,60,144})foreach(int speed in new[]{1,20}){
                var world=new World(42,EvoSimBridge.DemoConfig());var clock=new SimulationClock();
                double duration=400*(double)World.Dt/speed,elapsed=0;
                while(elapsed<duration){double dt=Math.Min(1.0/fps,duration-elapsed);clock.Advance(world,dt,speed,false,160,double.PositiveInfinity);elapsed+=dt;}
                // Flush work left by a limited frame, without adding elapsed time.
                clock.Advance(world,0,speed,false,10000,double.PositiveInfinity);
                Require(world.StateHash()==reference.StateHash(),"Frame-rate / speed state mismatch: "+fps+" / "+speed);
                string before=world.StateHash();clock.Advance(world,.25,speed,true);Require(world.StateHash()==before,"Pause changed Core");
            }
            var limited=new World(42,EvoSimBridge.DemoConfig());var limiter=new SimulationClock();limiter.Advance(limited,.25,20,false,1,double.PositiveInfinity);
            Require(limiter.PendingSeconds>4.9,"Frame cap dropped time");limiter.Advance(limited,0,20,false,1000,double.PositiveInfinity);
            var hundred=new World(42,EvoSimBridge.DemoConfig());for(int i=0;i<99;i++)hundred.Step();Require(limited.StateHash()==hundred.StateHash(),"Budget drain changed ticks");
            // The float Core.Dt is slightly greater than 0.05; five exact seconds contain 99 ticks.
            int shores=0;var map=reference.Map;
            for(int y=1;y<map.Size;y++)for(int x=1;x<map.Size;x++)if(map.Tiles[x+y*map.Size]!=1&&(map.Tiles[x-1+y*map.Size]==1||map.Tiles[x+(y-1)*map.Size]==1)){
                Require(CoreTerrainView.HeightAt(map,x+.5f,y+.5f)>CoreTerrainView.WaterHeight,"Land center lost to shore smoothing");shores++;
            }
            Require(shores>0,"No shore tested");
            Directory.CreateDirectory("../validation");File.WriteAllText("../validation/clock-and-shore.txt","PASS: 30/60/144 FPS at 1x/20x; 400-tick independent Core hash equality; pause; capped-budget backlog; "+shores+" shore vertices.\n");
            SessionState.SetBool("EVOSIM_VALIDATE",true);EditorApplication.isPlaying=true;
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    public static void Geometry(){
        var world=new World(42,EvoSimBridge.DemoConfig());string before=world.StateHash();var root=new GameObject("Geometry test");root.AddComponent<CoreTerrainView>().Build(world);
        float maxError=0;int count=0;
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
            var mesh=filter.sharedMesh;var vertices=mesh.vertices;var indices=mesh.GetTriangles(0);
            for(int i=0;i<indices.Length;i+=3){var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];if(Vector3.Cross(b-a,c-a).sqrMagnitude<.00000001f)continue;var p=(a+b+c)/3;float error=Math.Abs(p.y-CoreTerrainView.HeightAt(world.Map,p.x,p.z));maxError=Math.Max(maxError,error);Require(error<.001f,"Surface interpolation mismatch: "+error);count++;}
        }
        for(int y=0;y<world.Map.Size;y++)for(int x=0;x<world.Map.Size;x++)Require((CoreTerrainView.ShoreField(world.Map,x+.5f,y+.5f)<0)==(world.Map.Tiles[x+y*world.Map.Size]==1),"Core cell center changed classification");
        Require(world.StateHash()==before,"Geometry modified Core");
        File.WriteAllText("../validation/soft-shore-geometry.json","{\"passed\":true,\"trianglesChecked\":"+count+",\"maxHeightError\":"+maxError.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"coreUnchanged\":true,\"allCellCentersPreserved\":true}");UnityEngine.Object.DestroyImmediate(root);
    }
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Poll(){
        if(!SessionState.GetBool("EVOSIM_VALIDATE",false)||!EditorApplication.isPlaying)return;
        EditorApplication.isPaused=false;Application.runInBackground=true;
        if(started==0)started=EditorApplication.timeSinceStartup;
        try{
            if(EditorApplication.timeSinceStartup-started>90)throw new Exception("Play validation timeout");
            var bridge=UnityEngine.Object.FindFirstObjectByType<EvoSimBridge>();if(bridge==null||bridge.World==null)return;
            if(++frames<120||bridge.World.Time<=2)return;
            bridge.paused=true;bridge.SyncViews();Require(bridge.ViewCount==bridge.World.Living,"Live view count mismatch");
            foreach(var c in bridge.World.Creatures){if(!c.Alive)continue;var go=bridge.transform.Find("Individual_"+c.Id);Require(go!=null,"Missing creature");Require(Math.Abs(go.localPosition.y-CoreTerrainView.HeightAt(bridge.World.Map,c.X,c.Y))<.0001f,"Floating creature");}
            Debug.Log("LIGHTS "+UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length+" RENDERERS "+UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length);
            foreach(var rr in bridge.GetComponentsInChildren<Renderer>())if(rr.name=="Individual_1"||rr.name=="Chunk 0 0")Debug.Log("BOUNDS "+rr.bounds+" MATERIAL "+rr.sharedMaterial.color);
            var cam=Camera.main;var rt=new RenderTexture(1600,1000,24);var request=new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt};UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,request);RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();Directory.CreateDirectory("../previews");File.WriteAllBytes("../previews/unity-live.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            File.WriteAllText("../validation/unity-play.json","{\"passed\":true,\"worldTime\":"+bridge.World.Time.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"liveViews\":"+bridge.ViewCount+",\"stateHash\":\""+bridge.StateHash()+"\"}");
            SessionState.SetBool("EVOSIM_VALIDATE",false);EditorApplication.Exit(0);
        }catch(Exception e){SessionState.SetBool("EVOSIM_VALIDATE",false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
