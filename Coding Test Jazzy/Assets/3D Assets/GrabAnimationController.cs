using UnityEngine;

public class GrabAnimationController : MonoBehaviour
{
    [Header("References")]
    public DualHooks dualHooks;
    public Animation leftHandAnimation;
    public Animation rightHandAnimation;

    [Header("Animation Names")]
    public string holdAnimationName = "Hold";

    [Header("Input Settings")]
    public KeyCode leftMouseButton = KeyCode.Mouse0;
    public KeyCode rightMouseButton = KeyCode.Mouse1;

    private void Update()
    {
        HandleHandHold(0, leftMouseButton, leftHandAnimation);
        HandleHandHold(1, rightMouseButton, rightHandAnimation);
    }

    private void HandleHandHold(int handIndex, KeyCode mouseButton, Animation handAnimation)
    {
        bool hookActive = dualHooks.swingsActive[handIndex];

        // Play animation only once when button is pressed
        if (hookActive && Input.GetKeyDown(mouseButton))
        {
            handAnimation[holdAnimationName].wrapMode = WrapMode.ClampForever;
            handAnimation.Play(holdAnimationName);
        }

        // Optional: return to idle when released
        if (Input.GetKeyUp(mouseButton))
        {
            handAnimation.Stop(holdAnimationName);
        }
    }

}
