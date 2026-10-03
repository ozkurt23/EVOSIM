using UnityEditor;
using UnityEngine;
public sealed class ArtImport : AssetPostprocessor
{
    public override uint GetVersion()=>2;
    void OnPreprocessModel(){if(!assetPath.StartsWith("Assets/Art/"))return;var importer=(ModelImporter)assetImporter;importer.globalScale=1;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;}
    Material OnAssignMaterialModel(Material source,Renderer renderer){
        if(!assetPath.StartsWith("Assets/Art/"))return null;
        var shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)return null;
        string folder="Assets/Art/Materials";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Materials");
        string path=folder+"/"+source.name.Replace('/','_')+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material!=null)return material;
        material=new Material(shader){name=source.name,enableInstancing=true};material.SetColor("_BaseColor",source.color);material.SetFloat("_Smoothness",.08f);AssetDatabase.CreateAsset(material,path);return material;
    }
}
