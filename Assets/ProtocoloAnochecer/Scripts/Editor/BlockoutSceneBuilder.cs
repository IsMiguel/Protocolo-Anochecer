using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Genera una misión de exploración y combate experimental.</summary>
public static class BlockoutSceneBuilder
{
    private const string ScenePath = "Assets/ProtocoloAnochecer/Scenes/BlockoutPrototype.unity";
    private const string MaterialFolder = "Assets/ProtocoloAnochecer/Art/Materials";
    private const string MaterialPath = MaterialFolder + "/Blockout_White.mat";
    private const string TerrainMeshAssetPath = "Assets/ProtocoloAnochecer/Art/Models/BlockoutTerrain.asset";
    private const string RifleModelPath = "Assets/ProtocoloAnochecer/Art/Models/Weapons/Mauser_1909_Prototype.fbx";
    private const string PistolModelPath = "Assets/ProtocoloAnochecer/Art/Models/Weapons/Colt_M1911A1_Prototype.fbx";

    private const float MapHalfSize = 64f;
    private const int TerrainResolution = 129;

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

        Material white = GetOrCreateWhiteMaterial();
        Material dirt = GetOrCreateMaterial(MaterialFolder + "/Field_Dirt.mat", new Color(0.28f, 0.24f, 0.18f));
        Material foliage = GetOrCreateMaterial(MaterialFolder + "/Field_Foliage.mat", new Color(0.15f, 0.20f, 0.13f));
        Material wood = GetOrCreateMaterial(MaterialFolder + "/Field_Wood.mat", new Color(0.24f, 0.16f, 0.09f));
        Material sandbag = GetOrCreateMaterial(MaterialFolder + "/Field_Sandbag.mat", new Color(0.39f, 0.35f, 0.27f));
        Material uniform = GetOrCreateMaterial(MaterialFolder + "/Field_Uniform.mat", new Color(0.18f, 0.22f, 0.16f));
        Material gloves = GetOrCreateMaterial(MaterialFolder + "/Field_Gloves.mat", new Color(0.075f, 0.085f, 0.075f));
        Material targetColor = GetOrCreateMaterial(MaterialFolder + "/Prototype_Target.mat", new Color(0.40f, 0.18f, 0.13f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject environment = new GameObject("Provisional Field Mission");

        Material field = EuropeanFieldMaterialBuilder.GetOrCreate(white);
        CreateTerrain(environment.transform, field);
        CreateRoad(environment.transform, dirt);
        CreateTreeLine(environment.transform, foliage, wood);
        CreateRoadsideCover(environment.transform, white, sandbag, wood);
        CreateOutpost(environment.transform, white, wood, dirt, out Transform meetingPoint);
        CreateTestTargets(environment.transform, targetColor, sandbag);
        CreateLighting();
        CreatePlayer(environment.transform, uniform, gloves, meetingPoint, out Text objectiveText);
        ConfigureObjective(meetingPoint, objectiveText);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Prototipo de misión FPS creado en {ScenePath}");
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
        SetColor(material, Color.white);
        SetFloat(material, "_Smoothness", 0f);
        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }

    private static Material GetOrCreateMaterial(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            SetColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
        SetColor(material, color);
        SetFloat(material, "_Smoothness", 0.12f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SetColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property)) material.SetFloat(property, value);
    }

    private static void CreateTerrain(Transform parent, Material material)
    {
        int resolution = TerrainResolution;
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = x - MapHalfSize;
                float worldZ = z - MapHalfSize;
                int vertexIndex = z * resolution + x;
                vertices[vertexIndex] = new Vector3(worldX, TerrainHeight(worldX, worldZ), worldZ);
                uvs[vertexIndex] = new Vector2(worldX / 8f, worldZ / 8f);
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

        Mesh terrainMesh = new Mesh { name = "Field Mission Terrain Mesh" };
        terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.uv = uvs;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateTangents();
        terrainMesh.RecalculateBounds();
        string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(TerrainMeshAssetPath);
        AssetDatabase.CreateAsset(terrainMesh, meshAssetPath);

        GameObject terrain = new GameObject("Rolling European Field");
        terrain.transform.SetParent(parent);
        terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
        terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
        terrain.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
    }

    private static float TerrainHeight(float x, float z)
    {
        float hillA = 2.8f * Mathf.Exp(-((x - 38f) * (x - 38f) + (z - 14f) * (z - 14f)) / 520f);
        float hillB = 2.1f * Mathf.Exp(-((x + 41f) * (x + 41f) + (z + 5f) * (z + 5f)) / 470f);
        float ridge = 1.3f * Mathf.Exp(-((x - 22f) * (x - 22f)) / 270f) * Mathf.Exp(-((z - 31f) * (z - 31f)) / 520f);
        float rawHeight = hillA + hillB + ridge;
        float landingClearing = Mathf.Exp(-(x * x / 250f + (z + 52f) * (z + 52f) / 190f));
        float meetingClearing = Mathf.Exp(-((x - 5f) * (x - 5f) / 280f + (z - 36f) * (z - 36f) / 240f));
        float roadValley = Mathf.Exp(-(x * x / 120f + (z + 5f) * (z + 5f) / 2500f));
        float flatten = Mathf.Max(landingClearing, Mathf.Max(meetingClearing, roadValley * 0.72f));
        return rawHeight * (1f - flatten * 0.94f);
    }

    private static void CreateRoad(Transform parent, Material material)
    {
        const int segments = 16;
        const float startZ = -57f;
        const float segmentLength = 6f;
        for (int i = 0; i < segments; i++)
        {
            float z = startZ + i * segmentLength;
            float x = Mathf.Sin((z + 57f) * 0.045f) * 2.6f;
            float nextX = Mathf.Sin((z + segmentLength + 57f) * 0.045f) * 2.6f;
            float angle = Mathf.Atan2(nextX - x, segmentLength) * Mathf.Rad2Deg;
            float y = TerrainHeight(x, z + segmentLength * 0.5f) + 0.035f;
            GameObject road = CreateCube("Dirt Track", new Vector3(x, y, z + segmentLength * 0.5f), new Vector3(6.2f, 0.08f, segmentLength + 0.15f), material, parent);
            road.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        CreateCube("Landing Clearing", new Vector3(0f, TerrainHeight(0f, -51f) + 0.02f, -51f), new Vector3(19f, 0.08f, 15f), material, parent);
    }

    private static void CreateTreeLine(Transform parent, Material foliage, Material wood)
    {
        for (int i = 0; i < 38; i++)
        {
            float angle = i * Mathf.PI * 2f / 38f;
            float radius = 54f + Mathf.Sin(i * 2.17f) * 5f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            if (z < -44f && Mathf.Abs(x) < 15f)
                continue;

            float scale = 0.82f + (i % 5) * 0.09f;
            CreateTree(parent, new Vector3(x, TerrainHeight(x, z), z), scale, foliage, wood);
        }

        Vector2[] hedgerow =
        {
            new Vector2(-15f, -36f), new Vector2(-18f, -27f), new Vector2(-14f, -17f),
            new Vector2(16f, -36f), new Vector2(18f, -25f), new Vector2(14f, -14f),
            new Vector2(-18f, 4f), new Vector2(-21f, 12f), new Vector2(20f, 5f), new Vector2(18f, 14f)
        };
        foreach (Vector2 position in hedgerow)
            CreateTree(parent, new Vector3(position.x, TerrainHeight(position.x, position.y), position.y), 0.7f, foliage, wood);
    }

    private static void CreateTree(Transform parent, Vector3 position, float scale, Material foliage, Material wood)
    {
        GameObject tree = new GameObject("Field Tree");
        tree.transform.SetParent(parent);
        tree.transform.position = position;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 1.8f * scale, 0f);
        trunk.transform.localScale = new Vector3(0.35f * scale, 1.8f * scale, 0.35f * scale);
        trunk.GetComponent<Renderer>().sharedMaterial = wood;

        for (int i = 0; i < 3; i++)
        {
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(tree.transform, false);
            crown.transform.localPosition = new Vector3((i - 1) * 0.62f * scale, (3.1f + (i == 1 ? 0.7f : 0f)) * scale, 0f);
            crown.transform.localScale = Vector3.one * (2.15f * scale);
            crown.GetComponent<Renderer>().sharedMaterial = foliage;
            Object.DestroyImmediate(crown.GetComponent<Collider>());
        }
    }

    private static void CreateRoadsideCover(Transform parent, Material white, Material sandbag, Material wood)
    {
        Vector3[] positions =
        {
            new Vector3(-7f, 0f, -34f), new Vector3(7f, 0f, -26f), new Vector3(-8f, 0f, -15f),
            new Vector3(8f, 0f, -5f), new Vector3(-8f, 0f, 8f), new Vector3(10f, 0f, 18f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 position = positions[i];
            position.y = TerrainHeight(position.x, position.z);
            CreateCube("Sandbag Cover", position + Vector3.up * 0.42f, new Vector3(4.2f, 0.84f, 0.75f), sandbag, parent);
            if (i % 2 == 0)
                CreateCube("Field Crate", position + new Vector3(3f, 0.55f, 2.4f), new Vector3(1.1f, 1.1f, 1.1f), wood, parent);
            else
                CreateCube("Concrete Road Block", position + new Vector3(-3f, 0.6f, -2.2f), new Vector3(1.1f, 1.2f, 1.1f), white, parent);
        }
    }

    private static void CreateOutpost(Transform parent, Material white, Material wood, Material dirt, out Transform meetingPoint)
    {
        const float x = 5f;
        const float z = 37f;
        float ground = TerrainHeight(x, z);
        Vector3 center = new Vector3(x, ground, z);

        CreateCube("Outpost Yard", center + Vector3.down * 0.035f, new Vector3(26f, 0.07f, 24f), dirt, parent);
        CreateCube("Meeting Post Floor", center + new Vector3(0f, 0.05f, 0f), new Vector3(14f, 0.2f, 12f), white, parent);
        CreateCube("Outpost Back Wall", center + new Vector3(0f, 1.8f, 5.9f), new Vector3(14f, 3.6f, 0.2f), white, parent);
        CreateCube("Outpost Left Wall", center + new Vector3(-6.9f, 1.8f, 0f), new Vector3(0.2f, 3.6f, 12f), white, parent);
        CreateCube("Outpost Right Wall", center + new Vector3(6.9f, 1.8f, 0f), new Vector3(0.2f, 3.6f, 12f), white, parent);
        CreateCube("Outpost Front Left", center + new Vector3(-4.7f, 1.8f, -5.9f), new Vector3(4.6f, 3.6f, 0.2f), white, parent);
        CreateCube("Outpost Front Right", center + new Vector3(4.7f, 1.8f, -5.9f), new Vector3(4.6f, 3.6f, 0.2f), white, parent);
        CreateCube("Outpost Door Header", center + new Vector3(0f, 3.25f, -5.9f), new Vector3(4.8f, 0.7f, 0.2f), white, parent);
        CreateCube("Outpost Roof", center + new Vector3(0f, 3.7f, 0f), new Vector3(15f, 0.25f, 13f), wood, parent);
        CreateCube("Meeting Table", center + new Vector3(0f, 0.65f, 1.1f), new Vector3(2.8f, 0.12f, 1.2f), wood, parent);

        GameObject lamp = new GameObject("Outpost Interior Light");
        lamp.transform.position = center + new Vector3(0f, 2.7f, 0f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 12f;
        light.intensity = 3f;
        light.color = new Color(1f, 0.79f, 0.56f);

        meetingPoint = new GameObject("Rally Point").transform;
        meetingPoint.SetParent(parent);
        meetingPoint.position = center + new Vector3(0f, 0f, 0f);
    }

    private static void CreateTestTargets(Transform parent, Material targetMaterial, Material sandbag)
    {
        Vector3[] positions =
        {
            new Vector3(22f, 0f, 22f), new Vector3(26f, 0f, 31f), new Vector3(23f, 0f, 42f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 position = positions[i];
            position.y = TerrainHeight(position.x, position.z);
            GameObject target = new GameObject($"Experimental Target {i + 1:00}");
            target.transform.SetParent(parent);
            target.transform.position = position;

            GameObject body = CreateCube("Target Torso", new Vector3(position.x, position.y + 1.15f, position.z), new Vector3(0.72f, 1.05f, 0.34f), targetMaterial, parent);
            body.transform.SetParent(target.transform, true);
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Target Head";
            head.transform.SetParent(target.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            head.transform.localScale = new Vector3(0.48f, 0.48f, 0.42f);
            head.GetComponent<Renderer>().sharedMaterial = sandbag;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            BoxCollider hitbox = target.AddComponent<BoxCollider>();
            hitbox.center = new Vector3(0f, 1.1f, 0f);
            hitbox.size = new Vector3(0.9f, 2.1f, 0.55f);
            PrototypeTarget damageable = target.AddComponent<PrototypeTarget>();
            SerializedObject serialized = new SerializedObject(damageable);
            serialized.FindProperty("hitRenderer").objectReferenceValue = body.GetComponent<Renderer>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Field Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(43f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.46f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.54f, 0.58f, 0.52f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0025f;
    }

    private static void CreatePlayer(Transform parent, Material uniformMaterial, Material gloveMaterial, Transform meetingPoint, out Text objectiveText)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(0f, TerrainHeight(0f, -53f) + 0.05f, -53f);
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 0.9f, 0f);
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 45f;

        GameObject bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyVisual.name = "Player Silhouette (not rendered in first person)";
        bodyVisual.transform.SetParent(player.transform, false);
        bodyVisual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        bodyVisual.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        bodyVisual.layer = 2;
        bodyVisual.GetComponent<Renderer>().sharedMaterial = uniformMaterial;
        Object.DestroyImmediate(bodyVisual.GetComponent<Collider>());

        GameObject cameraObject = new GameObject("First Person Camera");
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 75f;
        camera.nearClipPlane = 0.04f;
        camera.cullingMask &= ~(1 << 2);
        cameraObject.AddComponent<AudioListener>();

        FirstPersonController movement = player.AddComponent<FirstPersonController>();
        SerializedObject movementData = new SerializedObject(movement);
        movementData.FindProperty("viewCamera").objectReferenceValue = camera;
        movementData.ApplyModifiedPropertiesWithoutUndo();

        Transform weaponRoot = new GameObject("First Person Arms and Weapon").transform;
        weaponRoot.SetParent(cameraObject.transform, false);
        weaponRoot.localPosition = new Vector3(0.2f, -0.25f, 0.58f);

        CreateArms(cameraObject.transform, uniformMaterial);
        GameObject rifle = CreateWeaponView(RifleModelPath, "Mauser Rifle View", weaponRoot, new Vector3(0.18f, 0f, 0f), uniformMaterial, true);
        GameObject pistol = CreateWeaponView(PistolModelPath, "M1911 Pistol View", weaponRoot, new Vector3(0.15f, -0.03f, 0f), uniformMaterial, false);
        pistol.SetActive(false);
        CreateGlove("Right Glove", weaponRoot, gloveMaterial, new Vector3(0.05f, -0.10f, 0.10f));
        CreateGlove("Support Glove", weaponRoot, gloveMaterial, new Vector3(-0.03f, -0.13f, 0.34f));

        GameObject muzzle = new GameObject("Muzzle Flash");
        muzzle.transform.SetParent(weaponRoot, false);
        Light muzzleFlash = muzzle.AddComponent<Light>();
        muzzleFlash.type = LightType.Point;
        muzzleFlash.color = new Color(1f, 0.55f, 0.16f);
        muzzleFlash.intensity = 4f;
        muzzleFlash.range = 2.5f;
        muzzleFlash.enabled = false;

        CreateHud(out objectiveText, out Text weaponLabel, out Text ammunitionLabel, out Text statusLabel);

        FirstPersonWeaponController weapons = player.AddComponent<FirstPersonWeaponController>();
        SerializedObject weaponData = new SerializedObject(weapons);
        weaponData.FindProperty("aimCamera").objectReferenceValue = camera;
        weaponData.FindProperty("weaponRoot").objectReferenceValue = weaponRoot;
        weaponData.FindProperty("rifleView").objectReferenceValue = rifle;
        weaponData.FindProperty("pistolView").objectReferenceValue = pistol;
        weaponData.FindProperty("muzzleFlash").objectReferenceValue = muzzleFlash;
        weaponData.FindProperty("weaponLabel").objectReferenceValue = weaponLabel;
        weaponData.FindProperty("ammunitionLabel").objectReferenceValue = ammunitionLabel;
        weaponData.FindProperty("statusLabel").objectReferenceValue = statusLabel;
        weaponData.ApplyModifiedPropertiesWithoutUndo();

        CreateWayfindingMarker(meetingPoint.position, uniformMaterial);
    }

    private static void CreateArms(Transform camera, Material material)
    {
        CreateForearm("Left Forearm", camera, material, new Vector3(-0.27f, -0.34f, 0.38f), Quaternion.Euler(62f, 0f, -14f));
        CreateForearm("Right Forearm", camera, material, new Vector3(0.24f, -0.34f, 0.43f), Quaternion.Euler(58f, 0f, 12f));
    }

    private static void CreateForearm(string name, Transform parent, Material material, Vector3 position, Quaternion rotation)
    {
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        arm.name = name;
        arm.transform.SetParent(parent, false);
        arm.transform.localPosition = position;
        arm.transform.localRotation = rotation;
        arm.transform.localScale = new Vector3(0.14f, 0.34f, 0.14f);
        arm.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(arm.GetComponent<Collider>());
    }

    private static void CreateGlove(string name, Transform parent, Material material, Vector3 position)
    {
        GameObject glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glove.name = name;
        glove.transform.SetParent(parent, false);
        glove.transform.localPosition = position;
        glove.transform.localScale = new Vector3(0.13f, 0.09f, 0.16f);
        glove.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(glove.GetComponent<Collider>());
    }

    private static GameObject CreateWeaponView(string modelPath, string fallbackName, Transform parent, Vector3 offset, Material fallbackMaterial, bool rifle)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject view;
        if (model != null)
        {
            view = (GameObject)PrefabUtility.InstantiatePrefab(model);
            view.name = fallbackName;
            view.transform.SetParent(parent, false);
            view.transform.localPosition = offset;
            view.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            view.transform.localScale = Vector3.one;
        }
        else
        {
            view = new GameObject(fallbackName + " (fallback blockout)");
            view.transform.SetParent(parent, false);
            view.transform.localPosition = offset;
            CreateCube("Receiver", new Vector3(0f, 0f, 0f), rifle ? new Vector3(0.12f, 0.12f, 0.36f) : new Vector3(0.12f, 0.1f, 0.22f), fallbackMaterial, view.transform);
            CreateCube("Barrel", new Vector3(0f, 0.015f, rifle ? 0.25f : 0.16f), rifle ? new Vector3(0.045f, 0.045f, 0.42f) : new Vector3(0.035f, 0.035f, 0.2f), fallbackMaterial, view.transform);
            CreateCube("Stock", new Vector3(0f, -0.03f, -0.22f), rifle ? new Vector3(0.1f, 0.14f, 0.38f) : new Vector3(0.08f, 0.15f, 0.14f), fallbackMaterial, view.transform);
        }
        return view;
    }

    private static void CreateHud(out Text objective, out Text weapon, out Text ammunition, out Text status)
    {
        GameObject canvasObject = new GameObject("Mission HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        objective = CreateText("Mission Objective", canvasObject.transform, new Vector2(0.035f, 0.80f), new Vector2(0.46f, 0.96f), 24, TextAnchor.UpperLeft);
        objective.text = "OBJETIVO\nEncontrá el puesto de reunión";
        weapon = CreateText("Weapon Label", canvasObject.transform, new Vector2(0.67f, 0.10f), new Vector2(0.97f, 0.16f), 18, TextAnchor.MiddleRight);
        ammunition = CreateText("Ammunition", canvasObject.transform, new Vector2(0.75f, 0.035f), new Vector2(0.97f, 0.11f), 30, TextAnchor.MiddleRight);
        status = CreateText("Weapon Status", canvasObject.transform, new Vector2(0.55f, 0.16f), new Vector2(0.97f, 0.23f), 18, TextAnchor.MiddleRight);
        CreateText("Reticle", canvasObject.transform, new Vector2(0.49f, 0.48f), new Vector2(0.51f, 0.52f), 24, TextAnchor.MiddleCenter).text = "+";
        CreateText("Controls", canvasObject.transform, new Vector2(0.03f, 0.025f), new Vector2(0.62f, 0.09f), 16, TextAnchor.LowerLeft).text = "WASD mover · Shift correr · 1/2 cambiar arma · Click disparar · R recargar · Esc cursor";
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void ConfigureObjective(Transform meetingPoint, Text objectiveText)
    {
        GameObject triggerObject = new GameObject("Meeting Point Objective Trigger");
        triggerObject.transform.position = meetingPoint.position;
        BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(11f, 3f, 9f);
        Rigidbody triggerBody = triggerObject.AddComponent<Rigidbody>();
        triggerBody.isKinematic = true;
        triggerBody.useGravity = false;
        MissionObjectiveTrigger objective = triggerObject.AddComponent<MissionObjectiveTrigger>();
        objective.Configure(objectiveText, "OBJETIVO CUMPLIDO\nLlegaste al puesto de reunión");
    }

    private static void CreateWayfindingMarker(Vector3 target, Material material)
    {
        GameObject marker = new GameObject("Meeting Point Marker");
        marker.transform.position = target + new Vector3(0f, 2.4f, 0f);
        LineRenderer line = marker.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, marker.transform.position);
        line.SetPosition(1, marker.transform.position + Vector3.up * 2.4f);
        line.startWidth = 0.08f;
        line.endWidth = 0.04f;
        line.sharedMaterial = material;
        line.useWorldSpace = true;
        line.startColor = new Color(0.82f, 0.72f, 0.40f);
        line.endColor = new Color(0.82f, 0.72f, 0.40f);
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = material;
        return cube;
    }
}
