using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class PlayerID : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnNameChanged))] // Nome único para cada jogador
    public string playerName;

    [SerializeField] private TextMesh playerNameText; // Nome visível no jogo

    private Material playerMaterialClone;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            string generatedName = $"Player {netId}";
            CmdSetupPlayer(generatedName);
        }
        
    }


    
    [Command]
    private void CmdSetupPlayer(string _name)
    {
        playerName = _name;
    }

    void OnNameChanged(string oldName, string newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = newName;
        }

        
    }


}
