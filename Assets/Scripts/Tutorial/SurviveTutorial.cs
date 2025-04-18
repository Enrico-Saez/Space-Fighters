using System.Collections;
using UnityEngine;
public class SurviveTutorial : MonoBehaviour
{
    [SerializeField] private TutorialController tutorialController;

    void OnEnable()
    {
        StartCoroutine(Survive());
    }

    private IEnumerator Survive()
    {
        yield return new WaitForSeconds(30f);
        tutorialController.TutorialIsDone();
    }
}
