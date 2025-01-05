using UnityEngine;
using TMPro;
using Mirror;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void UpdateScoreText(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }

    [ClientRpc]
    public void RpcUpdateScoreText(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }
}