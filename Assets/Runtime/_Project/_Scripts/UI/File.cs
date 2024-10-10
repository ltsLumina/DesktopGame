#region
using System.Collections;
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;
using Image = UnityEngine.UI.Image;
#endregion

public abstract class File
    : MonoBehaviour, Interfaces.IFile, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDeselectHandler
{
    [Tab("Initialization")]
    [Tooltip("The initial position of the object on the grid")]
    [SerializeField] Vector2 initialPosition;

    [Header("File Information")]
    [SerializeField] protected Window.Windows windowType;
    [SerializeField] [ReadOnly] Window window;
    [SerializeField] FileInfo fileInfo;

    [Tab("References")]
    [Tooltip("The image that will be displayed when the object is hovered")]
    [SerializeField] Image onHover;

    [Tooltip("The image that will be displayed when the object is selected")]
    [SerializeField] Image onSelected;

    [Tab("Settings")]
    [SerializeField] [ReadOnly] bool isSelected;
    [SerializeField] [ReadOnly] Slot currentSlot;

    // -- Cached References -- \\

    Vector2 pointerOffset;
    Image image;
    Vector2 position;
    PointerEventData onSelectPointerData;
    ContextMenu contextMenu;

    // -- Properties -- \\

    public FileInfo FileInfo => fileInfo;

    /// <summary>
    ///     Is this object selected?
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        private set => isSelected = value;
    }

    /// <summary>
    ///     The position of the object on the grid and the object's "parent".
    /// </summary>
    public Slot CurrentSlot
    {
        get => currentSlot;
        set => currentSlot = value;
    }

    public Window Window
    {
        get => window;
        private set => window = value;
    }

    // -- End --  \\

    void OnValidate()
    {
        initialPosition.x = Mathf.Clamp(initialPosition.x, 1, 20);
        initialPosition.y = Mathf.Clamp(initialPosition.y, 1, 20);
    }

    void Awake()
    {
        Debug.Assert(onHover    != null, "The onHover image is not set");
        Debug.Assert(onSelected != null, "The onSelected image is not set");
    }

    IEnumerator Start()
    {
        this.Initialize(out image);

        // Disable the onHover and onSelected images
        onHover.enabled    = false;
        onSelected.enabled = false;

        // -- Initialization

        // Parse the vector 2 into two separate integers
        int row    = (int) initialPosition.x;
        int column = (int) initialPosition.y;

        var grid = FindObjectOfType<DesktopGrid>();

        yield return new WaitUntil(() => grid.transform.childCount >= grid.GetSlots().Length);

        // Find a slot with the given row and column
        Slot slot = grid.GetSlot(row, column);

        // Set the current slot of the object
        SetCurrentSlot(slot);
    }

    public virtual void Select(PointerEventData pointerData)
    {
        IsSelected = true;

        // Give a slight background color change to indicate that the object is selected
        onSelected.enabled = true;

        // Select the object
        pointerData.selectedObject = gameObject;
        pointerData.selectedObject.GetComponent<Selectable>().Select();
    }

    public virtual void Deselect(PointerEventData pointerData)
    {
        IsSelected = false;

        // Reset the background color change
        onSelected.enabled = false;
    }

    public virtual void Open(PointerEventData pointerData, File file)
    {
        Taskbar.AddItem(file);

        Window = Window.GetWindow(file);

        if (Window != null)
        {
            Window.Open();
            return;
        }

        // If the window does not exist, create it
        Window = Window.Create<Window>(file, windowType);
    }

    public virtual void OnPointerClick(PointerEventData pointerData)
    {
        // Check for single-click
        if (pointerData.clickCount == 1) Select(pointerData);

        // Check for double-click
        if (pointerData.clickCount == 2)
        {
            // Open the object
            Select(pointerData);
            Open(pointerData, this);
        }
    }

    public virtual void OnPointerEnter(PointerEventData pointerData) =>

        // Give a slight background color change to indicate that the object is being hovered
        onHover.enabled = true;

    public virtual void OnPointerExit(PointerEventData pointerData) =>

        // Reset the background color change
        onHover.enabled = false;

    public virtual void OnPointerDown(PointerEventData pointerData) { }

    public virtual void OnPointerUp(PointerEventData pointerData) { }

    public virtual void OnBeginDrag(PointerEventData pointerData)
    {
        Deselect(pointerData);

        image.raycastTarget = false;

        position = transform.position;

        // Calculate the offset between the pointer and the object's position
        pointerOffset = (Vector2) transform.position - pointerData.position;
    }

    public virtual void OnDrag(PointerEventData pointerData) =>

        // Add the offset to the pointer's position
        transform.position = pointerData.position + pointerOffset;

    public virtual void OnEndDrag(PointerEventData pointerData)
    {
        image.raycastTarget = true;

        if (CurrentSlot != null && pointerData.pointerCurrentRaycast.gameObject != CurrentSlot.gameObject) transform.position = position;

        // Reset the offset
        pointerOffset = Vector2.zero;
    }

    /// <summary>
    ///     If you wish to set the current slot of the object, use this method.
    ///     Used for instance at the start of the game.
    /// </summary>
    /// <param name="slot"></param>
    public void SetCurrentSlot(Slot slot)
    {
        CurrentSlot              = slot;
        slot.Image.raycastTarget = false;
        transform.position       = slot.transform.position;
    }

    public void OnDeselect(BaseEventData eventData) => Deselect(eventData as PointerEventData);
}
