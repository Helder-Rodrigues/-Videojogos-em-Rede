using UnityEngine;
using Mirror;

public class EatingMechanic : NetworkBehaviour
{
    private float baseGrowthFactor = 0.1f;
    private const float minPlayerSize = 0.31f;

    private const float tolerance = 0.001f; // A small threshold for floating-point comparison

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocalPlayer) return; // Only the local player triggers this

        string colliderName = collision.name.ToLower();
        bool foodEaten = colliderName.Contains("food");
        bool playerEaten = colliderName.Contains("player");
        bool powerUpEaten = colliderName.Contains("pup");

        if (foodEaten || playerEaten || powerUpEaten)
        {
            float playerSize = transform.localScale.x;
            float colliderSize = collision.transform.localScale.x;
            // Does nothing if the player can't eat
            // Use tolerance to compare sizes
            if (playerSize - colliderSize < tolerance)
                return;

            if (foodEaten || playerEaten)
            {
                float growFactor = foodEaten ? baseGrowthFactor : colliderSize * (colliderSize / playerSize);

                // Call command to grow the player
                CmdGrowPlayer(growFactor);
            }
            else if (powerUpEaten)
            {
                ApplyPowerUpEffect(collision);
            }

            // Call command to destroy the eaten object
            CmdDestroyEatenObject(collision.gameObject, playerEaten);
        }
    }

    // Method to handle power-up effects
    private void ApplyPowerUpEffect(Collider2D collision)
    {
        // Get the PowerUp script from the collided object
        PowerUp powerUp = collision.GetComponent<PowerUp>();
        if (powerUp == null) return;

        float playerSize = transform.localScale.x;

        // Apply power-up effect based on the type
        switch (powerUp.powerUpType)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                transform.gameObject.GetComponent<PlayerMovement>().speed *= 1.5f;
                break;

            case PowerUp.PowerUpType.DoubleSize:
            case PowerUp.PowerUpType.HalfSize:
                float sizeChangeFactor = (powerUp.powerUpType == PowerUp.PowerUpType.DoubleSize) ? playerSize : -(playerSize / 2);
                CmdGrowPlayer(sizeChangeFactor);
                break;
        }
    }

    [Command]
    private void CmdGrowPlayer(float growFactor)
    {
        // Only the server should perform the grow action
        GrowPlayer(growFactor);
    }

    // Grow player on the server
    private void GrowPlayer(float growFactor)
    {
        Vector3 newScale = transform.localScale + new Vector3(growFactor, growFactor, 0);

        // Apply the minimum scale constraint
        newScale.x = Mathf.Max(newScale.x, minPlayerSize); // Ensure X scale doesn't go below minPlayerSize
        newScale.y = Mathf.Max(newScale.y, minPlayerSize); // Ensure Y scale doesn't go below minPlayerSize

        // Apply the new scale to the player
        transform.localScale = newScale;

        // Sync the new scale to all clients
        RpcSyncScale(transform.localScale);
    }

    [ClientRpc]
    private void RpcSyncScale(Vector3 newScale)
    {
        // Sync the scale to all clients
        transform.localScale = newScale;
    }

    [Command]
    private void CmdDestroyEatenObject(GameObject eatenObject, bool playerEaten)
    {
        // Destroy the object on the server (this propagates to all clients)
        if (eatenObject != null)
        {
            RpcDestroyObject(eatenObject); // Notify clients before destruction
            NetworkServer.Destroy(eatenObject); // Ensures it is removed server-side
        }

        if (playerEaten)
        {
            eatenObject.GetComponent<EatingMechanic>().KillPlayer();
        }
    }

    [ClientRpc]
    private void RpcDestroyObject(GameObject eatenObject)
    {
        // Destroy the object on the client
        if (eatenObject != null)
        {
            Destroy(eatenObject);
        }
    }

    public void KillPlayer()
    {
        NetworkConnectionToClient conn = gameObject.GetComponent<NetworkIdentity>().connectionToClient;
        if (conn != null)
        {
            // Notify clients about the player being removed
            RpcNotifyPlayerLeft(gameObject);

            // Remove the player from the server
            //NetworkServer.RemovePlayerForConnection(conn);

            conn.Disconnect();
        }
    }

    [ClientRpc]
    private void RpcNotifyPlayerLeft(GameObject playerLeft)
    {
        // Notify all clients that the player has left
        Debug.Log($"{playerLeft.name} has left the game.");
    }
}