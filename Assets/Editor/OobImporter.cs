
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

// https://stackoverflow.com/questions/71563182/unity-access-hlsl-file-like-a-textasset

[ScriptedImporter(1, "oob")]
public class OobImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var ta = new TextAsset(File.ReadAllText(ctx.assetPath));
        ctx.AddObjectToAsset("main obj", ta);
        ctx.SetMainObject(ta);
    }
}
