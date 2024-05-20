#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#endregion

[DisallowMultipleComponent]
public abstract class Window : MonoBehaviour, Interfaces.IWindow, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    ///     Determines which window will be instantiated when the file is opened.
    ///     <remarks> Important: The window type must have the same name as the prefab. </remarks>
    /// </summary>
    public enum Windows
    {
        [UsedImplicitly] CMD,
    }

    enum State
    {
        Default,
        Minimized,
        Maximized,
    }

    State state;

    [SerializeField] [ReadOnly] File file;

    [Space(5)]
    [Header("Default Window Size")]
    [SerializeField] [Min(250)] int width = 350;
    [SerializeField] [Min(150)] int height = 250;

    [Space(5)]
    [Header("Window Specific")]
    [Header("Header")]
    [SerializeField] protected Image header;
    [ColorUsage(false)]
    [SerializeField] protected Color headerColor;

    [Header("Icon")]
    [SerializeField] Image icon;

    [Header("Title")]
    [SerializeField] protected string title;
    [SerializeField] protected TMP_Text titleText;

    [Header("Background")]
    [SerializeField] protected Image background;

    RectTransform rect;
    Vector2 position;
    Vector2 pointerOffset;
    
    // this is a test commit for JetBrains Space

    readonly int widthHalf = Screen.width   / 2;
    readonly int heightHalf = Screen.height / 2;

    public bool IsMinimized { get; private set; }
    public bool IsMaximized { get; private set; }

    public delegate void WindowEvent();
    public event WindowEvent OnClose;
    public event WindowEvent OnMinimize;
    public event WindowEvent OnMaximize;

    void OnValidate()
    {
        if (header    != null) header.color   = headerColor;
        if (titleText != null) titleText.text = title;
    }

    public virtual void Start()
    {
        rect = GetComponent<RectTransform>();

        // Set the position to zero, zero on the canvas
        rect.SetAnchoredPosition(position);

        state = State.Default;
    }

    void OnEnable()
    {
        OnClose += () => state = State.Default;

        OnMaximize += () =>
        {
            state       = State.Maximized;
            IsMaximized = true;
        };

        OnMinimize += () =>
        {
            state       = State.Minimized;
            IsMinimized = true;
        };
    }

    public void Open()
    {
        if (gameObject.activeSelf) return;

        // Open the window
        gameObject.SetActive(true);

        if (state == State.Default) Restore(true);

        transform.SetAsLastSibling();
    }

    public void Close()
    {
        position = rect.GetAnchoredPosition();

        Taskbar.RemoveItem(file);

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
        state = State.Default;

        IsMaximized = false;
        IsMinimized = false;
        Vector2 randomOffset = Random.insideUnitCircle * 10;

        if (!instant) this.Resize(rect, new (width, height), new (widthHalf, heightHalf), () => { transform.SetAsLastSibling(); });
        else WindowResizer.Resize(rect, transform, new (width, height), new (widthHalf + randomOffset.x, heightHalf + randomOffset.y), () => { transform.SetAsLastSibling(); });

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
        this.Resize(rect, new (Screen.width - 154, Screen.height - 115), new (widthHalf, heightHalf + 20), () => { OnMaximize?.Invoke(); });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for single-click
        if (eventData.clickCount == 1) SetAsTopWindow();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SetAsTopWindow();

        if (IsMaximized) return;

        // Calculate the offset between the pointer and the object's position
        pointerOffset = (Vector2) transform.position - eventData.position;
    }

    public void OnDrag(PointerEventData pointerData)
    {
        if (IsMaximized) return;

        // Add the offset to the pointer's position
        transform.position = pointerData.position + pointerOffset;
        position           = transform.position;
    }

    public void OnEndDrag(PointerEventData eventData) => pointerOffset = Vector2.zero;

    public static T Create<T>(File file, Windows type)
        where T : Window
    {
        if (!AssertWindowsAreValid()) return null;

        T window = Initialize();

        return window;

        T Initialize()
        {
            var window = Resources.Load<T>($"PREFABS/Windows/{type.ToString()}");
            window      = Instantiate(window, GameObject.FindWithTag("MainCanvas").transform, true);
            window.name = window.Rename(file.FileInfo.name);
            window.transform.SetParent(GameObject.FindWithTag("Windows").transform);
            window.transform.SetAsLastSibling();
            window.icon           = file.FileInfo.CreateSprite(window.header.transform, window.icon.transform, new (10, 10));
            window.titleText.text = window.title;
            window.file           = file;
            return window;
        }

        // Asserts that each window in the Windows enum has a corresponding prefab in the Resources folder.
        // If the window names do not match the prefab names, an error will be thrown.
        static bool AssertWindowsAreValid()
        {
            List<string> windows = (from Windows types in Enum.GetValues(typeof(Windows)) select types.ToString()).ToList();
            List<string> prefabs = Resources.LoadAll<GameObject>("PREFABS/Windows").Select(prefab => prefab.name).ToList();

            if (windows.Except(prefabs).Any())
            {
                const string correctFolder     = "Resources/PREFABS/Windows";
                string       invalidEnumValues = string.Join(", ", windows.Except(prefabs).ToArray());
                string       windowScriptPath  = GetWindowScriptPath(nameof(Window));
                int          invalidLine       = GetInvalidLine(windowScriptPath, invalidEnumValues);
                string       hyperlink         = $"<a href=\"{windowScriptPath}\" line=\"{invalidLine}\">Click to navigate to the invalid enum.</a>";

                string warning = $"The values of the \"{nameof(Windows)}\" enum are not valid. " + "\n" + $"The following enum values are not valid: {invalidEnumValues}" + "\n" +
                                 $"Please ensure that the enum values match the prefab names, and that the prefabs are in the correct folder. ({correctFolder})" + "\n" + $"{hyperlink}";

                Logger.LogError(warning);
                return false;
            }

            return true;
        }

        static string GetWindowScriptPath(string scriptName)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            string   file  = files.FirstOrDefault(file => Path.GetFileNameWithoutExtension(file) == scriptName);
            return ConvertToUnityPath(file);
        }

        static string ConvertToUnityPath(string fullPath)
        {
            string unityPath                = fullPath.Replace("\\", "/");
            int    assetsIndex              = unityPath.IndexOf("/Assets/", StringComparison.Ordinal);
            if (assetsIndex >= 0) unityPath = unityPath.Substring(assetsIndex + 1);
            return unityPath;
        }

        static int GetInvalidLine(string scriptPath, string enumName)
        {
            string[] lines = System.IO.File.ReadAllLines(scriptPath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(enumName)) return i + 1;
            }

            return -1;
        }
    }

    public static Window GetWindow(File file) => file.Window;

    /// <summary>
    ///     Is the window active?
    /// </summary>
    /// <param name="window"></param>
    /// <returns> The window. </returns>
    public static bool GetWindow(Window window) => window.gameObject.activeSelf;

    /// <summary>
    ///     Sets the window as the top-most window.
    /// </summary>
    void SetAsTopWindow() => transform.SetAsLastSibling();
}

public static class RectTransformUtils
{
    public static void SetAnchoredPosition(this RectTransform rectTransform, Vector2 position) => rectTransform.anchoredPosition = position;

    public static Vector2 GetAnchoredPosition(this RectTransform rectTransform) => rectTransform.anchoredPosition;
}
