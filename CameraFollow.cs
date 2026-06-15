using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform boat;
    private void Update()
    {
    transform.localPosition = boat.localPosition + new Vector3(0f, 0f, -10f);
    }
}
