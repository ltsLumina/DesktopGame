#region
using UnityEngine.EventSystems;
#endregion

public class Program : File, Interfaces.IProgram
{
    public override void Open(PointerEventData pointerData, File file)
    {
        Taskbar.AddItem(file);

        Window = Window.GetWindow(file);

        if (Window != null)
        {
            Window.Open();
            return;
        }

        // If the window does not exist, create it
        Window = Window.Create<Window>(file, "CMD");
    }
}
