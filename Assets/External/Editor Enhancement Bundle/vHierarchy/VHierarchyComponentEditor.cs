#if UNITY_EDITOR
#region
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static VHierarchy.Libs.VUtils;
using static VHierarchy.Libs.VGUI;
#endregion

namespace VHierarchy
{
public class VHierarchyComponentEditor : CustomPopupWindow
{
    void OnGUI()
    {
        if (!component)
        {
            Close();
            return;
        }

        void background() => position.SetPos(0, 0).Draw(backgroundCol);

        void header()
        {
            Rect headerRect = ExpandWidthLabelRect(18).Resize(-1).AddWidthFromMid(6);

            void background_()
            {
                headerRect.Draw(EditorGUIUtility.isProSkin ? Greyscale(.25f) : backgroundCol);
                headerRect.SetHeightFromBottom(1).Draw(EditorGUIUtility.isProSkin ? Greyscale(.2f) : Greyscale(.6f));
            }

            void icon()
            {
                Rect iconRect = headerRect.SetWidth(20).MoveX(14).MoveY(-1);

                GUI.Label(iconRect, VHierarchy.GetComponentIcon(component));
            }

            void toggle()
            {
                PropertyInfo pi_enabled = component.GetType().GetProperty("enabled") ?? component.GetType().BaseType?.GetProperty("enabled") ??
                    component.GetType().BaseType?.BaseType?.GetProperty("enabled") ?? component.GetType().BaseType?.BaseType?.BaseType?.GetProperty("enabled");

                if (pi_enabled == null) return;

                Rect toggleRect = headerRect.MoveX(36).SetSize(20, 20);

                bool enabled = (bool) pi_enabled.GetValue(component);

                if (GUI.Toggle(toggleRect, enabled, "") != enabled)
                {
                    component.RecordUndo();
                    pi_enabled.SetValue(component, !enabled);
                }
            }

            void name()
            {
                Rect nameRect = headerRect.MoveX(54).MoveY(-1);

                string s = VHierarchy.GetComponentName(component);

                if (!closeOnFocusLost) s += " of " + component.gameObject.name;

                SetLabelBold();
                GUI.Label(nameRect, s);
                ResetLabelStyle();
            }

            void dummyButton()
            {
                GUI.color = Color.clear;
                GUI.Button(headerRect, "");
                GUI.color = Color.white;
            }

            background_();
            icon();
            toggle();
            name();

            HeaderButtonsGUI<VHierarchyComponentEditor>(headerRect, true, w => w.Init(component));
            UpdateDragging(headerRect.AddHeight(3));

            dummyButton(); // for blocking mouse move in hierarchy
        }

        void body()
        {
            BeginIndent(16);
            editor?.OnInspectorGUI();
            EndIndent();
        }

        background();
        header();

        Space(3);
        body();

        Space(7);
        UpdateSize(false, true);

        if (Application.platform != RuntimePlatform.OSXEditor) DrawOutline();

        if (!closeOnFocusLost) Repaint();
    }

    public void Init(Component component)
    {
        this.component = component;
        editor         = Editor.CreateEditor(component);

        Undo.undoRedoPerformed += RepaintOnUndoRedo;
    }

    void OnDestroy()
    {
        Undo.undoRedoPerformed -= RepaintOnUndoRedo;
        editor.DestroyImmediate();
        editor = null;
    }

    void RepaintOnUndoRedo() => Repaint();

    Component component;
    Editor editor;

    public override float initWidth => 300;
    public override float initHeight => 345;
}
}
#endif
