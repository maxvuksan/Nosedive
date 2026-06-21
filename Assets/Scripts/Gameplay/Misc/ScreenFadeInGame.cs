using UnityEngine;

public class ScreenFadeInGame : MonoBehaviour
{

    [SerializeField] private GameObject _screenFadeWhite;
    [SerializeField] private GameObject _screenFadeBlack;

    void OnEnable()
    {
        bool showWinFade = FindFirstObjectByType<SimpleWalker>(FindObjectsInactive.Include).ReachedWinFlag;

        _screenFadeWhite.SetActive(showWinFade);
        _screenFadeBlack.SetActive(!showWinFade);
    }
    void OnDisable()
    {
        _screenFadeBlack.SetActive(false);
        _screenFadeBlack.SetActive(false);
    }

}
