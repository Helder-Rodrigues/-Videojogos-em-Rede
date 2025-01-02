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
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    [Command]
    public void CmdAddScore(int value)
    {
        score += value;
    }
}
