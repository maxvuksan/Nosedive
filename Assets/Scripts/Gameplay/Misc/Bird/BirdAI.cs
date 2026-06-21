using System;
using UnityEngine;

[System.Serializable]
public class IdleAnimation
{
    public string TriggerName;
    public float Weight;
}


[System.Serializable]
public class BirdConfig
{
    public IdleAnimation[] IdleAnimations = new IdleAnimation[]
    {
        new IdleAnimation { TriggerName = "Peck", Weight = 3f },
        new IdleAnimation { TriggerName = "LookAround", Weight = 2f },
        new IdleAnimation { TriggerName = "Hop", Weight = 1f }
    };
    public float FlyDirectionVarianceDegrees = 30; 
    public float FlyAwayHeight;
    public float FlyAwayDistance;
    public float FlySpeedMax;
    public float FlySpeedInitalMin;
    public float FlySpeedInitalMax;
    public float FlySpeedIncrease;
    public float ScareOtherBirdsDistance;
    public float ScareOtherBirdsPropogateTimeMin = 0;
    public float ScareOtherBirdsPropogateTimeMax = 0.2f;
    public float FacingAngleLerpSpeed = 0.6f;

}

public class BirdAI : MonoBehaviour
{
    [HideInInspector] public Vector3 PreferredFlyDirection = new(0,0,0);
    private Animator _animator;
    public BirdConfig Config;
    private float _flySpeedTracked;
    private float _targetFacingAngleDegrees;
    private float _trackedFacingAngleDegrees;
    
    private static string[] _animatorIdleTriggers =
    {
        "Peck",
        "LookAround",
        "Hop"
    };

    private static Action s_OnScareAway;


    public float ScareAwayInTimeTracked { get => _scareAwayInTimeTracked; }
    private float _scareAwayInTimeTracked; // will scare the bird away in this amount of time

    public bool Flying { get => _flying; }
    private bool _flying;
    private int _propogateLayer;
    private Vector3 _flyEndPosition;

    public void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void Start()
    {
        GameStateManager.OnStateSelectingLevel += OnStateSelectingLevel;
    }
    public void OnDestroy()
    {
        GameStateManager.OnStateSelectingLevel -= OnStateSelectingLevel;
    }

    public void OnStateSelectingLevel()
    {
        // return bird to manager
        BirdManager.MarkBirdForReuse(this);
    }

    public void OnEnable()
    {
        _flying = false;
        _targetFacingAngleDegrees = UnityEngine.Random.Range(0,360);
        _trackedFacingAngleDegrees = _targetFacingAngleDegrees;
        _scareAwayInTimeTracked = 0;

        // pick inital idle animation
        OnIdleAnimationFinish();
    }


    /// <summary>
    /// Automatically plays a new idle animation when the last one ends, this function is called at the end of the idle animation
    /// </summary>
    public void OnIdleAnimationFinish()
    {
        if (_flying)
        {
            return;
        }

        if (Config.IdleAnimations.Length == 0) return;

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var anim in Config.IdleAnimations)
        {
            totalWeight += anim.Weight;
        }

        // Pick random value in weight range
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        // Find which animation this value lands on
        float cumulativeWeight = 0f;
        foreach (var anim in Config.IdleAnimations)
        {
            cumulativeWeight += anim.Weight;
            if (randomValue <= cumulativeWeight)
            {
                // hop to a random new rotation
                if(anim.TriggerName == "Hop"){
                    int degreeShift = 50;
                    _targetFacingAngleDegrees += UnityEngine.Random.Range(-degreeShift, degreeShift);
                }

                _animator.SetTrigger(anim.TriggerName);
                return;
            }
        }
    }

    public void ScareAwayOtherBirds(Vector2 scarePosition, int propogateLayer)
    {
        var otherBirds = FindObjectsByType<BirdAI>(FindObjectsSortMode.None);

        float distance;
        foreach(var bird in otherBirds)
        {
            distance = Vector3.Distance(transform.position, bird.transform.position);

            if(distance < Config.ScareOtherBirdsDistance)
            {
                if(!bird.Flying && bird.ScareAwayInTimeTracked == 0)
                {
                    bird._propogateLayer = propogateLayer + 1;
                    bird._flyEndPosition = -new Vector3(scarePosition.x, 0, scarePosition.y);
                    _scareAwayInTimeTracked = UnityEngine.Random.Range(Config.ScareOtherBirdsPropogateTimeMin, Config.ScareOtherBirdsPropogateTimeMax);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        SimpleWalker player = other.GetComponent<SimpleWalker>();

        if(player == null)
        {
            return;
        }

        TriggerBirdScareAway(other.transform.position, 1);
    }

    /// <summary>
    /// Triggers a bird to fly away
    /// </summary>
    /// <param name="otherPosition">The position the bird is being scared from</param>
    /// <param name="propogateLayer">How indirectly was the bird scared? if the player scared it directly, this is 1, if a bird which the player scared scares it, it is 2, etc...</param>
    private void TriggerBirdScareAway(Vector3 otherPosition, int propogateLayer = 0)
    {
        // already in air, ignore scare away
        if (_flying)
        {
            return;
        }

        // unparent so it is not unloaded when levels change
        transform.parent = null;

        Vector3 positionDifference = transform.position - otherPosition;

        Vector3 flyDirection;

        if(PreferredFlyDirection != Vector3.zero)
        {
            flyDirection = PreferredFlyDirection;
        }
        else
        {
            flyDirection = new Vector3(positionDifference.x, Config.FlyAwayHeight, positionDifference.z);
            // Add rotational variance around the Y axis
            float randomAngle = UnityEngine.Random.Range(-Config.FlyDirectionVarianceDegrees, Config.FlyDirectionVarianceDegrees);
            flyDirection = Quaternion.Euler(0, randomAngle, 0) * flyDirection;
        }
        
        flyDirection.Normalize();

        _targetFacingAngleDegrees = MyMaths.Vector2ToDegrees(new Vector2(flyDirection.x, flyDirection.z));

        _animator.SetTrigger("Flying");

        _flyEndPosition = transform.position + flyDirection * Config.FlyAwayDistance;
        _flySpeedTracked = UnityEngine.Random.Range(Config.FlySpeedInitalMin, Config.FlySpeedInitalMax);
        _flying = true;

        BirdManager.TriggerBirdFlyAwaySound(this, _propogateLayer);

        ScareAwayOtherBirds(otherPosition, propogateLayer);
    }

    public void FixedUpdate()
    {
        // TODO: This lerp may not be FPS the same between different FixedUpdate rates
        _trackedFacingAngleDegrees = Mathf.LerpAngle(_trackedFacingAngleDegrees, 180 - _targetFacingAngleDegrees, Config.FacingAngleLerpSpeed * Time.deltaTime);
    }

    public void Update()
    {
        // propogate birds scaring each other
        if(_scareAwayInTimeTracked > 0)
        {
            _scareAwayInTimeTracked -= Time.deltaTime;
        
            if(_scareAwayInTimeTracked <= 0)
            {
                TriggerBirdScareAway(_flyEndPosition, _propogateLayer);
            }
        }

        // playout flying behaviour
        if (_flying)
        {
            _flySpeedTracked += Config.FlySpeedIncrease * Time.deltaTime;
            if(_flySpeedTracked > Config.FlySpeedMax)
            {
                _flySpeedTracked = Config.FlySpeedMax;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _flyEndPosition, _flySpeedTracked * Time.deltaTime);

            float distanceToGoal = Vector3.Distance(transform.position, _flyEndPosition);
            if(distanceToGoal < 10.0f)
            {
                BirdManager.MarkBirdForReuse(this);
            }

            float t =  Mathf.InverseLerp(0, Config.FlySpeedMax * 0.5f, _flySpeedTracked);
            t = Mathf.Clamp01(t);

            _animator.SetFloat("FlapSpeed", Mathf.InverseLerp(0, Config.FlySpeedMax * 0.5f, _flySpeedTracked));
        }

        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, _trackedFacingAngleDegrees, transform.rotation.z));
    }
}
