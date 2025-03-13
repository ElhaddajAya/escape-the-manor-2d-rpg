using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    public AudioClip footstepClip; // Son des pas
    public float stepInterval = 0.2f; // Temps entre chaque pas
    public float stepStartTime = 0.2f; // Début du son (ex: 0.5s dans l'audio)
    public float stepDuration = 0.2f; // Durée du pas
    private AudioSource audioSource;
    private float stepTimer;
    private bool isPlayingStep = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        stepTimer = 0f;
    }

    void Update()
    {
        if (IsWalking())
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f && !isPlayingStep)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    bool IsWalking()
    {
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }

    void PlayFootstep()
    {
        if (footstepClip != null)
        {
            isPlayingStep = true;
            audioSource.clip = footstepClip;
            audioSource.time = stepStartTime; // Joue à partir de stepStartTime
            audioSource.Play();
            StartCoroutine(StopAfterDelay(stepDuration));
        }
        else
        {
            Debug.LogWarning("Footstep clip is missing!");
        }
    }

    System.Collections.IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop(); // Arrête après stepDuration
        isPlayingStep = false;
    }
}