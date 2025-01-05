using UnityEngine;
using TMPro;
using Mirror;

public class RankingManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text rankText;

    public void UpdateRankText(int newRank)
    {
        rankText.text = "Rank: " + newRank.ToString();
    }

    [ClientRpc]
    public void RpcUpdateRankText()
    {
        int rank = PlayerManager.Instance.players.FindIndex(p => p.netId == netId) + 1;
        
        Debug.Log($"Updating rank of {netId} to " + rank);
        rankText.text = "Rank: " + rank.ToString();
    }

    [ClientRpc]
    public void RpcChangeRank()
    {
        int value = PlayerManager.Instance.players.FindIndex(p => p.netId == netId) + 1;

        Debug.Log($"Updating rank of player {netId} to: {value}");
        PlayerManager.Instance.players[value-1].rank = value;
    }
}
