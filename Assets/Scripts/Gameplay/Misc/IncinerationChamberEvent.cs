using UnityEngine;

/// <summary>
/// Orchestrates the incineration chamber events
/// </summary>
public class IncinerationChamberEvent : PressurePlateListener
{
    /// <summary>
    /// Blast doors, these will open/close when the pressure plate triggers
    /// </summary>
    [SerializeField] private BlastDoor _blastDoorBehind;
    [SerializeField] private BlastDoor _blastDoorInFront;

    [SerializeField] private MeshRenderer[] _timerChipMeshRenderers;
    [SerializeField] private Material _timerChipOffMaterial;
    [SerializeField] private Material _timerChioOnMaterial;
    [SerializeField] private float _timerLength;
    
    public override void OnSwitchState(bool pressurePlateState)
    {
        if (pressurePlateState)
        {
            StartSequence();
        }
    }

    private void StartSequence()
    {
        _blastDoorBehind.PlayAnimationOpenToClose();
        _blastDoorInFront.PlayAnimationCloseToOpen();
    }
}
