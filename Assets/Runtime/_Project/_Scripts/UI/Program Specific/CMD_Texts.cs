#region
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

// /*Left*/ rectTransform.offsetMin.x;
// /*Right*/ rectTransform.offsetMax.x;
// /*Top*/ rectTransform.offsetMax.y;
// /*Bottom*/ rectTransform.offsetMin.y;

public class CommandPromptTexts : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text baseText;
    [SerializeField] TMP_Text userText;
    [SerializeField] TMP_Text outputText;
    [SerializeField] ScrollRect scrollRect;

    [Header("Caret")]
    [SerializeField] Image caret;

    const string baseString = @"C:\Users\";
    const string userString = "User"; // Will be replaced with the actual username.

    void OnEnable()
    {
        // Initialize the texts.
        baseText.text = baseString;
        userText.text = $"{userString}" + " > ";

        StartCoroutine(Caret());

        // Adjust the right offset of the user text
        userText.rectTransform.offsetMax = new (userText.text.Length * 10, userText.rectTransform.offsetMax.y);

        scrollRect.content.offsetMin = new (scrollRect.content.offsetMin.x, -3000);

        inputField.onValueChanged.AddListener
        (_ =>
        {
            if (outputText.text.Length > 0) scrollRect.content.offsetMin = new (scrollRect.content.offsetMin.x, scrollRect.content.offsetMin.y - outputText.text.Length * 10);
        });
    }

    void Update()
    {
        bool isVisible = inputField.text.Length <= 0 && !inputField.isFocused;
        caret.gameObject.SetActive(isVisible);
    }

    void OnDisable() => StopCoroutine(Caret());

    IEnumerator Caret()
    {
        while (true)
        {
            if (caret == null) yield break;

            caret.enabled = !caret.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
