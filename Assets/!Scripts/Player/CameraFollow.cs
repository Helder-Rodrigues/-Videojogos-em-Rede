using UnityEngine;
using Mirror;

public class CameraFollow : NetworkBehaviour
{
    private Transform target;
    private float smoothSpeed = 0.125f;
    private Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            // Activate only this player's camera
            playerCamera.enabled = true;
            target = transform;
        }
    }

    void LateUpdate()
    {
        if (target == null || playerCamera == null || !playerCamera.gameObject.activeSelf) return;

        // Smoothly follow the target
        Vector3 smoothedPosition = Vector3.Lerp(playerCamera.transform.position, target.position, smoothSpeed);
        playerCamera.transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, -10);
    }
}
