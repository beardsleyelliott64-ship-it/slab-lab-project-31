#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BuildSettingsFix
{
    static BuildSettingsFix()
    {
        const string scenePath="Assets/Scenes/Main.unity";
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)!=null)
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
    }
}
#endif
