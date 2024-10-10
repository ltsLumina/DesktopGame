#region
using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
#endregion

public class CMD : Window
{
    [SerializeField] CMD_Commands commands;

    [Header("References")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI output;
    [Tooltip("The 'Content' object of the scroll view.")]
    [SerializeField] RectTransform scrollRect;

    public override void Update()
    {
        base.Update();

        // Debug log when the input field is completed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            commands.Execute(new (inputField.text));

            inputField.text = string.Empty;
        }
    }

    public void Help()
    {
        output.text += "Available commands:\n";

        foreach (CMD_Commands.Command command in commands.commands)
        {
            output.text += $"{command.CommandName} - {command.Description}" + "\n";
            Debug.Log($"{command.CommandName} - {command.Description}");
        }
    }

    /// <summary>
    ///     Outputs the directory tree of the project to the command prompt window.
    ///     <seealso cref="Window" />
    /// </summary>
    public void Tree()
    {
        var directory = new DirectoryInfo(Application.dataPath);
        StartCoroutine(PrintDirectoryTree(directory));
    }

    IEnumerator PrintDirectoryTree(DirectoryInfo directory, string indent = "")
    {
        output.text += indent + "├── " + directory.Name + "\n";
        MoveScrollRect(scrollRect, output.text.Length);
        yield return new WaitForSeconds(0.1f); // adjust the delay as needed

        foreach (DirectoryInfo subdirectory in directory.GetDirectories()) { yield return StartCoroutine(PrintDirectoryTree(subdirectory, indent + "│   ")); }
    }

    void MoveScrollRect(RectTransform rect, float targetPosition)
    {
        var target   = new Vector2(rect.position.x, 150 + targetPosition * 0.65f);
        var sequence = DOTween.Sequence();
        sequence.Append(rect.DOMove(target, 1f).SetEase(Ease.OutBack));
    }
}
