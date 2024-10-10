#if UNITY_EDITOR
#region
using System.Linq;
using UnityEditor;
using UnityEngine;
#endregion

namespace VInspector
{
[CustomPropertyDrawer(typeof(VariantsAttribute))]
public class VIVariantsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        string[] variants = ((VariantsAttribute) attribute).variants;

        EditorGUI.BeginProperty(rect, label, prop);

        int iCur = prop.hasMultipleDifferentValues ? -1 : variants.ToList().IndexOf(prop.stringValue);

        int iNew = EditorGUI.IntPopup(rect, label.text, iCur, variants, Enumerable.Range(0, variants.Length).ToArray());

        if (iNew != -1) prop.stringValue                            = variants[iNew];
        else if (!prop.hasMultipleDifferentValues) prop.stringValue = variants[0];

        EditorGUI.EndProperty();
    }
}
}
#endif
