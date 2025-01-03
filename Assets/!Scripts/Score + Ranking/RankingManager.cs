using UnityEngine;
using TMPro;
using Mirror;

public class RankingManager : NetworkBehaviour
{
    public TMP_Text rankText;
    [SyncVar(hook = nameof(OnRankChanged))]
    private int rank;

    void Start()
    {
        if (isLocalPlayer)
        {
            UpdateRankText();
        }
    }

    void OnRankChanged(int oldRank, int newRank)
    {
        Debug.Log($"Rank changed from {oldRank} to {newRank}");
        UpdateRankText();
    }

    void UpdateRankText()
    {
        if (rankText != null)
        {
            rankText.text = "Rank: " + rank.ToString();
        }
    }

    [Command]
    public void CmdUpdateRank(int newRank)
    {
        Debug.Log($"Updating rank to: {newRank}");
        rank = newRank;
    }
}
