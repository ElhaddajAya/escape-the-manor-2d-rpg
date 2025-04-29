using UnityEngine;
using TMPro;
using System.Collections;

public class SceneTitleDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public float displayTime = 3f;

    void Start()
    {
        if (titleText != null)
        {
            StartCoroutine(ShowSceneTitle());
        }
    }

    IEnumerator ShowSceneTitle()
    {
        titleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        titleText.gameObject.SetActive(false);
    }
}
