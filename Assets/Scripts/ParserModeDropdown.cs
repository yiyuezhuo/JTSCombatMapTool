using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ParserModeDropdown : MonoBehaviour
{
    public GameManager Manager;
    TMP_Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOptionChanged(int idx)
    {
        Manager.ParserMode = dropdown.options[idx].text;
    }
}
