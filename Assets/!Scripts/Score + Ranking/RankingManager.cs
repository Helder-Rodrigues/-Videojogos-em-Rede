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
        UpdateRankText();
    }

    void UpdateRankText()
    {
        rankText.text = "Rank: " + rank.ToString();
    }

    [Command]
    public void CmdUpdateRank(int newRank)
    {
        rank = newRank;
    }
}
