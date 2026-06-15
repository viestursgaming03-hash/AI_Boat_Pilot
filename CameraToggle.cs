using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera cameraFollow;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mainCamera.gameObject.SetActive(!mainCamera.gameObject.activeSelf);
            cameraFollow.gameObject.SetActive(!mainCamera.gameObject.activeSelf);
        }
    }
}
