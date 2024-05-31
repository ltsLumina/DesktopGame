#region
using UnityEngine.EventSystems;
#endregion

public abstract class Interfaces
{
    public interface IFile
    {
        void Select(PointerEventData eventData);

        void Deselect(PointerEventData eventData);

        void Open(PointerEventData eventData, File file);
    }

    public interface IProgram : IFile
    {
    }

    public interface IImage : IFile
    {
    }

    public interface IFolder : IFile
    {
    }

    public interface IMenu
    {
        void Open(PointerEventData pointerData, File file);

        void Close(PointerEventData pointerData);
    }

    public interface IWindow
    {
        void Close();

        void Minimize();

        void Maximize();
    }
    
    public interface IPopUp
    {
        void Close();
    }
}
