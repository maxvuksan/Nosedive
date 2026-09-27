using UnityEngine;

/// <summary>
/// Provides an abstract over user input (to enable different input modes e.g. keyboard + mouse, controller)
/// </summary>
public static class InputManager {

     #region Sustained Inputs

    public static bool LeftInputPressed()
    {
        return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
    }

    public static bool RightInputPressed()
    {
        return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
    }

    public static bool UpInputPressed()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
    }

    public static bool DownInputPressed()
    {
        return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    }

    #endregion


    #region Momentary Inputs

    public static bool LeftInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
    }
    public static bool RightInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
    }
    public static bool UpInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
    }
    public static bool DownInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    }

    public static bool JumpInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
    }
    public static bool InteractInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E);
    }
    public static bool EscapeInputOnPress()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    #endregion
}