using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    public float lifetime = 2f;
    public int damageForce = 10;
    private Rigidbody2D rb;

    private void Awake() // ✅ Initialisation plus tôt dans le cycle de vie
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null) 
            rb = GetComponent<Rigidbody2D>(); // 🔥 Sécurité en cas de problème

        rb.velocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle); // Tourner vers la direction du joueur

        Destroy(gameObject, lifetime); // Auto-destruction après un temps
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerObj player = collision.GetComponent<PlayerObj>();
            if (player != null)
            {
                player.TakeDamage(damageForce);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
