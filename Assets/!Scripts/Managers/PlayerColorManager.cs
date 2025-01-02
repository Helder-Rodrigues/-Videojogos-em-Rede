using Mirror;
using UnityEngine;

public class PlayerColorManager : NetworkBehaviour
{
    [SyncVar]
    private Color playerColor;

    // This method will be called by the server to update the color for the new player
    [ClientRpc]
    public void RpcUpdateColorForNewPlayer(Color color)
    {
        playerColor = color;
        GetComponent<SpriteRenderer>().color = playerColor;
    }

    // This method sets the color for the player when they first spawn
    public void SetColor(Color color)
    {
        playerColor = color;
        GetComponent<SpriteRenderer>().color = playerColor;
    }
}
