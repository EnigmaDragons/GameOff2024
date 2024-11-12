using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PeopleLinePlacer : EditorWindow
{
    private GameObject prefabToPlace;
    private float spacing = 1f;
    private bool isDrawing = false;
    private Vector3 startPoint;
    private List<Vector3> splinePoints = new List<Vector3>();
    private List<GameObject> placedObjects = new List<GameObject>();
    private LineRenderer splineMesh;

    [MenuItem("Tools/People Line Placer")]
    static void Init()
    {
        PeopleLinePlacer window = (PeopleLinePlacer)EditorWindow.GetWindow(typeof(PeopleLinePlacer));
        window.Show();
    }

    void OnGUI()
    {
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Prefab to Place", prefabToPlace, typeof(GameObject), false);
        spacing = EditorGUILayout.FloatField("Spacing Distance", spacing);

        if (GUILayout.Button("Clear All"))
        {
            ClearPlacedObjects();
        }

        if (GUILayout.Button("Undo Last"))
        {
            UndoLastPlacement();
        }

        EditorGUILayout.HelpBox("Click and hold in scene view to start drawing.\nRelease to finish and place objects.", MessageType.Info);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                isDrawing = true;
                startPoint = hit.point;
                splinePoints.Clear();
                splinePoints.Add(startPoint);

                // Create spline mesh object
                GameObject splineObj = new GameObject("SplineMesh");
                splineMesh = splineObj.AddComponent<LineRenderer>();
                splineMesh.startWidth = 0.1f;
                splineMesh.endWidth = 0.1f;
                splineMesh.material = new Material(Shader.Find("Sprites/Default"));
                splineMesh.startColor = Color.blue;
                splineMesh.endColor = Color.blue;
            }
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isDrawing)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (Vector3.Distance(splinePoints[splinePoints.Count - 1], hit.point) >= spacing)
                {
                    splinePoints.Add(hit.point);
                    splineMesh.positionCount = splinePoints.Count;
                    splineMesh.SetPositions(splinePoints.ToArray());
                    SceneView.RepaintAll();
                }
            }
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0 && isDrawing)
        {
            isDrawing = false;
            PlaceObjects();
            DestroyImmediate(splineMesh.gameObject);
            e.Use();
        }

        if (isDrawing)
        {
            Handles.color = Color.blue;
            for (int i = 0; i < splinePoints.Count - 1; i++)
            {
                Handles.DrawLine(splinePoints[i], splinePoints[i + 1]);
            }
        }
    }

    private void PlaceObjects()
    {
        if (prefabToPlace == null || splinePoints.Count < 2) return;

        Undo.RecordObject(this, "Place People Line");
        List<GameObject> newObjects = new List<GameObject>();

        for (int i = 0; i < splinePoints.Count; i++)
        {
            GameObject obj = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;
            obj.transform.position = splinePoints[i];
            obj.transform.localScale = Vector3.one; // Ensure scale is 1,1,1
            
            // Rotate to face next point in line
            if (i < splinePoints.Count - 1)
            {
                Vector3 directionToNext = splinePoints[i + 1] - obj.transform.position;
                directionToNext.y = 0; // Keep rotation only around Y axis
                if (directionToNext != Vector3.zero)
                {
                    obj.transform.rotation = Quaternion.LookRotation(directionToNext);
                }
            }
            else if (i > 0)
            {
                // Last object faces same direction as previous one
                obj.transform.rotation = newObjects[i - 1].transform.rotation;
            }
            
            newObjects.Add(obj);
            Undo.RegisterCreatedObjectUndo(obj, "Place Person");
        }

        placedObjects.AddRange(newObjects);
    }

    private void UndoLastPlacement()
    {
        if (placedObjects.Count == 0) return;

        Undo.RecordObject(this, "Undo People Placement");

        // Find the last group of objects placed (all objects from the last line drawing)
        int lastIndex = placedObjects.Count - 1;
        Vector3 lastPosition = placedObjects[lastIndex].transform.position;
        
        // Remove all objects that were placed in the same position.y (same line)
        for (int i = placedObjects.Count - 1; i >= 0; i--)
        {
            if (Mathf.Approximately(placedObjects[i].transform.position.y, lastPosition.y))
            {
                GameObject obj = placedObjects[i];
                Undo.DestroyObjectImmediate(obj);
                placedObjects.RemoveAt(i);
            }
            else
            {
                break;
            }
        }
    }

    private void ClearPlacedObjects()
    {
        Undo.RecordObject(this, "Clear People");
        
        foreach (GameObject obj in placedObjects)
        {
            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
        placedObjects.Clear();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}
