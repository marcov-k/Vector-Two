using UnityEngine;
using UnityEngine.EventSystems;

public class SortLabel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SortIcon myIcon;

    // pass click to sort icon when label clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        myIcon.OnPointerClick(eventData);
    }
}
