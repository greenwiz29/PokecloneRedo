using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LocationPortal))]
public class LocationPortalEditor : Editor
{
    [SerializeField]
    GameObject locationPortalPrefab;

    SerializedProperty targetSceneName;
    SerializedProperty entryPointId;

    const string PortalPrefabPrefKey = "LocationPortal_PrefabPath";

    string[] sceneNames;
    string[] entryPointIds;
    Dictionary<string, int> entryPointIdCounts;

    void OnEnable()
    {
        targetSceneName = serializedObject.FindProperty("targetSceneName");
        entryPointId = serializedObject.FindProperty("entryPointId");

        sceneNames = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        RefreshEntryPoints();
    }

    public override void OnInspectorGUI()
    {
        DrawPortalPrefabField();

        serializedObject.Update();

        DrawSceneDropdown();
        DrawEntryPointDropdown();

        DrawValidationWarnings(); // 👈 NEW

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto-Create Return Portal"))
        {
            AutoCreateReturnPortal();
        }

    }

    void DrawPortalPrefabField()
    {
        EditorGUILayout.LabelField("Portal Prefab", EditorStyles.boldLabel);

        string path = EditorPrefs.GetString(PortalPrefabPrefKey, "");
        GameObject prefab = string.IsNullOrEmpty(path)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(path);

        var newPrefab = (GameObject)EditorGUILayout.ObjectField(
            prefab,
            typeof(GameObject),
            false
        );

        if (newPrefab != prefab && newPrefab != null)
        {
            string newPath = AssetDatabase.GetAssetPath(newPrefab);
            EditorPrefs.SetString(PortalPrefabPrefKey, newPath);
        }
    }

    void AutoCreateReturnPortal()
    {
        string prefabPath = EditorPrefs.GetString(PortalPrefabPrefKey, "");
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("No LocationPortal prefab assigned.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("LocationPortal prefab could not be loaded.");
            return;
        }

        var targetScene = SceneManager.GetSceneByName(targetSceneName.stringValue);
        if (!targetScene.isLoaded)
        {
            Debug.LogError("Target scene must be loaded to create return portal.");
            return;
        }

        // Instantiate prefab
        var portalGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        SceneManager.MoveGameObjectToScene(portalGO, targetScene);
        portalGO.name = $"ReturnPortal_to_{serializedObject.targetObject.name}";

        var portalComponent = portalGO.GetComponent<LocationPortal>();

        var entry = portalGO.GetComponentInChildren<SceneEntryPoint>();
        if (entry == null)
        {
            Debug.LogError("LocationPortal prefab is missing a SceneEntryPoint child.");
            return;
        }

        // Set portal data
        var portalSO = new SerializedObject(portalComponent);
        portalSO.FindProperty("targetSceneName").stringValue =
            ((LocationPortal)target).gameObject.scene.name;
        portalSO.FindProperty("entryPointId").stringValue = entryPointId.stringValue;
        portalSO.ApplyModifiedProperties();

        // Set entry point ID
        var entrySO = new SerializedObject(entry);
        entrySO.FindProperty("entryId").stringValue = entryPointId.stringValue;
        entrySO.ApplyModifiedProperties();


        Selection.activeGameObject = portalGO;
        EditorSceneManager.MarkSceneDirty(targetScene);
    }

    string GetNormalizedPortalName(LocationPortal portal)
    {
        string fromScene = portal.gameObject.scene.name;
        string toScene = portal.TargetSceneName;
        string entryId = portal.EntryPointId;

        if (string.IsNullOrEmpty(toScene) || string.IsNullOrEmpty(entryId))
            return portal.gameObject.name;

        return $"Portal_{fromScene}_to_{toScene}_{entryId}";
    }

    string GetNormalizedEntryPointName(SceneEntryPoint entry)
    {
        if (string.IsNullOrEmpty(entry.EntryId))
            return entry.gameObject.name;

        return $"Entry_{entry.EntryId}";
    }

    void DrawFixableWarning(string message, MessageType type, System.Action fixAction, string fixLabel)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox(message, type);

        if (fixAction != null && GUILayout.Button(fixLabel))
        {
            fixAction.Invoke();
        }

        EditorGUILayout.EndVertical();
    }

    // -----------------------------
    // Scene Dropdown
    // -----------------------------
    void DrawSceneDropdown()
    {
        if (sceneNames.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "No scenes enabled in Build Settings.",
                MessageType.Warning
            );
            return;
        }

        int currentIndex = Mathf.Max(
            0,
            System.Array.IndexOf(sceneNames, targetSceneName.stringValue)
        );

        int newIndex = EditorGUILayout.Popup(
            "Target Scene",
            currentIndex,
            sceneNames
        );

        if (newIndex != currentIndex)
        {
            targetSceneName.stringValue = sceneNames[newIndex];
            RefreshEntryPoints();
        }
    }

    // -----------------------------
    // Entry Point Dropdown
    // -----------------------------
    void DrawEntryPointDropdown()
    {
        if (entryPointIds == null)
        {
            EditorGUILayout.HelpBox(
                "Target scene is not loaded. Entry Point ID must be typed manually.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(entryPointId);
            return;
        }

        if (entryPointIds.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "No SceneEntryPoints found in target scene.",
                MessageType.Warning
            );

            EditorGUILayout.PropertyField(entryPointId);
            return;
        }

        int currentIndex = Mathf.Max(
            0,
            System.Array.IndexOf(entryPointIds, entryPointId.stringValue)
        );

        int newIndex = EditorGUILayout.Popup(
            "Entry Point",
            currentIndex,
            entryPointIds
        );

        entryPointId.stringValue = entryPointIds[newIndex];
    }

    // -----------------------------
    // Validation
    // -----------------------------
    void DrawValidationWarnings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        // 1️⃣ Scene not in Build Settings
        if (!sceneNames.Contains(targetSceneName.stringValue))
        {
            DrawFixableWarning(
                $"Scene '{targetSceneName.stringValue}' is not in Build Settings.",
                MessageType.Warning,
                () => AddSceneToBuildSettings(targetSceneName.stringValue),
                "Add to Build Settings"
            );
        }

        // 2️⃣ Scene not loaded
        Scene scene = SceneManager.GetSceneByName(targetSceneName.stringValue);
        if (!string.IsNullOrEmpty(targetSceneName.stringValue) && !scene.isLoaded)
        {
            DrawFixableWarning(
                $"Scene '{targetSceneName.stringValue}' is not loaded. Entry points cannot be validated.",
                MessageType.Info,
                () => LoadSceneInEditor(targetSceneName.stringValue),
                "Load Scene"
            );
            return;
        }

        if (entryPointIds == null)
            return;

        // 3️⃣ Entry point missing
        if (!string.IsNullOrEmpty(entryPointId.stringValue) &&
            !entryPointIds.Contains(entryPointId.stringValue))
        {
            DrawFixableWarning(
                $"Entry point '{entryPointId.stringValue}' does not exist in scene '{targetSceneName.stringValue}'.",
                MessageType.Error,
                () => CreateEntryPointInScene(scene, entryPointId.stringValue), "Create Entry Point"
            );
        }

        // 4️⃣ Duplicate entry IDs
        if (entryPointIdCounts != null)
        {
            foreach (var kvp in entryPointIdCounts)
            {
                if (kvp.Value > 1)
                {
                    DrawFixableWarning(
                        $"Duplicate SceneEntryPoint ID '{kvp.Key}' found ({kvp.Value} instances).",
                        MessageType.Warning,
                        () => SelectDuplicateEntryPoints(scene, kvp.Key),
                        "Select Duplicates"
                    );
                }
            }
        }

        // 5️⃣ Likely missing return portal (heuristic)
        var portal = (LocationPortal)target;
        var entry = portal.GetComponentInChildren<SceneEntryPoint>();

        if (entry == null)
        {
            DrawFixableWarning(
                "Portal is missing a SceneEntryPoint child.",
                MessageType.Error,
                () => FixMissingEntryPoint(portal),
                "Create Entry Point"
            );

            return; // Can't check mismatch if it doesn't exist
        }

        if (entry.EntryId != portal.EntryPointId)
        {
            DrawFixableWarning(
                $"Entry ID mismatch:\nPortal = '{portal.EntryPointId}'\nEntryPoint = '{entry.EntryId}'",
                MessageType.Warning,
                () => FixEntryPointIdMismatch(portal, entry),
                "Sync Entry IDs"
            );
        }

        // -----------------------------
        // Normalized Naming
        // -----------------------------

        string expectedPortalName = GetNormalizedPortalName(portal);
        if (portal.gameObject.name != expectedPortalName)
        {
            DrawFixableWarning(
                $"Portal name is not normalized.\nExpected: {expectedPortalName}",
                MessageType.Info,
                () => FixPortalName(portal),
                "Fix Portal Name"
            );
        }

        if (entry != null)
        {
            string expectedEntryName = GetNormalizedEntryPointName(entry);
            if (entry.gameObject.name != expectedEntryName)
            {
                DrawFixableWarning(
                    $"EntryPoint name is not normalized.\nExpected: {expectedEntryName}",
                    MessageType.Info,
                    () => FixEntryPointName(entry),
                    "Fix Entry Name"
                );
            }
        }

    }

    // -----------------------------
    // Entry Point Discovery
    // -----------------------------
    void RefreshEntryPoints()
    {
        entryPointIds = null;
        entryPointIdCounts = null;

        if (string.IsNullOrEmpty(targetSceneName.stringValue))
            return;

        Scene scene = SceneManager.GetSceneByName(targetSceneName.stringValue);
        if (!scene.isLoaded)
            return;

        var entryPoints = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<SceneEntryPoint>(true))
            .ToList();

        entryPointIds = entryPoints
            .Select(ep => ep.EntryId)
            .Distinct()
            .ToArray();

        entryPointIdCounts = entryPoints
            .GroupBy(ep => ep.EntryId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    void LoadSceneInEditor(string sceneName)
    {
        string path = EditorBuildSettings.scenes
            .FirstOrDefault(s => s.path.Contains($"/{sceneName}.unity"))?.path;

        if (!string.IsNullOrEmpty(path))
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }
    }

    void CreateEntryPointInScene(Scene scene, string id)
    {
        GameObject go = new GameObject($"Entry_{id}");
        SceneManager.MoveGameObjectToScene(go, scene);

        var entry = go.AddComponent<SceneEntryPoint>();
        entry.SetEditorId(id);

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(scene);
    }

    void SelectDuplicateEntryPoints(Scene scene, string id)
    {
        var duplicates = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<SceneEntryPoint>(true))
            .Where(ep => ep.EntryId == id)
            .Select(ep => ep.gameObject)
            .ToArray();

        Selection.objects = duplicates;
    }

    void AddSceneToBuildSettings(string sceneName)
    {
        var scenes = EditorBuildSettings.scenes.ToList();

        string path = AssetDatabase.FindAssets($"t:Scene {sceneName}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    void FixMissingEntryPoint(LocationPortal portal)
    {
        GameObject child = new GameObject("SpawnPoint");
        child.transform.SetParent(portal.transform, false);

        var entry = child.AddComponent<SceneEntryPoint>();
        entry.SetEditorId(portal.EntryPointId);

        EditorUtility.SetDirty(portal);
        EditorSceneManager.MarkSceneDirty(portal.gameObject.scene);

        Selection.activeGameObject = child;
    }

    void FixEntryPointIdMismatch(LocationPortal portal, SceneEntryPoint entry)
    {
        var entrySO = new SerializedObject(entry);
        entrySO.FindProperty("entryId").stringValue = portal.EntryPointId;
        entrySO.ApplyModifiedProperties();

        EditorUtility.SetDirty(entry);
        EditorSceneManager.MarkSceneDirty(entry.gameObject.scene);

        Selection.activeGameObject = entry.gameObject;
    }

    void FixPortalName(LocationPortal portal)
    {
        string expected = GetNormalizedPortalName(portal);
        portal.gameObject.name = expected;

        EditorUtility.SetDirty(portal);
        EditorSceneManager.MarkSceneDirty(portal.gameObject.scene);
    }

    void FixEntryPointName(SceneEntryPoint entry)
    {
        string expected = GetNormalizedEntryPointName(entry);
        entry.gameObject.name = expected;

        EditorUtility.SetDirty(entry);
        EditorSceneManager.MarkSceneDirty(entry.gameObject.scene);
    }

}
