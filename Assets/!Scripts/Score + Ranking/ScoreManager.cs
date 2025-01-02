using UnityEngine;
using TMPro;
using Mirror;

public class ScoreManager : NetworkBehaviour
{
    public TMP_Text scoreText;
    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    void Start()
    {
        if (isLocalPlayer)
        {
            UpdateScoreText();
        }
    }

    void OnScoreChanged(int oldScore, int newScore)
    {
        Debug.Log($"Score changed from {oldScore} to {newScore}");
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    [Command]
    public void CmdAddScore(int value)
    {
        Debug.Log($"Adding score: {value}");
        score += value;
    }
}
