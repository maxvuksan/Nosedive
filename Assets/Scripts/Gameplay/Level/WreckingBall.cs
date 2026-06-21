using UnityEngine;

public class WreckingBall : PressurePlateListener
{
    [SerializeField] public Transform _horizontalMover;
    [SerializeField] private float _maxHorizontalPosition;
    [SerializeField] public Transform _verticalMover;
    [SerializeField] private float _maxVerticalPosition;

    [SerializeField] public float _movementSpeed;
    [SerializeField] public Animator _brokenBeamsAnimator;

    private bool _movingLeft = false;
    private bool _movingRight = false;
    private bool _movingForward = false;
    private bool _movingBack = false;
    
    private Vector2 _movementSignal = new(0,0);


    public void HitBoards()
    {
        _brokenBeamsAnimator.SetTrigger("Break");
    }
    

    public override void OnSwitchState(bool pressurePlateState, int pressurePlateIndex)
    {
        // 0: forward
        // 1: back
        // 2: left
        // 3: right
        
        switch (pressurePlateIndex)
        {
            case 0:
                _movingForward = pressurePlateState;
                break;
            case 1:
                _movingBack = pressurePlateState;
                break;
            case 2:
                _movingLeft = pressurePlateState;
                break;
            case 3:
                _movingRight = pressurePlateState;
                break;
        }

        _movementSignal.x = 0;
        _movementSignal.y = 0;

        if (_movingForward)
        {
            _movementSignal.y = -1;
        }
        else if(_movingBack)
        {
            _movementSignal.y = 1;
        }

        if (_movingLeft)
        {
            _movementSignal.x = 1;
        }
        else if(_movingRight)
        {
            _movementSignal.x = -1;
        }

        print(_movementSignal);
    }

    void Update(){
        _horizontalMover.transform.position = _horizontalMover.transform.position + new Vector3(_movementSignal.x, 0, 0) * Time.deltaTime * _movementSpeed;
        _verticalMover.transform.position = _verticalMover.transform.position + new Vector3(0, 0, _movementSignal.y) * Time.deltaTime * _movementSpeed;
    }
}
