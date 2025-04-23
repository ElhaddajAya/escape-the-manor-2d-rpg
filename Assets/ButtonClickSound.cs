using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayPartialClip()
    {
        StartCoroutine(PlayShortSound());
    }

    private System.Collections.IEnumerator PlayShortSound()
    {
        audioSource.Play();
        yield return new WaitForSeconds(0.1f); // play only 0.5s
        audioSource.Stop();
    }
}
