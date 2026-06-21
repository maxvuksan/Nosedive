using UnityEngine;

public class RaisingBlock : PressurePlateListener
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Transform _localOffTransform;
    [SerializeField] private Transform _localOnTransform;
    [SerializeField] private float _speed;

    private Vector3 _targetRotation;
    private Vector3 _targetPosition;

    private Rigidbody _rigidBody;


    void Start()
    {
        _rigidBody = _target.AddComponent<Rigidbody>();
        _rigidBody.isKinematic = true;
        OnSwitchState(false);
    }

    void FixedUpdate()
    {
        _rigidBody.MovePosition(
            Vector3.MoveTowards(_rigidBody.position, _targetPosition, _speed * Time.fixedDeltaTime)
        );

        _rigidBody.MoveRotation(
            Quaternion.RotateTowards(_rigidBody.rotation, Quaternion.Euler(_targetRotation), _speed * 90f * Time.fixedDeltaTime)
        );
    }


    public override void OnSwitchState(bool openState)
    {
        if (openState)
        {
            _targetPosition = _localOnTransform.position;
            _targetRotation = _localOnTransform.rotation.eulerAngles;
        }
        else
        {
            _targetPosition = _localOffTransform.position;
            _targetRotation = _localOffTransform.rotation.eulerAngles;
        }

    }
}
