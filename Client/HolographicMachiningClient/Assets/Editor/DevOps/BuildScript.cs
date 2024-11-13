using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace Editor.DevOps
{
    public class BuildAllPlatforms
    {
        [MenuItem("Build/BuildUWP")]
        public static void BuildHL2()
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                target = BuildTarget.WSAPlayer,
                locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Builds/")),
                targetGroup = BuildTargetGroup.WSA
            };

            // Set UWP build settings
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);

            // Change this line - Using string value instead of enum
            EditorUserBuildSettings.SetPlatformSettings("WindowsStoreApps", "BuildConfiguration", "Release");
            EditorUserBuildSettings.SetPlatformSettings("WindowsStoreApps", "Architecture", "ARM64");

            // Set IL2CPP settings through scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WSA, ScriptingImplementation.IL2CPP);

            // Optimize for speed
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WSA, Il2CppCompilerConfiguration.Release);

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log(report);
        }
    }
}