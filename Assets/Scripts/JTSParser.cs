using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using YYZ.JTS.NB;

public class JTSParser : MonoBehaviour
{
    // public TextAsset OobText;

    public GameObject FrenchUnitPrefab;
    public GameObject AlliedUnitPrefab;

    static string LoadText(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        return textAsset.text;
    }

    // Start is called before the first frame update
    void Start()
    {
        // const string path = "JTSData/peninsula/OOBs/Coruna Campaign";
        // const string path = "JTSData/peninsula/OOBs/Baza";
        const string oobPath = "JTSData/peninsula/OOBs/Coruna";
        const string scnPath = "JTSData/peninsula/Scenarios/011.Coruna4_BrAI";
        // const string oobPath = "Coruna_oob";
        // const string scnPath = "Coruna_scn";

        // var raw = Resources.Load(oobPath);

        // Debug.Log($"{raw}, {raw == null}");

        // var oobTextAsset = Resources.Load<TextAsset>(oobPath);

        // Debug.Log($"textAsset={textAsset}");

        var unitGroup = JTSOobParser.ParseUnits(LoadText(oobPath));
        foreach(var unit in unitGroup.Walk())
        {
            var group = unit as UnitGroup;
            if (group != null)
                Debug.Log(group);
            // Debug.Log(unit);
        }

        var unitStatus = new JTSUnitStatus();
        unitStatus.Extract(unitGroup, LoadText(scnPath));
        
        foreach(var state in unitStatus.UnitStates)
        {
            Debug.Log(state);
            Debug.Log(state.OobItem.Country);

            var pos = new Vector3(state.X, state.Y, 0);
            if (state.OobItem.Country == "French")
            {
                Instantiate(FrenchUnitPrefab, pos, Quaternion.identity);
            }
            else
            {
                Instantiate(AlliedUnitPrefab, pos, Quaternion.identity);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
