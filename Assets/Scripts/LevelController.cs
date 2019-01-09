using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_loadingScreen;
    [SerializeField]
    private Slider m_slider;


    public void LoadLevel (int sceneIndex) {

        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously (int sceneIndex) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        m_loadingScreen.SetActive(true);
        
        while (!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            m_slider.value = progress;

            yield return null;
        }
    }
}
