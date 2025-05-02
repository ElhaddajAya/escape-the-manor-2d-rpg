using UnityEngine;
using System.Collections;

public class UnfreezeGame : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // Wait a bit for load safety

        // Find the player and reset movement
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Reset velocity
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // Optional: reset move target if you use one
            var playerController = playerObj.GetComponent<PlayerObj>();
            if (playerController != null)
            {
                playerController.SetMovePos(playerObj.transform.position);
            }
        }

        // Finally unfreeze input
        GameState.IsFrozen = false;
    }
}
