using UnityEngine;
using TMPro;

public class FileBox : MonoBehaviour
{
    FileManager fileManager;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dateText;
    [SerializeField] TextMeshProUGUI sizeText;
    SaveFile myFile;

    void Awake()
    {
        fileManager = FindFirstObjectByType<FileManager>();
    }

    public void SetData(SaveFile file)
    {
        (myFile, nameText.text, dateText.text, sizeText.text) = (file, file.name, file.time.ToString(), file.size);
    }
}
