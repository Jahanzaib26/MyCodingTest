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

        // parent
        Transform root = playerId.transform;

        // child (movement + rigidbody)
        Rigidbody rb = root.GetComponentInChildren<Rigidbody>();
        PlayerMovementDualSwinging move =
            root.GetComponentInChildren<PlayerMovementDualSwinging>();

        // 🔒 LOCK CLIENT INPUT
        move.teleportLock = true;

        // stop physics
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        // 🚀 TELEPORT (server authoritative moment)
        rb.transform.position = teleportPoint.position;
        rb.transform.rotation = teleportPoint.rotation;

        root.position = teleportPoint.position;
        root.rotation = teleportPoint.rotation;

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;

        // 🔓 UNLOCK after short delay
        yield return new WaitForSeconds(0.1f);
        move.teleportLock = false;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        NetworkIdentity id =
            other.GetComponentInParent<NetworkIdentity>();

        if (id != null)
            teleportingPlayers.Remove(id.netId);
    }


}
