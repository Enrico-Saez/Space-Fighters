using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject movementTutorial;
    [SerializeField] private GameObject shootingTutorial;
    private Queue<GameObject> _tutorialQueue;
    void Start()
    {
        StartCoroutine(StartNewTutorial(movementTutorial));
    }

    private IEnumerator StartNewTutorial(GameObject tutorial)
    {
        yield return new WaitForSeconds(3);
        tutorial.SetActive(true);
    }
}
