using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace AntiHantacoreSpray
{
    public class Main
    {
        public static HashSet<Assembly> HantacoreMods { get { return _hantacoreMods; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        private static HashSet<Assembly> _hantacoreMods = new HashSet<Assembly>();
    }

    [HarmonyPatch(typeof(UnityModManager.ModEntry), nameof(UnityModManager.ModEntry.Invoke))]
    class ModEntry_Invoke_Patch
    {
        private static bool Prefix(UnityModManager.ModEntry __instance)
        {
            if (__instance.Info.Author.ToLower().StartsWith("hantacor")) Main.HantacoreMods.Add(__instance.Assembly);
            return true;
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.Log), new Type[] { typeof(object) })]
    class Debug_Log_Patch
    {
        private static bool Prefix()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame callingFrame = stackTrace.GetFrame(FRAME_TO_CHECK);

            if (callingFrame != null)
            {
                Assembly assembly = callingFrame.GetMethod()?.DeclaringType?.Assembly;
                return !FromHantacore(assembly);
            }

            return true;
        }

        private static bool FromHantacore(Assembly assembly)
        {
            if (assembly == null) return false;
            return Main.HantacoreMods.Contains(assembly);
        }

        private const int FRAME_TO_CHECK = 2;
    }
}
