#region
using UnityEngine;
#endregion

public class DesktopGrid : MonoBehaviour
{
    [SerializeField] int rows = 4;
    [SerializeField] int columns = 4;
    [SerializeField] Slot prefab;

    void Start()
    {
        var   canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float slotWidth  = canvasRect.rect.width  / columns;
        float slotHeight = canvasRect.rect.height / rows;

        float startX = -850;
        float startY = 450;

        // Return if there already are slots in the grid
        if (transform.childCount >= rows * columns) return;

        for (int row = 1; row <= rows; row++)
        {
            for (int column = 1; column <= columns; column++)
            {
                Slot slot     = Instantiate(prefab, transform);
                var  slotRect = slot.GetComponent<RectTransform>();

                slotRect.sizeDelta        = new (300, 300);
                slotRect.anchoredPosition = new (startX + (column - 1) * slotWidth, startY - (row - 1) * slotHeight);

                // Set the slot's row and column
                slot.Row    = row;
                slot.Column = column;

                // Rename the slot to include its position
                slot.name = $"Slot ({row}, {column})";
            }
        }

        // Deletes the last row of slots.
        // This is done as to not create slots that overlap with the taskbar.
        // Not very elegant, but it works.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var slot = transform.GetChild(i).GetComponent<Slot>();
            if (slot.Row == rows) Destroy(slot.gameObject);
        }
    }

    public Slot[] GetSlots()
    {
        var slots = new Slot[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) { slots[i] = transform.GetChild(i).GetComponent<Slot>(); }

        return slots;
    }

    public Slot GetSlot(int row, int column)
    {
        foreach (Transform child in transform)
        {
            var slot = child.GetComponent<Slot>();
            if (slot.Row == row && slot.Column == column) return slot;
        }

        Debug.LogWarning($"Slot at row {row} and column {column} not found.");
        return null;
    }
}
