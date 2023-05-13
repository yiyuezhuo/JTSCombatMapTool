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
    public UnitDirection Direction { get => UnitState.Direction; }
    MapUnitDirection IMapUnit.Direction { get => (MapUnitDirection)(int)UnitState.Direction; }
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
            return $"{division.Name}\n{brigade.Name}";
        }
    }

    public string Summary()
    {
        var lines = new List<string>() { ReportMenAndGuns() };
        var oobItem = MapUnits[0].UnitState.OobItem;
        while(oobItem.Parent != null)
        {
            oobItem = oobItem.Parent;
            lines.Add(oobItem.Name);
        }
        return string.Join("\n", lines);
    }

    public bool IsCavalryGroup()
    {
        foreach(var unit in MapUnits)
        {
            var unitOob = unit.UnitState.OobItem as UnitOob;
            if (unitOob != null && unitOob.Category == UnitCategory.Infantry) // Accepts Cavalry Artillery Complex
                return false;
        }
        return !IsArtilleryGroup();
    }

    public bool IsArtilleryGroup()
    {
        foreach (var unit in MapUnits)
        {
            var unitOob = unit.UnitState.OobItem as UnitOob;
            if (unitOob != null && unitOob.Category != UnitCategory.Artillery) // Accepts Cavalry Artillery Complex
                return false;
        }
        return true;
    }

    public string ReportMenAndGuns()
    {
        var men = 0;
        var guns = 0;
        foreach(var unit in MapUnits)
        {
            var unitOob = unit.UnitState.OobItem as UnitOob;
            if(unitOob != null)
            {
                if(unitOob.Category == UnitCategory.Artillery)
                {
                    guns += unit.Strength;
                }
                else
                {
                    men += unit.Strength;
                }
            }
        }
        var menWord = men == 1 ? "man" : "men";
        var gunsWord = guns == 1 ? "gun" : "guns";
        if(guns == 0)
        {
            return men.ToString();
        }
        else if(men == 0)
        {
            return $"{guns} {gunsWord}";
        }
        return $"{men} {menWord}, {guns} {gunsWord}";
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

    public int DistanceThreshold = 3;

    bool showIndependentArtillery = false;

    public string currentScenarioName = "011.Coruna4_BrAI";

    static string RemoveExtension(string p)
    {
        var sl = p.Split(".");
        return string.Join(".", sl.Take(sl.Length - 1));
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup(currentScenarioName);
    }

    static float MapUnitDistance(MapUnitAdaptor a, MapUnitAdaptor b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Mathf.Sqrt(dx * dx + dy * dy);
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

        foreach (var KV in unitStatus.GroupByBrigade())
        {
            var brigade = KV.Key;
            var unitStates = KV.Value;
            var _mapUnits = unitStates.Select(unitState => new MapUnitAdaptor(unitState)).ToList();

            var prefab = brigade.Country == "French" ? FrenchUnitPrefab : AlliedUnitPrefab;
            var mapUnitsList = YYZ.Stats.Clustering.Cluster(_mapUnits, MapUnitDistance, DistanceThreshold);
            // Debug.Log($"mapUnitsList.Count={mapUnitsList.Count}");

            foreach (var mapUnits in mapUnitsList)
            {
                CreateGameUnit(mapUnits, prefab);
            }
        }
    }

    void CreateGameUnit(List<MapUnitAdaptor> mapUnits, GameObject prefab)
    {
        var mapGroup = new MapGroupAdaptor() { MapUnits = mapUnits };

        if (!showIndependentArtillery && mapGroup.IsArtilleryGroup())
            return;

        // Debug.Log(mapGroup.Name2);
        var rect = (mapGroup as IMapGroup<MapUnitAdaptor>).GetRectTransform();

        var pos = new Vector3((float)rect.X, (float)rect.Y, 0);
        var deg = rect.Rotation / (2 * System.Math.PI) * 360;
        var gameObject = Instantiate(prefab, pos, Quaternion.identity, UnitContainer.transform);
        var gameUnit = gameObject.GetComponent<GameUnit>();

        var scaleX = Mathf.Max(1, (float)rect.WidthMain * 3);
        var scaleY = 1;
        // var scaleY = (float)rect.WidthSub;

        gameUnit.SetSize(scaleX, scaleY);
        gameUnit.SetRotation((float)deg);
        gameUnit.SetText("");

        var dr = gameUnit.DirectionRect;
        if (rect.UnitDirection == null)
        {
            dr.SetActive(false);
        }
        else
        {
            dr.SetActive(true);
            var rot = System.Math.Atan2(rect.UnitDirection[1], rect.UnitDirection[0]) / (2 * System.Math.PI) * 360;
            dr.transform.rotation = Quaternion.Euler(0, 0, (float)rot);
            var d = new Vector2((float)rect.UnitDirection[0], (float)rect.UnitDirection[1]);
            var coef = rect.UnitDirectionModeIndex == 0 || rect.UnitDirectionModeIndex == 2 ? scaleX : scaleY;
            dr.transform.localPosition = d * (coef / 2 + dr.transform.localScale.x / 2);
        }

        if(mapGroup.IsCavalryGroup())
        {
            gameUnit.SetUnitCategory(1);
        }

        viewMap[mapGroup] = gameUnit;
        modelMap[gameUnit] = mapGroup;

        // Debug.Log($"rect.UnitDirection={rect.UnitDirection}");

        // Debug
        gameUnit.DebugName = mapGroup.Name2;
        gameUnit.DebugRotation = (float)rect.Rotation;
        gameUnit.DebugWidthMain = (float)rect.WidthMain;
        gameUnit.DebugWidthSub = (float)rect.WidthSub;
        gameUnit.DebugX = (float)rect.X;
        gameUnit.DebugY = (float)rect.Y;
        gameUnit.DebugUnitDirection = rect.UnitDirection?.Select(x => (float)x).ToArray();
        gameUnit.DebugUnitDirectionModeIndex = rect.UnitDirectionModeIndex;
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

    public void OnToggleShowIndependentArtillery(bool showIndependentArtillery)
    {
        this.showIndependentArtillery = showIndependentArtillery;
        ReloadScenario(currentScenarioName);
    }

    public void ReloadScenario(string name)
    {
        currentScenarioName = name;
        Debug.Log($"ReloadScenario name={name}");
        Reset();
        Setup(name);
    }

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
                    KV.Value.SetText(KV.Key.ReportMenAndGuns());
                break;
            case 2: // Direct (Brigade in most cases) Name
                foreach (var KV in viewMap)
                    KV.Value.SetText(KV.Key.Name);
                break;
            case 3: // 2 level Namee
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
