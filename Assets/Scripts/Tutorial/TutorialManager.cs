using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void LoadScenePrimerosPasos()
    {
        StartCoroutine(loadScene("Primeros Pasos"));
    }

    public void UnloadScenePrimerosPasos()
    {
        StartCoroutine(unloadScene("Primeros Pasos"));
    }

    public void LoadScenePiloto()
    {
        StartCoroutine(loadScene("Piloto"));
    }

    public void UnloadScenePiloto()
    {
        StartCoroutine(unloadScene("Piloto"));
    }

    public void LoadSceneCopiloto()
    {
        StartCoroutine(loadScene("Copiloto"));
    }

    public void UnloadSceneCopiloto()
    {
        StartCoroutine(unloadScene("Copiloto"));
    }

    IEnumerator loadScene(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading scene " + " [][] Progress: " + asyncLoad.progress);
            yield return null;
        }

        StudentManager.Instance.HideStudentCanvas();
    }

    IEnumerator unloadScene(string name)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(name);

        while (!asyncUnload.isDone)
        {
            Debug.Log("Unloading scene " + " [][] Progress: " + asyncUnload.progress);
            yield return null;
        }

        StudentManager.Instance.ShowStudentCanvas();

    }
}
