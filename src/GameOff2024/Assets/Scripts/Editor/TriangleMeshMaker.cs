using UnityEngine;
using UnityEditor;

public class TriangleMeshMaker : EditorWindow
{
    private float cylinderHeight = 1f;
    private int cylinderSegments = 8;

    [MenuItem("Tools/Triangle Mesh Maker")]
    static void Init()
    {
        var window = (TriangleMeshMaker)EditorWindow.GetWindow(typeof(TriangleMeshMaker));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Triangle Mesh Creator", EditorStyles.boldLabel);

        if (GUILayout.Button("Create 45-45-90 Triangle"))
        {
            Create4545Triangle();
        }

        if (GUILayout.Button("Create 60-60-60 Triangle")) 
        {
            Create606060Triangle();
        }

        GUILayout.Space(10);
        GUILayout.Label("Triangle Cylinder Creator", EditorStyles.boldLabel);
        
        cylinderHeight = EditorGUILayout.FloatField("Cylinder Height", cylinderHeight);
        cylinderSegments = EditorGUILayout.IntField("Segments", cylinderSegments);

        if (GUILayout.Button("Create 45-45-90 Triangle Cylinder"))
        {
            Create4545TriangleCylinder();
        }

        if (GUILayout.Button("Create 60-60-60 Triangle Cylinder"))
        {
            Create606060TriangleCylinder();
        }
    }

    void Create4545Triangle()
    {
        Mesh mesh = new Mesh();
        
        // For a 45-45-90 triangle with hypotenuse of 1
        float side = 0.707106781f; // 1/sqrt(2)
        
        Vector3[] vertices = new Vector3[3]
        {
            new Vector3(0, 0, 0),
            new Vector3(side, 0, 0),
            new Vector3(0, side, 0)
        };
        
        int[] triangles = new int[3] { 0, 1, 2 };
        
        Vector2[] uv = new Vector2[3]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        SaveMesh(mesh, "45-45-90_Triangle");
    }

    void Create606060Triangle()
    {
        Mesh mesh = new Mesh();
        
        float side = 1f;
        float height = side * Mathf.Sqrt(3) / 2f;
        
        Vector3[] vertices = new Vector3[3]
        {
            new Vector3(-side/2f, 0, 0),
            new Vector3(side/2f, 0, 0),
            new Vector3(0, height, 0)
        };
        
        int[] triangles = new int[3] { 0, 1, 2 };
        
        Vector2[] uv = new Vector2[3]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        SaveMesh(mesh, "60-60-60_Triangle");
    }

    void Create4545TriangleCylinder()
    {
        Mesh mesh = new Mesh();
        
        // For a 45-45-90 triangle with hypotenuse of 1
        float side = 0.707106781f; // 1/sqrt(2)
        
        Vector3[] vertices = new Vector3[6]
        {
            // Bottom triangle
            new Vector3(0, 0, 0),
            new Vector3(side, 0, 0),
            new Vector3(0, 0, side),
            // Top triangle
            new Vector3(0, cylinderHeight, 0),
            new Vector3(side, cylinderHeight, 0),
            new Vector3(0, cylinderHeight, side)
        };
        
        int[] triangles = new int[24] 
        { 
            // Bottom face
            0, 2, 1,
            // Top face
            3, 4, 5,
            // Side faces
            0, 1, 4, // First side
            0, 4, 3,
            1, 2, 5, // Second side
            1, 5, 4,
            2, 0, 3, // Third side
            2, 3, 5
        };
        
        Vector2[] uv = new Vector2[6]
        {
            // Bottom triangle
            new Vector2(0.75f, 0), // Bottom center
            new Vector2(1, 0.25f), // Bottom right
            new Vector2(0.5f, 0.25f), // Bottom left
            // Top triangle  
            new Vector2(0.75f, 1), // Top center
            new Vector2(1, 0.75f), // Top right
            new Vector2(0.5f, 0.75f) // Top left
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        SaveMesh(mesh, "45-45-90_Triangle_Cylinder");
    }

    void Create606060TriangleCylinder()
    {
        Mesh mesh = new Mesh();
        
        float side = 1f;
        float height = side * Mathf.Sqrt(3) / 2f;
        
        Vector3[] vertices = new Vector3[6]
        {
            // Bottom triangle
            new Vector3(-side/2f, 0, 0),
            new Vector3(side/2f, 0, 0),
            new Vector3(0, 0, height),
            // Top triangle
            new Vector3(-side/2f, cylinderHeight, 0),
            new Vector3(side/2f, cylinderHeight, 0),
            new Vector3(0, cylinderHeight, height)
        };
        
        int[] triangles = new int[24] 
        { 
            // Bottom face (facing down)
            0, 1, 2,
            // Top face (facing up) 
            3, 5, 4,
            // Side faces (facing outward)
            0, 3, 1, // First side
            1, 3, 4,
            1, 4, 2, // Second side 
            2, 4, 5,
            2, 5, 0, // Third side
            0, 5, 3
        };
        
        Vector2[] uv = new Vector2[6]
        {
            // Bottom triangle
            new Vector2(0.75f, 0), // Bottom center
            new Vector2(0.5f, 0.25f), // Bottom left
            new Vector2(1, 0.25f), // Bottom right  
            // Top triangle
            new Vector2(0.75f, 1), // Top center
            new Vector2(0.5f, 0.75f), // Top left
            new Vector2(1, 0.75f) // Top right
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        SaveMesh(mesh, "60-60-60_Triangle_Cylinder");
    }

    void SaveMesh(Mesh mesh, string name)
    {
        string path = EditorUtility.SaveFilePanel(
            "Save Triangle Mesh",
            "Assets/",
            name,
            "asset"
        );

        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);
        
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }
}
