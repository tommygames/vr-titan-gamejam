using UnityEngine;

public class ShieldBlockerTimer : MonoBehaviour
{
    private float timer = 0;

    private void Update()
    {
        if (BoltNetwork.IsClient)
        {
            timer += Time.deltaTime;
            
            if (timer > 10)
            {
                BoltNetwork.Destroy(GetComponent<BoltEntity>());
            }
        }
    }
}
