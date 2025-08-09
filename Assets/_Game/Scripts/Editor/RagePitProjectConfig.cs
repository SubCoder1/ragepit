using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public class RagePitProjectConfig : EditorWindow
{
    [MenuItem("RagePit/Apply Project Settings")]
    public static void ApplySettings()
    {
        // ==== GENERAL PLAYER SETTINGS ====
        PlayerSettings.companyName = "MidGames";
        PlayerSettings.productName = "RagePit";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        // ==== RESOLUTION & PRESENTATION ====
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = true;

        // ==== ANDROID SPECIFIC SETTINGS ====
        var androidTarget = NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
        PlayerSettings.SetApplicationIdentifier(androidTarget, "com.midgames.ragepit");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // ==== SCRIPTING SETTINGS ====
        PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(androidTarget, ApiCompatibilityLevel.NET_Unity_4_8);

        // ==== QUALITY & PERFORMANCE ====
        QualitySettings.vSyncCount = 0;

        // ==== RENDERING ====
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.MTRendering = true;

        // ==== MIPMAP STREAMING ====
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsAddAllCameras = true;
        QualitySettings.streamingMipmapsMemoryBudget = 128f;

        Debug.Log("RagePit project settings applied successfully.");
    }
}