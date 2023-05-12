using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUnit : MonoBehaviour
{
    public GameObject BaseRect;
    public GameObject DirectionRect;
    public TMP_Text text;
    SpriteRenderer sRenderer;

    // Debug properties

    public string DebugName;
    public float DebugRotation;
    public float DebugWidthMain;
    public float DebugWidthSub;
    public float DebugX;
    public float DebugY;
    public float[] DebugUnitDirection;
    public int DebugUnitDirectionModeIndex;

    // Start is called before the first frame update
    void Start()
    {
        sRenderer = BaseRect.GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(float mainSize, float subSize)
    {
        BaseRect.transform.localScale = new Vector3(mainSize, subSize, 1);
        // BaseRect.transform.localScale = new Vector3(mainSize, 1, 1);
    }

    public void SetRotation(float deg)
    {
        BaseRect.transform.rotation = Quaternion.Euler(0, 0, (float)deg);
    }

    public void SetText(string s)
    {
        text.text = s;
    }

    /*
    public void SetUnitDirection()
    {

    }
    */

    public void OnDeselected()
    {
        sRenderer.material.SetInt("_HighlightMode", 0);
    }

    public void OnSelected()
    {
        sRenderer.material.SetInt("_HighlightMode", 1);
    }
}
