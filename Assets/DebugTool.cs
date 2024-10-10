#region
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
#endregion

public class DebugTool : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Alpha 1 was pressed.");
            PopUp.Create<PopUp>(PopUp.PopUps.Error).Open();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Alpha 2 was pressed.");
            PopUp[] popUps = FindObjectsOfType<PopUp>();
            foreach (PopUp popUp in popUps) { popUp.Close(); }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Alpha 3 was pressed.");
            StartCoroutine(CreateMultipleErrors(12, 0.2f));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Alpha 4 was pressed.");
            StartCoroutine(IconicMultipleErrors(12, 0.2f));
        }
    }

    IEnumerator CreateMultipleErrors(int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            PopUp.Create<PopUp>(PopUp.PopUps.Error).Open();
            yield return new WaitForSeconds(Random.Range(0, delay + 0.3f));
        }
    }

    IEnumerator IconicMultipleErrors(int count, float delay)
    {
        Vector2 position = new Vector2(100, 100); // Starting position for the first pop-up

        for (int i = 0; i < count; i++)
        {
            var popUp = PopUp.Create<PopUp>(PopUp.PopUps.Error);
            popUp.Open();

            var rect = popUp.GetComponent<RectTransform>();
            rect.SetAnchoredPosition(position);

            position.x -= 10; // Move 10 units to the left
            position.y -= 10; // Move 10 units down

            yield return new WaitForSeconds(Random.Range(0, delay + 0.3f));
        }
    }
}
