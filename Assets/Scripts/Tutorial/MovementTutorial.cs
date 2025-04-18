using System.Collections;
using UnityEngine;

public class MovementTutorial : MonoBehaviour
{
    [SerializeField] private GameObject movementTutorial;
    [SerializeField] private GameObject shootingTutorial;
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
