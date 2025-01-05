using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using Telepathy;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public SyncList<EatingMechanic> players = new SyncList<EatingMechanic>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void GetValues(uint currNetId)
    {
        EatingMechanic player = Instance.players.Find(p => p.netId == currNetId);

        player.TargerUpdateValues(player.connectionToClient, player.score, player.rank);
    }
    public int GetRank(uint currNetId)
    {

        EatingMechanic a = players.Find(p => p.netId == currNetId);
        if (a == null)
        {
            Debug.Log("yEP NULL");
            return 888;
        }
        else
            Debug.Log("it's fine");
        return players.Find(p => p.netId == currNetId).rank;
    }

    [Server]
    public void RegisterPlayer(EatingMechanic player)
    {
        if (!players.Contains(player))
        {
            Debug.Log("player added, and correct? " + player.scoreManager != null);
            players.Add(player);
        }
        else
        {
            Debug.Log("player already in list");
        }
    }

    private void Update()
    {
        Debug.Log(players.Count);
    }

    [Server]
    public void CmdUnregisterPlayer(EatingMechanic player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            Debug.Log($"Player {player.netId} unregistered.");
        }
    }

    [Server]
    public void HandlePlayerExit(EatingMechanic player)
    {
        // Update remaining players' ranks after removing the player
        CmdUnregisterPlayer(player);

        // Call AddScore (or update logic) safely
        foreach (var remainingPlayer in players)
        {
            if (Instance.CheckIfRankShouldChange(remainingPlayer.netId))
            {
                remainingPlayer.UpdateRankOnPlayerExit(remainingPlayer.connectionToClient);
                break;
            }
        }
    }

    [Server]
    public void OrderPlayerRanks()
    {
        // temporary List from the SyncList
        List<EatingMechanic> tempList = new List<EatingMechanic>(players);

        // Sort the List based on the score
        tempList.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        // update the SyncList with the sorted values
        players.Clear();
        foreach (var player in tempList)
            players.Add(player);
    }

    [Server]
    public void UpdateRankings()
    {
        Debug.Log("ranking updating");

        //RpcChangeRank

        for (int i = 0; i < players.Count; i++)
        {
            players[i].rank = i + 1;

            // Call the RpcUpdateRank on the player's RankingManager
            //players[i].ChangeRank(i + 1);
        }
    }

    public bool CheckIfRankShouldChange(uint currNetId)
    {
        // Get the current rank of the updated player
        int currentRank = players.FindIndex(p => p.netId == currNetId);
        Debug.Log("player index: " + currentRank);
        EatingMechanic currPlayer = players[currentRank];

        // Check for invalid index
        if (currentRank < 0 || currentRank >= players.Count) return false;

        // Check if the player is already at the top rank and is supposed to be there
        if (currentRank == 0)
        {
            if (currPlayer.rank == currentRank + 1)
                return false;
            return true;
        }

        // Get the player just above in the rankings
        EatingMechanic playerAbove = players[currentRank - 1];

        // If the updated player's score surpasses the player above, we need to reorder
        if (currPlayer.score > playerAbove.score)
        {
            Debug.Log("Rank needs reordering due to score change.");
            return true;
        }

        // Check if the player isn't in the rank that is supposed to be
        if (currPlayer.rank != currentRank + 1)
            return true;

        return false;
    }
}
