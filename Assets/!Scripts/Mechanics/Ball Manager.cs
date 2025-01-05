using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BallManager : NetworkBehaviour
{
    public GameObject ballPrefab; // Prefab da bola
    public int maxBalls = 50; // Número máximo de bolas no mapa
    public float ballRadius = 0.5f; // Raio padrão das bolas
    public Vector2 spawnArea = new Vector2(13, 8); // Área de spawn das bolas

    private List<GameObject> balls = new List<GameObject>();

    void Start()
    {
        if (isServer) // Apenas o servidor controla o spawn das bolas
        {
            SpawnInitialBalls();
        }
    }

    [ServerCallback]
    void Update()
    {
        if (isServer) // Apenas o servidor controla o spawn das bolas
        {
            MaintainBallCount();
        }
    }

    [Server]
    void SpawnInitialBalls()
    {
        for (int i = 0; i < maxBalls; i++)
        {
            SpawnBall();
        }
    }

    [Server]
    void MaintainBallCount()
    {
        while (balls.Count < maxBalls)
        {
            SpawnBall();
        }
    }

    [Server]
    void SpawnBall()
    {
        // Calculate a random position within the defined spawn area
        float randomX = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
        float randomY = Random.Range(-spawnArea.y / 2, spawnArea.y / 2);
        Vector3 randomPosition = new Vector3(randomX, randomY, 0);

        // Instantiate the ball at the calculated position
        GameObject newBall = Instantiate(ballPrefab, randomPosition, Quaternion.identity);

        // Set the scale of the ball based on its radius
        newBall.transform.localScale = Vector3.one * ballRadius * 2;

        Color GenerateBrightColor()
        {
            Color color;
            float brightnessThreshold = 2f; // Limite mínimo de brilho (soma de R, G e B)

            do
            {
                color = new Color(
                    Random.Range(0f, 1f), // Componente R
                    Random.Range(0f, 1f), // Componente G
                    Random.Range(0f, 1f)  // Componente B
                );
            }
            while (color.r + color.g + color.b < brightnessThreshold); // Repetir se for muito escuro

            return color;
        }

        // Gerar uma cor aleatória
        Color randomColor = GenerateBrightColor();

        // Aplicar a cor ao material da bola
        Renderer ballRenderer = newBall.GetComponent<Renderer>();
        if (ballRenderer != null)
        {
            ballRenderer.material.color = randomColor;
        }

        // Add the ball to the list and spawn it on the network
        balls.Add(newBall);
        NetworkServer.Spawn(newBall);
    }

    [Server]
    public void RemoveBall(GameObject ball)
    {
        if (balls.Contains(ball))
        {
            balls.Remove(ball);
            Destroy(ball);
        }
    }
}
