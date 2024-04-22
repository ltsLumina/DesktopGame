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

    [Header("Caret")]
    [SerializeField] Image caret;

    readonly string baseString = @"C:\Users\";
    readonly string userString = "User"; // Will be replaced with the actual username.

    void OnEnable()
    {
        // Initialize the texts.
        baseText.text = baseString;
        userText.text = $"{userString}" + " > ";

        StartCoroutine(Caret());

        // Adjust the right offset of the user text
        userText.rectTransform.offsetMax = new (userText.text.Length * 10, userText.rectTransform.offsetMax.y);
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
