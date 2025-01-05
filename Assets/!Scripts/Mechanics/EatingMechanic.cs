using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Linq;

public class EatingMechanic : NetworkBehaviour
{
    private float baseGrowthFactor = 0.1f;
    private const float minPlayerSize = 0.31f;
    private const float tolerance = 0.001f; // A small threshold for floating-point comparison

    public ScoreManager scoreManager = null;
    public RankingManager rankManager = null;

    public int score;
    public int rank;

    public override void OnStopServer()
    {
        base.OnStopServer();
        PlayerManager.Instance.HandlePlayerExit(this);
    }

    [TargetRpc]
    public void UpdateRankOnPlayerExit(NetworkConnection target)
    {
        CmdAddScore(0);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (isLocalPlayer)
        {
            Debug.Log("Starting local player and loading managers...");
            LoadManagers();

            // Register the player
            Debug.Log("Calling CmdRegisterPlayer for local player...");
            CmdRegisterPlayer(this);

            Debug.Log("adding score");
            // Add score to the player
            Debug.Log($"player {netId} is correct? " + (scoreManager != null));
            CmdAddScore(0);
        }
    }

    public void LoadManagers()
    {
        if (scoreManager != null && rankManager != null)
            return;
        
        if (!isLocalPlayer)
            return;

        Debug.Log("Loading Managers...");
        scoreManager = FindObjectOfType<ScoreManager>();
        rankManager = FindObjectOfType<RankingManager>();

        Debug.Log($"Found ScoreManager: {scoreManager != null}, RankManager: {rankManager != null}");
    }

    [Command]
    public void CmdRegisterPlayer(EatingMechanic player)
    {
        // Ensure the player has a NetworkIdentity and is a valid networked object
        if (player != null && player.GetComponent<NetworkIdentity>() != null)
        {
            Debug.Log("CmdRegisterPlayer called on server.");
            PlayerManager.Instance.RegisterPlayer(player);
        }
        else
        {
            Debug.LogWarning("Invalid player object or NetworkIdentity missing.");
        }
    }

    public void UpdateRankText()
    {
        Debug.Log("updating rank text for " + netId);
        rankManager.UpdateRankText(rank);
    }

    [Command]
    public void CmdAddScore(int value)
    {
        Debug.Log($"B-CmdAddScore: Updating score to {score + value} for player {netId}");

        // Update the score
        score += value;

        // Check if rank should change
        bool rankShouldChange = PlayerManager.Instance.CheckIfRankShouldChange(netId);

        if (rankShouldChange)
        {
            if (PlayerManager.Instance == null)
            {
                Debug.LogError("CmdAddScore: PlayerManager.Instance is null.");
                return;
            }

            PlayerManager.Instance.OrderPlayerRanks();
            PlayerManager.Instance.UpdateRankings();
        }
        else
            Debug.Log("not worth to update rank");

        Debug.Log("Update All values and UI");
        // Call GetValues for each player and invoke TargetRpc
        foreach (var player in PlayerManager.Instance.players)
            PlayerManager.Instance.GetValues(player.netId);
    }

    private void Update()
    {
        Debug.Log("rank value: " + rank);
        Debug.Log("score value: " + score);
    }

    [TargetRpc]
    public void TargerUpdateValues(NetworkConnection target, int newScore, int newRank)
    {
        Debug.Log($"Updating UI for player {netId}: score from {score} to {newScore}, rank from {rank} to {newRank}");
        score = newScore;
        rank = newRank;

        ScoreManager scoreMangr = FindObjectOfType<ScoreManager>();
        RankingManager rankMangr = FindObjectOfType<RankingManager>();

        Debug.Log(scoreMangr == scoreManager);

        scoreMangr.UpdateScoreText(score);
        rankMangr.UpdateRankText(rank);
    }

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

                // Call command to grow the player and increase score
                CmdGrowPlayer(growFactor);

                // Increase score for eating
                CmdAddScore(foodEaten ? 10 : 50);
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
            case PowerUp.PowerUpType.Faster:
                transform.gameObject.GetComponent<PlayerMovement>().speed *= 1.1f;
                break;

            case PowerUp.PowerUpType.Bigger:
            case PowerUp.PowerUpType.Smaller:
                float sizeChangeFactor = (powerUp.powerUpType == PowerUp.PowerUpType.Bigger) ? playerSize / 1.25f : -(playerSize / 2);
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
