using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public TMP_Text text;

    public GameObject UnitSelectionPublisher;
    // IUnitSelectionPublisher unitSelectionPublisher;

    // Start is called before the first frame update
    void Start()
    {
        SetText("");
        gameObject.SetActive(false);

        var unitSelectionPublisher = UnitSelectionPublisher.GetComponent<IUnitSelectionPublisher>();

        unitSelectionPublisher.UnitDeselected += OnUnitDeselected;
        unitSelectionPublisher.UnitSelected += OnUnitSelected;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(string s) => text.text = s;

    public void OnUnitSelected(object sender, MapGroupAdaptor unit)
    {
        Debug.Log($"Selected: {unit}");

        SetText(unit.Summary());
        gameObject.SetActive(true);

        Debug.Log($"Input.mousePosition={Input.mousePosition}, transform.position={transform.position}");
        transform.position = Input.mousePosition;
    }

    public void OnUnitDeselected(object sender, MapGroupAdaptor unit)
    {
        Debug.Log($"Deselected: {unit}");

        gameObject.SetActive(false);
    }
}
