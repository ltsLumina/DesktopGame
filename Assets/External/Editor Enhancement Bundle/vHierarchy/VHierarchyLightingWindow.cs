#if UNITY_EDITOR
#region
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static VHierarchy.Libs.VUtils;
using static VHierarchy.Libs.VGUI;
#endregion

namespace VHierarchy
{
public class VHierarchyLightingWindow : CustomPopupWindow
{
    void OnGUI()
    {
        void directionalLight()
        {
            Light light = FindObjects<Light>().Where(r => r.type == LightType.Directional && r.gameObject.scene == SceneManager.GetActiveScene()).FirstOrDefault();

            if (!light) return;

            light.RecordUndo();
            light.transform.RecordUndo();

            ObjectFieldWidhoutPicker("Directional Light", light);

            Space(2);
            BeginIndent(8);
            EditorGUIUtility.labelWidth += 2;

            float rotX = light.transform.eulerAngles.x.Loop(-180, 180).Round();
            float rotY = light.transform.eulerAngles.y.Loop(-180, 180).Round();
            rotX = EditorGUILayout.Slider("Rotation X", rotX, 0, 90);
            rotY = EditorGUILayout.Slider("Rotation Y", rotY, -179, 180);
            if (light.transform.rotation != Quaternion.Euler(rotX, rotY, light.transform.eulerAngles.z)) light.transform.rotation = Quaternion.Euler(rotX, rotY, light.transform.eulerAngles.z);

            Space(3);
            light.intensity = EditorGUILayout.Slider("Intensity", light.intensity, 0, 2);
            light.color     = SmallColorField(ExpandWidthLabelRect().AddWidthFromMid(-1).MoveX(-.5f), "Color", light.color);

            EndIndent();
        }

        void ambientLight()
        {
            RenderSettings.ambientMode = (AmbientMode) EditorGUILayout.IntPopup
            ("Ambient Light", (int) RenderSettings.ambientMode, new[]
             { "\u2009Skybox", "\u2009Gradient", "\u2009Color" }, new[]
             { 0, 1, 3 });

            foreach (RenderSettings r in FindObjects<RenderSettings>()) { r.RecordUndo(); }

            Space(2);
            BeginIndent(8);
            EditorGUIUtility.labelWidth += 4;

            if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                Color.RGBToHSV(RenderSettings.ambientSkyColor, out float h, out float s, out float v);
                v                              = EditorGUILayout.Slider("Intensity", v, .01f, 2);
                RenderSettings.ambientSkyColor = Color.HSVToRGB(h, s, v, true);

                RenderSettings.ambientSkyColor = SmallColorField("Color", RenderSettings.ambientSkyColor, false, true);
            }

            if (RenderSettings.ambientMode == AmbientMode.Skybox) RenderSettings.ambientIntensity = EditorGUILayout.Slider("Intensity", RenderSettings.ambientIntensity, 0, 2);

            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor     = SmallColorField("Color Sky", RenderSettings.ambientSkyColor, false, true);
                RenderSettings.ambientEquatorColor = SmallColorField("Color Horizon", RenderSettings.ambientEquatorColor, false, true);
                RenderSettings.ambientGroundColor  = SmallColorField("Color Ground", RenderSettings.ambientGroundColor, false, true);
            }

            EndIndent();
        }

        void fog()
        {
            int mode = EditorGUILayout.IntPopup
            ("Fog", RenderSettings.fog ? (int) RenderSettings.fogMode : 0, new[]
             { "\u2009Off", "\u2009Linear", "\u2009Exponential", "\u2009Exponential Squared" }, new[]
             { 0, 1, 2, 3 });

            if (RenderSettings.fog = mode != 0) RenderSettings.fogMode = (FogMode) mode;

            if (!RenderSettings.fog) return;

            Space(2);
            BeginIndent(8);
            EditorGUIUtility.labelWidth += 4;

            if (RenderSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = EditorGUILayout.FloatField("Start", RenderSettings.fogStartDistance);
                RenderSettings.fogEndDistance   = EditorGUILayout.FloatField("End", RenderSettings.fogEndDistance);
            }
            else { RenderSettings.fogDensity = ExpSlider(ExpandWidthLabelRect().AddWidthFromRight(1.5f), "Density", RenderSettings.fogDensity, 0, .05f); }

            RenderSettings.fogColor = SmallColorField("Color", RenderSettings.fogColor);

            EndIndent();
        }

        HeaderGUI<VHierarchyLightingWindow>("Lighting");

        BeginIndent(6);
        EditorGUIUtility.labelWidth = 115;

        Space(11);
        directionalLight();

        Space(18);
        ambientLight();

        Space(18);
        fog();

        EndIndent(6);

        Space(21);
        UpdateSize(false, true);

        EditorGUIUtility.labelWidth = 0;

        if (Application.platform != RuntimePlatform.OSXEditor) DrawOutline();

        Repaint();
    }

    public override float initWidth => 250;
    public override float initHeight => 320;
}
}
#endif
