using System.Collections;
using UnityEngine;
public class evadeTutorial : MonoBehaviour
{
    [SerializeField] private TutorialController tutorialController;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            tutorialController.TutorialIsDone();
        }
    }
}
