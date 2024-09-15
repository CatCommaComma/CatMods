using System;
using UnityEngine;
using Beam;
using Beam.Crafting;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Beam.Utilities;
using System.Collections;
using Beam.UI;
using Funlabs;

namespace NoMoreCollision
{
    [HarmonyPatch(typeof(PlacementModifier_SnapToTerrain), nameof(PlacementModifier_SnapToTerrain.Modify), new Type[] { typeof(Beam.Crafting.Placement), typeof(BeamRay)})]
    class PlacementModifier_SnapToTerrain_Patch
    {
        private static bool Prefix(Beam.Crafting.Placement __result, Beam.Crafting.Placement placement, BeamRay ray)
        {
            if (Main.BuildAnywhere && !Main.AllowSnapping)
            {
                __result = placement;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PlacementModifier_SnapToLayer), nameof(PlacementModifier_SnapToLayer.Modify), new Type[] { typeof(Beam.Crafting.Placement), typeof(BeamRay) })]
    class PlacementModifier_SnapToLayer_Patch
    {
        private static bool Prefix(Beam.Crafting.Placement __result, Beam.Crafting.Placement placement, BeamRay ray)
        {
            if (Main.BuildAnywhere && !Main.AllowSnapping)
            {
                __result = placement;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Constructing), nameof(Constructing.OwnerPlacing), new Type[] { typeof(Transform), typeof(BeamRay), typeof(float) })]
    class Constructing_OwnerPlacing_Patch
    {
        private static readonly AccessTools.FieldRef<Constructing, Beam.Crafting.PlacementModifier[]> _placementModifiersRef = AccessTools.FieldRefAccess<Constructing, Beam.Crafting.PlacementModifier[]>("_placementModifiers");
        private static readonly AccessTools.FieldRef<Constructing, Connector> _prevSnappedConnectorRef = AccessTools.FieldRefAccess<Constructing, Connector>("_prevSnappedConnector");
        private static readonly AccessTools.FieldRef<Constructing, Connector> _snappedConnectorRef = AccessTools.FieldRefAccess<Constructing, Connector>("_snappedConnector");
        private static readonly AccessTools.FieldRef<Constructing, Connector> _myConnectorRef = AccessTools.FieldRefAccess<Constructing, Connector>("_myConnector");
        private static readonly AccessTools.FieldRef<Constructing, bool> _snappedRef = AccessTools.FieldRefAccess<Constructing, bool>("_snapped");
        private static readonly AccessTools.FieldRef<Constructing, LayerMask> _connectorLayerMaskRef = AccessTools.FieldRefAccess<Constructing, LayerMask>("_connectorLayerMask");
        private static readonly AccessTools.FieldRef<Constructing, int> _rotationAngleRef = AccessTools.FieldRefAccess<Constructing, int>("_rotationAngle");
        private static readonly AccessTools.FieldRef<Constructing, Constructing.ERotationMode> _rotationModeRef = AccessTools.FieldRefAccess<Constructing, Constructing.ERotationMode>("_rotationMode");

        private static bool Prefix(ref bool __result, Constructing __instance, Transform ghost, BeamRay ray, float yRotation)
        {
            if (Main.BuildAnywhere)
            {
                try
                {
                    Beam.Crafting.Placement placement = SetupPlacement(ghost, __instance, ray);

                    _prevSnappedConnectorRef(__instance) = _snappedConnectorRef(__instance);
                    _snappedConnectorRef(__instance) = null;
                    _snappedRef(__instance) = false;

                    if (Main.AllowSnapping)
                    {
                        CheckConnectors(ray, __instance, placement, ghost);

                        if (_prevSnappedConnectorRef(__instance) != null && 
                            _snappedConnectorRef(__instance) != null &&
                            _prevSnappedConnectorRef(__instance) != _snappedConnectorRef(__instance) &&
                            _snappedConnectorRef(__instance).Constructing != null &&
                            _snappedConnectorRef(__instance).Constructing.Structure != null &&
                            _prevSnappedConnectorRef(__instance).Constructing != null &&
                            _prevSnappedConnectorRef(__instance).Constructing.Structure)
                        {
                            ghost.GetComponent<Ghost>().SetCollisionForConstructions(_prevSnappedConnectorRef(__instance).Constructing.Structure.Constructions, false);
                        }
                    }

                    _prevSnappedConnectorRef(__instance) = null;

                    if (_snappedRef(__instance)) HandleSnapped(__instance, placement, ghost, yRotation);
                    else HandleNonSnapped(__instance, placement, ghost, yRotation);
                }
                catch (Exception ex)
                {
                    Debug.Log($"[NoMoreCollision.Constructing_OwnerPlacing_Patch.Prefix] {ex}");
                }

                if (Main.BuildAtHeight) ghost.position = new Vector3(ghost.position.x, Main.BuildHeight, ghost.position.z);

                __result = true;
                return false;
            }

            return true;
        }

        private static void Postfix(Transform ghost, BeamRay ray, float yRotation)
        {
            if (!Main.BuildAnywhere && Main.BuildAtHeight)
            {
                ghost.position = new Vector3(ghost.position.x, Main.BuildHeight, ghost.position.z);
            }
        }

        private static bool CheckConnectors(BeamRay ray, Constructing thisConstructing, Beam.Crafting.Placement placement, Transform ghost)
        {
            IList<Connector> componentSphereCastAll = BeamPhysics.GetComponentSphereCastAll<Connector>(ray.BaseRay, 0.3f, ray.Length, _connectorLayerMaskRef(thisConstructing).value);

            if (componentSphereCastAll.Count > 0)
            {
                for (int i = 0; i < componentSphereCastAll.Count; i++)
                {
                    Connector connector = componentSphereCastAll[i];

                    try
                    {
                        _snappedRef(thisConstructing) = thisConstructing.MyConnector.ConnectTo(connector, placement);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }

                    if (_snappedRef(thisConstructing))
                    {
                        placement = (Beam.Crafting.Placement)AccessTools.Method(typeof(Constructing), "CheckPlacement", new Type[] { typeof(Beam.Crafting.Placement) }).Invoke(thisConstructing, new object[] { placement });

                        _snappedConnectorRef(thisConstructing) = connector;

                        if (_snappedConnectorRef(thisConstructing) != _prevSnappedConnectorRef(thisConstructing))
                        {
                            Ghost component = ghost.GetComponent<Ghost>();

                            IEnumerable<Constructing> constructions;

                            if (_snappedConnectorRef(thisConstructing).Constructing.Structure != null)
                            {
                                if (_snappedConnectorRef(thisConstructing).Constructing.Structure.DisableGhostCollisionOnSnap)
                                {
                                    IList<Constructing> list = (from c in _snappedConnectorRef(thisConstructing).Constructing.Structure.Constructions
                                                                where c is Construction_RAFT
                                                                select c).ToList<Constructing>();
                                    constructions = list;
                                }
                                else
                                {
                                    constructions = _snappedConnectorRef(thisConstructing).Constructing.Structure.Constructions;
                                }
                                component.SetCollisionForConstructions(constructions, true);
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
            else
            {
                try
                {
                    return _myConnectorRef(thisConstructing).ConnectTo(null, placement);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                    return false;
                }
            }
        }

        private static void HandleSnapped(Constructing thisConstructing, Beam.Crafting.Placement placement, Transform ghost, float yRotation)
        {
            int rotationMode = (int)_rotationModeRef(thisConstructing);

            Quaternion quaternion = placement.Rotation;
            if (rotationMode != 0)
            {
                float y = Mathf.Round(yRotation / (float)_rotationAngleRef(thisConstructing)) * (float)_rotationAngleRef(thisConstructing);
                quaternion *= Quaternion.Euler(new Vector3(0f, y, 0f));
            }
            ghost.position = placement.Position;
            ghost.rotation = quaternion;
            ghost.localScale = GetScale();
        }

        private static void HandleNonSnapped(Constructing thisConstructing, Beam.Crafting.Placement placement, Transform ghost, float yRotation)
        {
            int rotationMode = (int)_rotationModeRef(thisConstructing);

            ghost.position = placement.Position;
            ghost.rotation = placement.Rotation;
            ghost.localScale = GetScale();

            if (rotationMode != 0)
            {
                HandleRotations();
                ghost.rotation *= Quaternion.Euler(new Vector3(_xRotation, _yRotation, _zRotation));
                ghost.localScale = GetScale();
            }

            if (Main.AllowSnapping)
            {
                try
                {
                    for (int i = 0; i < thisConstructing.MyConnector.Checks.Count; i++)
                    {
                        if (!thisConstructing.MyConnector.Checks[i].Check(placement, null)) break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }

        private static void HandleRotations()
        {
            float val = Crafter_PlaceCraftable_Sequence_Patch.PlayerInput;
            ref float target = ref _xRotation;

            if (Main.StructureEditMode == Main.RotationType.Rotate)
            {
                switch (Main.TargetAxis)
                {
                    case Main.Axis.X:
                        target = ref _xRotation;
                        break;
                    case Main.Axis.Y:
                        target = ref _yRotation;
                        break;
                    case Main.Axis.Z:
                        target = ref _zRotation;
                        break;
                }
            }
            else
            {
                switch (Main.TargetAxis)
                {
                    case Main.Axis.X:
                        target = ref _xScale;
                        break;
                    case Main.Axis.Y:
                        target = ref _yScale;
                        break;
                    case Main.Axis.Z:
                        target = ref _zScale;
                        break;
                }
            }

            if (Main.StructureEditMode == Main.RotationType.Rotate)
            {
                target += val;
                if (target < 0f) target += 360f;
                if (target > 360f) target -= 360f;
            }
            else
            {
                target += val * 0.03f;
                target = Mathf.Clamp(target, 0.05f, 6f);
            }
        }

        private static Beam.Crafting.Placement SetupPlacement(Transform ghost, Constructing thisConstructing, BeamRay ray)
        {
            Beam.Crafting.Placement placement = new Beam.Crafting.Placement();
            placement.Position = ghost.position;
            placement.Rotation = ghost.rotation;

            if (Main.AllowSnapping)
            {
                for (int i = 0; i < _placementModifiersRef(thisConstructing).Length; i++)
                {
                    placement = _placementModifiersRef(thisConstructing)[i].Modify(placement, ray);
                }
            }

            return placement;
        }

        public static void Reset(Main.RotationType type, Main.Axis axis)
        {
            if (type == Main.RotationType.Rotate)
            {
                switch (axis)
                {
                    case Main.Axis.X:
                        _xRotation = 0f;
                        break;

                    case Main.Axis.Y:
                        _yRotation = 0f;
                        break;

                    case Main.Axis.Z:
                        _zRotation = 0f;
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case Main.Axis.X:
                        _xScale = 1f;
                        break;

                    case Main.Axis.Y:
                        _yScale = 1f;
                        break;

                    case Main.Axis.Z:
                        _zScale = 1f;
                        break;
                }
            }
        }

        public static Vector3 GetScale()
        {
            return new Vector3(_xScale, _yScale, _zScale);
        }

        public static void Reset()
        {
            _xRotation = 0f;
            _yRotation = 0f;
            _zRotation = 0f;

            _xScale = 1f;
            _yScale = 1f;
            _zScale = 1f;
        }

        private static float _xRotation = 0f;
        private static float _zRotation = 0f;
        private static float _yRotation = 0f;

        private static float _xScale = 1f;
        private static float _yScale = 1f;
        private static float _zScale = 1f;  
    }

    [HarmonyPatch(typeof(Crafter), nameof(Crafter.PlaceCraftable_Sequence))]
    class Crafter_PlaceCraftable_Sequence_Patch
    {
        private static readonly AccessTools.FieldRef<Crafter, IPlayer> _playerRef = AccessTools.FieldRefAccess<Crafter, IPlayer>("_player");
        private static readonly AccessTools.FieldRef<Crafter, ICraftable> _placingCraftableRef = AccessTools.FieldRefAccess<Crafter, ICraftable>("_placingCraftable");

        private static readonly AccessTools.FieldRef<Crafter, bool> _isPlacingRef = AccessTools.FieldRefAccess<Crafter, bool>("_isPlacing");
        private static readonly AccessTools.FieldRef<Crafter, bool> _cancelConstructionRef = AccessTools.FieldRefAccess<Crafter, bool>("_cancelConstruction");
        private static readonly AccessTools.FieldRef<Crafter, bool> _executingDelayedPlaceActionRef = AccessTools.FieldRefAccess<Crafter, bool>("_executingDelayedPlaceAction");

        private static readonly AccessTools.FieldRef<Crafter, Ghost> _ghostRef = AccessTools.FieldRefAccess<Crafter, Ghost>("_ghost");
        private static readonly AccessTools.FieldRef<Crafter, AudioClip> _buildCancellationSoundRef = AccessTools.FieldRefAccess<Crafter, AudioClip>("_buildCancellationSound");

        private static readonly AccessTools.FieldRef<Crafter, Color> __craftingHighlightColorRef = AccessTools.FieldRefAccess<Crafter, Color>("_craftingHighlightColor");
        private static readonly AccessTools.FieldRef<Crafter, Vector3> _placingCraftableFinalPosRef = AccessTools.FieldRefAccess<Crafter, Vector3>("_placingCraftableFinalPos");
        private static readonly AccessTools.FieldRef<Crafter, Quaternion> _placingCraftableFinalRotRef = AccessTools.FieldRefAccess<Crafter, Quaternion>("_placingCraftableFinalRot");

        private static readonly AccessTools.FieldRef<Crafter, EmptyEventHandler> StartedPlacingRef = AccessTools.FieldRefAccess<Crafter, EmptyEventHandler>("StartedPlacing");
        private static readonly AccessTools.FieldRef<Crafter, EmptyEventHandler> StoppedPlacingRef = AccessTools.FieldRefAccess<Crafter, EmptyEventHandler>("StoppedPlacing");

        public static bool IsPlacing
        { 
            get 
            { 
                if (_currentInstance == null) return false;
                else return _currentInstance.IsPlacing;
            }
        }

        public static void Unstuck()
        {
            if (_currentInstance != null && _craftable != null)
            {
                Crafter crafter = _currentInstance;

                AccessTools.Method(typeof(Crafter), "CancelCrafting").Invoke(crafter, new object[] { _craftable });
                _ghostRef(crafter).Disable();

                if (_playerRef(crafter).IsOwner)
                {
                    StoppedPlacingRef(crafter)?.Invoke();
                    AccessTools.Method(typeof(Crafter), "DisableCraftingInput").Invoke(crafter, new object[] { false });
                    _playerRef(crafter).PlayerUI.Refresh(RefreshUIMode.Refresh);
                }

                _craftable = null;
            }
        }

        //cheers for the ~250 line method, beamteam
        [HarmonyPostfix]
        private static IEnumerator PlaceCraftable_SequenceWrapper(IEnumerator result, Crafter __instance, ICraftable craftable)
        {
            _currentInstance = __instance;
            _craftable = craftable;

            if (!Main.KeepLastInfo) Constructing_OwnerPlacing_Patch.Reset();

            /*if (!Main.BuildAnywhere)
            {
                while (result.MoveNext())
                    yield return result.Current;
            }
            else
            {*/
            _placingCraftableRef(__instance) = craftable;
            if (_playerRef(__instance).IsOwner)
            {
                StartedPlacingRef(__instance)?.Invoke();
                AccessTools.Method(typeof(Crafter), "DisableCraftingInput").Invoke(__instance, new object[] { true });
            }

            _ghostRef(__instance).Enable();
            if (!_ghostRef(__instance).Initialize(craftable))
            {
                AccessTools.Method(typeof(Crafter), "CancelCrafting").Invoke(__instance, new object[] { craftable });
                yield break;
            }

            AccessTools.Method(typeof(Crafter), "ReplicatedSetActive").Invoke(__instance, new object[] { craftable, false, 0f });
            Constructing constructing = craftable as Constructing;
            MiniGuid replicantSnappedConnectorConstructingReferenceId = default(MiniGuid);
            int replicantSnappedConnectorLocalId = 0;
            Connector replicantSnappedConnector = null;
            float yRotation = 0f;
            _cancelConstructionRef(__instance) = false;
            _isPlacingRef(__instance) = true;

            while (_isPlacingRef(__instance))
            {
                if (_executingDelayedPlaceActionRef(__instance))
                {
                    break;
                }

                BeamRay orgRay;
                if (_playerRef(__instance).IsOwner)
                {
                    orgRay = _playerRef(__instance).PlayerCamera.GetRay(Main.PlacingDistance == 0 ? craftable.PlacingDistance : Main.PlacingDistance);
                    _playerRef(__instance).MultiplayerState.CrafterRayOrigin = orgRay.OriginPoint;
                    _playerRef(__instance).MultiplayerState.CrafterRayDirection = orgRay.Direction;
                    _playerRef(__instance).MultiplayerState.CrafterRayLength = orgRay.Length;
                }
                else
                {
                    orgRay = new BeamRay(_playerRef(__instance).MultiplayerState.CrafterRayOrigin, _playerRef(__instance).MultiplayerState.CrafterRayDirection, _playerRef(__instance).MultiplayerState.CrafterRayLength);
                }

                BeamRay orgRay2 = orgRay;

                if (!Main.IgnoreTerrain)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(orgRay2.OriginPoint, orgRay2.Direction, out raycastHit, orgRay2.Length, 1 << Layers.TERRAIN, QueryTriggerInteraction.Ignore))
                    {
                        float magnitude = (orgRay2.OriginPoint - raycastHit.point).magnitude;
                        orgRay2.Length = Mathf.Min(magnitude, orgRay2.Length);
                    }
                }

                Vector3 endPoint = orgRay2.EndPoint;
                Quaternion rotation = Quaternion.LookRotation(orgRay2.Direction.Mask(Vector3Tools.xz).normalized);

                if (_playerRef(__instance).IsOwner)
                {
                    PlayerInput = _playerRef(__instance).Input.GetAxis(16) * 3f;

                    yRotation += PlayerInput;
                    if (yRotation < 0f) yRotation += 360f;
                    if (yRotation > 360f) yRotation -= 360f;

                    _playerRef(__instance).MultiplayerState.CrafterYRotation = yRotation;
                }
                else
                {
                    yRotation = _playerRef(__instance).MultiplayerState.CrafterYRotation;
                }

                _ghostRef(__instance).transform.position = endPoint;
                _ghostRef(__instance).transform.rotation = rotation;
                float num = Mathf.Min(orgRay2.Length, 0.7f);
                orgRay2.OriginPoint += orgRay2.Direction * num;
                orgRay2.Length -= num;
                bool flag;

                if (_playerRef(__instance).IsOwner)
                {
                    try
                    {
                        flag = craftable.OwnerPlacing(_ghostRef(__instance).transform, orgRay2, yRotation);
                    }
                    catch
                    {
                        flag = false;
                    }

                    if (constructing != null)
                    {
                        if (constructing.IsSnapped)
                        {
                            MiniGuid referenceId = constructing.SnappedConnector.Constructing.ReferenceId;
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[0] = referenceId.ToIntLow();
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[1] = referenceId.ToIntHigh();
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorLocalId = constructing.SnappedConnector.GetLocalId();
                        }
                        else
                        {
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[0] = 0;
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[1] = 0;
                            _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorLocalId = 0;
                        }
                    }
                }
                else
                {
                    if (constructing != null)
                    {
                        MiniGuid miniGuid = new MiniGuid(_playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[0], _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorConstructing[1]);
                        int crafterSnappedConnectorLocalId = _playerRef(__instance).MultiplayerState.CrafterSnappedConnectorLocalId;

                        if (!replicantSnappedConnectorConstructingReferenceId.Equals(miniGuid) || replicantSnappedConnectorLocalId != crafterSnappedConnectorLocalId)
                        {
                            Constructing constructing2 = MultiplayerMng.FindObject<Constructing>(miniGuid, false);
                            replicantSnappedConnector = ((constructing2 != null) ? constructing2.GetConnectorById(crafterSnappedConnectorLocalId) : null);
                            replicantSnappedConnectorConstructingReferenceId = miniGuid;
                            replicantSnappedConnectorLocalId = crafterSnappedConnectorLocalId;
                        }
                        constructing.SetSnapped(replicantSnappedConnector);
                    }
                    flag = craftable.ReplicantPlacing(_ghostRef(__instance).transform, orgRay2, yRotation % 360f);
                }

                flag &= Mathf.Approximately(num, 0.7f);
                if (!Main.BuildAnywhere)
                {
                    PlacementModifier_SnapToTerrain placementModifier_SnapToTerrain;
                    if (craftable.gameObject.TryGetComponent<PlacementModifier_SnapToTerrain>(out placementModifier_SnapToTerrain) && !placementModifier_SnapToTerrain.Snapped)
                    {
                        flag = false;
                    }
                }

                if (!Main.BuildingFree && !(bool)AccessTools.Method(typeof(Crafter), "AreAllMaterialsAvailable").Invoke(__instance, new object[] { }))
                {
                    flag = false;
                }

                Color highlightColor = flag ? __craftingHighlightColorRef(__instance) : Color.red;
                AccessTools.Method(typeof(Crafter), "SetHighlightColor").Invoke(__instance, new object[] { highlightColor });

                if (_playerRef(__instance).IsOwner)
                {
                    if (_playerRef(__instance).Input.GetButtonDown(81))
                    {
                        if (!flag)
                        {
                            AudioManager.GetAudioPlayer().Play2D(_buildCancellationSoundRef(__instance), AudioMixerChannels.UI, AudioPlayMode.Single);
                        }
                        else
                        {
                            if (!_executingDelayedPlaceActionRef(__instance))
                            {
                                BeamTiming.WaitAnd((float)AccessTools.Method(typeof(Crafter), "GetCraftingDelay").Invoke(__instance, new object[] { }), delegate
                                {
                                    _executingDelayedPlaceActionRef(__instance) = false;
                                    if (!_ghostRef(__instance).Colliding && (Main.BuildingFree || (bool)AccessTools.Method(typeof(Crafter), "AreAllMaterialsAvailable").Invoke(__instance, new object[] { }) || (bool)AccessTools.Method(typeof(Crafter), "IsCraftingFree").Invoke(__instance, new object[] { })))
                                    {
                                        AccessTools.Method(typeof(Crafter), "ReplicatedFinishPlacing").Invoke(__instance, new object[] { _ghostRef(__instance).transform.position, _ghostRef(__instance).transform.rotation, orgRay, yRotation, craftable });
                                        _placingCraftableFinalPosRef(__instance) = _ghostRef(__instance).transform.position;
                                        _placingCraftableFinalRotRef(__instance) = _ghostRef(__instance).transform.rotation;
                                        _isPlacingRef(__instance) = false;
                                        return;
                                    }
                                    AccessTools.Method(typeof(Crafter), "CleanupCachedMaterials").Invoke(__instance, new object[] { });
                                });
                            }
                            _executingDelayedPlaceActionRef(__instance) = true;
                        }
                        //2
                    }
                    else if (_playerRef(__instance).Input.GetButtonDown(82))
                    {
                        AccessTools.Method(typeof(Crafter), "ReplicatedCancelPlacing").Invoke(__instance, new object[] { });
                    }
                }
                yield return null;
            }

            while (_executingDelayedPlaceActionRef(__instance))
            {
                yield return null;
            }

            if (craftable != null)
            {
                if (_cancelConstructionRef(__instance))
                {
                    AccessTools.Method(typeof(Crafter), "CancelCrafting").Invoke(__instance, new object[] { craftable });
                }
                else
                {
                    craftable.transform.position = _placingCraftableFinalPosRef(__instance);
                    craftable.transform.rotation = _placingCraftableFinalRotRef(__instance);
                    AccessTools.Method(typeof(Crafter), "ConfirmConstruction").Invoke(__instance, new object[] { craftable.transform.position });

                    if (Game.Mode.IsMaster())
                    {
                        AccessTools.Method(typeof(Crafter), "ReplicatedSetActive").Invoke(__instance, new object[] { craftable, true, MultiplayerMng.GetRoundTrip() });
                        craftable.Place();
                        AccessTools.Method(typeof(Crafter), "DoRefills").Invoke(__instance, new object[] { craftable });
                    }
                    else
                    {
                        ISaveablePrefab saveablePrefab = craftable as ISaveablePrefab;
                        bool flag3 = saveablePrefab != null && saveablePrefab.IsMultiplayerEntity;
                        bool flag4 = flag3 && ((ISaveablePrefab)craftable).IsOwner;
                        Constructing constructing3 = constructing;
                        bool flag5 = constructing3 != null && constructing3.IsStructurePrefabAMultiplayerEntity();
                        Constructing constructing4 = constructing;
                        bool flag6 = ((constructing4 != null) ? constructing4.SnappedConnector : null) == null;

                        if ((flag3 && flag4) || (flag6 && flag5))
                        {
                            AccessTools.Method(typeof(Crafter), "DoRefills").Invoke(__instance, new object[] { craftable });
                            MultiplayerMng.Destroy((SaveablePrefab)craftable, DestroyDelay.NOW);
                        }
                        else
                        {
                            craftable.gameObject.SetActive(true);
                            craftable.Place();
                        }
                    }
                    AccessTools.Method(typeof(Crafter), "FinishCrafting").Invoke(__instance, new object[] { craftable });
                    craftable.transform.localScale = Constructing_OwnerPlacing_Patch.GetScale();
                }
            }

            _ghostRef(__instance).Disable();
            if (_playerRef(__instance).IsOwner)
            {
                StoppedPlacingRef(__instance)?.Invoke();
                AccessTools.Method(typeof(Crafter), "DisableCraftingInput").Invoke(__instance, new object[] { false });
                _playerRef(__instance).PlayerUI.Refresh(RefreshUIMode.Refresh);
            }
            //}

            _craftable = null;
        }

        public static float PlayerInput = 0f;
        private static Crafter _currentInstance = null; 
        private static ICraftable _craftable;
    }

    [HarmonyPatch(typeof(Constructing), nameof(Constructing.CheckStructure))]
    class Constructing_CheckStructure_Patch
    {
        private static readonly AccessTools.FieldRef<Constructing, GameObject> _structurePrefabRef = AccessTools.FieldRefAccess<Constructing, GameObject>("_structurePrefab");
        private static readonly AccessTools.FieldRef<Constructing, Connector> _snappedConnectorRef = AccessTools.FieldRefAccess<Constructing, Connector>("_snappedConnector");

        private static bool Prefix(Constructing __instance)
        {
            if (Main.BuildAnywhere)
            {
                if (Main.AllowSnapping)
                {
                    bool flag = _structurePrefabRef(__instance) != null;

                    if (_snappedConnectorRef(__instance) != null && _snappedConnectorRef(__instance).Constructing != null)
                    {
                        __instance.Structure = _snappedConnectorRef(__instance).Constructing.Structure;
                    }
                    else if (__instance.Structure.IsNullOrDestroyed() && flag)
                    {
                        return true;
                    }

                    __instance.transform.parent = null;
                    if (flag) __instance.TransformParent = __instance.Structure.transform;

                    __instance.Parent();
                    __instance.transform.localScale = Vector3.one;

                    if (flag) __instance.Structure.AddConstruction(__instance);
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Construction_PANEL), "CheckForCaps")]
    class Construction_PANEL_CheckForCaps_Patch
    {
        private static bool Prefix(Construction_PANEL __instance)
        {
            if (Main.BuildAnywhere)
            {
                return false;
            }
            return true;
        }
    }

    /***************************************** removing collision/NoBuilding constraints ************************************/

    [HarmonyPatch(typeof(GhostCollider), "OnTriggerStay", new Type[] { typeof(Collider) })]
    class GhostCollider_OnTriggerStay_Patch
    {
        private static bool Prefix(Collider other)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(Constructing), nameof(Constructing.CheckProximity))]
    class Constructing_CheckProximity_Patch
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    /***************************************** one hit build/break ************************************/

    [HarmonyPatch(typeof(ConstructionObject_HAMMER), nameof(ConstructionObject_HAMMER.UseOnObject), new Type[] { typeof(IBase) })]
    class ConstructionObject_HAMMER_UseOnObject_Patch
    {
        private static void Postfix(IBase obj)
        {
            if (Main.OneHitBuild)
            {
                IConstructable constructable = obj as IConstructable;
                if (constructable != null)
                {
                    constructable.HealthPoints += 600;
                    constructable.CheckHealth();
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractiveObject), nameof(InteractiveObject.DamageObject), new Type[] { typeof(IBase) })]
    class InteractiveObject_DamageObject_Patch
    {
        private static void Postfix(InteractiveObject __instance, IBase obj)
        {
            if (Main.OneHitBreak && __instance.CraftingType.InteractiveType != InteractiveType.TOOLS_HAMMER)
            {
                IConstructable constructable = obj as IConstructable;
                if (constructable != null)
                {
                    obj.Damage(600, __instance);
                    constructable.CheckHealth();
                }
            }
        }
    }
}

//2

/*if (flag2)
                            {
                                LocalizedNotification.Post(_playerRef(__instance), NotificationPriority.Immediate, 4f, "CONSTRUCTING_NTF_CANT_BUILD_TOO_CLOSE_DESC");
                                AudioManager.GetAudioPlayer().Play2D(_buildCancellationSoundRef(__instance), AudioMixerChannels.UI, AudioPlayMode.Single);
                            }
                            else if (_ghostRef(__instance).Colliding)
                            {
                                LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
                                localizedNotification.Priority = NotificationPriority.Immediate;
                                localizedNotification.Duration = 4f;
                                localizedNotification.PlayerId = _playerRef(__instance).Id;
                                localizedNotification.MessageText.SetTerm("CONSTRUCTING_NTF_CANT_BUILD_IN_WAY_DESC");
                                localizedNotification.Raise();
                                AudioManager.GetAudioPlayer().Play2D(_buildCancellationSoundRef(__instance), AudioMixerChannels.UI, AudioPlayMode.Single);
                            }
                            else if (!flag)
                            {
                                AudioManager.GetAudioPlayer().Play2D(_buildCancellationSoundRef(__instance), AudioMixerChannels.UI, AudioPlayMode.Single);
                            }
                            else
                            {
                                if (!_executingDelayedPlaceActionRef(__instance))
                                {
                                    BeamTiming.WaitAnd((float)AccessTools.Method(typeof(Crafter), "GetCraftingDelay").Invoke(__instance, new object[] { }), delegate
                                    {
                                        _executingDelayedPlaceActionRef(__instance) = false;
                                        if (!_ghostRef(__instance).Colliding && (Main.BuildingFree || (bool)AccessTools.Method(typeof(Crafter), "AreAllMaterialsAvailable").Invoke(__instance, new object[] { }) || (bool)AccessTools.Method(typeof(Crafter), "IsCraftingFree").Invoke(__instance, new object[] { })))
                                        {
                                            AccessTools.Method(typeof(Crafter), "ReplicatedFinishPlacing").Invoke(__instance, new object[] { _ghostRef(__instance).transform.position, _ghostRef(__instance).transform.rotation, orgRay, yRotation, craftable });
                                            _placingCraftableFinalPosRef(__instance) = _ghostRef(__instance).transform.position;
                                            _placingCraftableFinalRotRef(__instance) = _ghostRef(__instance).transform.rotation;
                                            _isPlacingRef(__instance) = false;
                                            return;
                                        }
                                        AccessTools.Method(typeof(Crafter), "CleanupCachedMaterials").Invoke(__instance, new object[] { });
                                    });
                                }
                                _executingDelayedPlaceActionRef(__instance) = true;
                            }*/
