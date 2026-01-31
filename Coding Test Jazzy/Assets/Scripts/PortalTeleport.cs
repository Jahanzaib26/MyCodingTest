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
        if (playerId == null) yield break;

        // parent (has NetworkIdentity)
        Transform root = playerId.transform;

        // child (has Rigidbody + NetworkTransformHybrid)
        Rigidbody rb = root.GetComponentInChildren<Rigidbody>();
        Transform child = rb.transform;

        // 🔒 lock physics
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        // 🚀 TELEPORT CHILD (THIS IS THE KEY)
        child.position = teleportPoint.position;
        child.rotation = teleportPoint.rotation;

        // 🔁 keep parent aligned (important!)
        root.position = teleportPoint.position;
        root.rotation = teleportPoint.rotation;

        yield return new WaitForFixedUpdate();

        // 🔓 unlock physics
        rb.isKinematic = false;
    }

}
