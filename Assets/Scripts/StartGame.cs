using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartTutorialScene()
    {
        SceneManager.LoadScene("Scenes/Tutorial");
    }

    public void StartGameScene()
    {
        SceneManager.LoadScene("Scenes/Game");
    }
}
