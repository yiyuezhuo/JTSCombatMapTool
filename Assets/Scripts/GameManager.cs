using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

using YYZ.JTS.NB;

// Models, while implements some interfaces at the same time.

public class MapUnitAdaptor: IMapUnit
{
    public UnitState UnitState;
    public MapUnitAdaptor(UnitState unitState)
    {
        this.UnitState = unitState;
    }

    public int Strength { get => UnitState.CurrentStrength; }
    public string Name { get => UnitState.OobItem.Name; }
    public float X { get => UnitState.X; }
    public float Y { get => UnitState.Y + offset; }
    float offset { get => X % 2 == 0 ? 0f : -0.5f; }
}

public class MapGroupAdaptor: IMapGroup<MapUnitAdaptor>
{
    public List<MapUnitAdaptor> MapUnits { get; set; }

    public float Strength { get => MapUnits.Sum(unit => unit.Strength); }
    public string Name { get => MapUnits[0].UnitState.OobItem.Parent.Name; }
    public string Name2
    {
        get
        {
            var brigade = MapUnits[0].UnitState.OobItem.Parent;
            var division = brigade.Parent;
            return $"{division.Name}/{brigade.Name}";
        }
    }

    public string Summary()
    {
        var lines = new List<string>() { Strength.ToString() };
        var oobItem = MapUnits[0].UnitState.OobItem;
        while(oobItem.Parent != null)
        {
            lines.Add(oobItem.Name);
            oobItem = oobItem.Parent;
        }
        return string.Join("\n", lines);
    }
}

public interface IUnitSelectionPublisher
{
    event EventHandler<MapGroupAdaptor> UnitDeselected;
    event EventHandler<MapGroupAdaptor> UnitSelected;
}

public static class DataLoader
{
    public static string ScenarioPath = "JTSData/peninsula/Scenarios";
    public static string OobPath = "JTSData/peninsula/OOBs";

    public static string LoadScenario(string name)
    {
        Debug.Log($"LoadScenario: name={name}");
        var textAsset = Resources.Load<TextAsset>(ScenarioPath + "/" + name);
        var text = textAsset.text;
        return text;
    }

    public static string LoadOob(string name)
    {
        Debug.Log($"LoadOob: name={name}");
        var textAsset = Resources.Load<TextAsset>(OobPath + "/" + name);
        var text = textAsset.text;
        return text;
    }

}

// "Controller"

public class GameManager : MonoBehaviour, IUnitSelectionPublisher
{
    // public TextAsset OobText;

    public GameObject FrenchUnitPrefab;
    public GameObject AlliedUnitPrefab;

    public GameObject UnitContainer;

    UnitGroup unitGroup;
    JTSUnitStates unitStatus;
    Dictionary<MapGroupAdaptor, GameUnit> viewMap = new();
    Dictionary<GameUnit, MapGroupAdaptor> modelMap = new();

    GameUnit selectingGameUnit;

    public event EventHandler<MapGroupAdaptor> UnitDeselected;
    public event EventHandler<MapGroupAdaptor> UnitSelected;

    static string LoadText(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        return textAsset.text;
    }

    static string RemoveExtension(string p)
    {
        var sl = p.Split(".");
        return string.Join(".", sl.Take(sl.Length - 1));
    }

    void Start()
    {
        Setup("011.Coruna4_BrAI");
    }

    // Start is called before the first frame update
    void _Start()
    {
        // Debug.Log("French A II Corps d'Armée"); // Encoding & Font Test

        // const string path = "JTSData/peninsula/OOBs/Coruna Campaign";
        // const string path = "JTSData/peninsula/OOBs/Baza";
        // const string oobPath = "Coruna_oob";
        // const string scnPath = "Coruna_scn";

        // var raw = Resources.Load(oobPath);

        // Debug.Log($"{raw}, {raw == null}");

        // var oobTextAsset = Resources.Load<TextAsset>(oobPath);

        // Debug.Log($"textAsset={textAsset}");

        var scnText = DataLoader.LoadScenario("011.Coruna4_BrAI");

        var scenario = new JTSScenario();
        scenario.Extract(scnText);

        // Debug.Log($"scenario.OobFile={scenario.OobFile}");

        var oobName = RemoveExtension(scenario.OobFile);
        var oobText = DataLoader.LoadOob(oobName);

        unitGroup = JTSOobParser.ParseUnits(oobText);

        /*
        const string oobPath = "JTSData/peninsula/OOBs/Coruna";
        const string scnPath = "JTSData/peninsula/Scenarios/011.Coruna4_BrAI";

        unitGroup = JTSOobParser.ParseUnits(LoadText(oobPath));
        */

        foreach (var unit in unitGroup.Walk())
        {
            var group = unit as UnitGroup;
            if (group != null)
                Debug.Log(group);
            // Debug.Log(unit);
        }
        

        unitStatus = new JTSUnitStates();
        // unitStatus.Extract(unitGroup, LoadText(scnPath));
        unitStatus.ExtractByLines(unitGroup, scenario.DynamicCommandBlock);

        foreach (var state in unitStatus.UnitStates)
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
                var mapGroup = new MapGroupAdaptor() { MapUnits = unitStates.Select(unitState => new MapUnitAdaptor(unitState)).ToList()};
                var rect = (mapGroup as IMapGroup<MapUnitAdaptor>).GetRectTransform();

                var pos = new Vector3((float)rect.X, (float)rect.Y, 0);
                var deg = rect.Rotation / (2 * System.Math.PI) * 360;
                //var gameObject = Instantiate(prefab, pos, Quaternion.Euler(0, 0, (float)deg));
                // gameObject.transform.localScale = new Vector3((float)rect.WidthMain * 2, 1f, 1f);
                var gameObject = Instantiate(prefab, pos, Quaternion.identity, UnitContainer.transform);
                var gameUnit = gameObject.GetComponent<GameUnit>();
                gameUnit.SetSize((float)rect.WidthMain * 2, (float)rect.WidthSub);
                gameUnit.SetRotation((float)deg);
                gameUnit.SetText("");
                // gameUnit.SetText(mapGroup.Strength.ToString());

                viewMap[mapGroup] = gameUnit;
                modelMap[gameUnit] = mapGroup;
            }
        }
        
    }

    public void Setup(string scnName)
    {
        var scnText = DataLoader.LoadScenario(scnName);

        var scenario = new JTSScenario();
        scenario.Extract(scnText);

        var oobName = RemoveExtension(scenario.OobFile);
        var oobText = DataLoader.LoadOob(oobName);

        unitGroup = JTSOobParser.ParseUnits(oobText);

        /*
        foreach (var unit in unitGroup.Walk())
        {
            var group = unit as UnitGroup;
            if (group != null)
                Debug.Log(group);
            // Debug.Log(unit);
        }
        */

        unitStatus = new JTSUnitStates();
        unitStatus.ExtractByLines(unitGroup, scenario.DynamicCommandBlock);

        foreach (var state in unitStatus.UnitStates)
        {
            foreach (var KV in unitStatus.GroupByBrigade())
            {
                var brigade = KV.Key;
                var unitStates = KV.Value;

                var prefab = brigade.Country == "French" ? FrenchUnitPrefab : AlliedUnitPrefab;

                var mapGroup = new MapGroupAdaptor() { MapUnits = unitStates.Select(unitState => new MapUnitAdaptor(unitState)).ToList() };
                var rect = (mapGroup as IMapGroup<MapUnitAdaptor>).GetRectTransform();

                var pos = new Vector3((float)rect.X, (float)rect.Y, 0);
                var deg = rect.Rotation / (2 * System.Math.PI) * 360;
                var gameObject = Instantiate(prefab, pos, Quaternion.identity, UnitContainer.transform);
                var gameUnit = gameObject.GetComponent<GameUnit>();
                gameUnit.SetSize((float)rect.WidthMain * 2, (float)rect.WidthSub);
                gameUnit.SetRotation((float)deg);
                gameUnit.SetText("");

                viewMap[mapGroup] = gameUnit;
                modelMap[gameUnit] = mapGroup;
            }
        }
    }

    public void Reset()
    {
        foreach(var gameUnit in viewMap.Values)
        {
            Destroy(gameUnit.gameObject);
        }
        viewMap.Clear();
        modelMap.Clear();

        unitStatus = null;
        unitGroup = null;
    }

    public void ReloadScenario(string name)
    {
        Debug.Log($"ReloadScenario name={name}");
        Reset();
        Setup(name);
    }

    /*
    IEnumerable<GameUnit> IterateGameUnits()
    {
        foreach(Transform transform in UnitContainer.transform)
        {
            yield return transform.GetComponent<GameUnit>();
        }
    }
    */

    public void OnUnitTextModeChanged(int idx)
    {
        Debug.Log($"Text Mode Changed to {idx}");
        switch(idx)
        {
            case 0: // None
                foreach (var KV in viewMap)
                    KV.Value.SetText("");
                break;
            case 1: // Strength
                foreach (var KV in viewMap)
                    KV.Value.SetText(KV.Key.Strength.ToString());
                break;
            case 2: // Name
                foreach (var KV in viewMap)
                    KV.Value.SetText(KV.Key.Name2);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if(hit.collider != null)
        {
            // Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
            var gameUnit = hit.collider.transform.parent.GetComponent<GameUnit>();
            if(gameUnit != selectingGameUnit)
            {
                // Debug.Log($"> selectingGameUnit={selectingGameUnit}, gameUnit={gameUnit}");
                if (selectingGameUnit != null)
                {
                    selectingGameUnit.OnDeselected();
                    UnitDeselected?.Invoke(this, modelMap[selectingGameUnit]);
                }
                selectingGameUnit = gameUnit;
                if (selectingGameUnit != null)
                {
                    selectingGameUnit.OnSelected();
                    UnitSelected?.Invoke(this, modelMap[selectingGameUnit]);
                }
            }
        }
        else
        {
            if (selectingGameUnit != null)
            {
                // Debug.Log($"< selectingGameUnit={selectingGameUnit}");
                selectingGameUnit.OnDeselected();
                UnitDeselected?.Invoke(this, modelMap[selectingGameUnit]);
                selectingGameUnit = null;
            }
        }
    }
}
