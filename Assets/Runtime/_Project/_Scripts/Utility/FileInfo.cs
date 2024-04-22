#region
using System;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
#endregion

public abstract class FileInfo : ScriptableObject
{
    [Header("General Info")]
    new public string name;
    public string extension;

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

    [SerializeField] Texture2D icon;

    public Image CreateSprite(Transform parent)
    {
        var sprite = new GameObject($"{name} (Sprite)").AddComponent<Image>();
        sprite.sprite = Sprite.Create(icon, new (0, 0, icon.width, icon.height), Vector2.zero);
        sprite.transform.SetParent(parent);
        sprite.transform.localScale    = Vector3.one;
        sprite.transform.localPosition = Vector3.zero;
        sprite.rectTransform.sizeDelta = new (100, 100);

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
