#if UNITY_EDITOR
#region
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static VHierarchy.Libs.VUtils;
#endregion

namespace VHierarchy
{
public class VHierarchyData : ScriptableObject
{
    public SerializeableDicitonary<string, SceneData> sceneDatasByGuid = new ();
    public Dictionary<Scene, SceneData> sceneDatasByScene = new ();

    [Serializable]
    public class SceneData
    {
        public SerializeableDicitonary<string, GameObjectData> goDatasByGlobalId = new ();
        public SerializeableDicitonary<int, GameObjectData> goDatasByInstanceId = new (); // serializable so prefabs don't loose their icons on playmode enter
    }

    [Serializable]
    public class GameObjectData
    {
        public Color color => VHierarchyIconEditor.GetColor(iColor);
        public int iColor;
        public string icon = "";
    }
}
}
#endif
