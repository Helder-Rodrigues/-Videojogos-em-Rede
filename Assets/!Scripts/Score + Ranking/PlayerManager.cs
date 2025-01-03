using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private List<EatingMechanic> players = new List<EatingMechanic>();

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

    public void RegisterPlayer(EatingMechanic player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            UpdateRankings();
        }
    }

    public void UnregisterPlayer(EatingMechanic player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            UpdateRankings();
        }
    }

    public void UpdateRankings()
    {
        // Sort players by score in descending order
        players.Sort((p1, p2) => p2.Score.CompareTo(p1.Score));

        // Update player rankings
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdateRank(i + 1);
        }
    }
}
