using Mirror;
using UnityEngine;

public class PlayerColorManager : NetworkBehaviour
{
    [SyncVar]
    private Color playerColor;

    [SyncVar(hook = nameof(OnNameChanged))] // Nome único para cada jogador
    public string playerName;
    [SerializeField] private TextMesh playerNameText; // Nome visível no jogo

    //Add player name

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

    // Atualiza o nome do jogador localmente
    void OnNameChanged(string oldName, string newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = newName;
        }
    }
}
