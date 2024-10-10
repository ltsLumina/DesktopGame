#region
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
#endregion

/// <summary>
///     Taskbar entry.
///     Similar to a file, but specifically for the taskbar.
/// </summary>
public class Entry : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] [ReadOnly] File file;

    public File File
    {
        get => file;
        set => file = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Window window = File.Window;
    }
}
