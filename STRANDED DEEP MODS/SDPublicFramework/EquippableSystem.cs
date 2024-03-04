using Beam;
using System.Collections.Generic;

namespace SDPublicFramework
{
    public static class EquippableSystem
    {
        public enum EquippableArea
        {
            Head,
            Torso,
            Arms,
            Hands,
            Legs,
            Feet
        }

        public static List<IEquippable> EquippedItems { get { return _equippedEquippables; } }

        internal static void Initialize()
        {
            if (_equippedEquippables != null) _equippedEquippables.Clear();
            _equippedEquippables = new List<IEquippable>();
        }

        /// <summary> Stores unequipped item in hands, inventory or on ground. </summary>
        public static void HandleItemUnequip(IPlayer player, InteractiveObject equippable)
        {
            if (player != null)
            {
                if (player.Holder.CurrentObject == null)
                {
                    player.Holder.Pickup(equippable, false);
                }
                else if (!player.Inventory.GetSlotStorage().Push(equippable))
                {
                    CatUtility.PopNotification($"No space in inventory. {equippable.DisplayName} dropped on ground instead.", 5f);
                    equippable.transform.position = player.transform.position;
                    equippable.transform.rotation = player.transform.rotation;
                }
                else CatUtility.PopNotification($"{equippable.DisplayName} stored in inventory.", 4f);
            }
            else Logger.Warning($"Player passed in HandleItemUnequip is null");
        }

        public static void TryEquip(IEquippable newEquippable, bool initial = false)
        {
            foreach (IEquippable existingEquippable in _equippedEquippables)
            {
                if (existingEquippable.EquippableArea == newEquippable.EquippableArea)
                {
                    CatUtility.PopNotification($"{existingEquippable.EquippableArea} already busy with equipment!", 3f);
                    return;
                }
            }

            _equippedEquippables.Add(newEquippable);
            newEquippable.OnEquip(initial);
        }

        public static void UnequipAll()
        {
            if (_equippedEquippables.Count == 0) return;

            foreach (IEquippable existingEquippable in _equippedEquippables)
            {
                existingEquippable.OnUnequip();
            }

            _equippedEquippables.Clear();
        }

        private static List<IEquippable> _equippedEquippables;
    }
}