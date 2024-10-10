#region
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
#endregion

public abstract class FileInfo : ScriptableObject
{
    [Header("General Info")]
    new public string name;
    public string extension;
    public Texture2D icon;

    [TextArea(3, 10)]
    public string description;
    [TextArea(0, 1)]
    public string path;

    [Header("Size in MB")] [Min(1)]
    public float size;

    [Header("Dates")]
    [Delayed]
    public string dateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Delayed]
    public string dateModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [Delayed]
    public string dateAccessed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public Image CreateSprite(Transform parent, Transform position = default, Vector2 sizeDelta = default)
    {
        // Delete the existing Editor sprite.
        // The icon that is used in the editor. The actual icon is created at runtime.
        Image editorIcon = parent.GetComponentsInChildren<Image>().FirstOrDefault(i => i.CompareTag("Editor"));

        var sprite = new GameObject($"{name} (Sprite)").AddComponent<Image>();
        sprite.sprite = Sprite.Create(icon, new (0, 0, icon.width, icon.height), Vector2.zero);
        sprite.transform.SetParent(parent);

        if (editorIcon != null)
        {
            sprite.rectTransform.anchorMin = editorIcon.rectTransform.anchorMin;
            sprite.rectTransform.anchorMax = editorIcon.rectTransform.anchorMax;
        }

        sprite.transform.localScale    = Vector3.one;
        sprite.transform.localPosition = position  == default ? Vector3.zero : position.localPosition;
        sprite.rectTransform.sizeDelta = sizeDelta == default ? new (100, 100) : sizeDelta;

        // Delete the Editor sprite after initializing the new sprite.
        Destroy(editorIcon?.gameObject);

        return sprite;
    }

    public override string ToString() => $"Name: {name}\n"                  + $"Extension: {extension}\n" + $"Description: {description}\n" + $"Path: {path}\n" + $"Size: {size} MB\n" + $"Date Created: {dateCreated}\n" +
                                         $"Date Modified: {dateModified}\n" + $"Date Accessed: {dateAccessed}\n";

    void OnValidate()
    {
        // Prepend a dot to the extension if it's missing
        if (!string.IsNullOrEmpty(extension) && !extension[0].Equals('.')) extension = $".{extension}";

        // Ensure the date format is correct
        if (!DateTime.TryParse(dateCreated, out _))
        {
            dateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.LogWarning("\"Date Created\" is not in the correct format. It has been set to the current date and time." + "\nPlease use the format: yyyy-MM-dd HH:mm:ss");
        }

        if (!DateTime.TryParse(dateModified, out _))
        {
            dateModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.LogWarning("\"Date Modified\" is not in the correct format. It has been set to the current date and time." + "\nPlease use the format: yyyy-MM-dd HH:mm:ss");
        }

        if (!DateTime.TryParse(dateAccessed, out _))
        {
            dateAccessed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.LogWarning("\"Date Accessed\" is not in the correct format. It has been set to the current date and time." + "\nPlease use the format: yyyy-MM-dd HH:mm:ss");
        }
    }
}
