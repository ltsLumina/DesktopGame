#if UNITY_EDITOR
#region
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Type = System.Type;
using static VHierarchy.VHierarchyData;
using static VHierarchy.Libs.VUtils;
using static VHierarchy.Libs.VGUI;
#endregion

namespace VHierarchy
{
public static class VHierarchy
{
    static void RowGUI(int instanceId, Rect rowRect)
    {
        if (!data) return;

        UpdateExpandQueue();

        if (expandedIds == null) UpdateExpandedIdsList();

        if (EditorUtility.InstanceIDToObject(instanceId) is GameObject go) { GameObjectRowGUI(go, rowRect); }
        else
        {
            int iScene = -1;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).GetHashCode() == instanceId) iScene = i;
            }

            if (iScene != -1) SceneRowGUI(SceneManager.GetSceneAt(iScene), rowRect);
        }
    }

    static void GameObjectRowGUI(GameObject go, Rect rowRect)
    {
        SceneData      sceneData;
        GameObjectData goData;
        bool           isHovered;
        bool           isSelected;
        Rect           fullRect = rowRect.SetX(32).SetXMax(rowRect.xMax + (PrefabUtility.IsAnyPrefabInstanceRoot(go) && !PrefabUtility.IsPartOfModelPrefab(go) ? 3 : 16));

        void hydrateData()
        {
            void scene()
            {
                if (data.sceneDatasByScene.TryGetValue(go.scene, out sceneData)) return;

                string guid = go.scene.path.ToGuid();

                if (!data.sceneDatasByGuid.ContainsKey(guid)) data.sceneDatasByGuid[guid] = new ();

                sceneData = data.sceneDatasByScene[go.scene] = data.sceneDatasByGuid[guid];

                var deletedGids = new List<string>();

                foreach (KeyValuePair<string, GameObjectData> kvp in sceneData.goDatasByGlobalId)
                {
                    if (!GlobalObjectId.TryParse(kvp.Key, out GlobalObjectId gid))
                    {
                        deletedGids.Add(kvp.Key);
                        continue;
                    }

                    int iid = GlobalObjectId.GlobalObjectIdentifierToInstanceIDSlow(gid);

                    if (iid != 0) sceneData.goDatasByInstanceId[iid] = kvp.Value;
                    else deletedGids.Add(kvp.Key);
                }

                foreach (string r in deletedGids) { sceneData.goDatasByGlobalId.Remove(r); }
            }

            void go_() => sceneData.goDatasByInstanceId.TryGetValue(go.GetInstanceID(), out goData);

            scene();
            go_();
        }

        void ensureGoDataExists()
        {
            if (goData != null) return;

            goData                                                                                           = new ();
            sceneData.goDatasByGlobalId[GlobalObjectId.GetGlobalObjectIdSlow(go.GetInstanceID()).ToString()] = goData;
            sceneData.goDatasByInstanceId[go.GetInstanceID()]                                                = goData;
        }

        void mouseState()
        {
            isHovered = fullRect.IsHovered();

            if (eType == EventType.MouseDown && isHovered) mousePressedOnRowY = rowRect.y;

            if (eType == EventType.MouseUp || eType == EventType.MouseLeaveWindow || eType == EventType.DragPerform) mousePressedOnRowY = -1;

            isSelected = (Selection.objects.Contains(go) && mousePressedOnRowY == -1) || mousePressedOnRowY == rowRect.y;

            if (eType == EventType.Layout) hoveredGo = null;

            if (eType == EventType.Repaint && isHovered) hoveredGo = go;
        }

        void drawIcon()
        {
            if (!VHierarchyMenuItems.iconsEnabled) return;
            if (eType != EventType.Repaint) return;

            void background()
            {
                if (goData       == null) return;
                if (goData.color == default) return;

                Rect rect = rowRect.AddWidthFromRight(28).AddWidth(16).AddHeightFromMid(-1);

                Color color = goData.color;

                if (isSelected) color     =  Color.Lerp(color, new (.2f, .37f, .57f), 1f);
                else if (isHovered) color *= 1.1f;

                rect.Draw(color);
            }

            void name()
            {
                if (goData       == null) return;
                if (goData.color == default) return;

                Rect rect = rowRect.MoveX(16).MoveY(-.5f);

                SetGUIColor(go.activeInHierarchy ? Color.white : Greyscale(1, .5f));
                SetLabelSize(12);
                GUI.Label(rect, go.name);
                ResetGUIColor();
                ResetLabelStyle();
            }

            void triangle()
            {
                if (goData                  == null) return;
                if (goData.color            == default) return;
                if (go.transform.childCount == 0) return;

                Rect rect = rowRect.MoveX(-15).SetWidth(16).Resize(-1);

                GUI.Label(rect, EditorGUIUtility.IconContent(IsExpanded(go) ? "IN_foldout_on" : "IN_foldout"));
            }

            void iconBackground()
            {
                if (goData       == null) return;
                if (goData.icon  == "") return;
                if (goData.color != default) return;

                Color backgroundColor;

                Color selected          = new Color(.2f, .37f, .57f) * (EditorGUIUtility.isProSkin ? 1 : 1.2f);
                Color selectedUnfocused = EditorGUIUtility.isProSkin ? Greyscale(.3f) : Greyscale(.7f);
                Color hovered           = EditorGUIUtility.isProSkin ? Greyscale(.28f) : Greyscale(.7f);
                Color normal            = backgroundCol;

                if (isSelected) backgroundColor = EditorWindow.focusedWindow?.GetType().Name == "SceneHierarchyWindow" ? selected : selectedUnfocused;

                else if (isHovered) backgroundColor = hovered;

                else backgroundColor = normal;

                Rect bgRect = rowRect.SetWidth(16);

                bgRect.Draw(backgroundColor);
            }

            void icon()
            {
                if (goData == null) return;
                if (goData.icon == "" && goData.color == default) return;

                Rect iconRect = rowRect.SetWidth(16).Resize(-2);

                SetLabelAlignmentCenter();
                SetGUIColor(go.activeInHierarchy ? Color.white : Greyscale(1, .5f));
                GUI.Label(iconRect, goData.icon == "" ? new (PrefabUtility.GetIconForGameObject(go)) : EditorGUIUtility.IconContent(goData.icon));
                ResetGUIColor();
                ResetLabelStyle();
            }

            background();
            name();
            triangle();
            iconBackground();
            icon();
        }

        void componentMinimap()
        {
            if (!VHierarchyMenuItems.componentMinimapEnabled) return;

            void button(Rect rect, Component component)
            {
                Rect mouseRect = rect.SetWidthFromMid(13);

                void draw()
                {
                    if (eType != EventType.Repaint) return;

                    float opacity = EditorGUIUtility.isProSkin ? .33f : .7f;

                    SetGUIColor(Greyscale(1, mouseRect.IsHovered() && holdingAlt ? 1 : opacity));
                    GUI.Label(rect, GetComponentIcon(component));
                    ResetGUIColor();
                }

                void checkClick()
                {
                    if (eType != EventType.MouseDown) return;
                    if (!holdingAlt) return;
                    if (!mouseRect.IsHovered()) return;

                    if (editedComponent == component)
                    {
                        editedComponent = null;
                        return;
                    }

                    editedComponent = component;

                    Vector2 pos    = rect.position + new Vector2(-12, 16);
                    var     window = CustomPopupWindow.Create<VHierarchyComponentEditor>(true, GUIUtility.GUIToScreenPoint(pos));
                    window.Init(component);

                    e.Use();
                }

                void dummyButton() // to trigger repaint on mouse move onto another button
                {
                    if (!holdingAlt) return;

                    SetGUIColor(Color.clear);
                    GUI.Button(mouseRect, "");
                    ResetGUIColor();
                }

                draw();
                checkClick();
                dummyButton();
            }

            Rect buttonRect = fullRect.SetWidthFromRight(16).Resize(0);

            float minxButtonX = rowRect.x + go.name.LabelWidth() + 13;

            foreach (Component component in go.GetComponents<Component>())
            {
                if (component is Transform) continue;
                if (buttonRect.x < minxButtonX) continue;

                button(buttonRect, component);

                buttonRect = buttonRect.MoveX(-13);
            }

            if (isHovered && holdingAlt && go.GetComponent<Transform>()) button(fullRect.SetWidth(16), go.GetComponent<Transform>());
        }

        void editIcon()
        {
            if (!isHovered) return;
            if (!holdingAlt || !e.mouseDown()) return;
            if (StageUtility.GetCurrentStage() is PrefabStage) return;
            if (!VHierarchyMenuItems.iconsEnabled) return;

            ensureGoDataExists();

            Vector2 pos    = rowRect.SetWidth(16).position + new Vector2(-8, 15);
            var     window = CustomPopupWindow.Create<VHierarchyIconEditor>(true, GUIUtility.GUIToScreenPoint(pos));
            window.Init(go, goData);

            e.Use();
        }

        hydrateData();
        mouseState();

        drawIcon();
        componentMinimap();
        editIcon();
    }
    static float mousePressedOnRowY = -1;
    static GameObject hoveredGo;
    static Component editedComponent;

    static void SceneRowGUI(Scene scene, Rect rowRect)
    {
        if (!VHierarchyMenuItems.collapseAndLightingButtonsEnabled) return;

        void lighting()
        {
            Rect buttonRect = rowRect.SetWidthFromRight(18).MoveX(-4);

            SetGUIColor(Color.clear);
            bool clicked = GUI.Button(buttonRect, "");

            Color normalColor  = EditorGUIUtility.isProSkin ? Greyscale(.9f) : Greyscale(1f, .9f);
            Color hoveredColor = EditorGUIUtility.isProSkin ? Color.white : normalColor;

            SetGUIColor(buttonRect.IsHovered() ? hoveredColor : normalColor);
            GUI.Label(buttonRect.Resize(1).MoveY(-.5f), EditorGUIUtility.IconContent("Lighting"));
            ResetGUIColor();

            if (!clicked) return;

            CustomPopupWindow.Create<VHierarchyLightingWindow>();
        }

        void collapseEverything()
        {
            Rect buttonRect = rowRect.SetWidthFromRight(18).MoveX(-22);

            SetGUIColor(Color.clear);
            bool clicked = GUI.Button(buttonRect, "");

            Color normalColor  = EditorGUIUtility.isProSkin ? Greyscale(.85f) : Greyscale(.1f);
            Color hoveredColor = EditorGUIUtility.isProSkin ? Color.white : normalColor;

            SetGUIColor(buttonRect.IsHovered() ? hoveredColor : normalColor);
            GUI.Label(buttonRect.Resize(1.5f).MoveY(-.5f), EditorGUIUtility.IconContent("PreviewCollapse"));
            ResetGUIColor();

            if (!clicked) return;

            var expandedRoots    = new List<GameObject>();
            var expandedChildren = new List<GameObject>();

            foreach (int iid in expandedIds)
            {
                if (EditorUtility.InstanceIDToObject(iid) is GameObject expandedGo && expandedGo.scene == scene)
                    if (expandedGo.transform.parent) expandedChildren.Add(expandedGo);
                    else expandedRoots.Add(expandedGo);
            }

            expandQueue_toCollapseAfterAnimation = expandedChildren;
            expandQueue_toAnimate                = expandedRoots.Select(r => new ExpandQueueEntry(r.GetInstanceID(), false)).OrderBy(r => VisibleRowIndex(r.instanceId)).ToList();

            EditorApplication.RepaintHierarchyWindow();
        }

        lighting();
        collapseEverything();
    }

    static void CheckShortcuts()
    {
        if (EditorWindow.mouseOverWindow?.GetType() != t_SceneHierarchyWindow) return;
        if (e.type                                  != EventType.KeyDown) return;
        if (e.keyCode                               == KeyCode.None) return;

        void toggleExpanded()
        {
            if (!hoveredGo) return;
            if (e.modifiers != 0) return;
            if (e.type != EventType.KeyDown || e.keyCode != KeyCode.E) return;
            if (!VHierarchyMenuItems.expandCollapseEnabled) return;

            e.Use();

            if (hoveredGo.transform.childCount == 0) return;

            SetExpandedWithAnimation(hoveredGo.GetInstanceID(), !expandedIds.Contains(hoveredGo.GetInstanceID()));
            EditorApplication.RepaintHierarchyWindow();
        }

        void toggleActive()
        {
            if (!hoveredGo) return;
            if (e           == null) return;
            if (e.modifiers != 0) return;
            if (e.type != EventType.KeyDown || e.keyCode != KeyCode.A) return;
            if (!VHierarchyMenuItems.setActiveEnabled) return;

            GameObject[] gos = Selection.gameObjects.Contains(hoveredGo)
                ? Selection.gameObjects
                : new[]
                { hoveredGo };

            bool active = !gos.Any(r => r.activeSelf);

            foreach (GameObject r in gos)
            {
                r.RecordUndo();
                r.SetActive(active);
            }

            e.Use();
        }

        void delete()
        {
            if (!hoveredGo) return;
            if (e.modifiers != 0) return;
            if (eType != EventType.KeyDown || e.keyCode != KeyCode.X) return;
            if (!VHierarchyMenuItems.deleteEnabled) return;

            GameObject[] gos = Selection.gameObjects.Contains(hoveredGo)
                ? Selection.gameObjects
                : new[]
                { hoveredGo };

            foreach (GameObject r in gos) { Undo.DestroyObjectImmediate(r); }

            e.Use();
        }

        void collapseEverything()
        {
            if (e.modifiers != (EventModifiers.Shift | EventModifiers.Command) && e.modifiers != (EventModifiers.Shift | EventModifiers.Control)) return;
            if (eType       != EventType.KeyDown || e.keyCode                                 != KeyCode.E) return;
            if (!VHierarchyMenuItems.collapseEverythingEnabled) return;

            e.Use();

            var expandedRoots    = new List<GameObject>();
            var expandedChildren = new List<GameObject>();

            foreach (int iid in expandedIds)
            {
                if (EditorUtility.InstanceIDToObject(iid) is GameObject expandedGo)
                    if (expandedGo.transform.parent) expandedChildren.Add(expandedGo);
                    else expandedRoots.Add(expandedGo);
            }

            expandQueue_toCollapseAfterAnimation = expandedChildren;
            expandQueue_toAnimate                = expandedRoots.Select(r => new ExpandQueueEntry(r.GetInstanceID(), false)).OrderBy(r => VisibleRowIndex(r.instanceId)).ToList();

            EditorApplication.RepaintHierarchyWindow();
        }

        void collapseEverythingElse()
        {
            if (!hoveredGo) return;
            if (e.modifiers != EventModifiers.Shift) return;
            if (eType != EventType.KeyDown || e.keyCode != KeyCode.E) return;
            if (!VHierarchyMenuItems.collapseEverythingElseEnabled) return;

            e.Use();

            if (hoveredGo.transform.childCount == 0) return;

            var parents = new List<GameObject>();

            GameObject cur = hoveredGo;
            while (cur = cur.transform.parent?.gameObject) parents.Add(cur);

            var toCollapse = new List<GameObject>();

            foreach (int iid in expandedIds.ToList())
            {
                if (EditorUtility.InstanceIDToObject(iid) is GameObject expandedGo && !parents.Contains(expandedGo) && expandedGo != hoveredGo) toCollapse.Add(expandedGo);
            }

            expandQueue_toAnimate = toCollapse.Select(r => new ExpandQueueEntry(r.GetInstanceID(), false)).Append(new (hoveredGo.GetInstanceID(), true)).OrderBy(r => VisibleRowIndex(r.instanceId)).ToList();

            EditorApplication.RepaintHierarchyWindow();
        }

        void focus()
        {
            if (e.modifiers != 0) return;
            if (eType != EventType.KeyDown || e.keyCode != KeyCode.F) return;

            if (SceneView.sceneViews.Count == 0) return;

            var sv = SceneView.lastActiveSceneView;

            if (!sv || !sv.hasFocus) sv = SceneView.sceneViews.ToArray().FirstOrDefault(r => (r as SceneView).hasFocus) as SceneView;

            if (!sv) (sv = SceneView.lastActiveSceneView ?? SceneView.sceneViews[0] as SceneView).Focus();

            sv.Frame(hoveredGo.GetBounds(), false);
        }

        toggleExpanded();
        toggleActive();
        delete();
        collapseEverything();
        collapseEverythingElse();
        focus();
    }

    static void UpdateExpandQueue()
    {
        if (eType != EventType.Layout) return;
        if (hierarchyWindow.GetFieldValue("m_SceneHierarchy").GetFieldValue("m_TreeView").GetPropertyValue<bool>("animatingExpansion")) return;

        if (!expandQueue_toAnimate.Any())
        {
            if (!expandQueue_toCollapseAfterAnimation.Any()) return;

            foreach (GameObject r in expandQueue_toCollapseAfterAnimation) { SetExpanded(r.GetInstanceID(), false); }

            expandQueue_toCollapseAfterAnimation.Clear();

            return;
        }

        int  iid    = expandQueue_toAnimate.First().instanceId;
        bool expand = expandQueue_toAnimate.First().expand;

        if (expandedIds.Contains(iid) != expand) SetExpandedWithAnimation(iid, expand);

        expandQueue_toAnimate.RemoveAt(0);
    }
    static List<ExpandQueueEntry> expandQueue_toAnimate = new ();
    static List<GameObject> expandQueue_toCollapseAfterAnimation = new ();

    struct ExpandQueueEntry
    {
        public int instanceId;
        public bool expand;
        public ExpandQueueEntry(int instanceId, bool expand)
        {
            this.instanceId = instanceId;
            this.expand     = expand;
        }
    }

    static void UpdateExpandedIdsList()
    {
        expandedIds = hierarchyWindow?.GetFieldValue("m_SceneHierarchy")?.GetFieldValue("m_TreeViewState")?.GetPropertyValue<List<int>>("expandedIDs") ?? new List<int>();

        EditorApplication.delayCall -= UpdateExpandedIdsList;
        EditorApplication.delayCall += UpdateExpandedIdsList;
    }
    static List<int> expandedIds;

    static void SetExpandedWithAnimation(int instanceId, bool expanded) => hierarchyWindow.GetFieldValue("m_SceneHierarchy").GetFieldValue("m_TreeView").InvokeMethod("ChangeFoldingForSingleItem", instanceId, expanded);

    static void SetExpanded(int instanceId, bool expanded) => hierarchyWindow.InvokeMethod("SetExpanded", instanceId, expanded);

    static bool IsExpanded(GameObject go) => expandedIds.Contains(go.GetInstanceID());

    static bool IsVisible(GameObject go) => !go.transform.parent || (IsExpanded(go.transform.parent.gameObject) && IsVisible(go.transform.parent.gameObject));

    static int VisibleRowIndex(int instanceId) => hierarchyWindow.GetFieldValue("m_SceneHierarchy").GetFieldValue("m_TreeView").GetPropertyValue("data").InvokeMethod<int>("GetRow", instanceId);

    public static GUIContent GetComponentIcon(Component component)
    {
        if (!component) return new ();

        if (!componentIconsByType.ContainsKey(component.GetType())) componentIconsByType[component.GetType()] = new (EditorGUIUtility.ObjectContent(component, component.GetType()).image);

        return componentIconsByType[component.GetType()];
    }
    static Dictionary<Type, GUIContent> componentIconsByType = new ();

    public static string GetComponentName(Component component)
    {
        string s = new GUIContent(EditorGUIUtility.ObjectContent(component, component.GetType())).text;
        s = s.Substring(s.LastIndexOf('(') + 1);
        s = s.Substring(0, s.Length        - 1);

        return s;
    }

    static void Update_RepaintOnAlt()
    {
        if (EditorWindow.mouseOverWindow?.GetType().Name != "SceneHierarchyWindow") return;

        var curEvent = (Event) typeof(Event).GetField("s_Current", maxBindingFlags).GetValue(null);

        if (curEvent.alt != wasAlt) EditorApplication.RepaintHierarchyWindow();

        wasAlt = curEvent.alt;
    }
    static bool wasAlt;

#if !DISABLED
    [InitializeOnLoadMethod]
#endif
    static void Init()
    {
        void loadData()
        {
            data = AssetDatabase.LoadAssetAtPath<VHierarchyData>(EditorPrefs.GetString("vHierarchy-lastKnownDataPath-" + GetProjectId()));

            if (data) return;

            data = AssetDatabase.FindAssets("t:VHierarchyData").Select(guid => AssetDatabase.LoadAssetAtPath<VHierarchyData>(guid.ToPath())).FirstOrDefault();

            if (!data) return;

            EditorPrefs.SetString("vHierarchy-lastKnownDataPath-" + GetProjectId(), data.GetPath());
        }

        void createData()
        {
            if (data) return;

            data = ScriptableObject.CreateInstance<VHierarchyData>();

            AssetDatabase.CreateAsset(data, GetScriptPath("VHierarchy").GetParentPath().CombinePath("vHierarchy Data.asset"));
        }

        void loadDataDelayed()
        {
            if (data) return;

            EditorApplication.delayCall += () => EditorApplication.delayCall += loadData;

            // AssetDatabase isn't up to date at this point (it gets updated after InitializeOnLoadMethod)
            // and if current AssetDatabase state doesn't contain the data - it won't be loaded during Init()
            // so here we schedule an additional, delayed attempt to load the data
            // this addresses reports of data loss when trying to load it on a new machine
        }

        void createDataDelayed()
        {
            if (data) return;

            EditorApplication.delayCall += () => EditorApplication.delayCall += createData;
        }

        void subscribe()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= RowGUI;
            EditorApplication.hierarchyWindowItemOnGUI += RowGUI;

            EditorApplication.update -= Update_RepaintOnAlt;
            EditorApplication.update += Update_RepaintOnAlt;

            FieldInfo fi_globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", maxBindingFlags);
            fi_globalEventHandler.SetValue(null, CheckShortcuts + (EditorApplication.CallbackFunction) fi_globalEventHandler.GetValue(null));

            FieldInfo fi_projectWasLoaded = typeof(EditorApplication).GetField("projectWasLoaded", maxBindingFlags);
            fi_projectWasLoaded.SetValue(null, (UnityAction) fi_projectWasLoaded.GetValue(null) + OnProjectWasLoaded);
        }

        subscribe();

        loadData();
        loadDataDelayed();
        createDataDelayed();

        UpdateExpandedIdsList();
    }

    static void OnProjectWasLoaded()
    {
        if (!data) return;

        foreach (SceneData sceneData in data.sceneDatasByGuid.values) { sceneData.goDatasByInstanceId.Clear(); }
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (!data) return;

        data?.sceneDatasByScene?.Clear();
    }

    public static VHierarchyData data;

    static EditorWindow hierarchyWindow => _hierarchyWindow ? _hierarchyWindow : _hierarchyWindow = Resources.FindObjectsOfTypeAll(t_SceneHierarchyWindow).FirstOrDefault() as EditorWindow;
    static EditorWindow _hierarchyWindow;
    static Type t_SceneHierarchyWindow = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

    public const string version = "1.0.21";
}
}
#endif
