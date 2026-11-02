#if UNITY_EDITOR
using System.IO;
using PCGG.ProtocoloAnochecer.Prototype.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PCGG.ProtocoloAnochecer.Prototype.EditorTools
{
    /// <summary>
    /// EXPERIMENTAL. Genera la escena de prueba FPS_Foundation con un blockout básico.
    /// No toca SampleScene, Build Settings ni ajustes globales, y no crea materiales.
    /// Menú: PCGG > Prototype > Create FPS Foundation Scene
    /// </summary>
    public static class FPSFoundationSceneBuilder
    {
        const string SceneFolder = "Assets/Scenes/Prototype";
        const string ScenePath = SceneFolder + "/FPS_Foundation.unity";

        [MenuItem("PCGG/Prototype/Create FPS Foundation Scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("FPS_Foundation", "La escena ya existe. ¿Sobrescribirla?", "Sobrescribir", "Cancelar"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Directory.CreateDirectory(SceneFolder);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            // Material por defecto del pipeline activo (URP/Lit): no se crea ningún material nuevo.
            var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            Material mat = pipeline != null ? pipeline.defaultMaterial : null;

            BuildBlockout(mat);
            BuildPlayer();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"FPS_Foundation creada en {ScenePath}");
        }

        static void BuildBlockout(Material mat)
        {
            var root = new GameObject("Blockout");

            // Piso 30x30 con la cara superior en y = 0.
            Box(root, mat, "Floor", new Vector3(0, -0.1f, 0), new Vector3(30, 0.2f, 30));

            // Paredes perimetrales.
            Box(root, mat, "Wall_N", new Vector3(0, 2, 15), new Vector3(30, 4, 0.5f));
            Box(root, mat, "Wall_S", new Vector3(0, 2, -15), new Vector3(30, 4, 0.5f));
            Box(root, mat, "Wall_E", new Vector3(15, 2, 0), new Vector3(0.5f, 4, 30));
            Box(root, mat, "Wall_W", new Vector3(-15, 2, 0), new Vector3(0.5f, 4, 30));

            // Rampa ~20 grados (dentro del slope limit por defecto).
            var ramp = Box(root, mat, "Ramp", new Vector3(-8, 1.53f, 6), new Vector3(4, 0.3f, 8));
            ramp.transform.rotation = Quaternion.Euler(-20f, 0f, 0f);

            // Escalones de 0.15 m (menores al step offset por defecto de 0.3).
            for (int i = 0; i < 4; i++)
            {
                float h = 0.15f * (i + 1);
                Box(root, mat, $"Step_{i}", new Vector3(8, h * 0.5f, 4 + i * 0.6f), new Vector3(4, h, 0.6f));
            }

            // Obstáculos para probar colisiones.
            Box(root, mat, "Box_A", new Vector3(0, 0.5f, 6), new Vector3(1, 1, 1));
            Box(root, mat, "Box_B", new Vector3(3, 0.75f, 8), new Vector3(1.5f, 1.5f, 1.5f));
            Box(root, mat, "Pillar", new Vector3(-3, 2, 3), new Vector3(0.6f, 4, 0.6f));
            Box(root, mat, "LowWall", new Vector3(0, 0.5f, 11), new Vector3(8, 1, 0.3f));
        }

        static GameObject Box(GameObject parent, Material mat, string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        static void BuildPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 0.1f, -10);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 0.9f, 0);

            player.AddComponent<FPSInput>();
            player.AddComponent<FPSPlayerController>();
            player.AddComponent<FPSCameraLook>();

            // Reutiliza la Main Camera que trae la escena por defecto.
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }
            cam.transform.SetParent(player.transform, false);
            cam.transform.localPosition = new Vector3(0, 1.6f, 0);
            cam.transform.localRotation = Quaternion.identity;
        }
    }
}
#endif

