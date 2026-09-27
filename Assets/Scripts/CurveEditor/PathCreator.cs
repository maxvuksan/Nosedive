using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour {

    [HideInInspector]
    public PointPath path;

	public static Color anchorCol = Color.red;
    public static Color controlCol = Color.white;
    public static Color segmentCol = Color.green;
    public static Color selectedSegmentCol = Color.yellow;
    public static float anchorDiameter = 1f;
    public static float controlDiameter = 1f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new PointPath(transform.position);
    }

    void Reset()
    {
        CreatePath();
    }
}
