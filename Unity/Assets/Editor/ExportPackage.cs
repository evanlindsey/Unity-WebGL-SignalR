using UnityEditor;
using UnityEngine;

namespace UnityWebGLSignalR.Editor
{
    public static class ExportPackage
    {
        public static void Export()
        {
            var version = GetArgValue("-releaseVersion") ?? "0.0.0-local";

            var fileName = $"unity-webgl-signalr-{version}.unitypackage";
            AssetDatabase.ExportPackage(
                "Assets/Plugins/SignalR",
                fileName,
                ExportPackageOptions.Recurse);
            Debug.Log($"Exported {fileName}");
        }

        private static string GetArgValue(string argName)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == argName)
                    return args[i + 1];
            }
            return null;
        }
    }
}
