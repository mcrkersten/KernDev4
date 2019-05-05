using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OnHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public TextMeshPro theText;
    public Color hoverColor;
    public Color onNoHoverColor;

    public void OnPointerEnter(PointerEventData eventData) {
        theText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        theText.color = onNoHoverColor;
    }
}
