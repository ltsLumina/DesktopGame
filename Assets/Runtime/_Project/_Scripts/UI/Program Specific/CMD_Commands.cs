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
        [SerializeField] string description;
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
            get => description;
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

    /// <summary>
    ///     The "Help" command is the only built-in command, meaning it is defined in the CMD_Commands class, rather than the
    ///     CMD class as the others are.
    /// </summary>
    public void Help()
    {
        foreach (Command command in commands) { Debug.Log($"{command.CommandName} - {command.Description}"); }
    }

    public void Execute(Command command)
    {
        // Find the command (including aliases), ignoring case.
        Command cmd = commands.Find(c => c.CommandName.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase) || c.Aliases.Exists(a => a.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase)));

        if (cmd.Input == null) return;

        cmd.OnExecute.Invoke(cmd);
    }
}
