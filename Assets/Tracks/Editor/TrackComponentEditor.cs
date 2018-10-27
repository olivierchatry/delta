using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackComponent))]
public class TrackComponentEditor : Editor
{
    int hotIndex = -1;
    int removeIndex = -1;

    void ResetIndex()
    {
        var spline = target as TrackComponent;
        spline.ResetIndex();        
    }

    void OnSceneGUI()
    {
        var spline = target as TrackComponent;


        var e = Event.current;
        GUIUtility.GetControlID(FocusType.Passive);


        var mousePos = (Vector2)Event.current.mousePosition;
        var view = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);
        var mouseIsOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
        if (mouseIsOutside) return;

        var points = serializedObject.FindProperty("points");
        if (Event.current.control)
        {
            ShowClosestPointOnClosedSpline(points);
        }
        for (int i = 0; i < spline.points.Count; i++)
        {
            var prop = points.GetArrayElementAtIndex(i);
            var point = prop.vector3Value;
            var wp = spline.transform.TransformPoint(point);
            if (hotIndex == i)
            {
                var newWp = Handles.PositionHandle(wp, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : spline.transform.rotation);
                var delta = spline.transform.InverseTransformDirection(newWp - wp);
                if (delta.sqrMagnitude > 0)
                {
                    prop.vector3Value = point + delta;
                    ResetIndex();
                }
                HandleCommands(wp);
            }
            Handles.color = Color.red;
            var buttonSize = HandleUtility.GetHandleSize(wp) * 0.1f;
            if (Handles.Button(wp, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap))
                hotIndex = i;
            var v = SceneView.currentDrawingSceneView.camera.transform.InverseTransformPoint(wp);
            var labelIsOutside = v.z < 0;
            if (!labelIsOutside) Handles.Label(wp, i.ToString());
        }
        if (removeIndex >= 0 && points.arraySize > 4)
        {
            points.DeleteArrayElementAtIndex(removeIndex);
            ResetIndex();
        }
        removeIndex = -1;
        serializedObject.ApplyModifiedProperties();

        // RaycastHit hit;
        //if (!Physics.Raycast(new Ray(start, up), out hit))
        //    return;

    }

    void HandleCommands(Vector3 wp)
    {
        if (Event.current.type == EventType.ExecuteCommand)
        {
            if (Event.current.commandName == "FrameSelected")
            {
                SceneView.currentDrawingSceneView.Frame(new Bounds(wp, Vector3.one * 10), false);
                Event.current.Use();
            }
        }
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                removeIndex = hotIndex;
                Event.current.Use();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
        var spline = target as SplineComponent;        

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Flatten Y Axis"))
        {
            Undo.RecordObject(target, "Flatten Y Axis");
            Flatten(spline.points);
            ResetIndex();
        }
        if (GUILayout.Button("Center around Origin"))
        {
            Undo.RecordObject(target, "Center around Origin");
            CenterAroundOrigin(spline.points);
            ResetIndex();
        }
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
    }

    //[DrawGizmo(GizmoType.NonSelected)]
    //static void DrawGizmosLoRes(SplineComponent spline, GizmoType gizmoType)
    //{
    //    Gizmos.color = Color.white;
    //    DrawGizmo(spline, 64);
    //}

    [DrawGizmo(GizmoType.Selected)]
    static void DrawGizmosHiRes(SplineComponent spline, GizmoType gizmoType)
    {        
        var start =  spline.transform.TransformPoint(spline.GetPoint(0));
        var forward = spline.GetForward(0);
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Vector3 up = matrix.GetColumn(1);
        var position = start + up * 100;

        Gizmos.DrawRay(start, up);

        //Gizmos.color = Color.white;
        //DrawGizmo(spline, 1024);
    }

    void ShowClosestPointOnClosedSpline(SerializedProperty points)
    {
        var spline = target as SplineComponent;
        var plane = new Plane(spline.transform.up, spline.transform.position);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float center;
        if (plane.Raycast(ray, out center))
        {
            var hit = ray.origin + ray.direction * center;
            Handles.DrawWireDisc(hit, spline.transform.up, 5);
            var p = SearchForClosestPoint(Event.current.mousePosition);            
            var sp = spline.GetNonUniformPoint(p);
            Handles.DrawLine(hit, spline.transform.TransformPoint(sp));


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control)
            {
                var i = (Mathf.FloorToInt(p * spline.points.Count) + 2) % spline.points.Count;
                points.InsertArrayElementAtIndex(i);
                points.GetArrayElementAtIndex(i).vector3Value = sp;
                serializedObject.ApplyModifiedProperties();
                hotIndex = i;
            }
        }
    }

    float SearchForClosestPoint(Vector2 screenPoint, float A = 0f, float B = 1f, float steps = 1000)
    {
        var spline = target as SplineComponent;
        var smallestDelta = float.MaxValue;
        var step = (B - A) / steps;
        var closestI = A;
        for (var i = 0; i <= steps; i++)
        {
            var p = spline.transform.TransformPoint(spline.GetNonUniformPoint(i * step));
            var gp = HandleUtility.WorldToGUIPoint(p);
            var delta = (screenPoint - gp).sqrMagnitude;
            if (delta < smallestDelta)
            {
                closestI = i;
                smallestDelta = delta;
            }
        }
        return closestI * step;
    }

    void Flatten(List<Vector3> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = Vector3.Scale(points[i], new Vector3(1, 0, 1));
        }
    }


    void CenterAroundOrigin(List<Vector3> points)
    {
        var center = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            center += points[i];
        }
        center /= points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            points[i] -= center;
        }
    }

}