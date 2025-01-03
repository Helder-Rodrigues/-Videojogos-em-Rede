using UnityEngine;
using Mirror;

public class PlayerSpawner : NetworkManager
{
    private float minDistance = 5f; // Minimum distance between players
    private int maxAttempts = 100;  // Maximum attempts to find a valid spawn position

    // Override to handle custom spawning logic
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Vector3 spawnPosition = Vector3.zero;

        // Attempt to find a valid spawn position
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            spawnPosition = new Vector3(
                Random.Range(-GameManager.mapSize, GameManager.mapSize),
                Random.Range(-GameManager.mapSize, GameManager.mapSize),
                0
            );

            if (IsPositionValid(spawnPosition))
                break;
        }

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // Add Unique Color
        Color randomColor = GetUniqueRandomColor();
        player.GetComponent<SpriteRenderer>().color = randomColor;
        GameManager.allPlayersColors.Add(randomColor);

        

        // Add the player to the game
        NetworkServer.AddPlayerForConnection(conn, player);

        // Now call RpcUpdateColorForNewPlayer to sync the color across players
        SyncOtherPlayersColorsToNewPlayer(player, conn);
    }

    private void SyncOtherPlayersColorsToNewPlayer(GameObject newPlayer, NetworkConnectionToClient conn)
    {
        // Find all existing players
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            // Get the PlayerColorManager and call RpcUpdateColorForNewPlayer
            PlayerColorManager colorManager = player.GetComponent<PlayerColorManager>();
            colorManager.RpcUpdateColorForNewPlayer(player.GetComponent<SpriteRenderer>().color);
        }
    }

    private Color GetUniqueRandomColor()
    {
        Color randomColor;

        // Keep generating random colors until we find one that's not already used
        do
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        } while (GameManager.allPlayersColors.Contains(randomColor));

        return randomColor;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (Vector3.Distance(position, player.transform.position) < minDistance)
                return false;
        }
        return true;
    }
}
