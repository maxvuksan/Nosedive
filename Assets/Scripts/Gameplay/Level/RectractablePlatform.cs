using System.Collections.Generic;
using UnityEngine;


public struct RectractablePlatformEntry
{
    public Animator Animator;
}


/// <summary>
/// A rectractable bridge of platforms 
/// </summary>
public class RectractablePlatform : PressurePlateListener
{
    [SerializeField] private int _numberOfPlatforms;
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private float _timeBetweenPropogations = 10;
    private float _timeBetweenPropogationsTracked;
    private int _propogationIndex;
    private bool _unfolding;
    private List<RectractablePlatformEntry> _platforms;


    void Start()
    {
        _platforms = new();
        _propogationIndex = 0;
        _unfolding = false;

        Transform previousPlatformTransform = this.transform;

        for(int i = 0; i < _numberOfPlatforms; i++)
        {
            GameObject newPlatform = Instantiate(_platformPrefab, previousPlatformTransform);

            RectractablePlatformEntry entry = new()
            {
                Animator = newPlatform.GetComponentInChildren<Animator>()
            };


            newPlatform.transform.localPosition = new Vector3(0,0,0);
            previousPlatformTransform = entry.Animator.transform;

            _platforms.Add(entry);
        }
    }

    public override void OnSwitchState(bool pressurePlateState)
    {
        // we have already begun unfolding
        if(_propogationIndex != 0)
        {
            return;
        }

        if (pressurePlateState)
        {
            _timeBetweenPropogationsTracked = _timeBetweenPropogations;
            _unfolding = true;
        }
    }


    private void FixedUpdate() 
    {
        if (_unfolding)
        {
            _timeBetweenPropogationsTracked += Time.fixedDeltaTime;

            if(_timeBetweenPropogationsTracked > _timeBetweenPropogations)
            {
                _platforms[_propogationIndex].Animator.SetTrigger("Unfold");
                _propogationIndex++;
                _timeBetweenPropogationsTracked = 0;
            }

            if(_propogationIndex >= _platforms.Count)
            {
                _unfolding = false;
            }
        }
    }
    
}
