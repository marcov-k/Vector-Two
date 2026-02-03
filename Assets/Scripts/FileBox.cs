using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FileBox : MonoBehaviour, IPointerClickHandler
{
    FileManager fileManager;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dateText;
    [SerializeField] TextMeshProUGUI sizeText;
    [SerializeField] Color defaultColor;
    [SerializeField] Color highlightColor;
    Image myImage;
    public SaveFile myFile;

    void Awake()
    {
        fileManager = FindFirstObjectByType<FileManager>();
        myImage = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileManager.FileSelected(this);
    }

    public void SetData(SaveFile file)
    {
        (myFile, nameText.text, dateText.text, sizeText.text) = (file, file.name, file.time.ToString(), file.size);
    }

    public void Highlight(bool highlight)
    {
        myImage.color = (highlight) ? highlightColor : defaultColor;
    }
}
