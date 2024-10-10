#region
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
#endregion

public static class WindowResizer
{
    /// <summary>
    ///     Resizes the window instantly.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="transform"></param>
    /// <param name="newSize"></param>
    /// <param name="newPosition"></param>
    /// <param name="callback"></param>
    public static void Resize(RectTransform rect, Transform transform, Vector2 newSize, Vector2 newPosition, Action callback)
    {
        rect.sizeDelta     = newSize;
        transform.position = newPosition;
        callback?.Invoke();
    }

    /// <summary>
    ///     Resizes the window with a smooth animation.
    ///     Has a callback that is invoked when the animation is complete.
    /// </summary>
    /// <param name="host"> The window that is being resized. </param>
    /// <param name="rect"></param>
    /// <param name="newSize"></param>
    /// <param name="newPosition"></param>
    /// <param name="callback"></param>
    public static void Resize(this Window host, RectTransform rect, Vector2 newSize, Vector2 newPosition, Action callback) => host.StartCoroutine(ResizeRoutine(rect, newSize, newPosition, callback));

    static IEnumerator ResizeRoutine(RectTransform rect, Vector2 newSize, Vector2 newPosition, Action callback)
    {
        if (newPosition != default)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOMove(newPosition, 0.5f).SetEase(Ease.OutBack));
            sequence.Join(rect.DOSizeDelta(newSize, 0.5f).SetEase(Ease.OutQuart));

            yield return sequence.WaitForCompletion();

            callback?.Invoke();
        }
    }
}
