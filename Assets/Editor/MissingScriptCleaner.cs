using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptCleaner
{
    private const string AssetsRoot = "Assets/robor_runner_assets";

    [MenuItem("Tools/Robot Run/Cleanup Missing Scripts")]
    public static void Cleanup()
    {
        int removedTotal = 0;

        string[] scenePaths =
        {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/Game.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            removedTotal += CleanupScene(scenePath);
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { AssetsRoot });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            removedTotal += CleanupPrefab(path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"MissingScriptCleaner: removed {removedTotal} missing script components.");
    }

    private static int CleanupScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int removed = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            removed += RemoveMissingInHierarchy(root);
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        return removed;
    }

    private static int CleanupPrefab(string assetPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
        int removed = RemoveMissingInHierarchy(root);

        if (removed > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        }

        PrefabUtility.UnloadPrefabContents(root);
        return removed;
    }

    private static int RemoveMissingInHierarchy(GameObject root)
    {
        int removed = 0;
        List<Transform> stack = new List<Transform> { root.transform };

        for (int i = 0; i < stack.Count; i++)
        {
            Transform current = stack[i];
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(current.gameObject);

            for (int c = 0; c < current.childCount; c++)
            {
                stack.Add(current.GetChild(c));
            }
        }

        return removed;
    }
}
