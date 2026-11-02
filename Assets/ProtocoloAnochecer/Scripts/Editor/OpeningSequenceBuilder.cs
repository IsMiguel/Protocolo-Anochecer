using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class OpeningSequenceBuilder
{
    private const string ScenePath = "Assets/ProtocoloAnochecer/Scenes/OpeningPrototype.unity";
    private const string WhiteMaterialPath = "Assets/ProtocoloAnochecer/Art/Materials/Blockout_White.mat";
    private const string ScreenMaterialPath = "Assets/ProtocoloAnochecer/Art/Materials/Blockout_Screen.mat";
    private const string TerrainMeshAssetPath = "Assets/ProtocoloAnochecer/Art/Models/OpeningTerrain.asset";

    [MenuItem("Protocolo Anochecer/Crear introducción y primer mapa")]
    private static void CreateOpeningPrototype()
    {
        Material white = AssetDatabase.LoadAssetAtPath<Material>(WhiteMaterialPath);
        if (white == null)
        {
            Debug.LogError("No se encontró Blockout_White.mat. Esperá a que Unity importe los assets y volvé a intentarlo.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
            !EditorUtility.DisplayDialog(
                "Recrear prototipo de apertura",
                "La escena existente se reemplazará. El mesh de terreno anterior se conservará como asset separado.",
                "Recrear",
                "Cancelar"))
            return;

        Material screen = GetOrCreateScreenMaterial();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateTerrain(EuropeanFieldMaterialBuilder.GetOrCreate(white));
        CreateLighting();
        CreateTelevisionRoom(white, screen, out Text televisionText, out Transform televisionShot);
        CreateHelicopter(white, out Transform helicopter, out Transform rotor, out Transform cabinShot, out Transform flightShot);
        CreateLandingArea(white, out Transform playerStart, out Transform landingShot);
        Camera sequenceCamera = CreateCamera("Sequence Camera", new Vector3(0f, 2f, -7f));
        Camera playerCamera = CreateCamera("First Person Camera", Vector3.zero);
        playerCamera.enabled = false;
        FirstPersonController playerController = CreatePlayer(white, playerStart, playerCamera);
        CreateInterface(out Text subtitleText, out Text objectiveText, out CanvasGroup fadeGroup);

        GameObject sequenceObject = new GameObject("Opening Sequence");
        OpeningSequenceController sequence = sequenceObject.AddComponent<OpeningSequenceController>();
        SerializedObject serialized = new SerializedObject(sequence);
        SetObject(serialized, "sequenceCamera", sequenceCamera);
        SetObject(serialized, "playerCamera", playerCamera);
        SetObject(serialized, "playerController", playerController);
        SetObject(serialized, "televisionShot", televisionShot);
        SetObject(serialized, "cabinShot", cabinShot);
        SetObject(serialized, "flightShot", flightShot);
        SetObject(serialized, "landingShot", landingShot);
        SetObject(serialized, "helicopter", helicopter);
        SetObject(serialized, "rotor", rotor);
        SetObject(serialized, "playerStart", playerStart);
        SetObject(serialized, "televisionText", televisionText);
        SetObject(serialized, "subtitleText", subtitleText);
        SetObject(serialized, "objectiveText", objectiveText);
        SetObject(serialized, "fadeGroup", fadeGroup);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Introducción y primer mapa creados en {ScenePath}");
    }

    private static Material GetOrCreateScreenMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ScreenMaterialPath);
        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        material = new Material(shader) { name = "Blockout Screen" };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(0.025f, 0.03f, 0.035f));
        if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(0.025f, 0.03f, 0.035f));
        AssetDatabase.CreateAsset(material, ScreenMaterialPath);
        return material;
    }

    private static void CreateTerrain(Material material)
    {
        const int xCount = 65;
        const int zCount = 33;
        const float spacing = 2f;
        Vector3[] vertices = new Vector3[xCount * zCount];
        Vector2[] uvs = new Vector2[xCount * zCount];
        int[] triangles = new int[(xCount - 1) * (zCount - 1) * 6];

        for (int z = 0; z < zCount; z++)
        {
            for (int x = 0; x < xCount; x++)
            {
                float worldX = -16f + x * spacing;
                float worldZ = -30f + z * spacing;
                int vertexIndex = z * xCount + x;
                vertices[vertexIndex] = new Vector3(worldX, TerrainHeight(worldX, worldZ), worldZ);
                uvs[vertexIndex] = new Vector2(worldX / 8f, worldZ / 8f);
            }
        }

        int triangleIndex = 0;
        for (int z = 0; z < zCount - 1; z++)
        {
            for (int x = 0; x < xCount - 1; x++)
            {
                int lowerLeft = z * xCount + x;
                int lowerRight = lowerLeft + 1;
                int upperLeft = lowerLeft + xCount;
                int upperRight = upperLeft + 1;
                triangles[triangleIndex++] = lowerLeft;
                triangles[triangleIndex++] = upperLeft;
                triangles[triangleIndex++] = lowerRight;
                triangles[triangleIndex++] = lowerRight;
                triangles[triangleIndex++] = upperLeft;
                triangles[triangleIndex++] = upperRight;
            }
        }

        Mesh mesh = new Mesh { name = "Opening Terrain" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(TerrainMeshAssetPath);
        AssetDatabase.CreateAsset(mesh, meshAssetPath);
        GameObject terrain = new GameObject("European Landing Terrain");
        terrain.AddComponent<MeshFilter>().sharedMesh = mesh;
        terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
        terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private static float TerrainHeight(float x, float z)
    {
        const float landingCenterX = 82.5f;
        const float landingHalfWidth = 15.5f;
        const float landingHalfDepth = 12f;
        const float blendDistance = 8f;

        float distanceX = Mathf.Max(Mathf.Abs(x - landingCenterX) - landingHalfWidth, 0f);
        float distanceZ = Mathf.Max(Mathf.Abs(z) - landingHalfDepth, 0f);
        float distanceFromLanding = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);
        float blend = Mathf.SmoothStep(0f, 1f, distanceFromLanding / blendDistance);

        float landingHeight = RawTerrainHeight(80f, 0f);
        float naturalHeight = RawTerrainHeight(x, z);
        return Mathf.Lerp(landingHeight, naturalHeight, blend);
    }

    private static float RawTerrainHeight(float x, float z)
    {
        float hillA = 3f * Mathf.Exp(-((x - 99f) * (x - 99f) + (z - 18f) * (z - 18f)) / 150f);
        float hillB = 1.6f * Mathf.Exp(-((x - 52f) * (x - 52f) + (z - 24f) * (z - 24f)) / 100f);
        return hillA + hillB;
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Opening Directional Light", typeof(Light));
        lightObject.transform.rotation = Quaternion.Euler(50f, -32f, 0f);

        Light keyLight = lightObject.GetComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = new Color(1f, 0.94f, 0.84f);
        keyLight.intensity = 1.1f;
        keyLight.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.36f, 0.39f, 0.44f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.sun = keyLight;
    }

    private static void CreateTelevisionRoom(Material white, Material screen, out Text televisionText, out Transform cameraShot)
    {
        CreateCube("Briefing Room Floor", new Vector3(0f, -0.1f, 0f), new Vector3(16f, 0.2f, 14f), white);
        CreateCube("Briefing Room Back Wall", new Vector3(0f, 2f, 7f), new Vector3(16f, 4f, 0.2f), white);
        CreateCube("Briefing Room Left Wall", new Vector3(-8f, 2f, 0f), new Vector3(0.2f, 4f, 14f), white);
        CreateCube("Briefing Room Right Wall", new Vector3(8f, 2f, 0f), new Vector3(0.2f, 4f, 14f), white);
        CreateCube("CRT Television", new Vector3(0f, 1.5f, 1f), new Vector3(3.2f, 2.2f, 1f), white);
        CreateCube("Television Screen", new Vector3(0f, 1.5f, 0.48f), new Vector3(2.75f, 1.65f, 0.04f), screen);
        CreateCube("Television Stand", new Vector3(0f, 0.35f, 1.1f), new Vector3(4f, 0.5f, 1.8f), white);

        cameraShot = CreateShot("Television Shot", new Vector3(0f, 2.1f, -7f), new Vector3(0f, 1.5f, 0.5f));
        Canvas tvCanvas = new GameObject("VHS Presentation Display", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
        tvCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform tvRect = tvCanvas.GetComponent<RectTransform>();
        tvRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600f);
        tvRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 350f);
        tvRect.localScale = Vector3.one * 0.004f;
        tvRect.SetPositionAndRotation(new Vector3(0f, 1.5f, 0.455f), Quaternion.Euler(0f, 180f, 0f));

        Image background = tvCanvas.gameObject.AddComponent<Image>();
        background.color = new Color(0.025f, 0.03f, 0.035f);
        televisionText = CreateText("VHS Text", tvCanvas.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), 30, TextAnchor.MiddleCenter);
    }

    private static void CreateHelicopter(Material white, out Transform helicopter, out Transform rotor, out Transform cabinShot, out Transform flightShot)
    {
        GameObject helicopterObject = new GameObject("Helicopter Blockout");
        helicopter = helicopterObject.transform;
        helicopter.position = new Vector3(25f, 0f, 0f);
        CreateHelicopterCube(helicopter, "Cabin Floor", new Vector3(0f, 1f, 0f), new Vector3(8f, 0.2f, 3f), white);
        CreateHelicopterCube(helicopter, "Cabin Far Side", new Vector3(0f, 2f, 1.45f), new Vector3(8f, 1.8f, 0.15f), white);
        CreateHelicopterCube(helicopter, "Cabin Roof", new Vector3(-1f, 3f, 0f), new Vector3(5f, 0.2f, 3f), white);
        CreateHelicopterCube(helicopter, "Cockpit Nose", new Vector3(4f, 1.7f, 0f), new Vector3(1.6f, 1.4f, 2f), white);
        CreateHelicopterCube(helicopter, "Tail Boom", new Vector3(6.5f, 2f, 0f), new Vector3(5f, 0.35f, 0.4f), white);
        CreateHelicopterCube(helicopter, "Left Landing Skid", new Vector3(0f, 0.25f, -1.6f), new Vector3(8f, 0.15f, 0.15f), white);
        CreateHelicopterCube(helicopter, "Right Landing Skid", new Vector3(0f, 0.25f, 1.6f), new Vector3(8f, 0.15f, 0.15f), white);
        GameObject rotorObject = CreateHelicopterCube(helicopter, "Main Rotor", new Vector3(0f, 3.35f, 0f), new Vector3(0.2f, 0.1f, 8f), white);
        rotor = rotorObject.transform;
        CreateCapsule("Compañero (provisional)", helicopter, new Vector3(-1f, 1f, 0.25f), white);
        CreateCapsule("Protagonista (provisional)", helicopter, new Vector3(1f, 1f, -0.55f), white);

        cabinShot = CreateShot("Helicopter Cabin Shot", new Vector3(25f, 2.2f, -6f), new Vector3(25f, 1.8f, 0f));
        GameObject flightShotObject = new GameObject("Helicopter Flight Shot");
        flightShot = flightShotObject.transform;
        flightShot.SetParent(helicopter, false);
        flightShot.localPosition = new Vector3(2f, 2.5f, -0.1f);
        flightShot.localRotation = Quaternion.Euler(5f, 90f, 0f);
        helicopterObject.SetActive(false);
    }

    private static GameObject CreateHelicopterCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
    {
        GameObject part = CreateCube(name, parent.position + localPosition, scale, material);
        part.transform.SetParent(parent, true);
        return part;
    }

    private static void CreateCapsule(string name, Transform parent, Vector3 localPosition, Material material)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = name;
        capsule.transform.SetParent(parent, false);
        capsule.transform.localPosition = localPosition;
        capsule.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        Object.DestroyImmediate(capsule.GetComponent<Collider>());
        capsule.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
    private static void CreateLandingArea(Material white, out Transform playerStart, out Transform landingShot)
    {
        float groundHeight = TerrainHeight(80f, 0f);
        CreateCube("Landing Pad", new Vector3(80f, groundHeight - 0.1f, 0f), new Vector3(14f, 0.2f, 14f), white);
        CreateCube("Low Concrete Wall A", new Vector3(89f, groundHeight + 1f, 6f), new Vector3(0.4f, 2f, 6f), white);
        CreateCube("Low Concrete Wall B", new Vector3(94f, groundHeight + 1f, 3f), new Vector3(7f, 2f, 0.4f), white);
        CreateCube("Training Step A", new Vector3(83.5f, groundHeight + 0.1f, 9f), new Vector3(2f, 0.2f, 2f), white);
        CreateCube("Training Step B", new Vector3(85.5f, groundHeight + 0.3f, 9f), new Vector3(2f, 0.2f, 2f), white);
        CreateCube("Training Step C", new Vector3(87.5f, groundHeight + 0.5f, 9f), new Vector3(2f, 0.2f, 2f), white);

        GameObject startObject = new GameObject("Player Start");
        playerStart = startObject.transform;
        playerStart.position = new Vector3(80f, groundHeight + 0.05f, 0f);
        playerStart.rotation = Quaternion.LookRotation(Vector3.forward);
        landingShot = CreateShot("Landing Shot", new Vector3(76f, groundHeight + 3f, -10f), new Vector3(80f, groundHeight + 1f, 0f));
    }

    private static FirstPersonController CreatePlayer(Material white, Transform start, Camera camera)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetPositionAndRotation(start.position, start.rotation);
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 0.9f, 0f);
        characterController.slopeLimit = 45f;
        characterController.stepOffset = 0.3f;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Player Visual (Scene View Only)";
        body.transform.SetParent(player.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        body.layer = 2;
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<MeshRenderer>().sharedMaterial = white;

        camera.transform.SetParent(player.transform, false);
        camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        camera.transform.localRotation = Quaternion.identity;
        camera.cullingMask &= ~(1 << 2);
        FirstPersonController controller = player.AddComponent<FirstPersonController>();
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("viewCamera").objectReferenceValue = camera;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return controller;
    }

    private static Camera CreateCamera(string name, Vector3 position)
    {
        GameObject cameraObject = new GameObject(name);
        cameraObject.transform.position = position;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 75f;
        camera.nearClipPlane = 0.05f;
        camera.backgroundColor = new Color(0.75f, 0.82f, 0.9f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        return camera;
    }

    private static void CreateInterface(out Text subtitle, out Text objective, out CanvasGroup fadeGroup)
    {
        GameObject canvasObject = new GameObject("Opening UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject subtitlePanel = new GameObject("Subtitle Panel", typeof(RectTransform), typeof(Image));
        subtitlePanel.transform.SetParent(canvasObject.transform, false);
        RectTransform subtitlePanelRect = subtitlePanel.GetComponent<RectTransform>();
        subtitlePanelRect.anchorMin = new Vector2(0.05f, 0.04f);
        subtitlePanelRect.anchorMax = new Vector2(0.95f, 0.19f);
        subtitlePanelRect.offsetMin = Vector2.zero;
        subtitlePanelRect.offsetMax = Vector2.zero;
        subtitlePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        subtitle = CreateText("Subtitle", subtitlePanel.transform, Vector2.zero, Vector2.one, 28, TextAnchor.MiddleCenter);
        subtitle.text = string.Empty;

        objective = CreateText("Objective", canvasObject.transform, new Vector2(0.04f, 0.78f), new Vector2(0.35f, 0.94f), 28, TextAnchor.UpperLeft);
        objective.text = string.Empty;

        GameObject fadeObject = new GameObject("Fade", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        fadeObject.transform.SetParent(canvasObject.transform, false);
        RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        fadeObject.GetComponent<Image>().color = Color.black;
        fadeGroup = fadeObject.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(24f, 16f);
        rect.offsetMax = new Vector2(-24f, -16f);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Transform CreateShot(string name, Vector3 position, Vector3 target)
    {
        GameObject shot = new GameObject(name);
        shot.transform.position = position;
        shot.transform.rotation = Quaternion.LookRotation(target - position);
        return shot.transform;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = material;
        return cube;
    }

    private static void SetObject(SerializedObject serialized, string propertyName, Object value)
    {
        serialized.FindProperty(propertyName).objectReferenceValue = value;
    }
}
