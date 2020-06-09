using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 72;
        QualitySettings.vSyncCount = 1;
        OVRManager.display.displayFrequency = 72.0f;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
