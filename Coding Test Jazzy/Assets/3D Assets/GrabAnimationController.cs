using UnityEngine;

public class GrabAnimationController : MonoBehaviour
{
    [Header("References")]
    public DualHooks dualHooks;        // Your existing DualHooks script
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    [Header("Input Settings")]
    public KeyCode leftMouseButton = KeyCode.Mouse0;
    public KeyCode rightMouseButton = KeyCode.Mouse1;

    private void Update()
    {
        HandleHandHold(0, leftMouseButton, leftHandAnimator);
        HandleHandHold(1, rightMouseButton, rightHandAnimator);
    }

    private void HandleHandHold(int handIndex, KeyCode mouseButton, Animator handAnimator)
    {
        // Check if hook/swing is active for this hand
        bool hookActive = dualHooks.swingsActive[handIndex];

        if (hookActive && Input.GetKey(mouseButton))
        {
            handAnimator.SetBool("Hold", true);
        }
        else
        {
            handAnimator.SetBool("Hold", false);
        }
    }
}
