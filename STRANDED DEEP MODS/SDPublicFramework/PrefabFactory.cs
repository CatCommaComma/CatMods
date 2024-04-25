using Beam;
using UnityEngine;
using Beam.Utilities;
using System;
using System.Reflection;
using System.Collections.Generic;
using Beam.Crafting;
using System.IO;
using Funlabs;

namespace SDPublicFramework
{
    public static class PrefabFactory
    {
        public static GameObject CreateModdedObject(StreamReader reader, out uint id)
        {
            string objectName = "NAME_COULD_NOT_BE_READ";

            try
            {
                objectName = Framework.DeserializeValue("name", reader);
                Logger.Debugger($"Creating Object of name {objectName}.");

                string modelName = Framework.DeserializeValue("model", reader);

                string[] properties = Framework.DeserializeAll("specialproperty", reader);
                PrefabProperty[] specialproperties = new PrefabProperty[] { (PrefabProperty)int.Parse(properties[1]), (PrefabProperty)int.Parse(properties[2]) };

                GameObject prefab = SetupBaseObject(objectName, modelName, specialproperties, reader, out id).gameObject;
                prefab.name = objectName;
                prefab.SetActive(false);

                Logger.Debugger("Object setup succesful.");
                return prefab;

            }
            catch (Exception ex)
            {
                Logger.Exception($"Prefabs: Error while creating modded object ['{objectName}']: {ex}");
            }

            id = 0U;
            return null;
        }

        public static BaseObject InstantiateModdedObject(string prefabname, bool setActive = true)
        {
            GameObject foundObject = Framework.GetModdedPrefab(prefabname);

            if (foundObject != null)
            {
                BaseObject newInstance = UnityEngine.Object.Instantiate<GameObject>(foundObject).GetComponent<BaseObject>();
                newInstance.gameObject.SetActive(setActive);
                newInstance.DisplayName = foundObject.GetComponent<BaseObject>().DisplayName;
                newInstance.ReferenceId = MiniGuid.New();

                return newInstance;
            }

            return null;
        }

        public static BaseObject InstantiateModdedObject(uint id, bool setActive = true)
        {
            GameObject foundObject = Framework.GetModdedPrefab(id);

            if (foundObject != null)
            {
                BaseObject newInstance = UnityEngine.Object.Instantiate<GameObject>(foundObject).GetComponent<BaseObject>();
                newInstance.gameObject.SetActive(setActive);
                newInstance.DisplayName = foundObject.GetComponent<BaseObject>().DisplayName;

                return newInstance;
            }

            return null;
        }

        public static GameObject InstantiateProp(string prefabname, bool setActive = true)
        {
            GameObject foundObject = Framework.GetModdedPrefab(prefabname);

            if (foundObject != null)
            {
                GameObject newInstance = UnityEngine.Object.Instantiate<GameObject>(foundObject);
                newInstance.gameObject.SetActive(setActive);

                return newInstance;
            }

            return null;
        }

        private static GameObject SetupBaseObject(string prefabname, string modelName, PrefabProperty[] properties, StreamReader sr, out uint id)
        {
            GameObject prefab = null;
            bool vanillaItem = false;
            id = 0U;

            try
            {
                if (modelName.StartsWith("mod"))
                {
                    foreach (AssetBundle bundle in Framework.AssetBundles)
                    {
                        if (bundle.Contains(modelName))
                        {
                            prefab = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>(modelName));
                            Logger.Debugger("Prefabs: Object found in bundle.");
                            break;
                        }
                    }
                }
                else
                {
                    vanillaItem = true;
                    bool stripVanillaItem = bool.Parse(Framework.DeserializeValue("strip", sr));

                    prefab = MultiplayerMng.Instantiate(Resources.Load<GameObject>(modelName));

                    if (stripVanillaItem)
                    {
                        Component[] components = prefab.GetComponents<Component>();
                        foreach (Component component in components)
                        {
                            Type componentType = component.GetType();

                            //removing the multiplayer component too for safety
                            //i'm also making an assumption that vanilla items only have custom components on the top parent
                            if (componentType == typeof(Rigidbody) || componentType == typeof(BoxCollider) ||
                                componentType == typeof(CapsuleCollider) || componentType == typeof(SphereCollider) ||
                                componentType == typeof(MeshCollider) || componentType == typeof(Transform)) continue;

                            UnityEngine.Object.DestroyImmediate(component);
                        }
                    }

                    bool overrideValues = bool.Parse(Framework.DeserializeValue("override", sr));
                    if (!overrideValues)
                    {
                        prefab.SetActive(false);
                        id = uint.Parse(Framework.DeserializeValue("prefabid", sr));
                        prefab.GetComponent<BaseObject>().PrefabId = id;

                        string[] vcrafttype = Framework.DeserializeAll("craftingtype", sr);
                        CraftingType vct = new CraftingType((AttributeType)int.Parse(vcrafttype[1]), (InteractiveType)int.Parse(vcrafttype[2]));
                        fi_craftingType.SetValue(prefab.GetComponent<BaseObject>(), vct);

                        return prefab;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception($"Prefabs: Error loading prefab of name [{prefabname}]: {ex}");
                return null;
            }

            prefab.SetActive(false);
            prefab.SetLayerRecursively(Layers.INTERACTIVE_OBJECTS);

            if (properties[0] == PrefabProperty.CUSTOMCONSTRUCTION)
            {
                id = uint.Parse(Framework.DeserializeValue("prefabid", sr));
                return SetupCustomConstruction(prefabname, sr, prefab, id);
            }
            else if (properties[0] == PrefabProperty.CONSTRUCTION)
            {
                return FinishConstruction(prefabname, sr, prefab, id, null);
            }

            InteractiveObject interactiveComponent = null;
            id = uint.Parse(Framework.DeserializeValue("prefabid", sr));
            string[] crafttype = Framework.DeserializeAll("craftingtype", sr);
            CraftingType ct = new CraftingType((AttributeType)int.Parse(crafttype[1]), (InteractiveType)int.Parse(crafttype[2]));

            switch (properties[0])
            {
                case PrefabProperty.TOOL:
                    if (!vanillaItem) interactiveComponent = prefab.AddComponent<InteractiveObject>();
                    else
                    {
                        interactiveComponent = prefab.GetComponent<InteractiveObject>();
                        if (interactiveComponent == null) interactiveComponent = prefab.AddComponent<InteractiveObject>();
                    }
                    break;

                case PrefabProperty.MEDICAL:
                    prefab = SetupMedical(prefabname, sr, prefab, out interactiveComponent, !vanillaItem);
                    break;

                case PrefabProperty.FOOD:
                    prefab = SetupFood(prefabname, sr, prefab, out interactiveComponent, !vanillaItem);
                    break;

                case PrefabProperty.RECIPE:
                    prefab = SetupRecipe(prefabname, sr, prefab, out interactiveComponent, !vanillaItem);
                    break;

                case PrefabProperty.CUSTOMITEM:
                    prefab = SetupCustomItem(prefabname, sr, prefab, out interactiveComponent, properties[1], id);
                    break;

                case PrefabProperty.PROP:
                    return SetupProp(prefabname, sr, prefab);
            }

            if (interactiveComponent == null)
            {
                Logger.Exception($"Prefabs: InteractiveObject component of {prefabname} is null.");
                return null;
            }

            interactiveComponent.PrefabId = id;
            interactiveComponent.name = prefabname;

            fi_craftingType.SetValue(prefab.GetComponent<BaseObject>(), ct);

            interactiveComponent.DisplayName = Framework.DeserializeValue("displayname", sr);
            fi_originalDisplayName.SetValue(interactiveComponent, interactiveComponent.DisplayName);
            fi_descriptionTerm.SetValue(interactiveComponent, Framework.DeserializeValue("descriptionterm", sr));

            prefab = FinishInteractiveObject(prefabname, sr, prefab, interactiveComponent);

            UnityEngine.Object.DontDestroyOnLoad(prefab);
            return prefab;
        }

        private static GameObject SetupMedical(string prefabname, StreamReader sr, GameObject prefab, out InteractiveObject interactiveComponent, bool includeComponent = true)
        {
            InteractiveObject_MEDICAL medicalComponent;

            if (includeComponent) medicalComponent = prefab.AddComponent<InteractiveObject_MEDICAL>();
            else
            {
                medicalComponent = prefab.GetComponent<InteractiveObject_MEDICAL>();

                if (medicalComponent == null)
                {
                    Logger.Exception($"Prefabs: Medical component of {prefabname} is null.");
                    interactiveComponent = null;
                    return null;
                }
            }

            bool refund = bool.Parse(Framework.DeserializeValue("refund", sr));
            if (refund) fi_refundPrefabId.SetValue(medicalComponent, uint.Parse(Framework.DeserializeValue("refundid", sr)));

            interactiveComponent = medicalComponent;
            return prefab;
        }

        private static GameObject SetupFood(string prefabname, StreamReader sr, GameObject prefab, out InteractiveObject interactiveComponent, bool includeComponent = true)
        {
            InteractiveObject_FOOD foodComponent;

            if (includeComponent) foodComponent = prefab.AddComponent<InteractiveObject_FOOD>();
            else
            {
                foodComponent = prefab.GetComponent<InteractiveObject_FOOD>();

                if (foodComponent == null)
                {
                    Logger.Exception($"Prefabs: Food component of {prefabname} is null.");
                    interactiveComponent = null;
                    return null;
                }
            }

            foodComponent.Calories = int.Parse(Framework.DeserializeValue("calories", sr));
            foodComponent.Hydration = int.Parse(Framework.DeserializeValue("hydration", sr));
            foodComponent.Servings = int.Parse(Framework.DeserializeValue("servings", sr));

            pi_originalServings.SetValue(foodComponent, int.Parse(Framework.DeserializeValue("originalservings", sr)));
            fi_destroyOnEmptyServings.SetValue(foodComponent, bool.Parse(Framework.DeserializeValue("destroyonemptyservings", sr)));
            fi_startWithEmptyServings.SetValue(foodComponent, bool.Parse(Framework.DeserializeValue("startwithemptyservings", sr)));
            fi_vomitChance.SetValue(foodComponent, float.Parse(Framework.DeserializeValue("vomitchance", sr)));
            fi_isPoisonous.SetValue(foodComponent, bool.Parse(Framework.DeserializeValue("ispoisonous", sr)));

            foodComponent.Provenance = (MeatProvenance)int.Parse(Framework.DeserializeValue("provenance", sr));

            foodComponent.CanAttach = bool.Parse(Framework.DeserializeValue("canattach", sr));
            if (foodComponent.CanAttach)
            {
                /*****  v v v attach to spit/campfire location and rotation v v v  *****/
                Vector3 loc = default;

                string[] anchorc = Framework.DeserializeAll("anchor", sr);
                float xc = float.Parse(anchorc[1]), yc = float.Parse(anchorc[2]), zc = float.Parse(anchorc[3]);
                loc.Set(xc, yc, zc);

                Quaternion rot = default;
                rot.Set(0, 0, 0, 0);

                StaticAttachingAnchor anch = new StaticAttachingAnchor(loc, rot);
                fi_attachingAnchor.SetValue(foodComponent, anch);

                float cooktime = float.Parse(Framework.DeserializeValue("cooktime", sr));
                float smoketime = float.Parse(Framework.DeserializeValue("smoketime", sr));

                Cooking cook = prefab.AddComponent<Cooking>();
                fi_cookingHours.SetValue(cook, cooktime);
                cook.IsCooked = false;

                fi_originalCookingHours.SetValue(cook, cooktime);
                fi_cookingFood.SetValue(cook, foodComponent);
                fi_hasToBeCooked.SetValue(cook, true);

                Smoking smok = prefab.AddComponent<Smoking>();
                fi_smokingHours.SetValue(smok, smoketime);
                fi_originalSmokingHours.SetValue(smok, smoketime);
                fi_smokingFood.SetValue(smok, foodComponent);

                fi_cooking.SetValue(foodComponent, cook);
                fi_smoking.SetValue(foodComponent, smok);
            }
            foodComponent.Spoilable = bool.Parse(Framework.DeserializeValue("spoilable", sr));

            interactiveComponent = foodComponent;
            return prefab;
        }

        private static GameObject SetupRecipe(string prefabname, StreamReader sr, GameObject prefab, out InteractiveObject interactiveComponent, bool includeComponent = true)
        {
            InteractiveObject_Recipe recipeComponent;

            if (includeComponent) recipeComponent = prefab.AddComponent<InteractiveObject_Recipe>();
            else
            {
                recipeComponent = prefab.GetComponent<InteractiveObject_Recipe>();

                if (recipeComponent == null)
                {
                    Logger.Exception($"Prefabs: Recipe component of {prefabname} is null.");
                    interactiveComponent = null;
                    return null;
                }
            }

            recipeComponent.UnlockedRecipeNames = new List<string>();

            string[] unlocks = sr.ReadLine().Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < unlocks.Length; i++)
            {
                recipeComponent.UnlockedRecipeNames.Add(unlocks[i]);
            }

            interactiveComponent = recipeComponent;
            return prefab;
        }

        private static GameObject SetupCustomItem(string prefabname, StreamReader sr, GameObject prefab, out InteractiveObject interactiveComponent, PrefabProperty secondaryProperty, uint id)
        {
            Type customType = Framework.CustomTypes[id];

            if (customType == null)
            {
                Logger.Exception($"Prefabs: Custom class of {prefabname} is null.");
                interactiveComponent = null;
                return null;
            }

            var customComponent = prefab.AddComponent(customType);
            interactiveComponent = customComponent as InteractiveObject;

            switch (secondaryProperty)
            {
                case PrefabProperty.MEDICAL:
                    prefab = SetupMedical(prefabname, sr, prefab, out interactiveComponent, false);
                    break;

                case PrefabProperty.FOOD:
                    prefab = SetupFood(prefabname, sr, prefab, out interactiveComponent, false);
                    break;

                case PrefabProperty.RECIPE:
                    prefab = SetupRecipe(prefabname, sr, prefab, out interactiveComponent, false);
                    break;
            }

            return prefab;
        }

        private static GameObject FinishInteractiveObject(string prefabname, StreamReader sr, GameObject prefab, InteractiveObject interactiveComponent)
        {
            interactiveComponent.CanDrag = bool.Parse(Framework.DeserializeValue("candrag", sr));
            fi_isDraggable.SetValue(interactiveComponent, interactiveComponent.CanDrag);
            interactiveComponent.CanPickUp = bool.Parse(Framework.DeserializeValue("canpickup", sr));
            interactiveComponent.CanUse = bool.Parse(Framework.DeserializeValue("canuse", sr));
            interactiveComponent.CanInteract = bool.Parse(Framework.DeserializeValue("caninteract", sr));
            interactiveComponent.CanUseOnObjects = bool.Parse(Framework.DeserializeValue("canuseonobjects", sr));
            interactiveComponent.CanDamage = bool.Parse(Framework.DeserializeValue("candamage", sr));
            interactiveComponent.IsDamageable = bool.Parse(Framework.DeserializeValue("isdamageable", sr));

            interactiveComponent.CanParent = true;
            fi_invincible.SetValue(interactiveComponent, bool.Parse(Framework.DeserializeValue("invincible", sr)));
            fi_nonKinematic.SetValue(interactiveComponent, true);
            fi_damage.SetValue(interactiveComponent, float.Parse(Framework.DeserializeValue("damage", sr)));

            interactiveComponent.HealthPoints = float.Parse(Framework.DeserializeValue("healthpoints", sr));

            string originalDurabilityPoints = Framework.DeserializeValue("originaldurabilitypoints", sr);
            pi_originalDurabilityPoints.SetValue(interactiveComponent, float.Parse(originalDurabilityPoints));
            interactiveComponent.DurabilityPoints = float.Parse(originalDurabilityPoints);

            /*****************************************/

            Vector3 holdingInfo = default;

            string[] holdingLocation = Framework.DeserializeAll("localholdingposition", sr);
            float x = float.Parse(holdingLocation[1]), y = float.Parse(holdingLocation[2]), z = float.Parse(holdingLocation[3]);
            holdingInfo.Set(x, y, z);
            fi_localHoldingPosition.SetValue(interactiveComponent, holdingInfo);

            string[] holdingRotation = Framework.DeserializeAll("localholdingrotation", sr);
            x = float.Parse(holdingRotation[1]); y = float.Parse(holdingRotation[2]); z = float.Parse(holdingRotation[3]);
            holdingInfo.Set(x, y, z);
            fi_localHoldingRotation.SetValue(interactiveComponent, holdingInfo);

            fi_classification.SetValue(interactiveComponent, (InteractiveObjectClassification)int.Parse(Framework.DeserializeValue("classification", sr)));
            fi_idle.SetValue(interactiveComponent, (AnimationType)int.Parse(Framework.DeserializeValue("idle", sr)));
            fi_primary.SetValue(interactiveComponent, (AnimationType)int.Parse(Framework.DeserializeValue("primary", sr)));
            fi_primaryOnObject.SetValue(interactiveComponent, (AnimationType)int.Parse(Framework.DeserializeValue("primaryonobject", sr)));

            /*****************************************/

            Renderer[] allRenderers = SetupShaders(prefab);
            fi_renderers.SetValue(interactiveComponent, allRenderers);

            /*****************************************/

            Rigidbody rigidBody = prefab.GetComponent<Rigidbody>();

            fi_interactiveRigidbody.SetValue(interactiveComponent, rigidBody);

            List<Component> colliderComponents = CatUtility.GetAllComponentsOfType(prefab.transform, typeof(Collider));
            Collider[] colliders = new Collider[colliderComponents.Count];

            for (int i=0; i<colliders.Length; i++)
            {
                colliders[i] = (Collider)colliderComponents[i];
            }
            fi_colliders.SetValue(interactiveComponent, colliders);

            if (allRenderers == null || allRenderers.Length == 0)
            {
                Logger.Exception($"Renderers of " + prefabname + " are null or non existent. Object will be invisible.");
            }

            if (colliders == null || colliders.Length == 0)
            {
                Logger.Exception($"Colliders of {prefabname} are null. Object will ignore all other colliders.");
            }

            if (rigidBody == null)
            {
                Logger.Warning($"Rigidbody of {prefabname} is null. Object will not be affected by gravity and other rigidbody specific parameters.");
            }

            pi_originalDrag.SetValue(interactiveComponent, rigidBody.drag);
            pi_originalAngularDrag.SetValue(interactiveComponent, rigidBody.angularDrag);

            bool buoyant = bool.Parse(Framework.DeserializeValue("buoyant", sr));
            if (buoyant)
            {
                Buoyancy buoy = prefab.AddComponent<Buoyancy>();
                fi_buoyancyRigidbody.SetValue(buoy, rigidBody);
            }

            bool lootable = bool.Parse(Framework.DeserializeValue("injecttoloot", sr));
            if (lootable)
            {
                float weight = float.Parse(Framework.DeserializeValue("weight", sr));
                Framework.ModdedItemLootTable.Add(interactiveComponent.PrefabId, weight);
            }

            interactiveComponent.enabled = true;
            return prefab;
        }

        public static GameObject SetupProp(string prefabname, StreamReader sr, GameObject prefab)
        {
            SetupShaders(prefab);
            prefab.SetLayerRecursively(Layers.INTERACTIVE_OBJECTS);

            return prefab;
        }

        public static Renderer[] SetupShaders(GameObject prefab)
        {
            Renderer rend = prefab.GetComponent<Renderer>();
            if (rend != null)
            {
                Material eee = rend.sharedMaterial;
                if (eee.shader.name == "Standard") eee.shader = Shader.Find("Standard (Extra)");
                eee.SetFloat("_Cutoff", 0.99f);
                eee.SetFloat("_Glossiness", 0f);
            }

            Renderer[] rends = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
            {
                Material eee = r.sharedMaterial;
                if (eee.shader.name == "Standard") eee.shader = Shader.Find("Standard (Extra)");
                eee.SetFloat("_Cutoff", 0.99f);
                eee.SetFloat("_Glossiness", 0f);
            }
            return rends;
        }

        internal static void InitializeFieldInfos()
        {
            fi_originalDisplayName = typeof(BaseObject).GetField("_originalDisplayName", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_descriptionTerm = typeof(InteractiveObject).GetField("_descriptionTerm", BindingFlags.Instance | BindingFlags.NonPublic);

            fi_refundPrefabId = typeof(InteractiveObject_MEDICAL).GetField("_refundPrefabId", BindingFlags.Instance | BindingFlags.NonPublic);

            pi_originalServings = typeof(InteractiveObject_FOOD).GetProperty(nameof(InteractiveObject_FOOD.OriginalServings));
            fi_destroyOnEmptyServings = typeof(InteractiveObject_FOOD).GetField("_destroyOnEmptyServings", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_startWithEmptyServings = typeof(InteractiveObject_FOOD).GetField("_startWithEmptyServings", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_vomitChance = typeof(InteractiveObject_FOOD).GetField("_vomitChance", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_isPoisonous = typeof(InteractiveObject_FOOD).GetField("_isPoisonous", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_attachingAnchor = typeof(InteractiveObject_FOOD).GetField("_attachingAnchor", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_cookingHours = typeof(Cooking).GetField("_cookingHours", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_originalCookingHours = typeof(Cooking).GetField("_originalCookingHours", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_cookingFood = typeof(Cooking).GetField("_food", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_hasToBeCooked = typeof(Cooking).GetField("_hasToBeCooked", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_smokingHours = typeof(Smoking).GetField("_smokingHours", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_originalSmokingHours = typeof(Smoking).GetField("_originalSmokingHours", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_smokingFood = typeof(Smoking).GetField("_food", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_cooking = typeof(InteractiveObject_FOOD).GetField("_cooking", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_smoking = typeof(InteractiveObject_FOOD).GetField("_smoking", BindingFlags.Instance | BindingFlags.NonPublic);

            fi_isDraggable = typeof(InteractiveObject).GetField("_isDraggable", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_invincible = typeof(InteractiveObject).GetField("_invincible", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_nonKinematic = typeof(InteractiveObject).GetField("_nonKinematic", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_damage = typeof(InteractiveObject).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);
            pi_originalDurabilityPoints = typeof(InteractiveObject).GetProperty(nameof(InteractiveObject.OriginalDurabilityPoints));
            fi_localHoldingPosition = typeof(InteractiveObject).GetField("_localHoldingPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_localHoldingRotation = typeof(InteractiveObject).GetField("_localHoldingRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_classification = typeof(InteractiveObject).GetField("_classification", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_idle = typeof(InteractiveObject).GetField("_idle", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_primary = typeof(InteractiveObject).GetField("_primary", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_primaryOnObject = typeof(InteractiveObject).GetField("_primaryOnObject", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_craftingType = typeof(BaseObject).GetField("_craftingType", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_interactiveRigidbody = typeof(InteractiveObject).GetField("_rigidbody", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_colliders = typeof(InteractiveObject).GetField("_colliders", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_renderers = typeof(InteractiveObject).GetField("_renderers", BindingFlags.Instance | BindingFlags.NonPublic);
            pi_originalDrag = typeof(InteractiveObject).GetProperty(nameof(InteractiveObject.OriginalDrag));
            pi_originalAngularDrag = typeof(InteractiveObject).GetProperty(nameof(InteractiveObject.OriginalAngularDrag));
            fi_buoyancyRigidbody = typeof(Buoyancy).GetField("_rigidbody", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal static void CleanupReflection()
        {
            fi_originalDisplayName = null;
            fi_descriptionTerm = null;

            fi_refundPrefabId = null;

            pi_originalServings = null;
            fi_destroyOnEmptyServings = null;
            fi_startWithEmptyServings = null;
            fi_vomitChance = null;
            fi_isPoisonous = null;
            fi_attachingAnchor = null;
            fi_cookingHours =           null;
            fi_originalCookingHours = null;
            fi_cookingFood = null;
            fi_hasToBeCooked = null;
            fi_smokingHours = null;
            fi_originalSmokingHours = null;
            fi_smokingFood = null;
            fi_cooking = null;
            fi_smoking = null;

            fi_isDraggable = null;
            fi_invincible = null;
            fi_nonKinematic = null;
            fi_damage = null;
            pi_originalDurabilityPoints = null;
            fi_localHoldingPosition = null;
            fi_localHoldingRotation = null;
            fi_classification = null;
            fi_idle = null;
            fi_primary = null;
            fi_primaryOnObject = null;
            fi_craftingType = null;
            fi_interactiveRigidbody = null;
            fi_colliders = null;
            fi_renderers = null;
            pi_originalDrag = null;
            pi_originalAngularDrag = null;
            fi_buoyancyRigidbody = null;
        }

        private static FieldInfo fi_originalDisplayName;
        private static FieldInfo fi_descriptionTerm;

        private static FieldInfo fi_refundPrefabId;

        private static PropertyInfo pi_originalServings;
        private static FieldInfo fi_destroyOnEmptyServings;
        private static FieldInfo fi_startWithEmptyServings;
        private static FieldInfo fi_vomitChance;
        private static FieldInfo fi_isPoisonous;
        private static FieldInfo fi_attachingAnchor;
        private static FieldInfo fi_cookingHours;
        private static FieldInfo fi_originalCookingHours;
        private static FieldInfo fi_cookingFood;
        private static FieldInfo fi_hasToBeCooked;
        private static FieldInfo fi_smokingHours;
        private static FieldInfo fi_originalSmokingHours;
        private static FieldInfo fi_smokingFood;
        private static FieldInfo fi_cooking;
        private static FieldInfo fi_smoking;

        private static FieldInfo fi_isDraggable;
        private static FieldInfo fi_invincible;
        private static FieldInfo fi_nonKinematic;
        private static FieldInfo fi_damage;
        private static PropertyInfo pi_originalDurabilityPoints;
        private static FieldInfo fi_localHoldingPosition;
        private static FieldInfo fi_localHoldingRotation;
        private static FieldInfo fi_classification;
        private static FieldInfo fi_idle;
        private static FieldInfo fi_primary;
        private static FieldInfo fi_primaryOnObject;
        private static FieldInfo fi_craftingType;
        private static FieldInfo fi_interactiveRigidbody;
        private static FieldInfo fi_colliders;
        private static FieldInfo fi_renderers;
        private static PropertyInfo pi_originalDrag;
        private static PropertyInfo pi_originalAngularDrag;
        private static FieldInfo fi_buoyancyRigidbody;

        //CONSTRUCTIONS
        private static GameObject SetupCustomConstruction(string prefabname, StreamReader sr, GameObject prefab, uint id)
        {
            Type customType = Framework.CustomTypes[id];

            if (customType == null)
            {
                Logger.Exception($"Prefabs: Custom class of {prefabname} is null.");
                return null;
            }

            Component customComponent = prefab.AddComponent(customType);
            Constructing constructingComponent = customComponent as Constructing;

            if (constructingComponent == null)
            {
                Logger.Exception($"Prefabs: Could not cast type 'Constructing' from custom type '{customType.Name}'.");
                return null;
            }

            return FinishConstruction(prefabname, sr, prefab, id, constructingComponent);
        }

        private static GameObject FinishConstruction(string prefabname, StreamReader sr, GameObject prefab, uint id, Constructing constructingComponent)
        {
            Logger.Warning("Do not use constructions, they will not work. This is a test feature.");

            bool hasConnector = true;

            //crafting and placing
            typeof(Constructing).GetField("handlePlayerColission", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, true);
            typeof(Constructing).GetField("_craftImmediate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, false);

            typeof(Constructing).GetField("_placingDistance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, 4);
            typeof(Constructing).GetField("_rotationMode", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, Constructing.ERotationMode.Free);
            typeof(Constructing).GetField("_rotationAngle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, 90);
            typeof(Constructing).GetField("_proximityDistance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, 4.5f);

            typeof(Constructing).GetField("_proximityChecking", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, ConstructingProximityCheckMode.None);

            typeof(Constructing).GetField("_ghostRenderer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, prefab.transform.Find("Ghost").GetChild(0).GetComponent<MeshRenderer>());
            typeof(Constructing).GetField("_ghostMesh", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, prefab.transform.Find("Ghost").GetChild(0).GetComponent<MeshFilter>().mesh);
            typeof(Constructing).GetField("_ghostObject", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, prefab.transform.Find("Ghost").gameObject);

            constructingComponent.MaximumHealthPoints = 100;

            //connector when first placing
            if (hasConnector)
            {
                Connector connector = prefab.AddComponent<Connector>();
                connector.Constructing = constructingComponent;

                typeof(Connector).GetField("_type", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(connector, ConnectorType.LIGHT_HOOK);
                typeof(Connector).GetField("_useTransformPositionForSnapping", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(connector, true);
                typeof(Connector).GetField("_checkType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(connector, true);
                typeof(Connector).GetField("_isValid", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(connector, true);
                typeof(Connector).GetField("_canSet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(connector, true);

                typeof(Constructing).GetField("_myConnector", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructingComponent, connector);

                //Type enumType = typeof(Connector).GetNestedType("EConnectMode", BindingFlags.NonPublic);
                //FieldInfo freeField = enumType.GetField("Free", BindingFlags.Public | BindingFlags.Static);
                //FieldInfo requireConnectorField = enumType.GetField("RequireConnector", BindingFlags.Public | BindingFlags.Static);
            }

            //other info
            string[] crafttype = Framework.DeserializeAll("craftingtype", sr);
            CraftingType ct = new CraftingType((AttributeType)int.Parse(crafttype[1]), (InteractiveType)int.Parse(crafttype[2]));
            fi_craftingType.SetValue(prefab.GetComponent<BaseObject>(), ct);

            constructingComponent.PrefabId = id;
            constructingComponent.name = prefabname;
            constructingComponent.DisplayName = "Gattling Harpoon";
            fi_originalDisplayName.SetValue(constructingComponent, constructingComponent.DisplayName);

            UnityEngine.Object.DontDestroyOnLoad(prefab);
            return prefab;
        }

        //FISHY
        private static GameObject FinishFishie(string prefabname, StreamReader sr, GameObject prefab, Constructing constructingComponent)
        {
            Shootable shootable = prefab.AddComponent<Shootable>(); //v
            Interactive_FISHES interactiveFishes = prefab.AddComponent<Interactive_FISHES>(); //v
            SingleFish singleFish = prefab.AddComponent<SingleFish>(); //v
            SingleFishRenderer singleFishRenderer = prefab.AddComponent<SingleFishRenderer>(); //v
            FishMovement fishMovement = prefab.AddComponent<FishMovement>(); //v
            FishingData fishingData = new FishingData(); //v

            Interactive_FISH fish = null;

            //fishing data
            fishingData.FishType = FishSize.Medium;
            fishingData.FishPrefab = fish;

            //single fish renderer
            typeof(SingleFishRenderer).GetField("_visible", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, false);
            typeof(SingleFishRenderer).GetField("_meshRenderer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, prefab.GetComponent<Renderer>());
            typeof(SingleFishRenderer).GetField("_visible", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, false);

            typeof(FishRendererBase).GetField("_cullingDistance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, 35f);
            typeof(FishRendererBase).GetField("_castShadows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, true);
            typeof(FishRendererBase).GetField("_receiveShadows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFishRenderer, true);

            //single fish
            typeof(SingleFish).GetField("_interestRadius", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFish, 1f);

            typeof(FishBase).GetField("_prefabId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFish, 0U);
            typeof(FishBase).GetField("_fishingData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFish, fishingData);
            typeof(FishBase).GetField("_fishRenderer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(singleFish, singleFishRenderer);

            //INTERACTIVE_FISHES
            typeof(Interactive_FISHES).GetField("_displayName", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(interactiveFishes, "fish name");
            interactiveFishes.CanParent = false;

            typeof(Interactive_FISHES).GetField("_raycastMode", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(interactiveFishes, RaycastMode.Collider);
            typeof(Interactive_FISHES).GetField("_healthPoints", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(interactiveFishes, 0);
            typeof(Interactive_FISHES).GetField("_originalHealthPoints", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(interactiveFishes, 0);
            typeof(Interactive_FISHES).GetField("_fishes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(interactiveFishes, singleFish);

            //fish movement
            typeof(FishMovement).GetField("_fish", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, singleFish);
            typeof(FishMovement).GetField("_minWorldHeight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, -7d);
            typeof(FishMovement).GetField("_maxWorldHeight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, -15f);
            typeof(FishMovement).GetField("_speed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, 1.3f);
            typeof(FishMovement).GetField("_steer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, 2.5f);
            typeof(FishMovement).GetField("_stuck", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, 0.13f);
            typeof(FishMovement).GetField("_originalSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, 1.3f);
            typeof(FishMovement).GetField("_originalSteer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, 2.5f);
            typeof(FishMovement).GetField("_hooked", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fishMovement, false);

            UnityEngine.Object.DontDestroyOnLoad(prefab);
            return prefab;
        }
    }
}

/*
 * //wip
        private static BaseObject FinishConstruction(string prefabname, StreamReader sr, GameObject prefab)
        {
            Constructing constructionComponent = prefab.GetComponent<Constructing>();

            constructionComponent.IsDamageable = true;
            constructionComponent.CanParent = true;

            constructionComponent.MaximumHealthPoints = float.Parse(Framework.DeserializeValue(sr));
            constructionComponent.HealthPoints = constructionComponent.MaximumHealthPoints;
            //constructionComponent.CanPlace = true;

            typeof(Constructing).GetField("_craftImmediate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, false);

            //connectors
            //connector layer mask
            //connector

            /*****************************************

string[] crafttype = sr.ReadLine().Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
CraftingType ct = new CraftingType((AttributeType)int.Parse(crafttype[1]), (InteractiveType)int.Parse(crafttype[2]));

FieldInfo fi_craftingType = typeof(BaseObject).GetField("_craftingType", BindingFlags.Instance | BindingFlags.NonPublic);
fi_craftingType.SetValue(prefab.GetComponent<BaseObject>(), ct);


Renderer ccc = prefab.gameObject.GetComponent<Renderer>();
if (ccc != null)
{
    typeof(Constructing).GetField("_ghostRenderer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, ccc);

    Material eee = ccc.sharedMaterial;
    eee.shader = Shader.Find("Standard (Extra)");
    eee.SetFloat("_Cutoff", 0.99f);
    eee.SetFloat("_Glossiness", 0f);
}
else
{
    Logger.Warning($"Renderer of " + prefabname + " is null. Trying another method.");

    Renderer[] rends = prefab.GetComponentsInChildren<Renderer>();

    for (int i = 0; i < rends.Length; i++)
    {
        Renderer r = rends[i];

        Material eee = r.sharedMaterial;
        eee.shader = Shader.Find("Standard (Extra)");
        eee.SetFloat("_Cutoff", 0.99f);
        eee.SetFloat("_Glossiness", 0f);

        if (i == 2) typeof(Constructing).GetField("_ghostRenderer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, r);
    }
}

/*****************************************

typeof(Constructing).GetField("_structurePrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, prefab);
typeof(Constructing).GetField("_ghostObject", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, prefab);
typeof(Constructing).GetField("_colliders", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, prefab.GetComponentsInChildren<Collider>());
typeof(Constructing).GetField("_colliderType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(constructionComponent, GhostCollider.ColliderType.VEHICLE_SAIL);

constructionComponent.enabled = true;

return constructionComponent;
        }*/