using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;
using System.Linq;
using YYZ.JTS;


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


[System.Serializable]
public class OptionSelectedEvent : UnityEvent<TextInput>
{
}

public class ScenarioDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    public OptionSelectedEvent OptionSelected;

    public string InitialScenarioName = "011.Coruna4_BrAI";

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        var texts = dropdown.options.Select(x => x.text).ToList();
        var index = texts.IndexOf(InitialScenarioName);
        // OnDropdownUpdated(index);
        dropdown.value = index;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static string RemoveExtension(string p)
    {
        var sl = p.Split(".");
        return string.Join(".", sl.Take(sl.Length - 1));
    }


    public void OnDropdownUpdated(int index)
    {
        var option = dropdown.options[index];
        var scnName = option.text;
        var scnText = DataLoader.LoadScenario(scnName);

        /*
        var scenario = new JTSScenario();
        scenario.Extract(scnText); // TODO: add an "early stop mode"?
        */

        var parser = JTSParser.FromCode("NB");
        var scenario = parser.ParseScenario(scnText);

        var oobName = RemoveExtension(scenario.OobFile);
        var oobText = DataLoader.LoadOob(oobName);

        /*
        var textAsset = Resources.Load<TextAsset>(scenarioPath + "/" + name);
        var text = textAsset.text;
        */

        // var text = DataLoader.LoadScenario(name);
        // Debug.Log($"{name} => {text.Length} => {text.Substring(0, 100)}");

        OptionSelected.Invoke(new TextInput() { Scn=scnText, Oob=oobText});
    }
}
