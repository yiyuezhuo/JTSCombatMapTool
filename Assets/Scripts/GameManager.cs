using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using Newtonsoft.Json;

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

    public float[] GetCenter()
    {
        var X = 0f;
        var Y = 0f;
        var S = 0f;
        foreach(var mapUnit in MapUnits)
        {
            X += mapUnit.X * mapUnit.Strength;
            Y += mapUnit.Y * mapUnit.Strength;
            S += mapUnit.Strength;
        }

        return new float[] { X / S, Y / S };
    }

}

public interface IUnitSelectionPublisher
{
    event EventHandler<MapGroupAdaptor> UnitDeselected;
    event EventHandler<MapGroupAdaptor> UnitSelected;
}

public class TextInput
{
    public string Scn;
    public string Oob;
    public string ScnOld;
}

// "Controller"

public class GameManager : MonoBehaviour, IUnitSelectionPublisher
{
    // public TextAsset OobText;
    public TextAsset ColorJson;
    Dictionary<string, Color32> colorMap = new();

    // public GameObject FrenchUnitPrefab;
    // public GameObject AlliedUnitPrefab;
    public GameObject UnitPrefab;
    // public GameObject ManeuverLineFrenchPrefab;
    // public GameObject ManeuverLineAlliedPrefab;
    public GameObject ManeuverLinePrefab;
    
    public GameObject UnitContainer;
    public GameObject ManeuverLineContainer;

    UnitGroup unitGroup;
    // JTSUnitStates unitStatus;
    // JTSUnitStates unitStatusOld;
    List<MapGroupAdaptor> mapGroups;
    List<MapGroupAdaptor> mapGroupsOld;
    Dictionary<MapGroupAdaptor, GameUnit> viewMap = new();
    Dictionary<GameUnit, MapGroupAdaptor> modelMap = new();
    Dictionary<Tuple<MapGroupAdaptor, MapGroupAdaptor>, LineRenderer> lineViewMap = new();

    GameUnit selectingGameUnit;

    public event EventHandler<MapGroupAdaptor> UnitDeselected;
    public event EventHandler<MapGroupAdaptor> UnitSelected;

    public int DistanceThreshold = 3;

    bool showIndependentArtillery = false;

    public TextInput Text;

    public int TextMode = 0;

    public int ShowMode = 0; // 0 => Single, 1 => Comparison
    public string ParserMode = "NB"; // NB, CWB

    public void Awake()
    {
        var int3Map = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(ColorJson.text);
        foreach(var KV in int3Map)
        {
            colorMap[KV.Key] = new Color32(KV.Value[0], KV.Value[1], KV.Value[2], 255);
            Debug.Log($"{KV.Key} => {KV.Value[0]}, {KV.Value[1]}, {KV.Value[2]}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        // Setup(currentScenarioName);
        var int3Map = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(ColorJson.text);
        foreach(var KV in int3Map)
        {
            colorMap[KV.Key] = new Color32(KV.Value[0], KV.Value[1], KV.Value[2], 255);
            Debug.Log($"{KV.Key} => {KV.Value[0]}, {KV.Value[1]}, {KV.Value[2]}");
        }
        */
    }

    Color32 GetColor(string s)
    {
        // mapGroup.MapUnits[0].UnitState.OobItem.Country
        if(colorMap.TryGetValue(s, out var ret))
        {
            return ret;
        }
        return colorMap["Red"]; // "Allied" Fackback for NB titles
    }

    Color32 GetColor(MapGroupAdaptor mapGroup) => GetColor(mapGroup.MapUnits[0].UnitState.OobItem.Country);

    static float MapUnitDistance(MapUnitAdaptor a, MapUnitAdaptor b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    IEnumerable<Tuple<MapGroupAdaptor, MapGroupAdaptor>> MatchMapGroups()
    {
        var lookup = new Dictionary<MapGroupAdaptor, Dictionary<AbstractUnit, int>>();
        foreach(var _mapGroups in new List<List<MapGroupAdaptor>>() { mapGroups, mapGroupsOld})
        {
            foreach(var mapGroup in _mapGroups)
            {
                var dic = new Dictionary<AbstractUnit, int>();
                foreach (var mapUnit in mapGroup.MapUnits)
                {
                    dic[mapUnit.UnitState.OobItem] = mapUnit.Strength;
                }
                lookup[mapGroup] = dic;
            }
        }

        foreach(var mapGroup in mapGroups)
        {
            var unit2strength = lookup[mapGroup];
            var weightTotal = unit2strength.Values.Sum();

            foreach (var mapGroupOld in mapGroupsOld)
            {
                var unit2strengthOld = lookup[mapGroupOld];
                var weight = 0;
                foreach(var KV in unit2strength)
                {
                    if(unit2strengthOld.TryGetValue(KV.Key, out var strengthOld))
                    {
                        var strength = KV.Value;
                        weight += strength;
                    }
                }
                if(weight > weightTotal / 2)
                {
                    yield return new Tuple<MapGroupAdaptor, MapGroupAdaptor>(mapGroup, mapGroupOld);
                    break;
                }
            }
        }
    }

    public void Setup()
    {
        var scenario = new JTSScenario();
        scenario.Extract(Text.Scn);

        // unitGroup = JTSOobParser.ParseUnits(Text.Oob);
        unitGroup = JTSOobParser.FromCode(ParserMode).ParseUnits(Text.Oob);

        /*
        foreach (var unit in unitGroup.Walk())
        {
            var group = unit as UnitGroup;
            if (group != null)
                Debug.Log(group);
            // Debug.Log(unit);
        }
        */

        var unitStatus = new JTSUnitStates();
        unitStatus.ExtractByLines(unitGroup, scenario.DynamicCommandBlock);

        mapGroups = CreateGameUnits(unitStatus);

        if(ShowMode == 1)
        {
            if (Text.ScnOld == null || Text.ScnOld == "")
            {
                Debug.Log("Old File is not loaded?");
            }
            else
            {
                var scenarioOld = new JTSScenario();
                scenarioOld.Extract(Text.ScnOld);
                if (scenarioOld.OobFile != scenario.OobFile)
                {
                    Debug.Log("Oob File doesn't match. Potential problem?");
                    //return;
                }
                var unitStatusOld = new JTSUnitStates();
                unitStatusOld.ExtractByLines(unitGroup, scenarioOld.DynamicCommandBlock);

                mapGroupsOld = CreateGameUnits(unitStatusOld);

                foreach (var mapGroup in mapGroupsOld)
                {
                    // Debug.Log($"viewMap.Count={viewMap.Count}, viewMap.ContainsKey(mapGroup)={viewMap.ContainsKey(mapGroup)}");
                    var view = viewMap[mapGroup];
                    view.SetOld(1);

                }

                foreach (var mapGroupNewOldPair in MatchMapGroups())
                {
                    var mapGroup = mapGroupNewOldPair.Item1;
                    var mapGroupOld = mapGroupNewOldPair.Item2;
                    // var maneuverLinePrefab = mapGroup.MapUnits[0].UnitState.OobItem.Country == "French" ? ManeuverLineFrenchPrefab : ManeuverLineAlliedPrefab; // TODO: Supports more color.
                    var center = mapGroup.GetCenter();
                    var centerOld = mapGroupOld.GetCenter();

                    // Debug.Log($"mapGroup.Name2={mapGroup.Name2}, mapGroupOld.Name2={mapGroupOld.Name2}, center={center}, centerOld={centerOld}");

                    if ((center[0] - centerOld[0]) != 0 || (center[1] - centerOld[1]) != 0)
                    {
                        
                        var lineObj = Instantiate(ManeuverLinePrefab, ManeuverLineContainer.transform);
                        var line = lineObj.GetComponent<LineRenderer>();
                        var color = GetColor(mapGroup);
                        line.material.SetColor("_Color", color);
                        line.SetPositions(new Vector3[] { new Vector3(centerOld[0], centerOld[1], -1), new Vector3(center[0], center[1], -1) });

                        lineViewMap[mapGroupNewOldPair] = line;
                    }

                }
            }
        }

        UpdateTexts();
    }

    List<MapGroupAdaptor> CreateGameUnits(JTSUnitStates unitStatus)
    {
        var mapGroupList = new List<MapGroupAdaptor>();
        foreach (var KV in unitStatus.GroupByBrigade())
        {
            var brigade = KV.Key;
            var unitStates = KV.Value;
            var _mapUnits = unitStates.Select(unitState => new MapUnitAdaptor(unitState)).ToList();

            // var prefab = brigade.Country == "French" ? FrenchUnitPrefab : AlliedUnitPrefab;
            

            var mapUnitsList = YYZ.Stats.Clustering.Cluster(_mapUnits, MapUnitDistance, DistanceThreshold);
            // Debug.Log($"mapUnitsList.Count={mapUnitsList.Count}");

            foreach (var mapUnits in mapUnitsList)
            {
                var mapGroup = new MapGroupAdaptor() { MapUnits = mapUnits };
                var color = GetColor(mapGroup);
                CreateGameUnit(mapGroup, UnitPrefab, color);
                mapGroupList.Add(mapGroup);
            }
        }
        return mapGroupList;
    }

    void CreateGameUnit(MapGroupAdaptor mapGroup, GameObject prefab, Color32 color)
    {
        // var mapGroup = new MapGroupAdaptor() { MapUnits = mapUnits };


        // Debug.Log(mapGroup.Name2);
        var rect = (mapGroup as IMapGroup<MapUnitAdaptor>).GetRectTransform();

        var pos = new Vector3((float)rect.X, (float)rect.Y, 0);
        var deg = rect.Rotation / (2 * System.Math.PI) * 360;
        var gameObject = Instantiate(prefab, pos, Quaternion.identity, UnitContainer.transform);
        var gameUnit = gameObject.GetComponent<GameUnit>();

        var scaleX = Mathf.Max(1, (float)rect.WidthMain * 3);
        var scaleY = 1;
        // var scaleY = Mathf.Max(1, (float)rect.WidthSub);

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

        if (!showIndependentArtillery && mapGroup.IsArtilleryGroup()) // TODO: Since artillery units are usually used in detached state which break formation shape estimation, maybe add an option that just ignores all artillery unit?
        {
            gameUnit.gameObject.SetActive(false);
        }

        gameUnit.SetColor(color);

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
        foreach(var line in lineViewMap.Values)
        {
            Destroy(line.gameObject);
        }
        viewMap.Clear();
        modelMap.Clear();
        lineViewMap.Clear();

        unitGroup = null;
        mapGroups = null;
        mapGroupsOld = null;
    }

    public void OnToggleShowIndependentArtillery(bool showIndependentArtillery)
    {
        this.showIndependentArtillery = showIndependentArtillery;
        Reset();
        Setup();
    }

    public void ReloadScenario(TextInput text)
    {
        Text = text;
        if(text.ScnOld == null)
        {
            ShowMode = 0;
        }
        // Debug.Log($"ReloadScenario name={name}");
        Reset();
        Setup();
    }

    void UpdateTexts()
    {
        switch (TextMode)
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

    public void OnUnitTextModeChanged(int idx)
    {
        Debug.Log($"Text Mode Changed to {idx}");
        TextMode = idx;
        UpdateTexts();
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
