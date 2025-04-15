using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform character;        // La cible à suivre (le joueur / ennemi)
    public Vector3 offset = new Vector3(0, 1.5f, 0);   // Décalage au-dessus du character

    void LateUpdate()
    {
        // On suit la position du character sans la rotation
        transform.position = character.position + offset;
        transform.rotation = Quaternion.identity; // Reste toujours orienté neutre

        // Don't flip the bar sprite horizontally
        Vector3 characterScale = character.localScale;
        transform.localScale = new Vector3(characterScale.x, transform.localScale.y, transform.localScale.z);
    }
}
