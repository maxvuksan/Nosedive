using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 AxisSpeeds;

    void Update()
    {
        transform.Rotate(AxisSpeeds * Time.deltaTime);
    }
}
