using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class GameUnit : MonoBehaviour
{
    public GameObject BaseRect;
    public GameObject DirectionRect;
    public TMP_Text text;
    SpriteRenderer baseRectRenderer;
    SpriteRenderer directionRectRenderer;

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
    // void Start()
    void Awake()
    {
        baseRectRenderer = BaseRect.GetComponent<SpriteRenderer>();
        directionRectRenderer = DirectionRect.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color32 color)
    {
        baseRectRenderer.material.SetColor("_Color", color);
        directionRectRenderer.material.SetColor("_Color", color);
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

    public void SetUnitCategory(int mode)
    {
        baseRectRenderer.material.SetInt("_UnitCategory", mode);
    }

    public void SetOld(int mode)
    {
        baseRectRenderer.material.SetInt("_Old", mode);
        directionRectRenderer.material.SetInt("_Old", mode);

        if (mode == 1)
        {
            text.gameObject.SetActive(false);
            SetRectDepth(1);
        }
        else
        {
            text.gameObject.SetActive(true);
            SetRectDepth(0);
        }
    }

    void SetRectDepth(float depth)
    {
        BaseRect.transform.position = new Vector3(BaseRect.transform.position.x, BaseRect.transform.position.y, depth);
        DirectionRect.transform.position = new Vector3(DirectionRect.transform.position.x, DirectionRect.transform.position.y, depth);
    }

    /*
    public void SetUnitDirection()
    {

    }
    */

    public void OnDeselected()
    {
        baseRectRenderer.material.SetInt("_HighlightMode", 0);
        directionRectRenderer.material.SetInt("_HighlightMode", 0);
    }

    public void OnSelected()
    {
        baseRectRenderer.material.SetInt("_HighlightMode", 1);
        directionRectRenderer.material.SetInt("_HighlightMode", 1);
    }
}
