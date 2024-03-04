using Beam.Crafting;
using Beam.Events;
using Beam;
using HarmonyLib;
using System.Reflection;
using SDPublicFramework;
using System.Collections.Generic;
using System;

namespace CatsItems
{
    internal class LocalCraftingLogic
    {
        internal static void Initialize()
        {
            Framework.CraftingLogic.Add("regular_flask", (baseObject, crafter, combination, player, cachedItems) => FlaskFromDrinkableCoconut(baseObject, cachedItems));
            Framework.CraftingLogic.Add("regular_corrugatedscrap", (baseObject, combination, crafter, player, cachedItems) => CorrugatedScrapFromBarrel(baseObject, crafter, player, combination));
            Framework.CraftingLogic.Add("regular_endgamecrate", (baseObject, combination, crafter, player, cachedItems) => CraftEndgameCrate(baseObject));
            Framework.CraftingLogic.Add("regular_barrel", (baseObject, combination, crafter, player, cachedItems) => CraftBarrel(baseObject, player));
        }

        private static bool FlaskFromDrinkableCoconut(BaseObject baseObject, IList<IBase> cachedItems)
        {
            foreach (IBase item in cachedItems)
            {
                if (item.gameObject.name.Contains("COCONUT_DRINKABLE"))
                {
                    baseObject.gameObject.GetComponent<InteractiveObject_FOOD>().Servings = item.gameObject.GetComponent<InteractiveObject_FOOD>().Servings;
                    break;
                }
            }

            return true;
        }

        private static bool CorrugatedScrapFromBarrel(BaseObject baseObject, Crafter crafter, IPlayer player, CraftingCombination combination)
        {
            InteractiveObject interactiveObject = baseObject as InteractiveObject;

            if (player.Holder.CurrentObject == null) player.Holder.Pickup(interactiveObject, false);
            else if (player.Inventory.GetSlotStorage().CanPush(interactiveObject))
            {
                player.Inventory.GetSlotStorage().Push(interactiveObject);
                CatUtility.PopNotification(baseObject.DisplayName + " stored in the inventory.", 4.5f);
            }
            else
            {
                baseObject.gameObject.transform.position = player.transform.position;
                baseObject.gameObject.transform.rotation = player.transform.rotation;
                CatUtility.PopNotification("No space in inventory. " + baseObject.DisplayName + " dropped on ground instead.", 5f);
            }

            EventManager.RaiseEvent<ExperienceEvent>(new ExperienceEvent(player.Id, PlayerSkills.Skills.CRAFTSMANSHIP, combination.ExpRewardOnCraft));
            AccessTools.Method(typeof(Crafter), "DestroyBackupMaterials").Invoke(crafter, new object[] { });
            AccessTools.Method(typeof(Crafter), "CleanupCachedMaterials").Invoke(crafter, new object[] { });

            return false;
        }

        private static bool CraftBarrel(BaseObject baseObject, IPlayer player)
        {
            baseObject.transform.position = player.transform.position;

            return true;
        }

        private static bool CraftEndgameCrate(BaseObject baseObject)
        {
            List<StorageSlot<IPickupable>> slots = (List<StorageSlot<IPickupable>>)typeof(SlotStorage).GetField("_slotData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue((baseObject as Interactive_STORAGE).GetSlotStorage());

            foreach (StorageSlot<IPickupable> slot in slots)
            {
                slot.Objects.Clear();
            }

            return true;
        }
    }
}
