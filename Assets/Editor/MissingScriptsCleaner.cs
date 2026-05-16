using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptsCleaner
{
    [MenuItem("Tools/Robot Run/Clean Missing Scripts Everywhere")]
    public static void CleanMissingScriptsEverywhere()
    {
        CleanMissingScriptsInPrefabs();
        CleanMissingScriptsInAllScenes();
    }

    [MenuItem("Tools/Robot Run/Clean Missing Scripts In Prefabs")]
    public static void CleanMissingScriptsInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int cleanedPrefabsCount = 0;
        int cleanedComponentsCount = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            int removedInPrefab = RemoveMissingScriptsRecursive(prefabRoot);

            if (removedInPrefab > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                cleanedPrefabsCount++;
                cleanedComponentsCount += removedInPrefab;
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"MissingScriptsCleaner: cleaned {cleanedComponentsCount} missing scripts in {cleanedPrefabsCount} prefabs.");
    }

    [MenuItem("Tools/Robot Run/Clean Missing Scripts In All Scenes")]
    public static void CleanMissingScriptsInAllScenes()
    {
        string activeScenePath = SceneManager.GetActiveScene().path;
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int cleanedScenesCount = 0;
        int cleanedComponentsCount = 0;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            if (scenePath.StartsWith("Assets/") == false)
            {
                continue;
            }

            Scene openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int removedInScene = RemoveMissingScriptsInScene(openedScene);

            if (removedInScene > 0)
            {
                EditorSceneManager.MarkSceneDirty(openedScene);
                EditorSceneManager.SaveScene(openedScene);
                cleanedScenesCount++;
                cleanedComponentsCount += removedInScene;
            }
        }

        if (string.IsNullOrEmpty(activeScenePath) == false && activeScenePath.StartsWith("Assets/"))
        {
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"MissingScriptsCleaner: cleaned {cleanedComponentsCount} missing scripts in {cleanedScenesCount} scenes.");
    }

    private static int RemoveMissingScriptsRecursive(GameObject root)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        Transform rootTransform = root.transform;
        int childCount = rootTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            removed += RemoveMissingScriptsRecursive(rootTransform.GetChild(i).gameObject);
        }

        return removed;
    }

    private static int RemoveMissingScriptsInScene(Scene scene)
    {
        int removed = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            removed += RemoveMissingScriptsRecursive(roots[i]);
        }

        return removed;
    }
}
