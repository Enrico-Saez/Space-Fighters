using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject movementTutorial;
    [SerializeField] private GameObject shootingTutorial;
    [SerializeField] private GameObject evadeTutorial;
    [SerializeField] private GameObject surviveTutorial;
    [SerializeField] private GameObject victory;
    private Queue<GameObject> _tutorialQueue;
    private GameObject _currentTutorial;
    void Start()
    {
        _tutorialQueue = new Queue<GameObject>();
        _tutorialQueue.Enqueue(movementTutorial);
        _tutorialQueue.Enqueue(shootingTutorial);
        _tutorialQueue.Enqueue(evadeTutorial);
        _tutorialQueue.Enqueue(surviveTutorial);
        StartCoroutine(StartNewTutorial(_tutorialQueue.Dequeue()));
    }

    private IEnumerator StartNewTutorial(GameObject tutorial)
    {
        _currentTutorial = tutorial;
        yield return new WaitForSeconds(3f);
        tutorial.SetActive(true);
    }
    
    private IEnumerator Victory()
    {
        victory.SetActive(true);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void TutorialIsDone()
    {
        _currentTutorial.SetActive(false);
        if (_tutorialQueue.Count == 0)
        {
            StartCoroutine(Victory());
            return;
        }
        StartCoroutine(StartNewTutorial(_tutorialQueue.Dequeue()));
    }
}
