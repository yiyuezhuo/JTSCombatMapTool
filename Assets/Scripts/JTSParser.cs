using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using YYZ.JTS.NB;

public class JTSParser : MonoBehaviour
{
    public TextAsset OobText;

    // Start is called before the first frame update
    void Start()
    {
        // const string path = "JTSData/peninsula/OOBs/Coruna Campaign";
        // const string path = "JTSData/peninsula/OOBs/Baza";
        // const string path = "Coruna";
        const string path = "Coruna_test";

        var raw = Resources.Load(path);

        // Debug.Log($"{raw}, {raw == null}");

        var textAsset = Resources.Load<TextAsset>(path);

        // Debug.Log($"textAsset={textAsset}");

        var unitGroup = JTSOobParser.ParseUnits(textAsset.text);
        foreach(var unit in unitGroup.Walk())
        {
            var group = unit as UnitGroup;
            if (group != null)
                Debug.Log(group);
            // Debug.Log(unit);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
