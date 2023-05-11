using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUnit : MonoBehaviour
{
    public GameObject ColoredRect;
    public TMP_Text text;
    public SpriteRenderer sRenderer;

    // Start is called before the first frame update
    void Start()
    {
        sRenderer = ColoredRect.GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(float mainSize, float subSize)
    {
        ColoredRect.transform.localScale = new Vector3(mainSize, 1, 1);
    }

    public void SetRotation(float deg)
    {
        ColoredRect.transform.rotation = Quaternion.Euler(0, 0, (float)deg);
    }

    public void SetText(string s)
    {
        text.text = s;
    }

    public void OnDeselected()
    {
        sRenderer.material.SetInt("_HighlightMode", 0);
    }

    public void OnSelected()
    {
        sRenderer.material.SetInt("_HighlightMode", 1);
    }
}
