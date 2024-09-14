using System;
using Beam;
using Beam.UI;
using UnityEngine;

namespace FillableBarrels
{
    public class FillableBarrelManager
    {
        private enum BarrelType
        {
            Water,
            Fuel
        }

        internal static void HandleFillableBarrels()
        {
            if (PlayerRegistry.LocalPlayer != null && PlayerRegistry.LocalPlayer.GameInput != null)
            {
                bool triedToFill = Input.GetKeyDown(Main.settings.fill);
                bool triedToDeduct = Input.GetKeyDown(Main.settings.deduct);

                if (triedToFill || triedToDeduct)
                {
                    GameObject targetedObject = CastRay();
                    GameObject heldObject = PlayerRegistry.LocalPlayer.Holder.CurrentObject?.gameObject;

                    if (targetedObject != null && heldObject != null)
                    {
                        HandleObject(targetedObject, heldObject, triedToFill);
                    }
                }
                DoRainCatcher();
            }
        }

        private static GameObject CastRay()
        {
            Beam.Crafting.Raycaster ray = PlayerRegistry.LocalPlayer.GameInput.Raycaster;

            if (ray.CurrentObject != null)
            {
                return ray.CurrentObject.gameObject;
            }
            else return null;
        }

        private static void HandleObject(GameObject targetedObject, GameObject heldObject, bool filling)
        {
            BaseObject io = targetedObject.GetComponent<BaseObject>();
            MotorVehicleMovement motor = targetedObject.GetComponent<MotorVehicleMovement>();
            HelicopterVehicleMovement gyro = targetedObject.GetComponent<HelicopterVehicleMovement>();

            if (io != null && io.PrefabId == 142U)
            {
                InteractiveObject heldIo = heldObject.GetComponent<InteractiveObject>();

                bool hasProperItem = heldIo.CraftingType.InteractiveType == InteractiveType.FOOD_WATER_BOTTLE || heldIo.CraftingType.InteractiveType == InteractiveType.FOOD_WATER_SKIN ||
                                     heldIo.CraftingType.InteractiveType == InteractiveType.FOOD_COCONUT_FLASK || heldIo.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN;

                if (hasProperItem)
                {
                    HandleBarrel(io, heldObject, filling);
                }
            }
            if (gyro != null || motor != null)
            {
                InteractiveObject heldIo = heldObject.GetComponent<InteractiveObject>();
                bool hasProperItem = heldIo.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN;

                if (hasProperItem)
                {
                    HandleVehicle(targetedObject, heldObject, filling);
                }
            }
        }

        private static void HandleBarrel(BaseObject barrel, GameObject heldObject, bool filling)
        {
            InteractiveObject_FOOD food = heldObject.GetComponent<InteractiveObject_FOOD>();

            string barrelName = barrel.DisplayName;

            if (food != null)
            {
                bool flag = barrel.DisplayName.Contains("FUEL BARREL");

                if (flag && !barrel.DisplayName.ToLower().EndsWith("00 liters"))
                {
                    Main.FBDoNotif("This barrel is reserved for fuel.", 3.7f);
                }
                else if ((int)food.CraftingType.AttributeType == 50 && (int)food.CraftingType.InteractiveType == 87 && food.DisplayName.ToLower().Contains("sea"))
                {
                    if (filling) Main.FBDoNotif("Cannot fill barrel with salty sea water.", 3f);
                    else if (!flag && !barrel.DisplayName.Contains("00")) Main.FBDoNotif("Cannot fill salty water bucket with fresh water.", 3f);
                }
                else
                {
                    if (barrel.DisplayName.Contains("WATER BARREL:") && barrel.DisplayName.Contains("SERVINGS"))
                    {
                        int units = GetBarrelStorage(barrelName, BarrelType.Water);

                        if (filling && units < MAX_WATER_UNITS && food.Servings > 0)
                        {
                            food.Servings--;
                            units++;
                            Main.FBPlaySound(Main.fill1);
                        }
                        else if (!filling && units > 0 && food.Servings < food.OriginalServings)
                        {
                            food.Servings++;
                            units--;
                            PlayRandomGatherSound();
                        }
                        else return;

                        if (units < 10) barrel.DisplayName = "WATER BARREL: 0" + units + " SERVINGS";
                        else barrel.DisplayName = "WATER BARREL: " + units + " SERVINGS";
                    }
                    else if (filling && food.Servings > 0)
                    {
                        food.Servings--;

                        barrel.DisplayName = "WATER BARREL: 01 SERVINGS";
                        Main.FBPlaySound(Main.fill1);
                    }

                    PlayerRegistry.LocalPlayer.PlayerUI.GetController<HudPresenter>().Refresh(RefreshUIMode.Refresh);
                }
            }
            else
            {
                if (barrel.DisplayName.Contains("WATER BARREL") && !barrel.DisplayName.ToLower().EndsWith("00 servings"))
                {
                    Main.FBDoNotif("This barrel is reserved for water.", 3.7f);
                }
                else
                {
                    InteractiveObject_FUELCAN can = heldObject.GetComponent<InteractiveObject_FUELCAN>();
                    float canFuel = can.Fuel;

                    if (barrel.DisplayName.Contains("FUEL BARREL:") && barrel.DisplayName.Contains("LITERS"))
                    {
                        int units = GetBarrelStorage(barrelName, BarrelType.Fuel);

                        if (filling && units < MAX_GAS_UNITS && canFuel - 1 > -0.01f)
                        {
                            canFuel--;
                            units++;
                            Main.FBPlaySound(Main.fill1);
                        }
                        else if (!filling && units > 0 && canFuel + 0.99f < can.FuelCapacity)
                        {
                            canFuel++;
                            units--;
                            PlayRandomGatherSound();
                        }
                        else return;

                        if (units < 10) barrel.DisplayName = "FUEL BARREL: 0" + units + " LITERS";
                        else barrel.DisplayName = "FUEL BARREL: " + units + " LITERS";

                        Main.fi_FuelAmount.SetValue(can, canFuel);
                    }
                    else if (filling && canFuel > 0.99f)
                    {
                        canFuel--;

                        barrel.DisplayName = "FUEL BARREL: 01 LITERS";

                        Main.fi_FuelAmount.SetValue(can, canFuel);
                        Main.FBPlaySound(Main.fill1);
                    }

                    PlayerRegistry.LocalPlayer.PlayerUI.GetController<HudPresenter>().Refresh(RefreshUIMode.Refresh);
                }
            }
        }

        private static int GetBarrelStorage(string barrelname, BarrelType type)
        {
            int[] numbers = { 0, 0 };
            char [] chars = barrelname.ToCharArray();

            try
            {
                if (type == BarrelType.Water)
                {
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (i == 14) numbers[0] = int.Parse(chars[i].ToString());
                        else if (i == 15)
                        {
                            numbers[1] = int.Parse(chars[i].ToString());
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (i == 13) numbers[0] = int.Parse(chars[i].ToString());
                        else if (i == 14)
                        {
                            numbers[1] = int.Parse(chars[i].ToString());
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[SDFillableBarrels.GetBarrelStorage] Exception of barrel name '{barrelname}':\n {e}");
                return -5;
            }

            return (numbers[0] * 10 + numbers[1]);
        }

        private static void HandleVehicle(GameObject vehicle, GameObject heldObject, bool filling)
        {
            if (filling) return;

            InteractiveObject_FUELCAN can = heldObject.GetComponent<InteractiveObject_FUELCAN>();
            float canFuel = can.Fuel;

            if (Mathf.Approximately(canFuel, can.FuelCapacity)) return;

            float vehicleFuel = -1;
            MotorVehicleMovement motor = vehicle.GetComponent<MotorVehicleMovement>();
            HelicopterVehicleMovement gyro = vehicle.GetComponent<HelicopterVehicleMovement>();

            if (motor != null) vehicleFuel = motor.Fuel;
            else vehicleFuel = gyro.Fuel;

            bool didSomething = false;

            if (vehicleFuel > 1f)
            {
                vehicleFuel--;
                canFuel++;
                didSomething = true;
            }
            else if (!Mathf.Approximately(vehicleFuel, 0f))
            {
                canFuel += vehicleFuel;
                vehicleFuel = 0;
                didSomething = true;
            }

            if (didSomething)
            {
                if (motor != null) Main.fi_motorfuel.SetValue(motor, vehicleFuel);
                else Main.fi_gyrofuel.SetValue(gyro, vehicleFuel);

                Main.fi_FuelAmount.SetValue(can, canFuel);

                PlayRandomGatherSound();
            }
        }

        private static void PlayRandomGatherSound()
        {
            int sound = random.Next(0, 2);

            if (sound == 0) Main.FBPlaySound(Main.gather1);
            else Main.FBPlaySound(Main.gather2);
        }

        private static void DoRainCatcher()
        {
            if (Singleton<AtmosphereStorm>.Instance != null)
            {
                if (Singleton<AtmosphereStorm>.Instance.Rain > 0)
                {
                    if (rainTimer < (GameTime.OneGameHour * Main.settings.refillRate))
                    {
                        rainTimer += Time.deltaTime;
                    }
                    else
                    {
                        InteractiveObject[] bos = UnityEngine.Object.FindObjectsOfType<InteractiveObject>();
                        if (bos != null)
                        {
                            foreach (InteractiveObject bo in bos)
                            {
                                InteractiveObject_FOOD food = bo.gameObject.GetComponent<InteractiveObject_FOOD>();

                                if (food != null)
                                {
                                    if (bo.PrefabId == 405U) break;

                                    if (PlayerRegistry.LocalPlayer.Holder.CurrentObject != null)
                                    {
                                        if (PlayerRegistry.LocalPlayer.Holder.CurrentObject == bo as IPickupable)
                                        {
                                            break;
                                        }
                                    }
                                    float tilt = CheckTilt(bo.gameObject.transform);

                                    if (food.Servings < food.OriginalServings && tilt > 0.5f) food.Servings++;
                                }
                                else if (bo.CraftingType.AttributeType == AttributeType.Barrel && bo.CraftingType.InteractiveType != InteractiveType.None)
                                {
                                    if (bo.DisplayName.StartsWith("WATER BARREL: ") && bo.DisplayName.Contains("SERVINGS"))
                                    {
                                        string barrelname = bo.DisplayName;
                                        int units = GetBarrelStorage(barrelname, BarrelType.Water);
                                        float tilt = CheckTilt(bo.gameObject.transform);

                                        if (units < MAX_WATER_UNITS && tilt > 0.5f)
                                        {
                                            units++;
                                            if (units < 10) bo.DisplayName = "WATER BARREL: 0" + units + " SERVINGS";
                                            else bo.DisplayName = "WATER BARREL: " + units + " SERVINGS";
                                        }
                                    }
                                }
                            }
                        }
                        rainTimer = 0f;
                    }
                }
            }
        }

        private static float CheckTilt(Transform transform)
        {
            float tiltPercentage = Vector3.Dot(transform.up, Vector3.up);
            return tiltPercentage;
        }

        public static float rainTimer = 0f;
        private static System.Random random = new System.Random();

        private const int MAX_WATER_UNITS = 60;
        private const int MAX_GAS_UNITS = 40;
    }
}
