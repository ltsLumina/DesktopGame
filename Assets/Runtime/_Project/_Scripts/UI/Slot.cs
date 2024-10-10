#region
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#endregion

public class Slot : MonoBehaviour, IDropHandler
{
    public Image Image { get; private set; }
    public int Row { get; set; }
    public int Column { get; set; }

    void Start() => Image = GetComponent<Image>();

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            var item = eventData.pointerDrag.GetComponent<File>();
            if (item == null) return;

            // Update the previous slot's raycast 
            if (item.CurrentSlot != null) item.CurrentSlot.Image.raycastTarget = true;

            // Update the current slot
            item.CurrentSlot = this;

            eventData.pointerDrag.transform.position = transform.position;

            // Disable the raycastTarget
            Image.raycastTarget = false;
        }
    }
}
