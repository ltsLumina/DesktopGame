#region
using System.Collections.Generic;
using UnityEngine;
#endregion

public class Taskbar : MonoBehaviour
{
    [SerializeField] List<File> items;

    Transform itemsParent;

    readonly static List<File> s_items = new (9);

    public List<File> Items
    {
        get
        {
            items = s_items;
            return items;
        }
        set => items = value;
    }

    delegate void ItemAction(File file);
    static event ItemAction OnItemAdded;
    static event ItemAction OnItemRemoved;

    void Awake()
    {
        s_items.Clear();
        itemsParent = transform.GetChild(0);
    }

    void OnEnable()
    {
        OnItemAdded   += AddToTaskbar;
        OnItemRemoved += RemoveFromTaskbar;
    }

    void OnDisable()
    {
        OnItemAdded   -= AddToTaskbar;
        OnItemRemoved -= RemoveFromTaskbar;
    }

    void AddToTaskbar(File file)
    {
        // Create new taskbar entry.
        var entry = Resources.Load<Entry>("PREFABS/Entry");

        entry.Initialize(file, itemsParent);
    }

    void RemoveFromTaskbar(File file)
    {
        // Find the entry associated with the file.
        var entry = itemsParent.GetComponentInChildren<Entry>();

        // Destroy the entry.
        Destroy(entry.gameObject);
    }

    void Start()
    {
        Items = s_items;

        foreach (File item in items) { item.transform.SetParent(transform); }
    }

    /// <summary>
    ///     Adds a file to the taskbar.
    ///     Won't add the file if it already exists in the taskbar.
    /// </summary>
    /// <param name="file"></param>
    public static void AddItem(File file)
    {
        if (s_items.Contains(file)) return;

        s_items.Add(file);
        OnItemAdded?.Invoke(file);
    }

    public static void RemoveItem(File file)
    {
        if (!s_items.Contains(file))
        {
            Logger.LogWarning("You are trying to remove an file that is not in the taskbar.");
            return;
        }

        s_items.Remove(file);
        OnItemRemoved?.Invoke(file);
    }
}
