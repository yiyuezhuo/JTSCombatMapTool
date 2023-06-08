using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverLine : MonoBehaviour
{
    LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color32 color)
    {
        lineRenderer.material.SetColor("_Color", color);
    }
}
