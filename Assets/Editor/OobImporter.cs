
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

// https://stackoverflow.com/questions/71563182/unity-access-hlsl-file-like-a-textasset

[ScriptedImporter(1, "oob")]
public class OobImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
        // Debug.Log($"encoding={encoding}");
        var ta = new TextAsset(File.ReadAllText(ctx.assetPath, encoding));
        ctx.AddObjectToAsset("main obj", ta);
        ctx.SetMainObject(ta);
    }
}
