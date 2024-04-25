using Beam;
using Beam.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SDPublicFramework
{
    public class CatUtility
    {
        /// <summary> Spawns a modded prefab by name. </summary>
        public static BaseObject SpawnPrefab(string prefabname, Vector3 position = default)
        {
            Logger.Log($"Trying to spawn '{prefabname}.'");
            BaseObject prefab = PrefabFactory.InstantiateModdedObject(prefabname);

            if (prefab != null)
            {
                InteractiveObject io = prefab as InteractiveObject;
                io.ReferenceId = Funlabs.MiniGuid.New();

                prefab.transform.position = position == default ? PlayerRegistry.LocalPlayer.transform.position : position;
                prefab.transform.rotation = Quaternion.identity;
                io.DurabilityPoints = io.OriginalDurabilityPoints;

                prefab.gameObject.SetActive(true);
                Logger.Log($"Spawn successful.");
                io.EnablePhysics();
            }
            else
            {
                InstantiationPrefab = "PREFAB_IS_NULL";
                Logger.Log($"Spawn unsuccessful, prefab is null.");
            }

            return prefab;
        }

        /// <summary> Raises new notification </summary>
        public static void PopNotification(string text, float duration)
        {
            LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
            localizedNotification.Priority = NotificationPriority.Immediate;
            localizedNotification.Duration = duration;
            localizedNotification.PlayerId = PlayerRegistry.LocalPlayer.Id;
            localizedNotification.MessageText.SetTerm(text);
            localizedNotification.TitleText = _notificationTitle;

            localizedNotification.Raise();
        }

        public static byte[] ExtractEmbeddedResource(String filename, Assembly assembly)
        {
            using (Stream resFilestream = assembly.GetManifestResourceStream(filename))
            {
                if (resFilestream == null)
                {
                    Logger.Warning($"Extract embedded resource: Given assembly ({assembly.FullName}) does not have path of '{filename}'");
                    return null;
                }
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);

                Logger.Debugger($"Extracted embedded resource, stream length: {ba.Length}");

                return ba;
            }
        }

        public static byte[] ExtractNonEmbeddedResource(String path)
        {
            byte[] ba = null;

            try
            {
                ba = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Logger.Exception($"Cannot read non-embedded resource at path [{path}]: \n{ex}");
            }

            return ba;
        }

        public static void PrintChildren(Transform transform, int index)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Logger.Log($"[{ index}-{i}] child found: {transform.GetChild(i).name}");
                PrintChildren(transform.GetChild(i), index + 1);
            }
        }

        public static KeyCode ChooseNewKey(ref bool changing, KeyCode original)
        {
            var allKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    changing = false;

                    if (key == KeyCode.Escape) return original;
                    return key;
                }
            }
            return original;
        }

        internal static void HandlePrefabDebugger(CatSettings debugger, PrefabAdjuster adjuster)
        {
            if (debugger.InstantiatePrefab)
            {
                if (InstantiationPrefab != "none" || InstantiationPrefab != "")
                {
                    try
                    {
                        SpawnPrefab(InstantiationPrefab);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception($"Error trying to spawn [{InstantiationPrefab}]: \n{ex}");
                    }
                }
                debugger.InstantiatePrefab = false;
            }

            if (debugger.GetCurrentPrefab)
            {
                if (PlayerRegistry.LocalPlayer.Holder.CurrentObject != null)
                {
                    ChosenPrefab = PlayerRegistry.LocalPlayer.Holder.CurrentObject.gameObject.GetComponent<InteractiveObject>();
                    ChosenPrefabName = ChosenPrefab.name;
                }
                debugger.GetCurrentPrefab = false;
            }

            if (debugger.GetCurrentRaycastedPrefab)
            {
                if (PlayerRegistry.LocalPlayer.GameInput.Raycaster.CurrentObject != null)
                {
                    ChosenBaseObject = PlayerRegistry.LocalPlayer.GameInput.Raycaster.CurrentObject.gameObject.GetComponent<BaseObject>();
                    ChosenPrefab = PlayerRegistry.LocalPlayer.GameInput.Raycaster.CurrentObject.gameObject.GetComponent<InteractiveObject>();

                    if (ChosenBaseObject != null)
                    {
                        ChosenBasePrefabId = ChosenBaseObject.PrefabId;
                        ChosenBasePrefabName = ChosenBaseObject.name;
                        ChosenBasePrefabMiniGuid = ChosenBaseObject.ReferenceId;

                        try
                        {
                            ChosenPrefabCraftingType[0] = (int)ChosenBaseObject.CraftingType.AttributeType;
                            ChosenPrefabCraftingType[1] = (int)ChosenBaseObject.CraftingType.InteractiveType;

                            FindObjectInfo(ChosenPrefab == null? ChosenBaseObject : ChosenPrefab);
                        }
                        catch
                        {
                            Logger.Exception($"Error trying to get base object's [{ChosenBasePrefabName}] crafting type.");
                        }
                    }
                }

                debugger.GetCurrentRaycastedPrefab = false;
            }

            if (debugger.GetChosenPrefabInfo)
            {
                if (ChosenPrefab != null)
                {
                    adjuster.PositionX = ChosenPrefab.LocalHoldingPosition.x;
                    adjuster.PositionY = ChosenPrefab.LocalHoldingPosition.y;
                    adjuster.PositionZ = ChosenPrefab.LocalHoldingPosition.z;

                    adjuster.RotationX = ChosenPrefab.LocalHoldingRotation.x;
                    adjuster.RotationY = ChosenPrefab.LocalHoldingRotation.y;
                    adjuster.RotationZ = ChosenPrefab.LocalHoldingRotation.z;

                    adjuster.IdleAnimation = (int)ChosenPrefab.Idle;
                    adjuster.EmptySwingAnimation = (int)ChosenPrefab.Primary;
                    adjuster.HitSwingAnimation = (int)ChosenPrefab.PrimaryOnObject;

                    FindObjectInfo(ChosenPrefab);
                }
                debugger.GetChosenPrefabInfo = false;
            }

            if (adjuster.ResetPrefabInfo)
            {
                if (ChosenPrefab != null)
                {
                    adjuster.PositionX = 0f;
                    adjuster.PositionY = 0f;
                    adjuster.PositionZ = 0f;

                    adjuster.RotationX = 0f;
                    adjuster.RotationY = 0f;
                    adjuster.RotationZ = 0f;

                    adjuster.IdleAnimation = 1;
                    adjuster.EmptySwingAnimation = 0;
                    adjuster.HitSwingAnimation = 0;
                }
                adjuster.ResetPrefabInfo = false;
            }

            if (adjuster.UpdatePrefabInfo && ChosenPrefab != null)
            {
                if (ChosenPrefab != null)
                {
                    Vector3 posit = default;
                    Vector3 rotat = default;

                    posit.Set(adjuster.PositionX, adjuster.PositionY, adjuster.PositionZ);
                    rotat.Set(adjuster.RotationX, adjuster.RotationY, adjuster.RotationZ);

                    typeof(InteractiveObject).GetField("_localHoldingPosition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ChosenPrefab, posit);
                    typeof(InteractiveObject).GetField("_localHoldingRotation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ChosenPrefab, rotat);

                    typeof(InteractiveObject).GetField("_idle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ChosenPrefab, (AnimationType)adjuster.IdleAnimation);
                    typeof(InteractiveObject).GetField("_primary", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ChosenPrefab, (AnimationType)adjuster.EmptySwingAnimation);
                    typeof(InteractiveObject).GetField("_primaryOnObject", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ChosenPrefab, (AnimationType)adjuster.HitSwingAnimation);

                    PlayerRegistry.LocalPlayer.Holder.DropCurrent();
                    PlayerRegistry.LocalPlayer.Holder.Pickup(ChosenPrefab as IPickupable, false);
                }
                adjuster.UpdatePrefabInfo = false;
            }

            if (debugger.LogAllInformation)
            {
                GameObject prefab; //= PlayerRegistry.LocalPlayer.GameInput.Raycaster.CurrentObject.gameObject;
                RaycastHit hit;
                Physics.Raycast(PlayerRegistry.LocalPlayer.PlayerCamera.transform.position, PlayerRegistry.LocalPlayer.PlayerCamera.transform.forward, out hit, 15f, ~(1 << LayerMask.NameToLayer("Player")));

                if (hit.collider == null)
                {
                    Logger.Exception("Log information: Raycast did not hit anything.");
                    debugger.LogAllInformation = false;
                    return;
                }

                prefab = hit.collider.gameObject;

                Logger.Log("--------------- HIT COLLIDER ----------------");
                Logger.Log();
                Logger.Log($"Tag: {hit.collider.tag}");
                PrintAll(prefab);

                debugger.LogAllInformation = false;
            }
        }

        public static void PrintAll(GameObject prefab)
        {
            try
            {
                Logger.Log();
                Logger.Log("---------------------------------------------");
                Logger.Log($"ANALYZING PREFAB OF NAME '{prefab.name}':");
                Logger.Log("----------------- HERITAGE ------------------");
                Logger.Log();
                PrintHeritage(prefab.transform, 0, true);
                Logger.Log();
                Logger.Log("---------------- COMPONENTS -----------------");
                PrintComponents(prefab);
                Logger.Log();
                Logger.Log("---------------------------------------------");
                Logger.Log();
            }
            catch (Exception ex)
            {
                Logger.Exception("Error during log all info: " + ex);
            }
        }

        public static List<Component> GetAllComponentsOfType(Transform parent, Type type)
        {
            //_returnedComponents.Clear();
            _returnedComponents = new List<Component>();

            GetComponentOfType(parent, type);

            return _returnedComponents;
        }

        private static void GetComponentOfType(Transform parent, Type type)
        {
            Component comp = parent.GetComponent(type);
            if (comp != null) _returnedComponents.Add(comp);

            for (int i = 0; i < parent.childCount; i++)
            {
                GetComponentOfType(parent.GetChild(i), type);
            }
        }

        private static List<Component> _returnedComponents = new List<Component>();

        public static void PrintHeritage(Transform parent, int depth, bool withShader = false)
        {
            string depthStr = "";

            for (int i=0; i<depth; i++) depthStr += " ";

            string info = depthStr + parent.name;
            if (withShader)
            {
                Shader shader = parent.gameObject.GetComponent<Renderer>()?.material?.shader;
                if (shader != null)
                {
                    info += $" Shader found - '{shader.name}'";
                    int n = shader.GetPropertyCount();

                    for (int j = 0; j < n; j++)
                    {
                        info += $" |{j}| {shader.GetPropertyName(j)} - {(shader.GetPropertyType(j))}";
                    }
                }
                else
                {
                    info += " No shader found.";
                }
            }

            Logger.Log(info);

            for (int i = 0; i < parent.childCount; i++)
            {
                PrintHeritage(parent.GetChild(i), depth + 1, withShader);
            }
        }

        public static void PrintComponents(GameObject prefab)
        {
            Component[] components = prefab.GetComponents<Component>();
            foreach (Component c in components)
            {
                Logger.Log();
                Logger.Log("----- Deepest type name: " + c.GetType().Name + " -----");

                PrintFields(c.GetType(), c);
                Type baseType = c.GetType().BaseType;

                while(baseType != null && baseType != typeof(Component) && baseType != typeof(MonoBehaviour))
                {
                    Logger.Log();
                    Logger.Log("Inherited type name: " + baseType.Name);
                    PrintFields(baseType, c);
                    baseType = baseType.BaseType;
                }
            }
        }

        public static void PrintFields(Type type, object instance)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(instance);
                Logger.Log($"Field: {field.Name}  |1|  Value: {value}  |2|  Type: {field.FieldType}  |3|  Access: {(field.IsPrivate ? "private" : "public")}  |4|  Static: {field.IsStatic})");
            }
        }

        private static string GetFolder(InteractiveObjectClassification classification)
        {
            switch (classification)
            {
                case InteractiveObjectClassification.Craftable:
                    return "Crafting/";

                case InteractiveObjectClassification.Tool:
                    return "Tools/";

                case InteractiveObjectClassification.Medical:
                    return "Medical/";

                case InteractiveObjectClassification.Food:
                    return "Food/";

                case InteractiveObjectClassification.Awesome:
                    return "Easter Eggs/";
            }

            return "";
        }

        private static void FindObjectInfo(BaseObject baseObject)
        {
            ChosenPrefabCraftingType[0] = (int)baseObject.CraftingType.AttributeType;
            ChosenPrefabCraftingType[1] = (int)baseObject.CraftingType.InteractiveType;

            if (!ChosenPrefabName.StartsWith(Framework.MODDED_ITEM_NAME_START))
            {
                ChosenPrefabPrefabPath = "Prefabs/StrandedObjects/";
                ChosenPrefabIconPath = "Icons/Medium/";

                if (baseObject as InteractiveObject != null) ChosenPrefabPrefabPath += GetFolder((baseObject as InteractiveObject).Classification);
                else ChosenPrefabPrefabPath += "UNKNOWN_FOLDER/";

                try
                {
                    string prefabName = ChosenPrefabName;

                    prefabName = Regex.Replace(prefabName, @"\s*\(.*?\)\s*", "");
                    ChosenPrefabPrefabPath += prefabName;

                    prefabName = default;
                    prefabName += (InteractiveType)ChosenPrefabCraftingType[1];

                    if (ChosenPrefabCraftingType[0] != 0 && ChosenPrefabCraftingType[0] != -1)
                    {
                        string postfix = string.Empty;
                        postfix += (AttributeType)ChosenPrefabCraftingType[0];
                        postfix = postfix.ToUpper();
                        prefabName += "-" + postfix;
                    }

                    ChosenPrefabIconPath += prefabName;
                }
                catch (Exception ex)
                {
                    Logger.Exception($"Error while trying to find icon/prefab paths: {ex}");
                }
            }
            else
            {
                try
                {
                    ChosenBaseObject = baseObject;
                    ChosenBasePrefabName = ChosenBaseObject.name;
                    ChosenPrefabPrefabPath = baseObject.name;
                    ChosenPrefabIconPath = baseObject.name;
                }
                catch
                {
                    ChosenPrefab = null;
                }
            }

            ChosenBasePrefabId = baseObject.PrefabId;
            ChosenBasePrefabMiniGuid = baseObject.ReferenceId;
        }

        public static bool ValidateMedical(InteractiveObject_MEDICAL item, PlayerEffect effect, bool add)
        {
            IPlayer user = PlayerUtilities.GetPlayerFromId(item.OwnerId);

            if (user != null && item != null && item.DurabilityPoints > 0)
            {
                return ValidateEffect(effect, add, user);
            }

            return false;
        }

        public static bool ValidateFood(InteractiveObject_FOOD item, PlayerEffect effect, bool add)
        {
            IPlayer user = PlayerUtilities.GetPlayerFromId(item.OwnerId);

            if (user != null && item != null && item.Servings > 0)
            {
                return ValidateEffect(effect, add, user);
            }

            return false;
        }

        private static bool ValidateEffect(PlayerEffect effect, bool add, IPlayer user)
        {
            if (add)
            {
                return user.Statistics.ApplyStatusEffect(effect);
            }
            else
            {
                return user.Statistics.RemoveStatusEffect(effect);
            }
        }

        public static AudioClip MedicalConsumableSound(IConsumableSound customSound)
        {
            return FoodConsumableSound(true, customSound);
        }

        public static AudioClip FoodConsumableSound(bool isWater, IConsumableSound customSound)
        {
            if (customSound != null)
            {
                if (customSound.ConsumableClip == null) Logger.Exception("Consume sound attached to IConsumableSound is null.");
                return customSound.ConsumableClip;
            }
            else if (isWater) return BasicSounds.DrinkSound;
            else return BasicSounds.EatSound;
        }

        public static InteractiveObject ChosenPrefab { get; set; }
        public static int[] ChosenPrefabCraftingType = { 0, 0 };
        public static string ChosenPrefabName = "none";
        public static string ChosenPrefabPrefabPath = "none";
        public static string ChosenPrefabIconPath = "none";
        public static string InstantiationPrefab = Framework.MODDED_ITEM_NAME_START;

        public static BaseObject ChosenBaseObject { get; set; }
        public static uint ChosenBasePrefabId = 0;
        public static string ChosenBasePrefabName = "none";
        public static Funlabs.MiniGuid ChosenBasePrefabMiniGuid = default;

        private static Beam.Language.LocalizedText _notificationTitle = new Beam.Language.LocalizedText("Item framework", Array.Empty<string>());
    }
}