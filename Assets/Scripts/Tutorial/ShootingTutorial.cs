using System.Collections;
using UnityEngine;

public class ShootingTutorial : MonoBehaviour
{
    [SerializeField] private TutorialController tutorialController;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            tutorialController.TutorialIsDone();
        }
    }
}
