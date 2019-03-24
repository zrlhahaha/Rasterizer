using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Manager : MonoBehaviour {

    public GameObject HintText;
    int sceneIndex;
    private void Awake()
    {
        SceneManager.LoadScene(sceneIndex+1, LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            HintText.SetActive(!HintText.activeSelf);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            SceneManager.UnloadSceneAsync(sceneIndex+1);
            sceneIndex = (sceneIndex + 1) % 2;
            SceneManager.LoadScene(sceneIndex+1, LoadSceneMode.Additive);
        }
    }

}
