using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualStick : TouchHandler
{
    public Vector2 stickDir;

    protected float _radius = 60.0f;
    protected Rect _area;
    protected Vector2 _posPrev;

    protected VirtualStickUI _ui;

    public VirtualStick(Rect area, VirtualStickUI ui)
    {
        _area = area;
        _ui = ui;
        _ui.SetRadius(_radius);
    }

    bool TouchHandler.CanHandleTouch(Touch touch)
    {
        return _area.Contains(touch.position);
    }

    void TouchHandler.HandleTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            _posPrev = touch.position;
            if (_ui)
            {
                _ui.Show();
            }
        }
        else if (touch.phase == TouchPhase.Ended
            || touch.phase == TouchPhase.Canceled)
        {
            stickDir = Vector2.zero;
            if (_ui)
            {
                _ui.Hide();
            }
            return;
        }

        stickDir.x = touch.position.x - _posPrev.x;
        stickDir.y = touch.position.y - _posPrev.y;

        if (stickDir.magnitude > _radius) // Stick has moved out of the socket radius, drag the socket along with us
        {
            var recenterOffset = stickDir;
            recenterOffset.Normalize();
            recenterOffset *= _radius;
            _posPrev = touch.position - recenterOffset;
            stickDir = recenterOffset;
        }

        stickDir /= _radius;

        if (_ui != null)
        {
            _ui.SetPositions(_posPrev, touch.position - _posPrev);
        }
    }
}
