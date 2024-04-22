#region
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
            commands.Execute(inputField.text);

            inputField.text = string.Empty;
        }
    }

    public void Tree()
    {
        // print the tree structure
        const string tree = @"
        ├── Assets
        │   ├── _Project
        │   │   ├── _Prefabs
        │   │   │   ├── Entry.prefab
        │   │   │   └── File.prefab
        │   │   ├── _Resources
        │   │   │   ├── Entry.prefab
        │   │   │   └── File.prefab
        │   │   └── _Scripts
        │   │       ├── UI
        │   │       │   ├── Entry.cs
        │   │       │   ├── File.cs
        │   │       │   └── Taskbar.cs
        │   │       └── Window.cs
        │   └── _Scenes
        │       ├── MainScene.unity
        │       └── OtherScene.unity
        └── ProjectSettings
            ├── AudioManager.asset
            └── ClusterInputManager.asset";

        output.text = tree;
    }
}
