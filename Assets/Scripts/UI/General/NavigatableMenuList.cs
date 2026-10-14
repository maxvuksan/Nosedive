using UnityEngine;

/// <summary>
/// A list ui entries that can be navigating with the keyboard
/// </summary>
public class NavigatableMenuList : MonoBehaviour
{
    private const string UiBlipDownSoundLabel = "UIBlipDown";
    private const string UiBlipUpSoundLabel = "UIBlipUp";
    private const string UiBlipSubmitSoundLabel = "UIBlipSubmit";
    
    /// <summary>
    /// Decides if the selectedItem reverts to 0 when the component enables
    /// </summary>
    public bool ResetSelectedOnEnable { get; set; } = true;

    public NavigatableMenuItem[] Items;
    public int _selectedItem = 0;

    private void Awake()
    {
        foreach(var item in Items)
        {
            UIElementJolt jolter = item.gameObject.AddComponent<UIElementJolt>();
            jolter.JoltStrength = Helpers.Singleton.UiJoltStrength;
            jolter.JoltSpeed = Helpers.Singleton.UiJoltSpeed;

            if(item.HorizontalJoltTarget != null)
            {
                UIElementJolt jolterHorizontal = item.HorizontalJoltTarget.AddComponent<UIElementJolt>();
                jolterHorizontal.JoltStrength = Helpers.Singleton.UiHorizontalJoltStrength;
                jolterHorizontal.JoltSpeed = Helpers.Singleton.UiHorizontalJoltSpeed;   
            }
        }
    }

    public void OnEnable()
    {
        if (ResetSelectedOnEnable)
        {
            AssignSelectedIndex(0);
        }
    }

    public void Update()
    {
        // interacting with selected item
        if(InputManager.LeftInputOnPress())
        {   
            if(Items[_selectedItem].HorizontalJoltTarget != null)
            {
                Items[_selectedItem].HorizontalJoltTarget.GetComponent<UIElementJolt>().Jolt(new Vector2(-1, 0));
                FmodAudioManager.PlayOneShot(UiBlipDownSoundLabel);
            }
            Items[_selectedItem].OnLeftInput?.Invoke();
        }
        else if(InputManager.RightInputOnPress()) 
        {
            if(Items[_selectedItem].HorizontalJoltTarget != null)
            {
                Items[_selectedItem].HorizontalJoltTarget.GetComponent<UIElementJolt>().Jolt(new Vector2(1, 0));
                FmodAudioManager.PlayOneShot(UiBlipUpSoundLabel);

            }
            Items[_selectedItem].OnRightInput?.Invoke();
        }

        // Changing selected items up/down
        if(InputManager.UpInputOnPress()) 
        {
            StepToNextItem(-1);
            FmodAudioManager.PlayOneShot(UiBlipDownSoundLabel);

        }
        else if(InputManager.DownInputOnPress()) 
        {
            StepToNextItem(1);
            FmodAudioManager.PlayOneShot(UiBlipUpSoundLabel);
        }
        

        // Interacting with selected item main input
        // Note: We only allow this to execute if a function is assigned to OnMainInput
        if(InputManager.JumpInputOnPress() && Items[_selectedItem].OnMainInput.GetPersistentEventCount() > 0) 
        {
            FmodAudioManager.PlayOneShot(UiBlipSubmitSoundLabel);
            Items[_selectedItem].OnMainInput?.Invoke(); 
        }
    }

    /// <summary>
    /// Assigns the selected index on this list, this does not play animation or sound
    /// </summary>
    public void AssignSelectedIndex(int index)
    {
        _selectedItem = index;

        // Update colour
        StepToNextItem(0, false, false);   
    }

    private void StepToNextItem(int direction, bool shouldJolt = true, bool shouldInvoke = true)
    {
        int newIndex = direction + _selectedItem;

        if(newIndex == Items.Length)
        {
            newIndex = 0;
        }

        else if(newIndex < 0)
        {
            newIndex = Items.Length - 1;
        }

        _selectedItem = newIndex;

        for(int i = 0; i < Items.Length; i++)
        {
            if(i == _selectedItem)
            {
                if (shouldJolt)
                {
                    Items[i].GetComponent<UIElementJolt>().Jolt(new Vector2(0, -direction));
                }
                if (shouldInvoke)
                {
                    Items[i].OnSelectItem?.Invoke();
                }
                Items[i].SetColour(Helpers.Colours.UiSelected);
            }
            else
            {
                Items[i].SetColour(Helpers.Colours.UiIdle);
            }
        }
    }



}
