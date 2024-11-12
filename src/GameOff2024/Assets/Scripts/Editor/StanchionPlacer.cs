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
    private const float ROPE_LENGTH = 2f;

    [MenuItem("Tools/Stanchion Placer")]
    public static void ShowWindow()
    {
        GetWindow<StanchionPlacer>("Stanchion Placer");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        // You'll need to set these to your actual prefabs
        stanchionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Stanchion.prefab");
        ropePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rope.prefab");
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

        for (int i = 0; i < splinePoints.Count; i++)
        {
            // Place stanchion
            GameObject stanchion = PrefabUtility.InstantiatePrefab(stanchionPrefab) as GameObject;
            stanchion.transform.position = splinePoints[i];
            newObjects.Add(stanchion);
            Undo.RegisterCreatedObjectUndo(stanchion, "Place Stanchion");

            // Place rope between stanchions
            if (i < splinePoints.Count - 1)
            {
                GameObject rope = PrefabUtility.InstantiatePrefab(ropePrefab) as GameObject;
                Vector3 midPoint = (splinePoints[i] + splinePoints[i + 1]) / 2f;
                rope.transform.position = midPoint;
                
                // Orient rope between points
                Vector3 direction = splinePoints[i + 1] - splinePoints[i];
                rope.transform.rotation = Quaternion.LookRotation(direction);
                
                // Scale rope to fit distance
                float distance = Vector3.Distance(splinePoints[i], splinePoints[i + 1]);
                rope.transform.localScale = new Vector3(1, 1, distance);
                
                newObjects.Add(rope);
                Undo.RegisterCreatedObjectUndo(rope, "Place Rope");
            }
        }

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
