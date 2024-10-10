#region
using System.Text;
using UnityEngine;
using UnityEngine.UI;
#endregion

public static class Initializer
{
    /// <summary>
    ///     Initializes a file with a name and image.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="image"></param>
    public static void Initialize(this File file, out Image image)
    {
        file.name = file.Rename(file.FileInfo.name);
        image     = file.FileInfo.CreateSprite(file.transform);
    }

    /// <summary>
    ///     Initializes an entry with a file and items parent.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="file"></param>
    /// <param name="itemsParent"></param>
    public static void Initialize(this Entry entry, File file, Transform itemsParent)
    {
        entry      = Object.Instantiate(entry, itemsParent);
        entry.File = file;
        entry.name = $"Entry - {file.FileInfo.name}";
        entry.name = entry.Rename(file.FileInfo.name);
        entry.transform.SetParent(itemsParent);
        file.FileInfo.CreateSprite(entry.transform, default, new (75, 75));
    }

    public static string Rename(this object @class, string suffix)
    {
        var sb = new StringBuilder();

        // Check if the class has a base type (that isn't MonoBehaviour)
        bool hasBaseType = @class.GetType().BaseType != null && @class.GetType().BaseType != typeof(MonoBehaviour);
        sb.Append(hasBaseType ? @class.GetType().BaseType!.Name : @class.GetType().Name);
        sb.Append(" - ");
        sb.Append(suffix);

        return sb.ToString();
    }
}
