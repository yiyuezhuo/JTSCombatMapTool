using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using TMPro;

[CustomEditor(typeof(ScenarioDropdown))]
public class ScenarioDropdownEditor : Editor
{
    // static string scenarioPath = "/Resources/JTSData/peninsula/Scenarios";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Build From Resources"))
        {
            Debug.Log("Build From Resource");

            var resourcesPath = Application.dataPath + "/Resources/" + DataLoader.ScenarioPath;
            var pList = Directory.GetFiles(resourcesPath, "*.scn");
            System.Array.Sort(pList);

            // Debug.Log($"pList[0]={pList[0]}, Path.GetFileName(pList[0])={Path.GetFileName(pList[0])}, pList.Count={pList.Length}");

            var _dropdown = (ScenarioDropdown)target;
            var dropdown = _dropdown.gameObject.GetComponent<TMP_Dropdown>();

            var options = pList.Select(p => new TMP_Dropdown.OptionData(Path.GetFileNameWithoutExtension(p))).ToList();
            dropdown.options = options;

            EditorUtility.SetDirty(dropdown);
        }
    }
}
