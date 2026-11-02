using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BlockoutSceneBuilder
{
    private const string ScenePath = "Assets/ProtocoloAnochecer/Scenes/BlockoutPrototype.unity";
    private const string MaterialPath = "Assets/ProtocoloAnochecer/Art/Materials/Blockout_White.mat";
    private const string TerrainMeshAssetPath = "Assets/ProtocoloAnochecer/Art/Models/BlockoutTerrain.asset";

    [MenuItem("Protocolo Anochecer/Crear prototipo FPS")]
    private static void CreatePrototype()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
            !EditorUtility.DisplayDialog(
                "Recrear prototipo FPS",
                "La escena existente se reemplazará. El mesh de terreno anterior se conservará como asset separado.",
                "Recrear",
                "Cancelar"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Material whiteMaterial = GetOrCreateWhiteMaterial();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject environment = new GameObject("Blockout Environment");
        Material groundMaterial = EuropeanFieldMaterialBuilder.GetOrCreate(whiteMaterial);
        CreateTerrain(environment.transform, groundMaterial);
        CreateBuilding(environment.transform, whiteMaterial, new Vector3(-9f, 0f, -7f));
        CreateObstacles(environment.transform, whiteMaterial);
        CreateLighting();
        CreatePlayer(whiteMaterial);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Prototipo FPS creado en {ScenePath}");
    }

    private static Material GetOrCreateWhiteMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        material = new Material(shader) { name = "Blockout White" };
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", Color.white);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0f);

        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }

    private static void CreateTerrain(Transform parent, Material material)
    {
        const int resolution = 65;
        const float halfSize = 32f;
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = x - halfSize;
                float worldZ = z - halfSize;
                int vertexIndex = z * resolution + x;
                vertices[vertexIndex] = new Vector3(worldX, TerrainHeight(worldX, worldZ), worldZ);
                uvs[vertexIndex] = new Vector2(worldX / 6f, worldZ / 6f);
            }
        }

        int triangleIndex = 0;
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int bottomLeft = z * resolution + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + resolution;
                int topRight = topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;
            }
        }

        Mesh terrainMesh = new Mesh { name = "Blockout Terrain Mesh" };
        terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.uv = uvs;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateTangents();
        terrainMesh.RecalculateBounds();
        string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(TerrainMeshAssetPath);
        AssetDatabase.CreateAsset(terrainMesh, meshAssetPath);

        GameObject terrain = new GameObject("Uneven Terrain");
        terrain.transform.SetParent(parent);
        terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
        terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
        terrain.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
    }

    private static float TerrainHeight(float x, float z)
    {
        float firstHill = 3.2f * Mathf.Exp(-((x - 17f) * (x - 17f) + (z - 17f) * (z - 17f)) / 90f);
        float secondHill = 2.2f * Mathf.Exp(-((x + 19f) * (x + 19f) + (z - 2f) * (z - 2f)) / 65f);
        float ridge = 1.4f * Mathf.Exp(-((x - 3f) * (x - 3f)) / 24f) * Mathf.Exp(-((z - 22f) * (z - 22f)) / 120f);
        return firstHill + secondHill + ridge;
    }

    private static void CreateBuilding(Transform parent, Material material, Vector3 center)
    {
        float floorHeight = TerrainHeight(center.x, center.z);
        CreateCube("Structure Floor", center + Vector3.down * 0.1f, new Vector3(12f, 0.2f, 10f), material, parent);
        CreateCube("Back Wall", center + new Vector3(0f, 1.7f, 5f), new Vector3(12f, 3.4f, 0.2f), material, parent);
        CreateCube("Left Wall", center + new Vector3(-6f, 1.7f, 0f), new Vector3(0.2f, 3.4f, 10f), material, parent);
        CreateCube("Right Wall", center + new Vector3(6f, 1.7f, 0f), new Vector3(0.2f, 3.4f, 10f), material, parent);
        CreateCube("Front Wall Left", center + new Vector3(-4.4f, 1.7f, -5f), new Vector3(3.2f, 3.4f, 0.2f), material, parent);
        CreateCube("Front Wall Right", center + new Vector3(4.4f, 1.7f, -5f), new Vector3(3.2f, 3.4f, 0.2f), material, parent);
        CreateCube("Door Header", center + new Vector3(0f, 3.05f, -5f), new Vector3(5.6f, 0.7f, 0.2f), material, parent);
        CreateCube("Raised Interior Platform", center + new Vector3(0f, floorHeight + 0.2f, 0f), new Vector3(3f, 0.4f, 3f), material, parent);
    }

    private static void CreateObstacles(Transform parent, Material material)
    {
        CreateCube("Low Block", new Vector3(4f, 0.75f, 3f), new Vector3(2f, 1.5f, 2f), material, parent);
        CreateCube("Long Wall", new Vector3(8f, 1.25f, -2f), new Vector3(0.5f, 2.5f, 8f), material, parent);
        CreateCube("Step 1", new Vector3(5f, 0.2f, 8f), new Vector3(2f, 0.4f, 2f), material, parent);
        CreateCube("Step 2", new Vector3(7f, 0.6f, 8f), new Vector3(2f, 1.2f, 2f), material, parent);
        CreateCube("Step 3", new Vector3(9f, 1f, 8f), new Vector3(2f, 2f, 2f), material, parent);
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Blockout Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
    }

    private static void CreatePlayer(Material material)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, TerrainHeight(0f, -2f) + 0.05f, -2f);

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 0.9f, 0f);
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 45f;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Player Visual (Scene View Only)";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visual.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        visual.layer = 2;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial = material;

        GameObject cameraObject = new GameObject("First Person Camera");
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 75f;
        camera.nearClipPlane = 0.05f;
        camera.cullingMask &= ~(1 << 2);
        cameraObject.AddComponent<AudioListener>();

        FirstPersonController controller = player.AddComponent<FirstPersonController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("viewCamera").objectReferenceValue = camera;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
}
