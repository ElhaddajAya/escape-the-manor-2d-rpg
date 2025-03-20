using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    private UnityEngine.Rendering.Universal.Light2D candleLight; // Reference to 2D light
    public float minIntensity = 1.0f;  // Minimum brightness
    public float maxIntensity = 2.5f;  // Maximum brightness
    public float flickerSpeed = 5.0f;  // Speed of flickering

    void Start()
    {
        candleLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();  
    }

    void Update()
{
    if (candleLight != null)
    {
        // Create a more randomized flickering effect
        float randomFlicker = Random.Range(minIntensity, maxIntensity);
        
        // Use SmoothDamp for a more organic flicker effect
        candleLight.intensity = Mathf.Lerp(candleLight.intensity, randomFlicker, Time.deltaTime * flickerSpeed);

        // OPTIONAL: Slightly change radius for more realism
        candleLight.pointLightOuterRadius = Random.Range(1.8f, 2.2f);
    }
}

}
