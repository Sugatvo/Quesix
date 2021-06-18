using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPrimerosPasos : MonoBehaviour
{
    [SerializeField] CanvasGroup settingsCanvasGroup;
    [SerializeField] CanvasGroup popUpExitTutorial;
    [SerializeField] Animator tutorialAnimator;
    [SerializeField] Animator boardAnimator;
    [SerializeField] Animator initialPositionAnimator;
    [SerializeField] Animator cameraAnimator;
    [SerializeField] Animator robo_nave;
    [SerializeField] Animator quesix_1;
    [SerializeField] Animator quesix_2;
    [SerializeField] Animator quesix_3;
    [SerializeField] Animator quesix_4;
    [SerializeField] Animator trampa_1;
    [SerializeField] Animator trampa_2;

    public void StartTutorial()
    {
        tutorialAnimator.SetBool("Start", true);
    }

    public void IntroduccionTutorial()
    {
        tutorialAnimator.SetBool("Introduccion", true);
    }

    public void ObjetivoTutorial()
    {
        tutorialAnimator.SetBool("Objetivo", true);
    }

    public void PreparacionTutorial()
    {
        tutorialAnimator.SetBool("Preparacion", true);
    }

    public void TableroTutorial()
    {
        tutorialAnimator.SetBool("Tablero", true);
        boardAnimator.SetBool("MoveDown", true);
    }

    public void ElementosTutorial()
    {
        tutorialAnimator.SetBool("Elementos", true);
    }


    public void RoboNaveTutorial()
    {
        tutorialAnimator.SetBool("RoboNave", true);
        cameraAnimator.SetBool("isMoving", true);
        initialPositionAnimator.SetBool("Active", true);
    }

    public void PosicionInicialTutorial()
    {
        tutorialAnimator.SetBool("PosicionInicial", true);
        cameraAnimator.SetBool("isMoving", false);
        cameraAnimator.SetBool("backToLeft", true);
        initialPositionAnimator.SetBool("Active", false);
        robo_nave.SetBool("MoveDown", true);
    }

    public void QuesixTutorial()
    {
        tutorialAnimator.SetBool("Quesix", true);
        quesix_1.SetBool("MoveDown", true);
        quesix_2.SetBool("MoveDown", true);
        quesix_3.SetBool("MoveDown", true);
        quesix_4.SetBool("MoveDown", true);
    }

    public void TrampaTutorial()
    {
        tutorialAnimator.SetBool("Trampa", true);
        trampa_1.SetBool("MoveDown", true);
        trampa_2.SetBool("MoveDown", true);
    }

    public void CameraMovementTutorial()
    {
        tutorialAnimator.SetBool("CameraMovement", true);
    }

    public void OnClickSettings()
    {
        settingsCanvasGroup.alpha = 1f;
        settingsCanvasGroup.blocksRaycasts = true;
    }

    public void OnClickCloseSettings()
    {
        settingsCanvasGroup.alpha = 0f;
        settingsCanvasGroup.blocksRaycasts = false;
    }

    public void ExitTutorial()
    {
        popUpExitTutorial.alpha = 1.0f;
        popUpExitTutorial.blocksRaycasts = true;
    }

    public void PopUpExitTutorialNo()
    {
        popUpExitTutorial.alpha = 0.0f;
        popUpExitTutorial.blocksRaycasts = false;
    }

    public void PopUpExitTutorialYes()
    {
        TutorialManager.Instance.UnloadScenePrimerosPasos();
    }
}
