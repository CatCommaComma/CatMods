using UnityEngine;

namespace SDPublicFramework
{
    public static class Logger
    {
        public static void Log(string message = "")
        {
            Debug.Log($"[SDPublicFramework] {message}");
        }

        public static void Debugger(string message)
        {
            if (Main.ModSettings.PrefabDebugger) Debug.Log($"[SDPublicFramework] [DEBUGMODE] {message}");
        }

        public static void Warning(string message)
        {
            Debug.Log($"[SDPublicFramework] [!] {message}");
        }

        public static void Exception(string message)
        {
            Debug.Log($"[SDPublicFramework] [!!!] {message}");
        }
    }
}
