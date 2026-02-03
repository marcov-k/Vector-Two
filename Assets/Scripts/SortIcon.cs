using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class SortIcon : MonoBehaviour, IPointerClickHandler
{
    public uint id;
    uint currentMode;
    FileManager fileManager;
    Image myImage;
    RectTransform rectTrans;
    bool reversed = false;
    [SerializeField] Sprite selectSprite;
    [SerializeField] Sprite unselectSprite;
    [SerializeField, Range(0.0f, 1.0f)] float selectOpacity = 0.9f;
    [SerializeField, Range(0.0f, 1.0f)] float unselectOpacity = 0.5f;
    float yScale;

    void Awake()
    {
        fileManager = FindFirstObjectByType<FileManager>();
        myImage = GetComponent<Image>();
        rectTrans = GetComponent<RectTransform>();
        yScale = rectTrans.localScale.y;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetSortMode(id);
        fileManager.SortModeChanged(id, reversed);
    }

    void UpdateVisual()
    {
        if (currentMode == id)
        {
            myImage.sprite = selectSprite;
            myImage.color = new(myImage.color.r, myImage.color.g, myImage.color.b, selectOpacity);
            rectTrans.localScale = new(rectTrans.localScale.x, (reversed) ? -yScale : yScale);
        }
        else
        {
            myImage.sprite = unselectSprite;
            myImage.color = new(myImage.color.r, myImage.color.g, myImage.color.b, unselectOpacity);
        }
    }

    public void SetSortMode(uint mode)
    {
        if (mode == currentMode) reversed = !reversed;
        else reversed = false;
        currentMode = mode;
        UpdateVisual();
    }
}
