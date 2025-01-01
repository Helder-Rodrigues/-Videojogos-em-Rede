using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // Enum for power-up types
    public enum PowerUpType
    {
        SpeedBoost,
        DoubleSize,
        HalfSize
    }

    public PowerUpType powerUpType;

    private void Start()
    {
        powerUpType = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);
    }
}
