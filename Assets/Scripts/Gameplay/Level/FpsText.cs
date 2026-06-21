using UnityEngine;

public class FpsText : MonoBehaviour
{
    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f; // simple smoothing
    }

    void OnGUI()
    {
        if (!Helpers.Singleton.DebugMode){
            return;
        }

        // only show in debug mode...

        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 60;
        style.normal.textColor = Color.white;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = "DEBUG MODE: " + string.Format("{0:0.0} ms ({1:0.} FPS)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
