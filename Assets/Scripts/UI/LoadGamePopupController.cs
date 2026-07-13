using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TheDugout.Database;

public class LoadGamePopupController : MonoBehaviour
{
    [Header("References")]
    public GameObject popupRoot;
    public Transform contentParent;
    public GameObject saveEntryPrefab;

    public void OpenPopup()
    {
        popupRoot.SetActive(true);
        PopulateSaveList();
    }

    public void ClosePopup()
    {
        popupRoot.SetActive(false);
    }

    private void PopulateSaveList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<string> saves = SaveManager.GetAllSaves();

        if (saves.Count == 0)
        {
            Debug.Log("No saves found.");
            return;
        }

        foreach (string savePath in saves)
        {
            GameObject entry = Instantiate(saveEntryPrefab, contentParent);

            string fileName = Path.GetFileNameWithoutExtension(savePath);
            TMP_Text label = entry.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = fileName;

            // главният бутон (целия ред) зарежда сейва
            Button mainButton = entry.GetComponent<Button>();
            if (mainButton != null)
                mainButton.onClick.AddListener(() => LoadSelectedSave(savePath));

            // намери DeleteButton вътре в entry-то по име
            Transform deleteBtnTransform = entry.transform.Find("DeleteButton");
            if (deleteBtnTransform != null)
            {
                Button deleteButton = deleteBtnTransform.GetComponent<Button>();
                deleteButton.onClick.AddListener(() => DeleteSelectedSave(savePath));
            }
        }
    }

    private void LoadSelectedSave(string savePath)
    {
        GameDatabaseManager.Instance.LoadSave(savePath);
        SceneManager.LoadScene("Hub");
    }

    private void DeleteSelectedSave(string savePath)
    {
        SaveManager.DeleteSave(savePath);
        PopulateSaveList(); // презарежда списъка, за да изчезне изтрития ред веднага
    }
}