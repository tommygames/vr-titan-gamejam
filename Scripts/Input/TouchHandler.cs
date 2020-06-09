using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TouchHandler
{
    bool CanHandleTouch(Touch touch);
    void HandleTouch(Touch touch);
}
