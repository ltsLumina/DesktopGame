//#define DEPRECATED

#region
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
#endregion

public class WindowResizer : MonoBehaviour
#if DEPRECATED
    , IBeginDragHandler, IDragHandler, IEndDragHandler
#endif
{
    RectTransform rectTransform;
    Vector2 initialSize;
    Vector2 initialMousePosition;

    void Awake() => rectTransform = GetComponent<RectTransform>();

    /// <summary>
    ///     Resizes the window instantly.
    /// </summary>
    /// <param name="newSize"></param>
    /// <param name="newPosition"></param>
    public void Resize(Vector2 newSize, Vector2 newPosition)
    {
        rectTransform.sizeDelta = newSize;
        transform.position      = newPosition;
    }

    /// <summary>
    ///     Resizes the window with a smooth animation.
    ///     Has a callback that is invoked when the animation is complete.
    /// </summary>
    /// <param name="newSize"></param>
    /// <param name="newPosition"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator Resize(Vector2 newSize, Vector2 newPosition, Action callback)
    {
        if (newPosition != default)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOMove(newPosition, 0.5f).SetEase(Ease.OutBack));
            sequence.Join(rectTransform.DOSizeDelta(newSize, 0.5f).SetEase(Ease.OutBack));

            yield return sequence.WaitForCompletion();

            callback?.Invoke();
        }
    }

#if DEPRECATED
    public void OnBeginDrag(PointerEventData eventData)
    {
        initialSize = rectTransform.sizeDelta;
        initialMousePosition = eventData.position;

        command = new (this);
        commandManager.ExecuteCommand(command);
        
        Resizing = true;
    }

    Commands.ResizeCommand command;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = eventData.position;
        Vector2 difference = currentMousePosition - initialMousePosition;

        // Adjust the width and height separately
        var newSize = new Vector2(initialSize.x + difference.x, initialSize.y - difference.y);

        // Ensure the window has a minimum size
        newSize = Vector2.Max(newSize, new (250, 150));
        
        // Set a maximum size for the window
        newSize = Vector2.Min(newSize, new (Screen.width, Screen.height));

        rectTransform.sizeDelta = newSize;
    }

    public void OnEndDrag(PointerEventData eventData) => Resizing = false;

    public void UndoResize()
    {
        commandManager.UndoCommand(command);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            UndoResize();
        }
    }
#endif
}
