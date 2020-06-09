using UnityEngine;

public class VRBodyPart : Bolt.EntityBehaviour<IVRBodyPart>
{
    public Transform parent;

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
    }

    private void Update()
    {
        if (parent)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
    }
}
