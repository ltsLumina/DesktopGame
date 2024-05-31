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
public abstract class PopUp : MonoBehaviour, Interfaces.IPopUp, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    ///     Determines which pop-up window will be instantiated when the file is opened.
    ///     <remarks> Important: The pop-up window type must have the same name as the prefab. </remarks>
    /// </summary>
    public enum PopUps
    {
        [UsedImplicitly] Error,
    }

    [Space(5), Header("Default Pop-Up Window Size")]
    [SerializeField, Min(250)] int width = 350;
    [SerializeField, Min(100)]  int height = 100;

    [Space(5), Header("Pop-Up Window Specific"), Header("Header")] 
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

    readonly int widthHalf = Screen.width   / 2;
    readonly int heightHalf = Screen.height / 2;

    void OnValidate()
    {
        if (header    != null) header.color   = headerColor;
        if (titleText != null) titleText.text = title;
    }

    public virtual void Start()
    {
        rect = GetComponent<RectTransform>();

        // Set the position to (0, 0) on the canvas
        rect.SetAnchoredPosition(position);
    }

    public void Open()
    {
        if (gameObject.activeSelf) return;

        // Open the window
        gameObject.SetActive(true);

        SetAsTopWindow();
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for single-click
        if (eventData.clickCount == 1) SetAsTopWindow();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SetAsTopWindow();

        // Calculate the offset between the pointer and the object's position
        pointerOffset = (Vector2) transform.position - eventData.position;
    }

    public void OnDrag(PointerEventData pointerData)
    {
        // Add the offset to the pointer's position
        transform.position = pointerData.position + pointerOffset;
        position           = transform.position;
    }

    public void OnEndDrag(PointerEventData eventData) => pointerOffset = Vector2.zero;

    public static T Create<T>(PopUps type)
        where T : PopUp
    {
        if (!AssertWindowsAreValid()) return null;

        T window = Initialize();

        return window;

        // Sets the following properties of the window:
        // - Name
        // - Icon
        // - Title
        // - File
        // - Parent
        // Also sets the window as the top-most window.
        T Initialize()
        {
            var popUp = Resources.Load<T>($"PREFABS/Windows/Pop-Ups/{type.ToString()}");
            popUp = Instantiate(popUp, GameObject.FindWithTag("MainCanvas").transform, true);
            popUp.name = popUp.Rename(nameof(type));
            popUp.transform.SetParent(GameObject.FindWithTag("Windows").transform);
            popUp.transform.SetAsLastSibling(); // Note: window.SetAsTopWindow() does the same thing, but for the sake of clarity, I'm using SetAsLastSibling() here.
            //Note: Icon is set per pop-up in the inspector.
            popUp.titleText.text = popUp.title;
            return popUp;
        }

        // Asserts that each window in the Windows enum has a corresponding prefab in the Resources folder.
        // If the window names do not match the prefab names, an error will be thrown.
        static bool AssertWindowsAreValid()
        {
            List<string> windows = (from PopUps types in Enum.GetValues(typeof(PopUps)) select types.ToString()).ToList();
            List<string> prefabs = Resources.LoadAll<GameObject>("PREFABS/Windows/Pop-Ups").Select(prefab => prefab.name).ToList();

            if (windows.Except(prefabs).Any())
            {
                const string correctFolder     = "Resources/PREFABS/Windows/Pop-Ups";
                string       invalidEnumValues = string.Join(", ", windows.Except(prefabs).ToArray());
                string       windowScriptPath  = GetWindowScriptPath(nameof(Window));
                int          invalidLine       = GetInvalidLine(windowScriptPath, invalidEnumValues);
                string       hyperlink         = $"<a href=\"{windowScriptPath}\" line=\"{invalidLine}\">Click to navigate to the invalid enum.</a>";

                string warning = $"The values of the \"{nameof(PopUps)}\" enum are not valid. " + "\n" + $"The following enum values are not valid: {invalidEnumValues}" + "\n" +
                                 $"Please ensure that the enum values match the prefab names, and that the prefabs are in the correct folder. ({correctFolder})" + "\n" +
                                 $"{hyperlink}";

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
            if (assetsIndex >= 0) unityPath = unityPath[(assetsIndex + 1)..];
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

    /// <summary>
    ///     Is the pop-up active?
    /// </summary>
    /// <param name="popUp"></param>
    /// <returns> The popUp. </returns>
    public static bool GetPopUpWindowActiveState(PopUp popUp) => popUp.gameObject.activeSelf;

    /// <summary>
    ///     Sets the window as the top-most window.
    /// </summary>
    void SetAsTopWindow() => transform.SetAsLastSibling();
}
