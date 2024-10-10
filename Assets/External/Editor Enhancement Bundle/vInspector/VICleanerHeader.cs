#if UNITY_EDITOR
#region
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static VInspector.Libs.VUtils;
using static VInspector.Libs.VGUI;
#endregion

namespace VInspector
{
public class VICleanerHeader
{
    void OnGUI()
    {
        Color  bgNorm    = EditorGUIUtility.isProSkin ? Greyscale(.248f) : Greyscale(.8f);
        Color  bgHovered = EditorGUIUtility.isProSkin ? Greyscale(.28f) : Greyscale(.84f);
        string name      = script.GetType().Name.Decamelcase();
        Rect   nameRect  = headerElement.contentRect.MoveX(60).SetWidth(name.GetLabelWidth(isBold: true));

        void headerClick()
        {
            if (curEvent.isMouseDown) mousePressedOnHeader = true;

            if (curEvent.isMouseUp || curEvent.isDragUpdate) mousePressedOnHeader = false;
        }

        void scriptNameClick()
        {
            if (mousePressedOnName && curEvent.isMouseUp) window.Repaint();

            if (curEvent.isMouseUp || curEvent.isDragUpdate) mousePressedOnName = false;

            if (!nameRect.IsHovered()) return;
            if (!curEvent.isMouseDown) return;
            if (curEvent.mouseButton != 0) return;

            curEvent.Use();

            mousePressedOnName      = true;
            mousePressedOnNameAtPos = curEvent.mousePosition;

            MonoScript script = MonoScript.FromMonoBehaviour(this.script);

            if (curEvent.clickCount == 2) AssetDatabase.OpenAsset(script);

            if (curEvent.holdingAlt) PingObject(script);
        }

        void startDragIfMousePressedOnName()
        {
            if (!mousePressedOnName) return;

            window.Repaint();

            if (!curEvent.isMouseDrag) return;
            if ((mousePressedOnNameAtPos - curEvent.mousePosition).magnitude < 2) return;

            DragAndDrop.PrepareStartDrag();

            DragAndDrop.objectReferences = new[]
            { script };

            DragAndDrop.StartDrag(script.ToString());

            mousePressedOnName   = false;
            mousePressedOnHeader = false;
        }

        void hideScriptText()
        {
            Rect rect = headerElement.contentRect.SetWidth(60).MoveX(name.GetLabelWidth(isBold: true) + 60).SetHeightFromMid(15);

            // #if UNITY_2022_3_OR_NEWER
            //                 rect.x *= .94f;
            //                 rect.x += 2;
            // #endif

            rect.xMax = rect.xMax.Min(headerElement.contentRect.width - 60).Max(rect.xMin);

            rect.Draw(headerElement.contentRect.IsHovered() && (!mousePressedOnHeader || mousePressedOnName) ? bgHovered : bgNorm);
        }

        void greyoutScriptName()
        {
            if (!mousePressedOnName) return;

            nameRect.Resize(1).Draw(Greyscale(bgHovered.r, EditorGUIUtility.isProSkin ? .3f : .45f));
        }

        headerClick();
        scriptNameClick();
        startDragIfMousePressedOnName();

        if (!headerElement.contentRect.IsHovered()) mousePressedOnHeader = mousePressedOnName = false;

        defaultHeaderGUI();

        hideScriptText();
        greyoutScriptName();
    }

    bool mousePressedOnName;
    Vector2 mousePressedOnNameAtPos;
    bool mousePressedOnHeader;

    public void Update()
    {
        if (headerElement is VisualElement v && v.panel == null)
        {
            headerElement.onGUIHandler = defaultHeaderGUI;
            headerElement              = null;
        }

        if (headerElement != null && headerElement.name != script.GetType().Name.Decamelcase() + " (Script)Header")
        {
            headerElement.onGUIHandler = defaultHeaderGUI;
            headerElement              = null;
        } // fixes wrong headerElement after reordering components in 2022.3

        if (headerElement != null && headerElement.onGUIHandler.Method.DeclaringType == typeof(VICleanerHeader)) return;
        if (typeof(ScriptableObject).IsAssignableFrom(script.GetType())) return;
        if (!(editor.GetPropertyValue("propertyViewer") is EditorWindow window)) return;

        this.window = window;

        void findHeader(VisualElement element)
        {
            if (element == null) return;

            if (element.GetType().Name == "EditorElement")
            {
                IMGUIContainer curHeader = null;

                foreach (VisualElement child in element.Children())
                {
                    curHeader = curHeader ?? new[]
                    { child as IMGUIContainer }.FirstOrDefault(r => r != null && r.name.EndsWith("Header"));

                    if (curHeader is null) continue;
                    if (!(child is InspectorElement)) continue;

                    if (child.GetFieldValue<Editor>("m_Editor").target == script)
                    {
                        headerElement = curHeader;
                        return;
                    }
                }
            }

            foreach (VisualElement r in element.Children())
            {
                if (headerElement == null) findHeader(r);
            }
        }

        void setupGUICallbacks()
        {
            defaultHeaderGUI           = headerElement.onGUIHandler;
            headerElement.onGUIHandler = OnGUI;
        }

        findHeader(window.rootVisualElement);

        if (headerElement != null) setupGUICallbacks();
    }

    IMGUIContainer headerElement;
    Action defaultHeaderGUI;

    EditorWindow window;

    public VICleanerHeader(MonoBehaviour script, Editor editor)
    {
        this.script = script;
        this.editor = editor;
    }

    MonoBehaviour script;
    Editor editor;

    static void UpdateHeaders(Editor editor) // finishedDefaultHeaderGUI
    {
        if (!(editor.target is GameObject gameObject)) return;
        if (!curEvent.isLayout) return;

        int curOrderHash = gameObject.GetComponents<Component>().Aggregate(17, (hash, element) => hash * 31 + (element?.GetHashCode() ?? 0));

        if (componentOrderHashes_byEditor.ContainsKey(editor))
            if (curOrderHash != componentOrderHashes_byEditor[editor])
                cleanerHeaders.Clear();

        componentOrderHashes_byEditor[editor] = curOrderHash;

        foreach (MonoBehaviour script in gameObject.GetComponents<MonoBehaviour>())
        {
            if (script != null)
            {
                if (!cleanerHeaders.ContainsKey(script)) cleanerHeaders[script] = new (script, editor);

                cleanerHeaders[script].Update();
            }
        }
    }

    static Dictionary<MonoBehaviour, VICleanerHeader> cleanerHeaders = new ();

    static Dictionary<Editor, int> componentOrderHashes_byEditor = new ();

    [InitializeOnLoadMethod]
    static void Init()
    {
        if (!VIMenuItems.cleanerHeaderEnabled) return;

        Editor.finishedDefaultHeaderGUI -= UpdateHeaders;
        Editor.finishedDefaultHeaderGUI += UpdateHeaders;
    }
}
}
#endif
