using UnityEngine;

public class ChildRotationController : MonoBehaviour
{
    [Header("Custom Rotation When Unparented")]
    public Vector3 customRotation = new Vector3(45f, 30f, 0f); // Set this in Inspector

    private Vector3[] originalChildRotations;
    private Transform[] children;
    private Transform lastParent;

    void Start()
    {
        // Store references to children
        int childCount = transform.childCount;
        children = new Transform[childCount];
        originalChildRotations = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
            originalChildRotations[i] = children[i].localEulerAngles; // Store starting rotation
        }

        lastParent = transform.parent; // Track initial parent

        // Initial debug
        if (transform.parent == null)
            Debug.Log($"{gameObject.name} is unparented at start.");
        else
            Debug.Log($"{gameObject.name} is parented at start to {transform.parent.name}.");
    }

    void Update()
    {
        // Check if parent changed
        if (transform.parent != lastParent)
        {
            if (transform.parent == null)
            {
                // Object is unparented -> apply custom rotation to children
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].localEulerAngles = customRotation;
                }
                Debug.Log($"{gameObject.name} became unparented. Applied custom rotation to children.");
            }
            else
            {
                // Object is now parented -> restore original rotations
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].localEulerAngles = originalChildRotations[i];
                }
                Debug.Log($"{gameObject.name} became parented to {transform.parent.name}. Restored original child rotations.");
            }

            lastParent = transform.parent; // Update last parent
        }
    }
}
