using UnityEngine;

/// <summary>
/// A list of entries
/// </summary>
public class NavigatableMenuList : MonoBehaviour
{
    public NavigatableMenuItem[] Items;
    public int _selectedItem = 0;

    private void Start()
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
        // default to first element when enabled
        _selectedItem = 0;

        // update colour...
        StepToNextItem(0, false);
    }

    public void Update()
    {
        // interacting with selected item
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {   
            if(Items[_selectedItem].HorizontalJoltTarget != null)
            {
                Items[_selectedItem].HorizontalJoltTarget.GetComponent<UIElementJolt>().Jolt(new Vector2(-1, 0));
                AudioManager.Singleton.Play(Helpers.Singleton.UiBlipDownSoundLabel);
            }
            Items[_selectedItem].OnLeftInput?.Invoke();
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) 
        {
            if(Items[_selectedItem].HorizontalJoltTarget != null)
            {
                Items[_selectedItem].HorizontalJoltTarget.GetComponent<UIElementJolt>().Jolt(new Vector2(1, 0));
                AudioManager.Singleton.Play(Helpers.Singleton.UiBlipUpSoundLabel);
            }
            Items[_selectedItem].OnRightInput?.Invoke();
        }

        // Changing selected items up/down
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) 
        {
            StepToNextItem(-1);
            AudioManager.Singleton.Play(Helpers.Singleton.UiBlipUpSoundLabel);

        }
        else if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) 
        {
            AudioManager.Singleton.Play(Helpers.Singleton.UiBlipDownSoundLabel);
            StepToNextItem(1);
        }

        // Interacting with selected item main input
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) 
        {
            AudioManager.Singleton.Play(Helpers.Singleton.UiBlipSubmitSoundLabel);
            Items[_selectedItem].OnMainInput?.Invoke(); 
        }
    }

    private void StepToNextItem(int direction, bool shouldJolt = true)
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
                Items[i].SetColour(Helpers.Singleton.UiSelectedColour);
            }
            else
            {
                Items[i].SetColour(Helpers.Singleton.UiIdleColour);
            }
        }
    }



}
