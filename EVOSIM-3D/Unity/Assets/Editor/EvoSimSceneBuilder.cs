using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EvoSimSceneBuilder
{
    [MenuItem("EVOSIM/Build Core validation scene")]
    public static void Build(){
        Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Rendering");
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Rendering/Renderer.asset");
        if(renderer==null){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Rendering/Renderer.asset");}
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Rendering/URP.asset");
        if(pipeline==null){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Rendering/URP.asset");}
        pipeline.shadowDistance=250;pipeline.useSRPBatcher=true;
        GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
        PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        PlayerSettings.companyName="EVOSIM";PlayerSettings.productName="EVOSIM 3D Preview";
        var root=new GameObject("EVOSIM Core bridge");var bridge=root.AddComponent<EvoSimBridge>();
        bridge.herbivorePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Herbivore_A.fbx");bridge.carnivorePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Carnivore_A.fbx");bridge.treePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Tree_Oak_A.fbx");
        bridge.rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Rock_A.fbx");
        if(bridge.herbivorePrefab==null||bridge.carnivorePrefab==null||bridge.treePrefab==null)throw new InvalidOperationException("Blender FBX assets must be imported first.");
        var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.8f;sun.color=Color.white;RenderSettings.sun=sun;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-35,0);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.5f,.58f,.62f);
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=23;camera.transform.position=new Vector3(108,65,-8);camera.transform.LookAt(new Vector3(55,0,55));camera.farClipPlane=600;camera.backgroundColor=new Color(.15f,.23f,.25f);camera.clearFlags=CameraClearFlags.SolidColor;camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.gameObject.AddComponent<ObserverControls>();camera.gameObject.AddComponent<LiveCapture>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/CoreValidation.unity");AssetDatabase.SaveAssets();
        File.WriteAllText("../unity-scene-build-result.json","{\"sceneCreated\":true,\"coreRuntimeTested\":false}");
    }
    public static void BuildPlayer(){
        Build();EvoSimValidation.Geometry();
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/CoreValidation.unity"},locationPathName="../Build/EVOSIM-3D.x86_64",target=BuildTarget.StandaloneLinux64,options=BuildOptions.None});
        if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Player build failed: "+result.summary.result);
    }
}
