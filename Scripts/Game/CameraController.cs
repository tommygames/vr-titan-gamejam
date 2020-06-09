using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float cameraFollowAngle = 5.0f;
    public float cameraAngleDown = 25.0f;
    public float cameraHeight = 1.7f;
    
    private Camera camera;
    private Vector3 lastPlayerPosition;
    
    private void Start()
    {
        camera = gameObject.GetComponent<Camera>();
    }

    private void Update()
    {
        if (player)
        {
            var playerVect = player.transform.position;
            var cameraVect = camera.transform.position;

            playerVect.y = 0;
            cameraVect.y = 0;

            float angle = Mathf.Deg2Rad * Vector3.Angle(playerVect, cameraVect);
            float maxAngle = Mathf.Deg2Rad * cameraFollowAngle;
            if (Mathf.Abs(angle) > maxAngle)
            {
                Vector3 updatedCameraVect = Vector3.RotateTowards(playerVect, cameraVect, maxAngle, 0);
                cameraVect = cameraVect.magnitude * updatedCameraVect.normalized;
                cameraVect.y = cameraHeight;
                camera.transform.position = cameraVect;
                cameraVect.y = 0f;
                camera.transform.rotation = Quaternion.LookRotation(-cameraVect, Vector3.up);
                Vector3 rot = camera.transform.localRotation.eulerAngles;
                rot.x = cameraAngleDown;
                camera.transform.localRotation = Quaternion.Euler(rot);
            }
        }
//        else
//        {
//            camera.transform.position = Vector3.up * 5.5f;
//            camera.transform.rotation = Quaternion.LookRotation(Vector3.down);
//        }
    }
}
