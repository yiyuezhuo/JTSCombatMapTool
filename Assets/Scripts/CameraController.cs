using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    // Vector2 prevMousePos;
    // Vector2 prevCamPos;
    bool dragging = false;

    // public float MovingSpeed = 0.1f;
    public float ZoomSpeed = 1f;

    Vector2 lastTrackedPos;

    static Vector2 mouseAdjustedCoef = new Vector2(1, -1);

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    Vector2 GetHitPoint()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.forward, Vector3.zero);
        if (plane.Raycast(ray, out var distance))
        {
            var hitPoint = ray.GetPoint(distance);
            return (Vector2)hitPoint;
        }
        return Vector2.zero;
    }

    void UpdateHitPoint()
    {
        lastTrackedPos = GetHitPoint();
    }

    void DragHitPoint()
    {
        var newTrackedPos = GetHitPoint();
        var diff = newTrackedPos - lastTrackedPos;
        transform.Translate(-diff * mouseAdjustedCoef);
        UpdateHitPoint();
    }

    // Update is called once per frame
    void Update()
    {

        // Zoom
        if(Input.mouseScrollDelta.y != 0)
        {
            var newSize = cam.orthographicSize - Input.mouseScrollDelta.y * ZoomSpeed;
            if (newSize > 0)
            {
                cam.orthographicSize = newSize;
                GetHitPoint();
            }
        }

        // Dragging Navigation
        if(Input.GetMouseButton(1))
        {
            var mousePosition = (Vector2)Input.mousePosition * mouseAdjustedCoef;
            if (!dragging)
            {
                dragging = true;
                UpdateHitPoint();
            }
            else
            {
                DragHitPoint();
            }
        }
        else
        {
            dragging = false;
        }
    }
}
