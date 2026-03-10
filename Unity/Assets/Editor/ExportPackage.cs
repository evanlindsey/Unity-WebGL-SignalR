using UnityEditor;
using UnityEngine;

namespace UnityWebGLSignalR.Editor
{
    public static class ExportPackage
    {
        public static void Export()
        {
            // Version is passed from CI via the RELEASE_VERSION env var (tag name without 'v' prefix).
            var version = System.Environment.GetEnvironmentVariable("RELEASE_VERSION");
            if (string.IsNullOrEmpty(version))
                version = "0.0.0-local";

            var fileName = $"unity-webgl-signalr-{version}.unitypackage";
            AssetDatabase.ExportPackage(
                "Assets/Plugins/SignalR",
                fileName,
                ExportPackageOptions.Recurse);
            Debug.Log($"Exported {fileName}");
        }
    }
}
