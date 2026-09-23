#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

[InitializeOnLoad]
public static class QuestXRSetup
{
    static QuestXRSetup(){ EditorApplication.delayCall += Setup; }
    static void Setup()
    {
        try
        {
            var per = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if(per==null)
            {
                XRGeneralSettingsPerBuildTarget container;
                if(!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,out container))
                {
                    container=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(container,"Assets/XRGeneralSettingsPerBuildTarget.asset");
                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,container,true);
                    AssetDatabase.SaveAssets();
                }
                if(!container.HasSettingsForBuildTarget(BuildTargetGroup.Android))
                    container.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
                if(!container.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
                    container.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
                per=container.SettingsForBuildTarget(BuildTargetGroup.Android);
            }
            if(per==null || per.Manager==null) return;
            XRPackageMetadataStore.AssignLoader(per.Manager,"Unity.XR.Oculus.OculusLoader",BuildTargetGroup.Android);
            per.InitManagerOnStart=true;
            EditorUtility.SetDirty(per); AssetDatabase.SaveAssets();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.slaplab.quest3");
            PlayerSettings.SetArchitecture(NamedBuildTarget.Android,1);
            PlayerSettings.productName="SlapLab VR";
        }
        catch(System.Exception e){ Debug.LogWarning("SlapLab Quest XR setup: "+e.Message); }
    }
}
#endif
