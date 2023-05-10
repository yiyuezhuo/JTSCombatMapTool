using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using YYZ.JTS.NB;

public class MapUnitAdaptor: IMapUnit
{
    UnitState unitState;
    public MapUnitAdaptor(UnitState unitState)
    {
        this.unitState = unitState;
    }

    public int Strength { get => unitState.CurrentStrength; }
    public string Name { get => unitState.OobItem.Name; }
    public float X { get => unitState.X; }
    public float Y { get => unitState.Y + offset; }
    float offset { get => X % 2 == 0 ? 0f : -0.5f; }
}

public class MapGroupAdaptor<T>: IMapGroup<T> where T : IMapUnit
{
    public List<T> MapUnits { get; set; }
}

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
            // Debug.Log(state);
            // Debug.Log(state.OobItem.Country);

            /*
            var pos = new Vector3(state.X, state.Y, 0);
            if (state.OobItem.Country == "French")
            {
                Instantiate(FrenchUnitPrefab, pos, Quaternion.identity);
            }
            else
            {
                Instantiate(AlliedUnitPrefab, pos, Quaternion.identity);
            }
            */
            foreach(var KV in unitStatus.GroupByBrigade())
            {
                var brigade = KV.Key;
                var unitStates = KV.Value;

                var prefab = brigade.Country == "French" ? FrenchUnitPrefab : AlliedUnitPrefab;

                /*
                foreach(var unitState in unitStates)
                {
                    var offset = unitState.X % 2 == 0 ? 0f : -0.5f;
                    var pos = new Vector3(unitState.X, unitState.Y + offset, 0);
                    var gameObject = Instantiate(prefab, pos, Quaternion.identity);
                    // gameObject.transform.localScale = new Vector3();
                }
                */
                IMapGroup<MapUnitAdaptor> mapGroup = new MapGroupAdaptor<MapUnitAdaptor>() { MapUnits = unitStates.Select(unitState => new MapUnitAdaptor(unitState)).ToList()};
                var rect = mapGroup.GetRectTransform();

                var pos = new Vector3((float)rect.X, (float)rect.Y, 0);
                var deg = rect.Rotation / (2 * System.Math.PI) * 360;
                var gameObject = Instantiate(prefab, pos, Quaternion.Euler(0, 0, (float)deg));
                gameObject.transform.localScale = new Vector3((float)rect.WidthMain * 2, 1f, 1f);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
