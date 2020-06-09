using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualMouse : TouchHandler
{
    public Vector2 velocity;

    protected Rect _area;

    public VirtualMouse(Rect area)
    {
        _area = area;
    }

    bool TouchHandler.CanHandleTouch(Touch touch)
    {
        return _area.Contains(touch.position);
    }

    void TouchHandler.HandleTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Moved)
        {
            velocity.x = Mathf.Min(45.0f, Mathf.Pow(touch.deltaPosition.y, 2.0f) * Mathf.Sign(touch.deltaPosition.y) * Time.deltaTime);
            velocity.y = Mathf.Min(45.0f, Mathf.Pow(touch.deltaPosition.x, 2.0f) * Mathf.Sign(touch.deltaPosition.x) * Time.deltaTime);
        }
        else
        {
            velocity = Vector2.zero;
        }
    }
}
