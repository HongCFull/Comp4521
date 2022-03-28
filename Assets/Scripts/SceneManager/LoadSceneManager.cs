using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
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

    public void LoadScene(int sceneIndex)=> SceneManager.LoadScene(sceneIndex);
    
    public IEnumerator LoadSceneCoroutine(int index)
    {
        if (!IsValidSceneIndex(index)) {
            throw new Exception(index + " is an invalid Scene index!");
            yield break;
        }

        AsyncOperation loadSceneOperation= SceneManager.LoadSceneAsync(index);
        while (!loadSceneOperation.isDone) {
            yield return null;
        }
    }

    public int GetNextSceneIndex()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        return index == SceneManager.sceneCount ? 0 : index + 1;
    }

    private bool IsValidSceneIndex(int sceneIndex) => sceneIndex >= 0 && sceneIndex < SceneManager.sceneCount;
}
