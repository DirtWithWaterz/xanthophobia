using UnityEngine;

public class OffsetFlashlight : MonoBehaviour
{
    private Vector3 vectOffset;
    [SerializeField] private GameObject goFollow;
    [SerializeField] private float speed = 3.0f;


    void Start()
    {
        vectOffset = transform.position - goFollow.transform.position;
    }
    void LateUpdate()
    {
        transform.SetPositionAndRotation(goFollow.transform.position + vectOffset, Quaternion.Slerp(transform.rotation, goFollow.transform.rotation, speed * Time.deltaTime));
    }
}
