using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class InventoryStateManager : MonoBehaviour
{
    /// <summary>
    /// Which selected page is the inventory open for
    /// </summary>
    public enum InventorySelection{
        
        Papers,
        Gizmos,
        Warps,
    }

    [SerializeField] private NavigatableMenuList _navigationMenuList;

    public GameObject[] EnableOnPapers;
    public GameObject[] EnableOnWarps;
    public GameObject[] EnableOnGizmos;

    public static InventoryStateManager Singleton;

    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);        
        _navigationMenuList.ResetSelectedOnEnable = false;
    }

    private void OnEnable()
    {
        ShowPapers();
    }

    public void ShowPapers()
    {
        SetInventorySelection(InventorySelection.Papers);
        _navigationMenuList.AssignSelectedIndex(1);
    }
    public void ShowGizmos()
    {
        SetInventorySelection(InventorySelection.Gizmos);
        _navigationMenuList.AssignSelectedIndex(2);
    }
    public void ShowWarps()
    {
        SetInventorySelection(InventorySelection.Warps);
        _navigationMenuList.AssignSelectedIndex(3);
    }

    private void SetInventorySelection(InventorySelection selection)
    {
        Helpers.SetActiveGameObjectArray(EnableOnWarps, selection == InventorySelection.Warps);
        Helpers.SetActiveGameObjectArray(EnableOnGizmos, selection == InventorySelection.Gizmos);
        Helpers.SetActiveGameObjectArray(EnableOnPapers, selection == InventorySelection.Papers);
    }
    

}
