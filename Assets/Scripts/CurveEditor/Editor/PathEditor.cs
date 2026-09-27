using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor {

    PathCreator creator;
    PointPath Path
    {
        get
        {
            return creator.path;
        }
    }

    const float segmentSelectDistanceThreshold = .5f;
    int selectedSegmentIndex = -1;
    int selectedPointIndex = -1; // Tracks which vertex point index is currently active

    // Cached so we can detect the owning GameObject being moved and shift
    // every path point by the same delta, keeping the path attached to it.
    Vector3 cachedTransformPosition;
    bool transformCacheInitialized = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        creator = (PathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(creator, "Create new");
            creator.CreatePath();
            selectedPointIndex = -1; // Reset selection index safely
            transformCacheInitialized = false; // Re-sync with the new path's points
        }

        bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed");
        if (isClosed != Path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle closed");
            Path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != Path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            Path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        if (creator == null) creator = (PathCreator)target;
        if (Path == null) return;

        FollowTransform();
        Input();
        Draw();
    }

    // Keeps the path glued to its GameObject: if the transform has moved
    // since last time we checked, shift every point by the same delta.
    void FollowTransform()
    {
        Vector3 currentPosition = creator.transform.position;

        if (!transformCacheInitialized)
        {
            cachedTransformPosition = currentPosition;
            transformCacheInitialized = true;
            return;
        }

        Vector3 delta = currentPosition - cachedTransformPosition;
        if (delta != Vector3.zero)
        {
            Undo.RecordObject(creator, "Move path with transform");
            Path.TranslatePoints(delta);
            cachedTransformPosition = currentPosition;
        }
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        // Dynamic alignment plane built through the root gameobject position matrix
        Plane spacePlane = new Plane(Vector3.up, creator.transform.position);

        Vector3 mousePos = Vector3.zero;
        if (spacePlane.Raycast(mouseRay, out float enterDistance))
        {
            mousePos = mouseRay.GetPoint(enterDistance);
        }
        else
        {
            mousePos = mouseRay.origin + mouseRay.direction * 10f;
        }

        // Shift + left click: add a new segment, or split the hovered one
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creator, "Split segment");
                Path.SplitSegment(mousePos, selectedSegmentIndex);
            }
            else if (!Path.IsClosed)
            {
                Undo.RecordObject(creator, "Add segment");
                Path.AddSegment(mousePos);
            }
        }

        // Right click: delete the segment belonging to the nearest anchor
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDstToAnchor = PathCreator.anchorDiameter * .5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < Path.NumPoints; i += 3)
            {
                float dst = Vector3.Distance(mousePos, Path[i]);
                if (dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(creator, "Delete segment");
                Path.DeleteSegment(closestAnchorIndex);
                if (selectedPointIndex == closestAnchorIndex)
                {
                    selectedPointIndex = -1;
                }
            }
        }

        // Track which segment (bezier curve) the mouse is currently nearest to,
        // used above for shift-click splitting.
        if (guiEvent.type == EventType.MouseMove)
        {
            float minDstToSegment = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < Path.NumSegments; i++)
            {
                Vector3[] points = Path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minDstToSegment)
                {
                    minDstToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }

    void Draw()
    {
        // Draw the curve segments
        for (int i = 0; i < Path.NumSegments; i++)
        {
            Vector3[] points = Path.GetPointsInSegment(i);
            Color segmentColor = (i == selectedSegmentIndex) ? Color.yellow : Color.white;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2f);
        }

        // Draw each point as a clickable handle so it can be selected
        for (int i = 0; i < Path.NumPoints; i++)
        {
            bool isAnchor = (i % 3 == 0);

            // Skip control points entirely if the user has hidden them
            if (!isAnchor && !creator.displayControlPoints)
            {
                continue;
            }

            float handleSize = isAnchor ? PathCreator.anchorDiameter : PathCreator.controlDiameter;
            Color handleColor = (i == selectedPointIndex)
                ? Color.yellow
                : (isAnchor ? Color.red : Color.white);

            Handles.color = handleColor;
            if (Handles.Button(Path[i], Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
            {
                selectedPointIndex = i;
                Repaint();
            }
        }

        // Draw a transform gizmo for whichever point is currently selected.
        // This runs every OnSceneGUI call (not just on click), so it stays
        // visible and draggable until a different point is selected.
        if (selectedPointIndex >= 0 && selectedPointIndex < Path.NumPoints)
        {
            bool isAnchor = (selectedPointIndex % 3 == 0);
            if (isAnchor || creator.displayControlPoints)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(Path[selectedPointIndex], Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(creator, "Move point");
                    Path.MovePoint(selectedPointIndex, newPos);
                }
            }
        }
    }

    void OnEnable()
    {
        creator = (PathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
        transformCacheInitialized = false;
    }
}