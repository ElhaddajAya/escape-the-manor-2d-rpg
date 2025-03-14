using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepSound; // Le son du pas
    private AudioSource audioSource;
    private bool isMoving = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;  // Activer la boucle du son
        audioSource.clip = footstepSound;
    }

    void Update()
    {
        // Vérification continue du mouvement : si le joueur utilise les axes horizontaux ou verticaux
        bool currentMovement = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);

        // Si le joueur commence à bouger
        if (currentMovement && !isMoving)
        {
            isMoving = true;
            audioSource.Play(); // Lancer le son de pas
        }
        // Si le joueur arrête de bouger
        else if (!currentMovement && isMoving)
        {
            isMoving = false;
            audioSource.Stop(); // Arrêter le son
        }
    }
}
