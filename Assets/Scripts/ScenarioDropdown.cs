using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

[System.Serializable]
public class OptionSelectedEvent : UnityEvent<string>
{

}

public class ScenarioDropdown : MonoBehaviour
{
    // public static string scenarioPath = "JTSData/peninsula/Scenarios";

    TMP_Dropdown dropdown;
    public OptionSelectedEvent OptionSelected;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDropdownUpdated(int index)
    {
        var option = dropdown.options[index];
        var name = option.text;

        /*
        var textAsset = Resources.Load<TextAsset>(scenarioPath + "/" + name);
        var text = textAsset.text;
        */

        // var text = DataLoader.LoadScenario(name);
        // Debug.Log($"{name} => {text.Length} => {text.Substring(0, 100)}");

        OptionSelected.Invoke(name);
    }
}
