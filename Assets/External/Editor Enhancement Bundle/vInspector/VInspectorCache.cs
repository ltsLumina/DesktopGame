#if UNITY_EDITOR
#region
using System.Linq;
using UnityEditor;
using UnityEngine;
using static VInspector.Libs.VUtils;
#endregion

namespace VInspector
{
[FilePath("Library/vInspector Cache.asset", FilePathAttribute.Location.ProjectFolder)]
public class VInspectorCache : ScriptableSingleton<VInspectorCache>
{
    public SerializableDictionary<Object, VInspectorData> datas_byTarget = new ();

    public static void EnsureDataIsAlive(Object target)
    {
        if (instance.datas_byTarget[target]           != null) return;
        if (instance.datas_byTarget[target] as object == null) return;

        VInspectorData destroyedData = instance.datas_byTarget[target];

        var recreatedData = CreateInstance<VInspectorData>();

        recreatedData.buttons     = destroyedData.buttons;
        recreatedData.rootFoldout = destroyedData.rootFoldout;
        recreatedData.rootTab     = destroyedData.rootTab;

        instance.datas_byTarget[target] = recreatedData;

        // cached data survives domain reloads as System.Object
        // but gets destroyed as UnityEngine.Object
        // so here we may have to undestroy it
    }

    public static void KeepDatasAlive() // delayCall loop
    {
        foreach (Object target in instance.datas_byTarget.Keys.ToList())
        {
            if (!target) instance.datas_byTarget.Remove(target);
            else EnsureDataIsAlive(target);
        }

        EditorApplication.delayCall -= KeepDatasAlive;
        EditorApplication.delayCall += KeepDatasAlive;
    }

    [InitializeOnLoadMethod]
    public static void Init() => EditorApplication.delayCall += KeepDatasAlive;
}
}
#endif
