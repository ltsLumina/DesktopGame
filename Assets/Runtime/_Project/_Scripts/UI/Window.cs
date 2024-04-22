#region
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#endregion

public abstract class Window : MonoBehaviour, Interfaces.IWindow, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    enum WindowState
    {
        Default,
        Minimized,
        Maximized,
    }

    WindowState state;

    [SerializeField] [ReadOnly] File file;

    [Space(5)]
    [Header("Default Window Size")]
    [SerializeField] [Min(250)] int width = 400;
    [SerializeField] [Min(150)] int height = 400;

    [Space(5)]
    [Header("Window Specific")]
    [SerializeField] protected string title;
    [SerializeField] protected Image background;

    RectTransform rect;
    Vector2 position;
    Vector2 pointerOffset;

    readonly int widthHalf = Screen.width   / 2;
    readonly int heightHalf = Screen.height / 2;

    public File File
    {
        get => file;
        private set => file = value;
    }

    public bool IsMinimized { get; private set; }
    public bool IsMaximized { get; private set; }

    public delegate void WindowEvent();
    public event WindowEvent OnClose;
    public event WindowEvent OnMinimize;
    public event WindowEvent OnMaximize;

    public virtual void Start()
    {
        rect = GetComponent<RectTransform>();

        // Set the position to zero, zero on the canvas
        rect.SetAnchoredPosition(position);

        state = WindowState.Default;
    }

    void OnEnable()
    {
        OnClose += () => state = WindowState.Default;

        OnMaximize += () =>
        {
            state       = WindowState.Maximized;
            IsMaximized = true;
        };

        OnMinimize += () =>
        {
            state       = WindowState.Minimized;
            IsMinimized = true;
        };
    }

    public void Open()
    {
        if (gameObject.activeSelf) return;

        rect.SetAnchoredPosition(position);

        // Open the window
        gameObject.SetActive(true);

        if (state.Equals(WindowState.Default)) Restore(true);

        transform.SetAsLastSibling();
    }

    public void Close()
    {
        position = rect.GetAnchoredPosition();

        Taskbar.RemoveItem(File);

        gameObject.SetActive(false);
        OnClose?.Invoke();
    }

    public void Minimize()
    {
        transform.SetAsLastSibling();

        gameObject.SetActive(false);
        OnMinimize?.Invoke();
    }

    public void Restore(bool instant = false)
    {
        state = WindowState.Default;

        IsMaximized = false;
        IsMinimized = false;

        // Restore the window to the default size and setting.
        rect.SetAnchoredPosition(position);

        var resizer = GetComponentInChildren<WindowResizer>();

        if (!instant) { StartCoroutine(resizer.Resize(new (width, height), new (widthHalf, heightHalf), () => { transform.SetAsLastSibling(); })); }
        else
        {
            resizer.Resize(new (width, height), new (widthHalf, heightHalf));
            transform.SetAsLastSibling();
        }

        gameObject.SetActive(true);
    }

    public void Maximize()
    {
        // If the window is already maximized, minimize it
        if (IsMaximized)
        {
            Restore();
            return;
        }

        transform.SetAsLastSibling(); // Last sibling is the top-most window

        // Maximize the window
        var resizer = GetComponentInChildren<WindowResizer>();
        StartCoroutine(resizer.Resize(new (Screen.width - 154, Screen.height - 115), new (widthHalf, heightHalf + 20), () => { OnMaximize?.Invoke(); }));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for single-click
        if (eventData.clickCount == 1)

            // Set the window to the top of the hierarchy
            transform.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (state.Equals(WindowState.Maximized)) return;

        // Calculate the offset between the pointer and the object's position
        pointerOffset = (Vector2) transform.position - eventData.position;
    }

    public void OnDrag(PointerEventData pointerData)
    {
        if (state.Equals(WindowState.Maximized)) return;

        //TODO: Clamp the window to the screen

        // Add the offset to the pointer's position
        transform.position = pointerData.position + pointerOffset;
        position           = transform.position;
    }

    public void OnEndDrag(PointerEventData eventData) => pointerOffset = Vector2.zero;

    public static T Create<T>(File file, string name)
        where T : Window
    {
        // Create the window
        var window = Resources.Load<T>($"PREFABS/Windows/{name}");
        window      = Instantiate(window, GameObject.FindWithTag("MainCanvas").transform, true);
        window.name = $"Window - {file.FileInfo.name}";
        window.transform.SetParent(GameObject.FindWithTag("Windows").transform);
        window.transform.SetAsLastSibling();

        // Set the file
        window.File = file;

        return window;
    }

    public static Window GetWindow(File file) =>

        // Get the window for the file
        file.Window;

    /// <summary>
    ///     Is the window active?
    /// </summary>
    /// <param name="window"></param>
    /// <returns> The window. </returns>
    public static bool GetWindow(Window window) =>

        // Get the window state
        window.gameObject.activeSelf;
}

public static class RectTransformUtils
{
    public static void SetAnchoredPosition(this RectTransform rectTransform, Vector2 position) => rectTransform.anchoredPosition = position;

    public static Vector2 GetAnchoredPosition(this RectTransform rectTransform) => rectTransform.anchoredPosition;
}
