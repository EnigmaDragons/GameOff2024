using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class StanchionPlacer : EditorWindow
{
    private List<Vector3> splinePoints = new List<Vector3>();
    private bool isDrawing = false;
    private GameObject stanchionPrefab;
    private GameObject ropePrefab;
    private List<GameObject> placedObjects = new List<GameObject>();
    private const float ROPE_LENGTH = 3.6f;

    [MenuItem("Tools/Stanchion Placer")]
    public static void ShowWindow()
    {
        GetWindow<StanchionPlacer>("Stanchion Placer");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        // You'll need to set these to your actual prefabs
        stanchionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Obs_Pieces/Stanchion.prefab");
        ropePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Obs_Pieces/Stanchion_Rope_Pos.prefab");
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Click and drag in scene view to place stanchions");
        
        if (GUILayout.Button("Undo Last Placement"))
        {
            UndoLastPlacement();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isDrawing = true;
                splinePoints.Clear();
                splinePoints.Add(hit.point);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDrawing)
            {
                // Add points based on ROPE_LENGTH intervals
                if (splinePoints.Count > 0)
                {
                    Vector3 lastPoint = splinePoints[splinePoints.Count - 1];
                    Vector3 currentPoint = hit.point;
                    float distance = Vector3.Distance(lastPoint, currentPoint);
                    
                    if (distance >= ROPE_LENGTH)
                    {
                        splinePoints.Add(currentPoint);
                        // Draw green circle at completion point
                        Handles.color = Color.green;
                        Handles.DrawWireDisc(currentPoint, Vector3.up, 0.5f);
                        Handles.DrawSolidDisc(currentPoint, Vector3.up, 0.4f);
                    }
                }
                SceneView.RepaintAll();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isDrawing)
            {
                PlaceStanchionsAndRopes();
                isDrawing = false;
                e.Use();
            }
        }

        if (isDrawing)
        {
            Handles.color = Color.yellow;
            for (int i = 0; i < splinePoints.Count - 1; i++)
            {
                Handles.DrawLine(splinePoints[i], splinePoints[i + 1]);
            }
            
            if (splinePoints.Count > 0)
            {
                Handles.DrawLine(splinePoints[splinePoints.Count - 1], hit.point);
            }
        }
    }

    private void PlaceStanchionsAndRopes()
    {
        if (splinePoints.Count < 2) return;

        Undo.RecordObject(this, "Place Stanchions and Ropes");
        List<GameObject> newObjects = new List<GameObject>();

        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            // Place stanchion at start of rope segment
            GameObject stanchionStart = PrefabUtility.InstantiatePrefab(stanchionPrefab) as GameObject;
            stanchionStart.transform.position = splinePoints[i];
            newObjects.Add(stanchionStart);
            Undo.RegisterCreatedObjectUndo(stanchionStart, "Place Stanchion");

            // Place rope
            GameObject rope = PrefabUtility.InstantiatePrefab(ropePrefab) as GameObject;
            Vector3 midPoint = (splinePoints[i] + splinePoints[i + 1]) / 2f;
            rope.transform.position = midPoint;
            
            // Orient rope between points
            Vector3 direction = splinePoints[i + 1] - splinePoints[i];
            rope.transform.rotation = Quaternion.LookRotation(direction);
            
            newObjects.Add(rope);
            Undo.RegisterCreatedObjectUndo(rope, "Place Rope");
        }

        // Place final stanchion at end of last rope
        GameObject stanchionEnd = PrefabUtility.InstantiatePrefab(stanchionPrefab) as GameObject;
        stanchionEnd.transform.position = splinePoints[splinePoints.Count - 1];
        newObjects.Add(stanchionEnd);
        Undo.RegisterCreatedObjectUndo(stanchionEnd, "Place Stanchion");

        placedObjects.AddRange(newObjects);
    }

    private void UndoLastPlacement()
    {
        if (placedObjects.Count == 0) return;

        Undo.RecordObject(this, "Undo Stanchion Placement");
        
        // Remove last set of placed objects
        int objectsToRemove = placedObjects.Count >= 2 ? 2 : 1;
        for (int i = 0; i < objectsToRemove; i++)
        {
            if (placedObjects.Count > 0)
            {
                GameObject obj = placedObjects[placedObjects.Count - 1];
                Undo.DestroyObjectImmediate(obj);
                placedObjects.RemoveAt(placedObjects.Count - 1);
            }
        }
    }
}
