using UnityEngine;

public class FinishLine : MonoBehaviour
{   

    [SerializeField] private Transform _mesh;
    [SerializeField] private float _meshScale;
    [SerializeField] private float _timeForMeshToScale;
    private float _scaleTimeTracked;

    private bool _showHasWonAnimation;


    void OnEnable()
    {
        _mesh.localScale = new Vector3(1, 1, 1);
        _showHasWonAnimation = false;
        _scaleTimeTracked = 0;
    }


    public void OnTriggerEnter(Collider other)
    {
        // The player has reached the finish line

        SimpleWalker player = other.GetComponent<SimpleWalker>();
        
        if(player == null)
        {
            return;
        }

        player.ReachedWinFlag = true;
        _showHasWonAnimation = true;
    }

    public void Update()
    {
        if (_showHasWonAnimation)
        {
            _scaleTimeTracked += Time.deltaTime;

            float scaler = Mathf.Lerp(1, _meshScale, _scaleTimeTracked / _timeForMeshToScale);

            if(_scaleTimeTracked > _timeForMeshToScale)
            {
                _showHasWonAnimation = false;
            }

            _mesh.localScale = new Vector3(scaler, 1, scaler);
        }
    }
}
