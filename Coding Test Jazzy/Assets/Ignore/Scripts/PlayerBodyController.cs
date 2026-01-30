using Mirror;
using UnityEngine;

public class PlayerBodyController : NetworkBehaviour
{
    [Header("Body Renderers")]
    public SkinnedMeshRenderer[] bodyMeshes;

    public override void OnStartLocalPlayer()
    {
        // 🔥 Sirf LOCAL player ki body hide
        foreach (var mesh in bodyMeshes)
        {
            if (mesh != null)
                mesh.enabled = false;
        }
    }
}
