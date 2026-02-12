//Poylgon Planet - Contact. https://polygonplanet.com/contact/
//Copyright © 2016-2018 Polygon Planet. All rights reserved. https://polygonplanet.com/privacy-policy/
//This source file is subject to Unity Technologies Asset Store Terms of Service. https://unity3d.com/legal/as_terms
#pragma warning disable 0168 //Variable declared, but not used.
#pragma warning disable 0219 //Variable assigned, but not used.
#pragma warning disable 0414 //Private field assigned, but not used.
#pragma warning disable 0649 //Variable asisgned to, and will always have default value.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class AnchorSnapEditor : Editor
{
[MenuItem("Anchor Snap/parent and children", false, 0)]
private static void SnapParentAndChildrenMenuItem()
{
SnapAnchorsMultiple(Selection.activeGameObject);
}
[MenuItem("Anchor Snap/just parent", false, 0)]
private static void SnapParentMenuItem()
{
SnapAnchors(Selection.activeGameObject);
}
[MenuItem("GameObject/Anchor Snap/parent and children %[", false, 0)]
private static void SnapParentAndChildrenGameObject()
{
SnapAnchorsMultiple(Selection.activeGameObject);
}
[MenuItem("GameObject/Anchor Snap/just parent %]", false, 0)]
private static void SnapParentGameObject()
{
SnapAnchors(Selection.activeGameObject);
}
private static void SnapAnchors(GameObject gameObject)
{
RectTransform recTransform = null;
RectTransform parentTransform = null;
if (gameObject.transform.parent != null)
{
if (gameObject.GetComponent<RectTransform>() != null)
{
recTransform = gameObject.GetComponent<RectTransform>();
}
else
{
return;
}
if (parentTransform == null)
{
parentTransform = gameObject.transform.parent.GetComponent<RectTransform>();
}
Undo.RecordObject(recTransform, "Snap Anchors");
//Vector2 offsetMin = recTransform.offsetMin;
//Vector2 offsetMax = recTransform.offsetMax;
//Vector2 anchorMin = recTransform.anchorMin;
//Vector2 anchorMax = recTransform.anchorMax;

//Debug.Log(offsetMin + " " + offsetMax);
//Vector2 parent_scale = new Vector2(parentTransform.rect.width, parentTransform.rect.height);

//recTransform.anchorMin = new Vector2(anchorMin.x + (offsetMin.x / parent_scale.x), anchorMin.y + (offsetMin.y / parent_scale.y));
//recTransform.anchorMax = new Vector2(anchorMax.x + (offsetMax.x / parent_scale.x), anchorMax.y + (offsetMax.y / parent_scale.y));
//recTransform.offsetMin = Vector2.zero;
//recTransform.offsetMax = Vector2.zero;


Vector2 pivot = recTransform.pivot;
Vector2 parentSize = parentTransform.rect.size;
// Calculate the new anchor values based on the pivot
Vector2 newAnchorMin = new Vector2(
recTransform.anchorMin.x + (recTransform.anchoredPosition.x / parentSize.x),
recTransform.anchorMin.y + (recTransform.anchoredPosition.y / parentSize.y)
);
Vector2 newAnchorMax = new Vector2(
recTransform.anchorMax.x + (recTransform.anchoredPosition.x / parentSize.x),
recTransform.anchorMax.y + (recTransform.anchoredPosition.y / parentSize.y)
);
// Set the anchors to the pivot value
recTransform.anchorMin = newAnchorMin;
recTransform.anchorMax = newAnchorMax;
// Reset the anchored position to zero
recTransform.anchoredPosition = Vector2.zero;
//recTransform.offsetMin = Vector2.zero;
//recTransform.offsetMax = Vector2.zero;

}
}
private static void SnapAnchorsMultiple(GameObject gameObject)
{
SnapAnchors(gameObject);
for (int i = 0; i < gameObject.transform.childCount; i++)
{
SnapAnchorsMultiple(gameObject.transform.GetChild(i).gameObject);
}
}
}
#endif