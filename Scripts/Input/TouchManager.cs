using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchManager : MonoBehaviour
{
    public VirtualStick virtualStick;
    public VirtualStickUI virtualStickUI;
    public VirtualButtonArea virtualButton;
    public VirtualMouse virtualMouse;

    public static TouchManager Instance { get { return instance; } }
    protected static TouchManager instance;

    protected List<TouchHandler> _touchHandlers = new List<TouchHandler>();
    protected Dictionary<int, TouchHandler> _activeTouchHandlers = new Dictionary<int, TouchHandler>();

	// Use this for initialization
	void Start ()
    {
        Debug.Assert(instance == null);
        instance = this;

        virtualStickUI = gameObject.AddComponent<VirtualStickUI>();
        virtualStick = new VirtualStick(new Rect(0, 0, (Screen.width/4) * 3, Screen.height), virtualStickUI);
        _touchHandlers.Add(virtualStick);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                foreach (var touchHandler in _touchHandlers)
                {
                    if (touchHandler.CanHandleTouch(touch))
                    {
                        touchHandler.HandleTouch(touch);
                        _activeTouchHandlers.Add(touch.fingerId, touchHandler);
                        _touchHandlers.Remove(touchHandler);
                        break;
                    }
                }
            }
            else 
            {
                var touchHandler = _activeTouchHandlers[touch.fingerId];
                touchHandler.HandleTouch(touch);
                if (touch.phase == TouchPhase.Ended
                    || touch.phase == TouchPhase.Canceled)
                {
                    _activeTouchHandlers.Remove(touch.fingerId);
                    _touchHandlers.Add(touchHandler);
                }
            }
        }
    }
}
