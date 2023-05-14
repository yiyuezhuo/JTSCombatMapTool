using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlotManager : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_InputField ScnInputField;
    public TMP_InputField OobInputField;

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
        gameManager.ReloadScenario(input);
    }

    public void Record(TextInput text)
    {
        ScnInputField.text = text.Scn;
        OobInputField.text = text.Oob;
    }
}
