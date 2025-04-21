using UnityEngine;

public class OutlinePulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.8f;

    private SpriteRenderer spriteRenderer;
    private float alphaDirection = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Color color = spriteRenderer.color;
        color.a += alphaDirection * pulseSpeed * Time.deltaTime;

        if (color.a > maxAlpha)
        {
            color.a = maxAlpha;
            alphaDirection = -1f;
        }
        else if (color.a < minAlpha)
        {
            color.a = minAlpha;
            alphaDirection = 1f;
        }

        spriteRenderer.color = color;
    }
}
