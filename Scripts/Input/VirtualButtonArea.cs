using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualButtonArea : TouchHandler
{
    public enum Phase
    {
        Off,
        Began,
        Held,
        Ended
    };

    public Phase phase;
    protected Rect _area;

    public VirtualButtonArea(Rect area)
    {
        _area = area;
    }

    bool TouchHandler.CanHandleTouch(Touch touch)
    {
        return _area.Contains(touch.position);
    }

    void TouchHandler.HandleTouch(Touch touch)
    {
        switch(touch.phase)
        {
            case TouchPhase.Began:
                phase = Phase.Began;
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                phase = Phase.Held;
                break;
            case TouchPhase.Ended:
                phase = Phase.Ended;
                break;
            default:
                phase = Phase.Off;
                break;
        }
    }
}
