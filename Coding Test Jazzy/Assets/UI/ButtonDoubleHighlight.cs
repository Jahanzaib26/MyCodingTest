using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDoubleHighlight : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public GameObject highlightObj;
    public GameObject nonhighlightObj;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightObj.SetActive(true);
        nonhighlightObj.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightObj.SetActive(false);
        nonhighlightObj.SetActive(true);
    }
}
