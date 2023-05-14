using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlotManager : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_InputField ScnInputField;
    public TMP_InputField OobInputField;
    public TMP_InputField ScnOldInputField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Plot()
    {
        var input = new TextInput() { Oob = OobInputField.text, Scn = ScnInputField.text };
        gameManager.ShowMode = 0;
        gameManager.ReloadScenario(input);
    }

    public void PlotComparison()
    {
        var input = new TextInput() { Oob = OobInputField.text, Scn = ScnInputField.text, ScnOld = ScnOldInputField.text};
        gameManager.ShowMode = 1;
        gameManager.ReloadScenario(input);
    }

    public void Record(TextInput text)
    {
        ScnInputField.text = text.Scn;
        OobInputField.text = text.Oob;
        ScnOldInputField.text = text.ScnOld != null ? text.ScnOld : "";
    }

    public void SwapStates()
    {
        var t = ScnInputField.text;
        ScnInputField.text = ScnOldInputField.text;
        ScnOldInputField.text = t;
        if(ScnInputField.text != null && ScnInputField.text != "" && ScnOldInputField.text != null && ScnOldInputField.text != "")
            PlotComparison();
    }
}
