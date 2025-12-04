using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeButtonHandler : MonoBehaviour
{
    public void BlockDrag(BaseEventData eventData)
    {
        var pointerEventData = (PointerEventData)eventData;
        pointerEventData.pointerDrag = null; // È¡ÏûÍÏ×§
        Debug.Log("Drag blocked on upgrade button.");
    }
}
