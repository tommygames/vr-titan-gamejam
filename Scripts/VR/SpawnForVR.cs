using System.Collections;
using System.Collections.Generic;
using Microsoft.Applications.Events.DataModels;
using UnityEngine;

public class SpawnForVR : MonoBehaviour
{
    public GameObject prefabToSpawn;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            GameObject.Instantiate(prefabToSpawn);
//            BoltNetwork.Instantiate(BoltPrefabs.Avatar, Vector3.zero, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
