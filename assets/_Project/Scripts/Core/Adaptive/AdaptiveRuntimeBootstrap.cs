using UnityEngine;
using UnityEngine.SceneManagement;
using _Project.Scripts.Core.Adaptive.Adapters;

namespace _Project.Scripts.Core.Adaptive
{
    /// <summary>
    /// Auto-wires adaptive view components at runtime so the feature can run
    /// without modifying existing gameplay scene references.
    /// </summary>
    public static class AdaptiveRuntimeBootstrap
    {
        private const string PortraitBackgroundName = "Background PT";
        private const string LandscapeBackgroundName = "Background LS";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Camera cameraRef = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
            if (cameraRef == null)
            {
                return;
            }

            AdaptiveCameraController cameraController = GetOrAddComponent<AdaptiveCameraController>(cameraRef.gameObject);
            cameraController.Configure(cameraRef, enableDebug: true, enableLogs: true);

            AdaptiveViewOrchestrator orchestrator = GetOrAddComponent<AdaptiveViewOrchestrator>(cameraRef.gameObject);

            AdaptiveViewProfile profile = ScriptableObject.CreateInstance<AdaptiveViewProfile>();
            profile.hideFlags = HideFlags.HideAndDontSave;

            TileManager tileManager = Object.FindObjectOfType<TileManager>();
            CatManager catManager = Object.FindObjectOfType<CatManager>();
            DualInputHandler inputHandler = Object.FindObjectOfType<DualInputHandler>();

            TileLayoutAdapter tileAdapter = CreateTileAdapter(tileManager);
            CatBoundsAdapter leftAdapter = CreateCatAdapter(catManager != null ? catManager.transform.Find("LeftCat") : null);
            CatBoundsAdapter rightAdapter = CreateCatAdapter(catManager != null ? catManager.transform.Find("RightCat") : null);
            InputViewportAdapter inputAdapter = CreateInputAdapter(inputHandler);

            GameObject portraitBackground = FindGameObjectInScene(SceneManager.GetActiveScene(), PortraitBackgroundName);
            GameObject landscapeBackground = FindGameObjectInScene(SceneManager.GetActiveScene(), LandscapeBackgroundName);

            orchestrator.Configure(
                cameraController,
                profile,
                cameraRef,
                portraitBackground,
                landscapeBackground,
                tileAdapter,
                leftAdapter,
                rightAdapter,
                inputAdapter);

            orchestrator.ApplyCurrentMode();
        }

        private static TileLayoutAdapter CreateTileAdapter(TileManager tileManager)
        {
            if (tileManager == null)
            {
                return null;
            }

            TileLayoutAdapter adapter = GetOrAddComponent<TileLayoutAdapter>(tileManager.gameObject);
            adapter.Configure(tileManager);
            return adapter;
        }

        private static CatBoundsAdapter CreateCatAdapter(Transform catTransform)
        {
            if (catTransform == null)
            {
                return null;
            }

            CatController catController = catTransform.GetComponent<CatController>();
            if (catController == null)
            {
                return null;
            }

            CatBoundsAdapter adapter = GetOrAddComponent<CatBoundsAdapter>(catTransform.gameObject);
            adapter.Configure(catController);
            return adapter;
        }

        private static InputViewportAdapter CreateInputAdapter(DualInputHandler inputHandler)
        {
            if (inputHandler == null)
            {
                return null;
            }

            InputViewportAdapter adapter = GetOrAddComponent<InputViewportAdapter>(inputHandler.gameObject);
            adapter.Configure(inputHandler);
            return adapter;
        }

        private static T GetOrAddComponent<T>(GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            if (component == null)
            {
                component = owner.AddComponent<T>();
            }

            return component;
        }

        private static GameObject FindGameObjectInScene(Scene scene, string targetName)
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject match = FindInChildrenRecursive(roots[i].transform, targetName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static GameObject FindInChildrenRecursive(Transform node, string targetName)
        {
            if (node.name == targetName)
            {
                return node.gameObject;
            }

            for (int i = 0; i < node.childCount; i++)
            {
                GameObject match = FindInChildrenRecursive(node.GetChild(i), targetName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}

