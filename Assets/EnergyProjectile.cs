using UnityEngine;

public class EnergyProjectile : MonoBehaviour
{
    int damage;
    
    // Cette fonction est appelée lorsqu'un collider entre en contact avec le trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si l'objet touché est un ennemi
        if (other.CompareTag("Enemy")) // Assure-toi que tes ennemis ont le tag "Enemy"
        {
            EnemyAIBase enemy = other.GetComponent<EnemyAIBase>(); // Récupère le script de l'ennemi
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Inflige des dégâts à l'ennemi
            }
        }
    }
}
