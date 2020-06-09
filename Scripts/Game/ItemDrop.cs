using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public ItemsManager.Items typeOfItem;
    public bool hasDoneBefore = false;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("ENTERRR " + "hasDone: " + hasDoneBefore + " other collider: " + other.gameObject.name + " this collider: " + typeOfItem);

        if (BoltNetwork.IsServer)
        {
            if (hasDoneBefore)
            {
                return;
            }
                        
            if (other.gameObject.GetComponent<Player>() != null)
            {
                var itemEvent = ItemChanged.Create();
                itemEvent.Entity = other.gameObject.GetComponent<BoltEntity>();
                itemEvent.Item = (int) typeOfItem;
                itemEvent.Send();
                
                // Remove From Main.items list so new one will be spawned in the future
                List<GameObject> items = Main.Instance.items;
                for( int i = 0; i < items.Count; ++i )
                {
                    if ( items[i] == gameObject )
                    {
                        items.Remove( gameObject );
                        break;    // Quit For Loop
                    }
                }
                
                BoltNetwork.Destroy(GetComponent<BoltEntity>());
                hasDoneBefore = true;

            }
        }
    }
}
