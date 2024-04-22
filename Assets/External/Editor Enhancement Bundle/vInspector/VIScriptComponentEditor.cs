#if UNITY_EDITOR
#region
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using Tab = VInspector.VInspectorData.Tab;
using static VInspector.Libs.VUtils;
using static VInspector.Libs.VGUI;
#endregion

namespace VInspector
{
#if !DISABLED
[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
#endif
internal class VIScriptComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (scriptMissing)
        {
            ScriptMissingWarningGUI();
            return;
        }

        SerializedProperty curProperty     = serializedObject.GetIterator();
        string             selectedTabPath = "";

        void updateSelectedTabPath()
        {
            Tab tab = data.rootTab;

            while (tab != null)
            {
                selectedTabPath += "/" + tab.name;

                if (tab.subtabs.Any() && tab.selectedSubtab == null) tab.selectedSubtab = tab.subtabs.First();

                tab = tab.selectedSubtab;
            }

            selectedTabPath = selectedTabPath.Trim('/');
        }

        void setup()
        {
            if (data == null || !data.isIntact) SetupData();

            if (!VIResettablePropDrawer.scriptTypesWithVInspector.Contains(target.GetType())) VIResettablePropDrawer.scriptTypesWithVInspector.Add(target.GetType());

            curProperty.NextVisible(true);

            data.rootTab.ResetSubtabsDrawn();

            updateSelectedTabPath();
        }

        void drawScriptFieldOrSpace()
        {
            if (VIMenuItems.cleanerHeaderEnabled) Space(3);
            else
                using (new EditorGUI.DisabledScope(true)) { EditorGUILayout.PropertyField(curProperty); }
        }

        void drawBody()
        {
            bool   noVariablesShown   = true;
            string drawingTabPath     = "";
            string drawingFoldoutPath = "";
            bool   hide               = false;
            bool   disable            = false;

            var prevFieldDeclaringType = default(Type);

            void ensureNeededTabsDrawn()
            {
                if (!selectedTabPath.StartsWith(drawingTabPath)) return;

                void drawSubtabs(Tab tab)
                {
                    if (!tab.subtabs.Any()) return;

                    Space(noVariablesShown ? 2 : 6);

                    string selName = TabsMultiRow(tab.selectedSubtab.name, false, 24, tab.subtabs.Select(r => r.name).ToArray());

                    Space(5);

                    if (selName != tab.selectedSubtab.name)
                    {
                        data.RecordUndo();
                        tab.selectedSubtab = tab.subtabs.Find(r => r.name == selName);
                        updateSelectedTabPath();
                    }

                    GUI.backgroundColor = Color.white;

                    tab.subtabsDrawn = true;
                }

                Tab cur = data.rootTab;

                foreach (string name in drawingTabPath.Split('/').Where(r => r != ""))
                {
                    if (!cur.subtabsDrawn) drawSubtabs(cur);

                    cur = cur.subtabs.Find(r => r.name == name);
                }
            }

            void drawCurProperty()
            {
                FieldInfo fieldInfo = null;

                void findFieldInfo()
                {
                    Type curType = target.GetType();

                    while (fieldInfo == null && curType != null && curType != typeof(MonoBehaviour) && curType != typeof(ScriptableObject))
                    {
                        if (curType.GetField(curProperty.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) is FieldInfo fi) fieldInfo = fi;
                        else curType                                                                                                                            = curType.BaseType;
                    }
                }

                void updateIndentLevel(string path)
                {
                    int prev = EditorGUI.indentLevel;

                    EditorGUI.indentLevel = path.Split('/').Where(r => r != "").Count();

                    if (prev > EditorGUI.indentLevel) Space();
                }

                void ifs()
                {
                    var endIfAttribute = fieldInfo.GetCustomAttribute<EndIfAttribute>();

                    if (endIfAttribute != null) hide = disable = false;

                    var ifAttribute = fieldInfo.GetCustomAttribute<IfAttribute>();

                    if (ifAttribute is HideIfAttribute) hide       = ifAttribute.Evaluate(target);
                    if (ifAttribute is ShowIfAttribute) hide       = !ifAttribute.Evaluate(target);
                    if (ifAttribute is DisableIfAttribute) disable = ifAttribute.Evaluate(target);
                    if (ifAttribute is EnableIfAttribute) disable  = !ifAttribute.Evaluate(target);

                    Type curFieldDeclaringType = fieldInfo.DeclaringType;

                    if (prevFieldDeclaringType != null && prevFieldDeclaringType != curFieldDeclaringType) hide = disable = false;

                    prevFieldDeclaringType = curFieldDeclaringType;
                }

                void tabs()
                {
                    var tabAttribute    = fieldInfo.GetCustomAttribute<TabAttribute>();
                    var endTabAttribute = fieldInfo.GetCustomAttribute<EndTabAttribute>();

                    if (endTabAttribute != null)
                    {
                        drawingTabPath     = "";
                        drawingFoldoutPath = "";
                        hide               = disable = false;
                    }

                    if (tabAttribute != null)
                    {
                        drawingTabPath     = tabAttribute.name;
                        drawingFoldoutPath = "";
                        hide               = disable = false;
                    }

                    ensureNeededTabsDrawn();
                }

                void foldouts()
                {
                    var    foldoutAttribute                         = fieldInfo.GetCustomAttribute<FoldoutAttribute>();
                    var    endFoldoutAttribute                      = fieldInfo.GetCustomAttribute<EndFoldoutAttribute>();
                    string newFoldoutPath                           = drawingFoldoutPath;
                    if (endFoldoutAttribute != null) newFoldoutPath = "";
                    if (foldoutAttribute    != null) newFoldoutPath = foldoutAttribute.name;

                    string[] drawingPathSplit = drawingFoldoutPath.Split('/').Where(r => r != "").ToArray();
                    string[] newPathSplit     = newFoldoutPath.Split('/').Where(r => r     != "").ToArray();
                    int      sharedLength     = 0;

                    for (; sharedLength < newPathSplit.Length && sharedLength < drawingPathSplit.Length; sharedLength++)
                    {
                        if (drawingPathSplit[sharedLength] != newPathSplit[sharedLength]) break;
                    }

                    drawingFoldoutPath = string.Join("/", drawingPathSplit.Take(sharedLength));

                    for (int i = sharedLength; i < newPathSplit.Length; i++)
                    {
                        if (!data.rootFoldout.IsSubfoldoutContentVisible(drawingFoldoutPath)) break;

                        string prevPath = drawingFoldoutPath;
                        drawingFoldoutPath += '/' + newPathSplit[i];
                        drawingFoldoutPath =  drawingFoldoutPath.Trim('/');

                        updateIndentLevel(prevPath);
                        VInspectorData.Foldout foldout     = data.rootFoldout.GetSubfoldout(drawingFoldoutPath);
                        bool                   newExpanded = Foldout(foldout.name, foldout.expanded);

                        if (newExpanded != foldout.expanded)
                        {
                            data.RecordUndo();
                            foldout.expanded = newExpanded;
                        }
                    }
                }

                findFieldInfo();

                if (fieldInfo           == null) return;
                if (fieldInfo.FieldType == typeof(VInspectorData)) return;

                ifs();

                if (hide) return;

                GUI.enabled = !disable;

                tabs();

                if (!selectedTabPath.StartsWith(drawingTabPath)) return;

                noVariablesShown = false;

                foldouts();

                if (!data.rootFoldout.IsSubfoldoutContentVisible(drawingFoldoutPath)) return;

                if (fieldInfo.GetCustomAttribute<ButtonAttribute>() != null) return;

                updateIndentLevel(drawingFoldoutPath);

                EditorGUILayout.PropertyField(curProperty, true);
            }

            serializedObject.UpdateIfRequiredOrScript();

            while (curProperty.NextVisible(false)) drawCurProperty();

            if (noVariablesShown)
                using (new EditorGUI.DisabledScope(true)) { GUILayout.Label("No variables to show"); }

            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = 0;
        }

        void drawButtons()
        {
            bool noButtonsToShow = true;

            foreach (VInspectorData.Button button in data.buttons)
            {
                if (button.tab != "" && !selectedTabPath.StartsWith(button.tab)) continue;

                if (button.ifAttribute is HideIfAttribute && button.ifAttribute.Evaluate(target)) continue;
                if (button.ifAttribute is ShowIfAttribute && !button.ifAttribute.Evaluate(target)) continue;

                bool prevGuiEnabled                                                                               = GUI.enabled;
                if (button.ifAttribute is DisableIfAttribute && button.ifAttribute.Evaluate(target)) GUI.enabled  = false;
                if (button.ifAttribute is EnableIfAttribute  && !button.ifAttribute.Evaluate(target)) GUI.enabled = false;

                GUILayout.Space(button.space - 2);

                GUI.backgroundColor = button.isPressed() ? pressedButtonCol : Color.white;

                if (GUILayout.Button(button.name, GUILayout.Height(button.size)))
                    foreach (Object target in targets)
                    {
                        target.RecordUndo();
                        button.action(target);
                    }

                GUI.backgroundColor = Color.white;

                noButtonsToShow = false;
                GUI.enabled     = prevGuiEnabled;
            }

            if (noButtonsToShow) Space(-17);
        }

        setup();

        drawScriptFieldOrSpace();
        drawBody();

        Space(16);
        drawButtons();

        Space(4);
    }

    public void OnEnable()
    {
        CheckScriptMissing();

        if (scriptMissing) return;

        SetupData();
    }

    public void SetupData()
    {
        FieldInfo serializedDataField = target.GetType().GetFields(maxBindingFlags).FirstOrDefault(r => r.FieldType == typeof(VInspectorData));

        void getCached()
        {
            if (!VInspectorCache.instance.datas_byTarget.ContainsKey(target)) return;

            VInspectorCache.EnsureDataIsAlive(target);

            data = VInspectorCache.instance.datas_byTarget[target];
        }

        void getSerialized()
        {
            if (data) return;
            if (serializedDataField == null) return;

            data = serializedDataField.GetValue(target) as VInspectorData;
        }

        void createNew()
        {
            if (data) return;

            data = CreateInstance<VInspectorData>();
        }

        void markTargetDirty()
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(target)) return;
            if (serializedDataField                  == null) return;
            if (serializedDataField.GetValue(target) != null) return;

            target.Dirty();

            // serialized data field field won't be marked as prefab override without marking taget dirty for some reason
            // fixes data not getting serialized on prefab instances
        }

        void cache() => VInspectorCache.instance.datas_byTarget[target] = data;

        void serialize()
        {
            if (serializedDataField == null) return;

            serializedDataField.SetValue(target, data);
        }

        getCached();
        getSerialized();
        createNew();

        markTargetDirty();

        cache();
        serialize();

        data.Setup(target);
        data.Dirty();
    }

    //

    void CheckScriptMissing()
    {
        if (target) scriptMissing = target.GetType() == typeof(MonoBehaviour) || target.GetType() == typeof(ScriptableObject);
        else scriptMissing        = target is MonoBehaviour                   || target is ScriptableObject;
    }

    void ScriptMissingWarningGUI()
    {
        SetGUIEnabled(true);

        if (serializedObject.FindProperty("m_Script") is SerializedProperty scriptProperty)
        {
            EditorGUILayout.PropertyField(scriptProperty);
            serializedObject.ApplyModifiedProperties();
        }

        string s = "Script cannot be loaded";
        s += "\nPossible reasons:";
        s += "\n- Compile erros";
        s += "\n- Script is deleted";
        s += "\n- Script file name doesn't match class name";
        s += "\n- Class doesn't inherit from MonoBehaviour";

        Space(4);
        EditorGUILayout.HelpBox(s, MessageType.Warning, true);

        Space(4);

        ResetGUIEnabled();
    }

    bool scriptMissing;

    VInspectorData data;

    const string version = "1.2.29";
}
}
#endif
