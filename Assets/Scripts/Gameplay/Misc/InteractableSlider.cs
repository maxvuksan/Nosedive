using UnityEngine;

public class InteractableSlider : MonoBehaviour
{

    [SerializeField] private float _sliderSpeed = 0.5f;

    [Range(0, 1)]
    [SerializeField] private float _sliderT = 0;
    [SerializeField] Transform startPosition;
    [SerializeField] Transform endPosition;
    [SerializeField] Transform light;

    void Update()
    {
        light.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, _sliderT);

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _sliderT -= _sliderSpeed * Time.deltaTime;
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            _sliderT += _sliderSpeed * Time.deltaTime;
        }

    }


}
