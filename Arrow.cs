
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] Transform boat;
    [SerializeField] SpriteRenderer spriteRenderer;
    void Update()
    {

        spriteRenderer.color = new Color(boat.localPosition.x, boat.localPosition.y, boat.localPosition.z, 1f);
    }
}
