using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private string targetSpawnPointName;

    [Header("🔐 Lock Settings")]
    public bool requiresKey = false;
    public string requiredKeyName = "MasterBedroomKey"; // À adapter pour chaque porte
    public TextMeshProUGUI lockedTextUI; // UI Text (You need a key)
    public float fadeDuration = 1f;
    public float displayTime = 1.5f;

    private Coroutine fadeCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Cas : PAS de clé requise
            if (!requiresKey)
            {
                SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);
                StartCoroutine(LoadScene());
                return;
            }

            // Cas : Clé requise → vérifier si le joueur l’a
            Inventory inventory = GameObject.FindGameObjectWithTag("InventoryManager")?.GetComponent<Inventory>();
            if (inventory != null && inventory.HasItem(requiredKeyName))
            {
                // ✅ Le joueur possède la clé
                SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);
                StartCoroutine(LoadScene());
            }
            else
            {
                // ❌ Pas de clé → afficher message
                if (lockedTextUI != null)
                {
                    if (fadeCoroutine != null)
                        StopCoroutine(fadeCoroutine);

                    fadeCoroutine = StartCoroutine(ShowLockedText());
                }
            }
        }
    }

    private IEnumerator LoadScene()
    {
        animator.SetTrigger("Start");

        if (audioSource != null && doorSound != null)
        {
            audioSource.PlayOneShot(doorSound);
        }

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(sceneToLoad);
        yield return new WaitForEndOfFrame();

        animator.SetTrigger("End");
        yield return new WaitForSeconds(1);
    }

    private IEnumerator ShowLockedText()
    {
        lockedTextUI.gameObject.SetActive(true);

        // Fade In
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetTextAlpha(alpha);
            yield return null;
        }

        // Wait
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (t / fadeDuration));
            SetTextAlpha(alpha);
            yield return null;
        }

        lockedTextUI.gameObject.SetActive(false);
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = lockedTextUI.color;
        color.a = alpha;
        lockedTextUI.color = color;
    }
}
