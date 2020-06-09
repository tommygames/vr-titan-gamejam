using Bolt;
using UnityEngine;

public class VRBodyPartSpawner : MonoBehaviour
{
    public enum BodyPart
    {
        Head,
        LeftHand,
        RightHand
    }
    public BodyPart bodyPart;

    // Start is called before the first frame update
    void Start()
    {
        if (BoltNetwork.IsServer)
        {
            PrefabId prefabId;
            switch (bodyPart)
            {
                case BodyPart.Head:
                    prefabId = BoltPrefabs.Monster_Head;
                    break;
                case BodyPart.LeftHand:
                    prefabId = BoltPrefabs.Monster_HandLeft;
                    break;
                case BodyPart.RightHand:
                    prefabId = BoltPrefabs.Monster_HandRight;
                    break;
                default:
                    prefabId = BoltPrefabs.Monster_Head;
                    break;
            }
 
            var entity = BoltNetwork.Instantiate(prefabId);
            entity.transform.SetPositionAndRotation(transform.position, transform.rotation);
            entity.GetComponent<VRBodyPart>().parent = transform;
        }
    }
}
