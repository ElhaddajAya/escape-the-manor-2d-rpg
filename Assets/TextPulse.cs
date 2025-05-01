using UnityEngine;

public class TextPulse : MonoBehaviour
{
    public float pulseSpeed = 1.5f;
    public float pulseScale = 0.05f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
{
    Debug.Log("Pulsing"); // See if this shows in console
    float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
    transform.localScale = originalScale * scale;
}

}
