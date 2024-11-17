using UnityEngine;

public class RuntimeBillboard : MonoBehaviour
{
    [SerializeField] private float billboardDistance = 50f; // Distance to switch to billboard
    [SerializeField] private SkinnedNpc npc;
    
    private Camera mainCam;
    private SkinnedMeshRenderer characterMesh;
    private SpriteRenderer billboardSprite;
    private bool isBillboard = false;
    private bool isInitialized;

    void Start()
    {
        mainCam = Camera.main;
        
        // Create billboard sprite
        GameObject billboardObj = new GameObject("Billboard");
        billboardObj.transform.SetParent(transform);
        billboardSprite = billboardObj.AddComponent<SpriteRenderer>();
        
        // Create runtime texture from mesh
        RenderTexture rt = new RenderTexture(256, 256, 24);
        Camera.main.targetTexture = rt;
        Texture2D billboard = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        Camera.main.Render();
        RenderTexture.active = rt;
        billboard.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        billboard.Apply();
        
        // Create sprite from texture
        billboardSprite.sprite = Sprite.Create(billboard, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        billboardSprite.enabled = false;
        
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
    }

    void Update()
    {
        if (!isInitialized)
        {
            characterMesh = npc.Renderer;
            if (characterMesh != null)
                isInitialized = true;
            else
                return;
        }
        
        float distanceToCamera = Vector3.Distance(mainCam.transform.position, transform.position);
        
        if (distanceToCamera > billboardDistance && !isBillboard)
        {
            // Switch to billboard
            characterMesh.enabled = false;
            billboardSprite.enabled = true;
            isBillboard = true;
        }
        else if (distanceToCamera <= billboardDistance && isBillboard)
        {
            // Switch back to 3D mesh
            characterMesh.enabled = true;
            billboardSprite.enabled = false;
            isBillboard = false;
        }

        if (isBillboard)
        {
            // Make billboard face camera
            billboardSprite.transform.LookAt(mainCam.transform.position);
            billboardSprite.transform.Rotate(0, 180, 0);
        }
    }
}
