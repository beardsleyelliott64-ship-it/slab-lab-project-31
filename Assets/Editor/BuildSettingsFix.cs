#if UNITY_EDITOR

using UnityEditor;

[InitializeOnLoad]
public static class BuildSettingsFix
{
    static BuildSettingsFix()
    {
        const string scenePath = "Assets/Scenes/Main.unity";

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            return;

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
    }
}

#endif
