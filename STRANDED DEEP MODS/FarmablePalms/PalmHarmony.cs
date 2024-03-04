using System;
using UnityEngine;
using Beam;
using HarmonyLib;
using Beam.Crafting;
using System.Collections.Generic;
using Beam.Serialization.Json;
namespace FarmablePalms
{
    public class PalmHarmony
    {
        [HarmonyPatch(typeof(PlantModel), MethodType.Constructor, new Type[] { typeof(GameObject[]), typeof(GameObject), typeof(float) })]
        class PlantModel_Constructor_Patch
        {
            private static readonly AccessTools.FieldRef<PlantModel, GameObject[]> _plantStagesRef = AccessTools.FieldRefAccess<PlantModel, GameObject[]>("_plantStages");
            private static readonly AccessTools.FieldRef<PlantModel, GameObject> _deadStageRef = AccessTools.FieldRefAccess<PlantModel, GameObject>("_deadStage");
            private static readonly AccessTools.FieldRef<PlantModel, float> _growthTimeRef = AccessTools.FieldRefAccess<PlantModel, float>("_growthTime");
            private static readonly AccessTools.FieldRef<InteractiveObject_PALM, List<InteractiveObject_FOOD>> _fruitsRef = AccessTools.FieldRefAccess<InteractiveObject_PALM, List<InteractiveObject_FOOD>>("_fruits");

            private static bool Prefix(PlantModel __instance, GameObject[] plantStages, GameObject deadStage, float growthTime)
            {
                if (plantStages[4].gameObject.transform.parent.name == "WAVULAVULA_PLANTMODEL(Clone)") //Stage_4 wavulavula
                {
                    Transform parent = plantStages[0].transform.parent;

                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Food/COCONUT_ORANGE"));

                    gameObject.transform.rotation = parent.rotation;
                    //gameObject.transform = parent.transform;
                    gameObject.transform.localPosition = parent.localPosition;
                    gameObject.transform.position = parent.position;
                    gameObject.transform.parent = parent;

                    UnityEngine.Object.Destroy(plantStages[0]);
                    plantStages[0] = gameObject;

                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/YOUNG_PALM_2"));
                    gameObject2.transform.rotation = parent.rotation;
                    gameObject2.transform.localPosition = parent.localPosition;
                    gameObject2.transform.position = parent.position;
                    gameObject2.transform.parent = parent;
                    UnityEngine.Object.Destroy(plantStages[1]);
                    plantStages[1] = gameObject2;

                    GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/YOUNG_PALM_1"));
                    gameObject3.transform.rotation = parent.rotation;
                    gameObject3.transform.localPosition = parent.localPosition;
                    gameObject3.transform.position = parent.position;
                    gameObject3.transform.parent = parent;
                    UnityEngine.Object.Destroy(plantStages[2]);
                    plantStages[2] = gameObject3;

                    GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/PALM_4"));
                    gameObject4.transform.localScale *= 0.4f;
                    gameObject4.transform.rotation = parent.rotation;
                    gameObject4.transform.localPosition = parent.localPosition;
                    gameObject4.transform.position = parent.position;
                    gameObject4.transform.parent = parent;
                    UnityEngine.Object.Destroy(plantStages[3]);
                    plantStages[3] = gameObject4;

                    InteractiveObject_PALM interactiveObject_PALM = Resources.Load<InteractiveObject_PALM>("Prefabs/StrandedObjects/Trees/PALM_4");
                    GameObject palm = UnityEngine.Object.Instantiate<GameObject>(interactiveObject_PALM.gameObject);
                    _fruitsRef(palm.GetComponent<InteractiveObject_PALM>()).Clear();
                    palm.transform.rotation = parent.rotation;
                    palm.transform.localPosition = parent.localPosition;
                    palm.transform.parent = parent;
                    palm.SetActive(false);
                    //palm.gameObject.GetComponent<InteractiveObject_PALM>().ReferenceId = new Funlabs.MiniGuid();
                    plantStages[4].gameObject.AddComponent<FarmablePalms.FarmablePalm>();
                    plantStages[4].gameObject.GetComponent<FarmablePalms.FarmablePalm>()._farmablePalm = palm;

                    InteractiveObject_PALM interactiveObject_PALM2 = Resources.Load<InteractiveObject_PALM>("Prefabs/StrandedObjects/Trees/PALM_3");
                    GameObject palm2 = UnityEngine.Object.Instantiate<GameObject>(interactiveObject_PALM2.gameObject);
                    _fruitsRef(palm2.GetComponent<InteractiveObject_PALM>()).Clear();
                    palm2.transform.rotation = parent.rotation;
                    palm2.transform.localPosition = parent.localPosition;
                    palm2.transform.parent = parent;
                    palm2.SetActive(false);
                    //palm2.gameObject.GetComponent<InteractiveObject_PALM>().ReferenceId = new Funlabs.MiniGuid();
                    plantStages[4].gameObject.GetComponent<FarmablePalms.FarmablePalm>()._farmablePalm2 = palm2;

                    InteractiveObject_PALM interactiveObject_PALM3 = Resources.Load<InteractiveObject_PALM>("Prefabs/StrandedObjects/Trees/PALM_2");
                    GameObject palm3 = UnityEngine.Object.Instantiate<GameObject>(interactiveObject_PALM3.gameObject);
                    _fruitsRef(palm3.GetComponent<InteractiveObject_PALM>()).Clear();
                    palm3.transform.rotation = parent.rotation;
                    palm3.transform.localPosition = parent.localPosition;
                    palm3.transform.parent = parent;
                    palm3.SetActive(false);
                    //palm3.gameObject.GetComponent<InteractiveObject_PALM>().ReferenceId = new Funlabs.MiniGuid();
                    plantStages[4].gameObject.GetComponent<FarmablePalms.FarmablePalm>()._farmablePalm3 = palm3;

                    InteractiveObject_PALM interactiveObject_PALM4 = Resources.Load<InteractiveObject_PALM>("Prefabs/StrandedObjects/Trees/PALM_1");
                    GameObject palm4 = UnityEngine.Object.Instantiate<GameObject>(interactiveObject_PALM4.gameObject);
                    _fruitsRef(palm4.GetComponent<InteractiveObject_PALM>()).Clear();
                    palm4.transform.rotation = parent.rotation;
                    palm4.transform.localPosition = parent.localPosition;
                    palm4.transform.parent = parent;
                    palm4.SetActive(false);
                    //palm4.gameObject.GetComponent<InteractiveObject_PALM>().ReferenceId = new Funlabs.MiniGuid();
                    plantStages[4].gameObject.GetComponent<FarmablePalms.FarmablePalm>()._farmablePalm4 = palm4;

                    foreach (GameObject gameObject5 in plantStages)
                    {
                        UnityEngine.Object.Destroy(gameObject5.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(gameObject5.GetComponent<InteractiveObject>());
                        UnityEngine.Object.Destroy(gameObject5.GetComponent<Saveable>());
                    }
                    deadStage.gameObject.transform.localScale *= 1.7f;
                    growthTime = (float)Main.settings.growFirstStages;
                }
                _plantStagesRef(__instance) = plantStages;
                _deadStageRef(__instance) = deadStage;
                _growthTimeRef(__instance) = growthTime;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlantFruit), MethodType.Constructor, new Type[] { typeof(IFruitFactory), typeof(PlantType), typeof(Transform), typeof(float) })]
        class PlantFruit_Constructor_Patch
        {
            private static readonly AccessTools.FieldRef<PlantFruit, float> _growthTimeRef = AccessTools.FieldRefAccess<PlantFruit, float>("_growthTime");

            private static void Postfix(PlantFruit __instance, IFruitFactory fruitFactory, PlantType plantType, Transform fruitPosition, float growthTime)
            {
                if (plantType == PlantType.Wavulavula)
                {
                    growthTime = 999999;
                }

                _growthTimeRef(__instance) = growthTime;
            }
        }
        /*&& farmablePalm._farmablePalmRealSize2 == null && farmablePalm._farmablePalmRealSize3 == null && farmablePalm._farmablePalmRealSize4 == null*/

        [HarmonyPatch(typeof(PlantModel), nameof(PlantModel.UpdateStage))]
        class PlantModel_UpdateStage_Patch
        {
            private static readonly AccessTools.FieldRef<PlantModel, GameObject[]> _plantStagesRef = AccessTools.FieldRefAccess<PlantModel, GameObject[]>("_plantStages");
            private static readonly AccessTools.FieldRef<PlantModel, GameObject> _deadStageRef = AccessTools.FieldRefAccess<PlantModel, GameObject>("_deadStage");
            private static readonly AccessTools.FieldRef<PlantModel, float> _growthTimeRef = AccessTools.FieldRefAccess<PlantModel, float>("_growthTime");
            private static readonly AccessTools.FieldRef<PlantModel, DateTime> _startedGrowingTimeRef = AccessTools.FieldRefAccess<PlantModel, DateTime>("_startedGrowingTime");
            private static readonly AccessTools.FieldRef<PlantModel, int> _stageRef = AccessTools.FieldRefAccess<PlantModel, int>("_stage");
            private static readonly AccessTools.FieldRef<PlantModel, bool> _deadRef = AccessTools.FieldRefAccess<PlantModel, bool>("_dead");
            private static readonly AccessTools.FieldRef<InteractiveObject_PALM, List<InteractiveObject_FOOD>> _fruitsRef = AccessTools.FieldRefAccess<InteractiveObject_PALM, List<InteractiveObject_FOOD>>("_fruits");

            private static void Postfix(PlantModel __instance)
            {
                if (_plantStagesRef(__instance)[4].gameObject.transform.parent.name == "WAVULAVULA_PLANTMODEL(Clone)" && _stageRef(__instance) == 4 && !_deadRef(__instance))
                {
                    FarmablePalm farmablePalm = _plantStagesRef(__instance)[4].gameObject.GetComponent<FarmablePalm>();

                    float hoursSince = GameTime.GetHoursSince(_startedGrowingTimeRef(__instance));
                    _plantStagesRef(__instance)[4].SetActive(false);

                    Debug.Log("hoursSince: " + hoursSince);

                    if (farmablePalm._farmablePalmReal == null && hoursSince <= (float)Main.settings.growSecondStage)
                    {
                        farmablePalm._farmablePalmReal = UnityEngine.Object.Instantiate<GameObject>(farmablePalm._farmablePalm);
                        farmablePalm._farmablePalmReal.gameObject.AddComponent<FarmablePalms.PlotPalm>();
                        farmablePalm._farmablePalmReal.transform.position = _plantStagesRef(__instance)[4].transform.position;
                        farmablePalm._farmablePalmReal.transform.parent = farmablePalm._farmablePalm.transform.parent;
                        farmablePalm._farmablePalmReal.SetActive(true);
                        _fruitsRef(farmablePalm._farmablePalmReal.GetComponent<InteractiveObject_PALM>()).Clear();
                        farmablePalm._farmablePalmReal.gameObject.GetComponent<FarmablePalms.PlotPalm>().catLopPlot = Main.CatMagic(_plantStagesRef(__instance));
                        farmablePalm._farmablePalmReal.gameObject.GetComponent<FarmablePalms.PlotPalm>().catPalmPlantModel = __instance;
                    }
                    else if (hoursSince > (float)Main.settings.growFourthStage)
                    {
                        if (farmablePalm._farmablePalmReal != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmReal.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmReal.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmReal);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm);
                        }
                        if (farmablePalm._farmablePalmRealSize2 != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmRealSize2.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmRealSize2.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmRealSize2);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm2);
                        }
                        if (farmablePalm._farmablePalmRealSize3 != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmRealSize3.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmRealSize3.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmRealSize3);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm3);
                        }
                        if (farmablePalm._farmablePalmRealSize4 == null)
                        {
                            farmablePalm._farmablePalmRealSize4 = UnityEngine.Object.Instantiate<GameObject>(farmablePalm._farmablePalm4);
                            farmablePalm._farmablePalmRealSize4.gameObject.AddComponent<FarmablePalms.PlotPalm>();
                            farmablePalm._farmablePalmRealSize4.transform.position = _plantStagesRef(__instance)[4].transform.position;
                            farmablePalm._farmablePalmRealSize4.transform.parent = farmablePalm._farmablePalm4.transform.parent;
                            farmablePalm._farmablePalmRealSize4.SetActive(true);
                            _fruitsRef(farmablePalm._farmablePalmRealSize4.GetComponent<InteractiveObject_PALM>()).Clear();
                            farmablePalm._farmablePalmRealSize4.gameObject.GetComponent<FarmablePalms.PlotPalm>().catLopPlot = Main.CatMagic(_plantStagesRef(__instance));
                            farmablePalm._farmablePalmRealSize4.gameObject.GetComponent<FarmablePalms.PlotPalm>().catPalmPlantModel = __instance;
                        }
                    }
                    else if (hoursSince > (float)Main.settings.growThirdStage)
                    {
                        if (farmablePalm._farmablePalmReal != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmReal.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmReal.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmReal);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm);
                        }
                        if (farmablePalm._farmablePalmRealSize2 != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmRealSize2.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmRealSize2.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmRealSize2);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm2);
                        }
                        if (farmablePalm._farmablePalmRealSize3 == null)
                        {
                            farmablePalm._farmablePalmRealSize3 = UnityEngine.Object.Instantiate<GameObject>(farmablePalm._farmablePalm3);
                            farmablePalm._farmablePalmRealSize3.gameObject.AddComponent<FarmablePalms.PlotPalm>();
                            farmablePalm._farmablePalmRealSize3.transform.position = _plantStagesRef(__instance)[4].transform.position;
                            farmablePalm._farmablePalmRealSize3.transform.parent = farmablePalm._farmablePalm3.transform.parent;
                            farmablePalm._farmablePalmRealSize3.SetActive(true);
                            _fruitsRef(farmablePalm._farmablePalmRealSize3.GetComponent<InteractiveObject_PALM>()).Clear();
                            farmablePalm._farmablePalmRealSize3.gameObject.GetComponent<FarmablePalms.PlotPalm>().catLopPlot = Main.CatMagic(_plantStagesRef(__instance));
                            farmablePalm._farmablePalmRealSize3.gameObject.GetComponent<FarmablePalms.PlotPalm>().catPalmPlantModel = __instance;
                        }
                    }
                    else if (hoursSince > (float)Main.settings.growSecondStage)
                    {
                        if (farmablePalm._farmablePalmReal != null)
                        {
                            InteractiveObject_PALM palmie = farmablePalm._farmablePalmReal.GetComponent<InteractiveObject_PALM>();
                            AccessTools.Method(typeof(InteractiveObject_PALM), "DetachFruit").Invoke(palmie, null);
                            farmablePalm._farmablePalmReal.SetActive(false);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalmReal);
                            UnityEngine.Object.Destroy(farmablePalm._farmablePalm);
                        }
                        if (farmablePalm._farmablePalmRealSize2 == null)
                        {
                            farmablePalm._farmablePalmRealSize2 = UnityEngine.Object.Instantiate<GameObject>(farmablePalm._farmablePalm2);
                            farmablePalm._farmablePalmRealSize2.gameObject.AddComponent<FarmablePalms.PlotPalm>();
                            farmablePalm._farmablePalmRealSize2.transform.position = _plantStagesRef(__instance)[4].transform.position;
                            farmablePalm._farmablePalmRealSize2.transform.parent = farmablePalm._farmablePalm2.transform.parent;
                            farmablePalm._farmablePalmRealSize2.SetActive(true);
                            _fruitsRef(farmablePalm._farmablePalmRealSize2.GetComponent<InteractiveObject_PALM>()).Clear();
                            farmablePalm._farmablePalmRealSize2.gameObject.GetComponent<FarmablePalms.PlotPalm>().catLopPlot = Main.CatMagic(_plantStagesRef(__instance));
                            farmablePalm._farmablePalmRealSize2.gameObject.GetComponent<FarmablePalms.PlotPalm>().catPalmPlantModel = __instance;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InteractiveObject_PALM), nameof(InteractiveObject_PALM.Save))]
        class InteractiveObject_PALM_Save_Patch
        {
            private static void Postfix(ref JObject __result, InteractiveObject_PALM __instance)
            {
                if (__instance.gameObject.GetComponent<FarmablePalms.PlotPalm>() != null)
                {
                    Debug.Log("[SDFarmableTrees] PlotPalm component was not null!");
                    __result = new JObject();
                }
            }
        }

        [HarmonyPatch(typeof(Farming_PLOT), nameof(Farming_PLOT.InteractWithObject), new Type[] { typeof(IPlayer), typeof(IBase) })]
        class Farming_PLOT_InteractWithObject_Patch
        {
            private static readonly AccessTools.FieldRef<Farming_PLOT, Plot> _plotRef = AccessTools.FieldRefAccess<Farming_PLOT, Plot>("_plot");
            private static readonly AccessTools.FieldRef<Plantable, PlantType> _plantTypeRef = AccessTools.FieldRefAccess<Plantable, PlantType>("_plantType");

            private static void Postfix(ref bool __result, Farming_PLOT __instance, IPlayer player, IBase obj)
            {
                if (PlayerRegistry.LocalPlayer.Holder.CurrentObject != null)
                {
                    if (PlayerRegistry.LocalPlayer.Holder.CurrentObject.gameObject.name.Contains("COCONUT_ORANGE(Clone)"))
                    {
                        obj.gameObject.AddComponent<Plantable>();
                        Plantable a = obj.gameObject.GetComponent<Plantable>();
                        _plantTypeRef(a) = PlantType.Wavulavula;
                        Plantable component = obj.gameObject.GetComponent<Plantable>();
                        if (component != null)
                        {
                            bool flag = _plotRef(__instance).Plant(component);
                            if (flag)
                            {
                                Beam.Events.EventManager.RaiseEvent<CropPlantedEvent>(new CropPlantedEvent(__instance.transform.position, component.PlantType));
                                player.Holder.ReleaseAndDestroy(obj as IPickupable);
                                InteracterUnityEvent planted = __instance.Planted;
                                if (planted == null)
                                {
                                    __result = flag;
                                    return;
                                }
                                planted.Invoke(player);
                            }
                            __result = flag;
                            return;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Farming_PLOT), "Plot_CropCreated", new Type[] { typeof(PlantType) })]
        class Farming_PLOT_Plot_CropCreated_Patch
        {
            private static bool Prefix(InteractiveObject_PALM __instance, PlantType type)
            {
                if (type == PlantType.Wavulavula)
                {
                    string text = "Palm Tree";
                    __instance.DisplayNamePrefixes.AddOrIgnore(text + " - ", 0);
                    return false;
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(ZoneAttributes), MethodType.Constructor)]
        class ZoneAttributes_Constructor_Patch
        {
            private static readonly AccessTools.FieldRef<ZoneAttributes, Dictionary<PlantType, int>> _cropsRef = AccessTools.FieldRefAccess<ZoneAttributes, Dictionary<PlantType, int>>("_crops");

            private static bool Prefix(ZoneAttributes __instance)
            {
                _cropsRef(__instance) = new Dictionary<PlantType, int>();
                _cropsRef(__instance).Add(PlantType.Ajuga, 0);
                _cropsRef(__instance).Add(PlantType.Aloe, 0);
                _cropsRef(__instance).Add(PlantType.Kura, 0);
                _cropsRef(__instance).Add(PlantType.Pipi, 0);
                _cropsRef(__instance).Add(PlantType.Potato, 0);
                _cropsRef(__instance).Add(PlantType.Quwawa, 0);
                _cropsRef(__instance).Add(PlantType.Yucca, 0);
                _cropsRef(__instance).Add(PlantType.Wavulavula, 0);

                return false;
            }
        }

        [HarmonyPatch(typeof(InteractiveObject_PALM), "Lop")]
        class InteractiveObject_PALM_Lop_Patch
        {
            private static readonly AccessTools.FieldRef<Farming_PLOT, Plot> _plotRef = AccessTools.FieldRefAccess<Farming_PLOT, Plot>("_plot");
            private static readonly AccessTools.FieldRef<InteractiveObject_PALM, GameObject> _stumpRef = AccessTools.FieldRefAccess<InteractiveObject_PALM, GameObject>("_stump");

            static GameObject stump = null;
            static bool wasPlotted = false;

            private static bool Prefix(InteractiveObject_PALM __instance)
            {
                Debug.Log("[SDFarmableTrees] A palm was lopped!");
                wasPlotted = false;

                PlotPalm a = __instance.GetComponent<PlotPalm>();
                if (a != null)
                {
                    Debug.Log("[SDFarmableTrees] Plot is being cleared...");

                    stump = _stumpRef(__instance);
                    Farming_PLOT d = a.catLopPlot;
                    Plant f = (Plant)Main.fi_plant.GetValue(_plotRef(d));
                    f.Reset();

                    wasPlotted = true;

                    bool flag2 = _plotRef(d).ClearCrop();
                    if (flag2)
                    {
                        d.DisplayNamePrefixes.Clear();
                        UnityEngine.Events.UnityEvent cleared = d.Cleared;
                        __instance.gameObject.SetActive(false);
                        if (cleared == null)
                        {
                            return true;
                        }
                        cleared.Invoke();
                    }
                    Debug.Log("[SDFarmableTrees] Plot successfully cleared!");
                }
                return true;
            }

            private static void Postfix()
            {
                if (wasPlotted)
                {
                    UnityEngine.Object.Destroy(stump);
                    stump = null;
                    wasPlotted = false;
                }
            }
        }

        [HarmonyPatch(typeof(Farming_PLOT), nameof(Farming_PLOT.GetInteractionDescription), new Type[] { typeof(int), typeof(IBase) })]
        class Farming_PLOT_GetInteractionDescription_Patch
        {
            private static readonly AccessTools.FieldRef<Farming_PLOT, Plot> _plotRef = AccessTools.FieldRefAccess<Farming_PLOT, Plot>("_plot");

            private static void Postfix(Farming_PLOT __instance, ref string __result, int playerId, IBase obj)
            {
                if (PlayerRegistry.LocalPlayer.Holder.CurrentObject != null)
                {
                    if (PlayerRegistry.LocalPlayer.Holder.CurrentObject.gameObject.name.Contains("COCONUT_ORANGE(Clone)") && !_plotRef(__instance).Growing)
                    {
                        __result = "ITEM_INTERACTION_DESCRIPTION_PLANT " + PlayerRegistry.LocalPlayer.Holder.CurrentObject.DisplayName;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InteractiveObject_PALM), "GenerateFruit")]
        class InteractiveObject_PALM_GenerateFruits_Patch
        {
            private static bool Prefix(InteractiveObject_PALM __instance)
            {
                if (__instance.gameObject.GetComponent<PlotPalm>() != null)
                {
                    float hoursSince = GameTime.GetHoursSince(__instance.gameObject.GetComponent<PlotPalm>().catPalmPlantModel.StartedGrowingTime);
                    if (hoursSince < (float)Main.settings.growFourthStage)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Plot), nameof(Plot.ClearCrop))]
        class Plot_ClearCrop_Patch
        {
            private static readonly AccessTools.FieldRef<Plot, Plant> _plantRef = AccessTools.FieldRefAccess<Plot, Plant>("_plant");
            private static readonly AccessTools.FieldRef<PlantModel, GameObject[]> _plantStagesRef = AccessTools.FieldRefAccess<PlantModel, GameObject[]>("_plantStages");

            private static bool Prefix(Plot __instance, ref bool __result)
            {
                if (_plantRef(__instance) == null)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    PlantModel pm = (PlantModel)Main.fi_plantModel.GetValue(_plantRef(__instance));
                    FarmablePalm a = _plantStagesRef(pm)[4].gameObject.GetComponentInChildren<FarmablePalm>();
                    if (a != null)
                    {
                        UnityEngine.Object.Destroy(a._farmablePalmReal);
                        UnityEngine.Object.Destroy(a._farmablePalmRealSize2);
                        UnityEngine.Object.Destroy(a._farmablePalmRealSize3);
                        UnityEngine.Object.Destroy(a._farmablePalmRealSize4);

                        UnityEngine.Object.Destroy(a._farmablePalm);
                        UnityEngine.Object.Destroy(a._farmablePalm2);
                        UnityEngine.Object.Destroy(a._farmablePalm3);
                        UnityEngine.Object.Destroy(a._farmablePalm4);

                        _plantRef(__instance).Reset();
                    }
                }
                return true;
            }
        }
    }
}
