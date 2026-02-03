using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using TMPro;
using static Saver;

public class FileManager : MonoBehaviour
{
    const string pathRegexString = @"^.+\.vec2$";
    public static bool FilesOpen { get; private set; } = false;
    [SerializeField] GameObject background;
    [SerializeField] Transform fileHolder;
    [SerializeField] GameObject fileBoxPrefab;
    [SerializeField] TMP_InputField searchInput;
    [SerializeField] TMP_InputField nameInput;
    Warning warning;
    string Search
    {
        get
        {
            return search;
        }
        set
        {
            search = value;
            UpdateFileView();
        }
    }
    string search = string.Empty;
    string enteredName = string.Empty;
    readonly List<SaveFile> allFiles = new();
    readonly string[] sizeUnits = new string[] { "B", "KB", "MB", "GB" };
    readonly List<FileBox> shownBoxes = new();
    FileBox selectedBox;
    string selectedFile;

    void Awake()
    {
        warning = FindFirstObjectByType<Warning>();
        background.SetActive(false);
        InitFiles();
    }

    void InitFiles()
    {
        allFiles.Clear();

        string dirPath = Path.Combine(Application.dataPath, folderName);
        string[] filePaths = Directory.GetFiles(dirPath);
        Regex pathRegex = new(pathRegexString);
        List<SaveFile> files = new();
        foreach (string filePath in filePaths)
        {
            if (pathRegex.IsMatch(filePath))
            {
                FileInfo fileInfo = new(filePath);

                string name = Path.GetFileNameWithoutExtension(filePath);
                DateTime time = fileInfo.LastAccessTime;
                string size = FileSizeToString(fileInfo.Length);

                SaveFile file = new() { path = filePath, name = name, time = time, size = size };

                files.Add(file);
            }
        }
        allFiles.AddRange(SortFiles(files));
    }

    /// <summary>
    /// Sorts a list of SaveFiles in order from most recently modified.
    /// </summary>
    /// <param name="files">SaveFile list to be sorted.</param>
    /// <returns>SaveFile list sorted by time modified.</returns>
    List<SaveFile> SortFiles(List<SaveFile> files)
    {
        var sortedFiles = new SaveFile[files.Count];

        if (files.Count > 0)
        {
            sortedFiles[0] = files[0];

            SaveFile file;
            int checkIndex;
            for (int i = 1; i < files.Count; i++)
            {
                file = files[i];
                checkIndex = i;
                while (checkIndex > 0 && sortedFiles[checkIndex - 1].time < file.time)
                {
                    checkIndex--;
                    sortedFiles[checkIndex + 1] = sortedFiles[checkIndex];
                }
                sortedFiles[checkIndex] = file;
            }
        }

        return sortedFiles.ToList();
    }

    void UpdateFileView()
    {
        ClearFileView();

        foreach (var file in allFiles)
        {
            if (Search == string.Empty || file.name.Contains(Search))
            {
                var fileBoxObj = Instantiate(fileBoxPrefab, fileHolder);
                var fileBox = fileBoxObj.GetComponent<FileBox>();
                fileBox.SetData(file);
                shownBoxes.Add(fileBox);

                if (file.name == selectedFile)
                {
                    selectedBox = fileBox;
                    UpdateBoxHighlight();
                }
            }
        }
    }

    void ClearFileView()
    {
        shownBoxes.Clear();
        selectedBox = null;
        foreach (Transform fileBox in fileHolder)
        {
            Destroy(fileBox.gameObject);
        }
    }

    string FileSizeToString(long size)
    {
        double sizeD = size;
        int unitSteps = 0;
        while (sizeD > 1024 && unitSteps < sizeUnits.Length - 1)
        {
            sizeD /= 1024;
            unitSteps++;
        }
        return $"{sizeD:F1}{sizeUnits[unitSteps]}";
    }

    public void SearchChanged()
    {
        Search = searchInput.text;
        UpdateFileView();
    }

    public void NameChanged()
    {
        enteredName = nameInput.text;
        selectedBox = CheckFileName(enteredName);
        selectedFile = (selectedBox) ? selectedBox.myFile.name : string.Empty;
        UpdateBoxHighlight();
    }

    FileBox CheckFileName(string fileName)
    {
        foreach (var box in shownBoxes)
        {
            if (box.myFile.name == fileName)
            {
                return box;
            }
        }
        return null;
    }

    public void DeleteFile()
    {
        if (!DeleteState(nameInput.text))
        {
            warning.ShowWarning($"Could not delete file \"{((nameInput.text != string.Empty) ? nameInput.text : " ")}\"");
        }

        // update data and visuals

        InitFiles();
        UpdateFileView();
    }

    public void SaveFile()
    {
        if (!SaveState(nameInput.text))
        {
            warning.ShowWarning($"Please enter a valid file name");
        }
        else selectedFile = nameInput.text;

        // update data and visuals

        InitFiles();
        UpdateFileView();
    }

    public void LoadFile()
    {
        if (!LoadState(nameInput.text))
        {
            warning.ShowWarning($"Could not load file \"{((nameInput.text != string.Empty) ? nameInput.text : " ")}\"");
        }
        else ToggleViewer();
    }

    public void FileSelected(FileBox box)
    {
        selectedBox = box;
        nameInput.text = box.myFile.name;
        selectedFile = box.myFile.name;
        UpdateBoxHighlight();
    }

    void UpdateBoxHighlight()
    {
        foreach (var box in shownBoxes)
        {
            if (box == selectedBox)
            {
                box.Highlight(true);
            }
            else
            {
                box.Highlight(false);
            }
        }
    }

    public void ToggleViewer()
    {
        FilesOpen = !FilesOpen;
        background.SetActive(FilesOpen);
        if (FilesOpen) UpdateFileView();
    }
}

public struct SaveFile
{
    public string path;
    public string name;
    public DateTime time;
    public string size;
}