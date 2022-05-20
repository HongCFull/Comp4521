using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject loadSceneImage;
    [SerializeField] private Slider loadSceneProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    public static LoadSceneManager Instance { get; private set; }
    private void Start()
    {
        if (Instance) {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadSceneWithLoadSceneBG(int sceneIndex)=> StartCoroutine(LoadSceneCoroutine(sceneIndex));
    
    private IEnumerator LoadSceneCoroutine(int index)
    {
        if (!IsValidSceneIndex(index)) {
            Debug.Log(SceneManager.sceneCount);
            throw new Exception(index + " is an invalid Scene index!");
            // yield break;
        }

        AsyncOperation loadSceneOperation= SceneManager.LoadSceneAsync(index);
        loadSceneImage.SetActive(true);
        float progressPercentage = 0;
        while (!loadSceneOperation.isDone) {
            loadSceneProgressBar.value = loadSceneOperation.progress;
            loadingText.text = ((int)(loadSceneOperation.progress*100f)) + "%";
            yield return null;
        }
        loadSceneImage.SetActive(false);
    }

    public int GetNextSceneIndex()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        return index == SceneManager.sceneCountInBuildSettings ? 0 : index + 1;
    }

    private bool IsValidSceneIndex(int sceneIndex) => sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings;
}
