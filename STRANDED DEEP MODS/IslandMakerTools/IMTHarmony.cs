using System.Reflection;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using LE_LevelEditor.Core;
using LE_LevelEditor.UI;
using System;
using LE_LevelEditor.Events;
using LE_LevelEditor;
using System.Diagnostics.Tracing;
using UnityEngine.EventSystems;

namespace IslandMakerTools
{
    [HarmonyPatch(typeof(LE_GUIInterface), "Awake")]
    class LE_GUIInterface_Awake_Patch
    {
        private static void Postfix()
        {
            FixScalingButton(LE_GUIInterface.Instance.transform);
            _stop = false;
            _scale = false;
        }

        private static void FixScalingButton(Transform parent)
        {
            if (_stop) return;

            if (parent.name.EndsWith("_Scale"))
            {
                _scale = true;
            }

            if (parent.name.EndsWith("_Root"))
            {
                _stop = true;
                return;
            }

            if (_scale) EnableComponents(parent.gameObject);

            for (int i = 0; i < parent.childCount; i++)
            {
                FixScalingButton(parent.GetChild(i));
            }
        }

        private static void EnableComponents(GameObject prefab)
        {
            Toggle toggle = prefab.GetComponent<Toggle>();
            Image image = prefab.GetComponent<Image>();

            if (toggle != null) { toggle.enabled = true; _scaleButton = toggle; }
            if (image != null) { image.color = new Color(image.color.r, image.color.g, image.color.b, 1); }
        }

        public static void QuickScale()
        {
            if (_scaleButton != null)
            {
                //_scaleButton.Select();
                PointerEventData eventData = new PointerEventData(EventSystem.main);
                eventData.button = PointerEventData.InputButton.Left;
                _scaleButton.OnPointerClick(eventData);
            }
        }

        private static bool _scale = false;
        private static bool _stop = false;
        private static Toggle _scaleButton = null;
    }

    [HarmonyPatch(typeof(LE_Object), "Update")]
    class LE_Object_Update_Patch
    {
        private static readonly AccessTools.FieldRef<LE_Object, bool> m_isMovableRef = AccessTools.FieldRefAccess<LE_Object, bool>("m_isMovable");
        private static readonly AccessTools.FieldRef<LE_Object, bool> m_isRotatableRef = AccessTools.FieldRefAccess<LE_Object, bool>("m_isRotatable");
        private static readonly AccessTools.FieldRef<LE_Object, bool> m_isSelectedChangedRef = AccessTools.FieldRefAccess<LE_Object, bool>("m_isSelectedChanged");
        private static readonly AccessTools.FieldRef<LE_Object, LE_EObjectEditMode> m_editModeRef = AccessTools.FieldRefAccess<LE_Object, LE_EObjectEditMode>("m_editMode");
        private static readonly AccessTools.FieldRef<LE_Object, LE_ObjectEditHandle> m_editHandleRef = AccessTools.FieldRefAccess<LE_Object, LE_ObjectEditHandle>("m_editHandle");

        private static bool Prefix(LE_Object __instance)
        {
            if (m_isSelectedChangedRef(__instance))
            {
                m_isSelectedChangedRef(__instance) = false;
            }

            if (m_editModeRef(__instance) == LE_EObjectEditMode.NO_EDIT)
            {
                if (m_editHandleRef(__instance) != null)
                {
                    UnityEngine.Object.Destroy(m_editHandleRef(__instance).gameObject);
                    return false;
                }
            }

            else if (m_editHandleRef(__instance) == null || m_editHandleRef(__instance).EditMode != m_editModeRef(__instance))
            {
                if (m_editHandleRef(__instance) != null)
                {
                    UnityEngine.Object.Destroy(m_editHandleRef(__instance).gameObject);
                }

                if ((m_editModeRef(__instance) == LE_EObjectEditMode.MOVE && m_isMovableRef(__instance)) || (m_editModeRef(__instance) == LE_EObjectEditMode.ROTATE && m_isRotatableRef(__instance)) || m_editModeRef(__instance) == LE_EObjectEditMode.SCALE)
                {
                    string str = m_editModeRef(__instance).ToString();
                    GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("ObjectEditHandle" + str), __instance.transform.position, __instance.transform.rotation);
                    gameObject.transform.parent = __instance.transform;
                    m_editHandleRef(__instance) = gameObject.GetComponent<LE_ObjectEditHandle>();
                    LE_ObjectEditHandle editHandle = m_editHandleRef(__instance);
                    editHandle.m_onTransformed = (EventHandler)Delegate.Combine(editHandle.m_onTransformed, new EventHandler(delegate (object p_obj, EventArgs p_args)
                    {
                        LE_EventInterface.LevelDataChanged(m_editHandleRef(__instance));
                    }));
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(LE_GUI3dObject), nameof(LE_GUI3dObject.IsObjectPlaceable), new Type[] { typeof(LE_Object) })]
    class LE_GUI3dObject_IsObjectPlaceable_Patch
    {
        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(LE_ObjectEditHandle), "Scale")]
    class LE_ObjectEditHandle_Scale_Patch
    {
        private static readonly AccessTools.FieldRef<LE_ObjectEditHandle, int> m_dragSkipCounterRef = AccessTools.FieldRefAccess<LE_ObjectEditHandle, int>("m_dragSkipCounter");
        //private static readonly AccessTools.FieldRef<LE_ObjectEditHandle, Vector3> m_activeEditAxisRef = AccessTools.FieldRefAccess<LE_ObjectEditHandle, Vector3>("m_activeEditAxis");

        private static bool Prefix(LE_ObjectEditHandle __instance)
        {   
            if (Main.ScaleOnAllSides)
            {
                float editDelta = (float)AccessTools.Method(typeof(LE_ObjectEditHandle), "GetEditDelta").Invoke(__instance, new object[] {});
                if (m_dragSkipCounterRef(__instance) == 0)
                {
                    __instance.transform.parent.localScale += Vector3.one * editDelta;
                    __instance.transform.parent.localScale = Vector3.Max(Vector3.one * 0.05f, __instance.transform.parent.localScale);
                    if (__instance.m_onTransformed != null)
                    {
                        __instance.m_onTransformed(__instance, EventArgs.Empty);
                        return false;
                    }
                }
                else if (Mathf.Abs(editDelta) > 0.0005f)
                {
                    m_dragSkipCounterRef(__instance)++;
                }

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LE_LevelEditorMain), "LE_LevelEditor.LEInput.LE_IInputHandler.MoveCamera")]
    class LE_LevelEditorMain_MoveCamera_Patch
    {
        private static readonly AccessTools.FieldRef<LE_LevelEditorMain, Camera> m_camRef = AccessTools.FieldRefAccess<LE_LevelEditorMain, Camera>("m_cam");
        private static readonly AccessTools.FieldRef<LE_LevelEditorMain, bool> IS_CAMERA_MOVEMENTRef = AccessTools.FieldRefAccess<LE_LevelEditorMain, bool>("IS_CAMERA_MOVEMENT");

        private static Camera Cam(LE_LevelEditorMain instance)
        {
            if (m_camRef(instance) == null)
            {
                m_camRef(instance) = Camera.main;
            }

            return m_camRef(instance);
        }

        private static bool Prefix(LE_LevelEditorMain __instance, Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords, bool scrolling)// = false)
        {
            if (!IS_CAMERA_MOVEMENTRef(__instance))
            {
                return false;
            }

            Camera cam = Cam(__instance);
            if (scrolling && __instance.IsMouseOverGUI())
            {
                return false;
            }

            if (cam != null)
            {
                if (_enumAverage == null) _enumAverage = Enum.ToObject(typeof(LE_LevelEditorMain).GetNestedType("EEstimateDistanceMode", BindingFlags.NonPublic), 0);

                float num = (float)AccessTools.Method(typeof(LE_LevelEditorMain), "EstimateDistanceToLevel").Invoke(__instance, new object[] { _enumAverage });
                Vector3 vector = p_toScreenCoords - p_fromScreenCoords;
                vector = cam.transform.TransformDirection(vector);
                Vector3 b;

                if (cam.orthographic)
                {
                    float num2 = Vector3.Dot(vector, cam.transform.forward);
                    vector -= cam.transform.forward * num2;
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - num2 * (cam.orthographicSize / (float)Screen.width), 5f, 1000f);
                    b = vector * (cam.orthographicSize / (float)Screen.width);
                }
                else
                {
                    b = vector * (num / (float)Screen.width);
                }

                cam.transform.position += b;
                cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, -1000f, 1000f), Mathf.Clamp(cam.transform.position.y, -300f, 600f), Mathf.Clamp(cam.transform.position.z, -1000f, 1000f));
            }

            return false;
        }

        //EEstimateDistanceMode
        private static object _enumAverage = null;
    }

    /*[HarmonyPatch(typeof(), "")]
    class _Patch
    {
        private static readonly AccessTools.FieldRef<, >  = AccessTools.FieldRefAccess<, >("");

        private static void Postfix( __instance)
        {
        }
    }*/
}
