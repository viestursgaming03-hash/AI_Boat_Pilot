using UnityEngine;
using Unity.Mathematics;
using random = UnityEngine.Random;

public class FireworkProjectile : MonoBehaviour
{
    [SerializeField] ParticleSystem red;
    [SerializeField] ParticleSystem orange;
    [SerializeField] ParticleSystem yellow;
    [SerializeField] ParticleSystem green;
    [SerializeField] ParticleSystem blue;
    [SerializeField] ParticleSystem purple;
    [SerializeField] ParticleSystem pink;
    private float targetDistance = 32f;
    private float moveSpeed = 16f;
    private Vector3 forwardAngle = new (0f, 0f, 0f);
    private Vector3 startPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, random.Range(-60, -30));
        forwardAngle = new Vector3(0f, 0f, transform.localRotation.eulerAngles.z + 135);
        targetDistance = random.Range(moveSpeed * 1.5f, moveSpeed * 2.5f); // fireworks explode after 1.5-2.5 seconds
        startPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if ((startPosition - transform.localPosition).magnitude > targetDistance)
        {
            int randomizedInt = random.Range(1, 8);
            if (randomizedInt == 1)
            {
                Instantiate(red, transform.localPosition, Quaternion.identity);
            }
            else if (randomizedInt == 2)
            {
                Instantiate(orange, transform.localPosition, Quaternion.identity);
            }
            else if (randomizedInt == 3)
            {
                Instantiate(yellow, transform.localPosition, Quaternion.identity);
            }
            else if (randomizedInt == 4)
            {
                Instantiate(green, transform.localPosition, Quaternion.identity);
            }
            else if (randomizedInt == 5)
            {
                Instantiate(blue, transform.localPosition, Quaternion.identity);
            }
            else if (randomizedInt == 6)
            {
                Instantiate(purple, transform.localPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(pink, transform.localPosition, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        else
        {
        transform.position += new Vector3(moveSpeed * math.cos(math.radians(forwardAngle.z)) * Time.deltaTime, moveSpeed * math.sin(math.radians(forwardAngle.z)) * Time.deltaTime, 0f);

        }
    }
}
