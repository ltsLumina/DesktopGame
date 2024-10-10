#region
using UnityEngine.UI;
#endregion

public class WindowButton : Button
{
    Window window;

    bool addedListeners;

    protected override void Start()
    {
        base.Start();

        window = GetComponentInParent<Window>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // If the event count is 0, add the listener manually.
        // This is a backup in case Unity removes the listener for whatever reason.
        if (onClick.GetPersistentEventCount() == 0)
        {
            switch (name)
            {
                case "Minimize":
                    onClick.AddListener(Minimize);
                    break;

                case "Maximize":
                    onClick.AddListener(Maximize);
                    break;

                case "Close":
                    onClick.AddListener(Close);
                    break;
            }

            addedListeners = true;
            Logger.LogWarning($"Added listener to {name} button because Unity removed it." + "\nYou should probably look into this.");
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Only remove the listener if it was added by this script.
        if (addedListeners)
            switch (name)
            {
                case "Minimize":
                    onClick.RemoveListener(Minimize);
                    break;

                case "Maximize":
                    onClick.RemoveListener(Maximize);
                    break;

                case "Close":
                    onClick.RemoveListener(Close);
                    break;
            }
    }

    public void Minimize() => window.Minimize();

    public void Maximize() => window.Maximize();

    public void Close() => window.Close();
}
