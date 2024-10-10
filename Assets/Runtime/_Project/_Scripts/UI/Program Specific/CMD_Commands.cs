#region
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#endregion

public class CMD_Commands : MonoBehaviour
{
    [SerializeField] public List<Command> commands;

    /// <summary>
    ///     <para>Input: The command name.</para>
    ///     <para>onExecute: The event that is invoked when the command is executed.</para>
    /// </summary>
    [Serializable]
    public struct Command
    {
        [SerializeField] string commandName;
        [SerializeField] [Multiline] string description;
        [SerializeField] List<string> aliases;
        [Space(10)]
        [SerializeField] UnityEvent<Command> onExecute;

        public string Input
        {
            get => commandName;
            set => commandName = value;
        }

        public string CommandName
        {
            get => commandName;
            set => commandName = value;
        }

        public string Description
        {
            get
            {
                description = string.IsNullOrEmpty(description) ? "No description available." : description;
                return description;
            }
            set => description = value;
        }

        public List<string> Aliases
        {
            get => aliases;
            set => aliases = value;
        }

        public UnityEvent<Command> OnExecute
        {
            get => onExecute;
            set => onExecute = value;
        }

        public Command(string input) : this() { Input = input; }
    }

    public void Execute(Command command)
    {
        // Find the command (including aliases), ignoring case.
        Command cmd = commands.Find
        (c =>
        {
            // Check if the command name matches the input.
            bool doesCommandNameMatch = c.CommandName.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase);

            // Check if the input is an alias.
            bool isAlias = c.Aliases.Exists(a => a.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase));

            return doesCommandNameMatch || isAlias;
        });

        if (cmd.Input == null) return;

        cmd.OnExecute.Invoke(cmd);
    }
}
