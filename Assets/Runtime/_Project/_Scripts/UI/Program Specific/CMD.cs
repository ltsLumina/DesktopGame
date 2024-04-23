#region
using System.IO;
using TMPro;
using UnityEngine;
#endregion

public class CMD : Window
{
    [SerializeField] CMD_Commands commands;

    [Header("References")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI output;

    public override void Start()
    {
        base.Start();

        // Set the window's title
        title = "Command Prompt";
    }

    void Update()
    {
        // Debug log when the input field is completed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            commands.Execute(new (inputField.text));

            inputField.text = string.Empty;
        }
    }

    /// <summary>
    ///     Outputs the directory tree of the project to the command prompt window.
    ///     <seealso cref="Window" />
    /// </summary>
    public void Tree()
    {
        var directory = new DirectoryInfo(Application.dataPath);

        output.text = CreateDirectoryTree(directory);

        return;

        string CreateDirectoryTree(DirectoryInfo directory, string indent = "")
        {
            string tree = indent + "├── " + directory.Name + "\n";

            foreach (DirectoryInfo subdirectory in directory.GetDirectories()) { tree += CreateDirectoryTree(subdirectory, indent + "│   "); }

            return tree;
        }
    }
}
