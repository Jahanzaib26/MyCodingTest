using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleport : NetworkBehaviour
{
    public Transform teleportPoint;

    // STEP 2 — server-side gate
    private readonly HashSet<uint> teleportingPlayers = new HashSet<uint>();

    [Server]
    public void ServerRequestTeleport(NetworkIdentity playerId)
    {
        if (playerId == null) return;

        // 🔒 BLOCK repeat requests
        if (teleportingPlayers.Contains(playerId.netId))
            return;

        // 🔒 LOCK player
        teleportingPlayers.Add(playerId.netId);

        Debug.Log($"[SERVER] Teleport request accepted for player {playerId.netId}");

        StartCoroutine(TeleportRoutine(playerId));
    }

    [Server]
    private IEnumerator TeleportRoutine(NetworkIdentity playerId)
    {
        // wait for physics step
        yield return new WaitForFixedUpdate();

        Transform playerRoot = playerId.transform;
        Rigidbody rb = playerRoot.GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // teleport ROOT (Mirror authoritative)
        playerRoot.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );

        // realign physics child
        if (rb != null)
        {
            rb.transform.localPosition = Vector3.zero;
            rb.transform.localRotation = Quaternion.identity;
        }

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // ⚠️ DO NOT UNLOCK HERE (Step 3 later)
    }
}
