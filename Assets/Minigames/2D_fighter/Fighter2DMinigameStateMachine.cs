﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sidescroller.Fighting;
using Sidescroller.Control;
using Sidescroller.Canvas;
using Sidescroller.Music;
using TMPro;


namespace Sidescroller.StateMachine
{
    public enum MinigameState
    {
        Intro,
        Gameplay,
        EndRound,
        EndBattle

    }


    public class Fighter2DMinigameStateMachine : MonoBehaviour
    {

        #region Class Variables

        MinigameState currentState = MinigameState.Intro;

        [SerializeField] SideScrollerFighter fighter1 = null;
        [SerializeField] SideScrollerFighter fighter2 = null;

        [SerializeField] IntroControl introControl = null;
        [SerializeField] GameplayCanvasControl gameControl = null;
        [SerializeField] EndingControl endControl = null;

        [SerializeField] Animator sceneryAnimator = null;

        [SerializeField] TextMeshProUGUI fighter1ScoreDisplay = null;
        [SerializeField] TextMeshProUGUI fighter2ScoreDisplay = null;

        [SerializeField] TextMeshProUGUI roundDisplay = null;
        [SerializeField] TextMeshProUGUI finalRoundDisplay = null;
         

        [SerializeField] GameObject PauseCanvas = null;

        [SerializeField] MusicControl levelMusic = null;



        #endregion

        #region Gameplay Variables
        public int roundID = 0;
        public int maxRound = 3;

        public int fighter1Score = 0;
        public int fighter2Score = 0;

        #endregion

        #region Unity Methods

        void Start()
        {
            /////////////////////////
            /// Config Game Scene ///
            /////////////////////////

            // Check SerializeField variables
            if (!PublicVariablesAvailable()) return;

            // Start Intro Sequence
            StartIntroSequence();
        }

        // Check Current Minigamestate
        void Update()
        {
            /// Check Methods realise Action Methods acording to the current State///

           // Process acording for Conditions And Variables in the Cureent Scene State
            ProcessCurrentMinigameState();

            // Check For Player Input acording to current State
            ProcessCurrentInput();
        }

        #endregion

        #region Process Methods

        void ProcessCurrentMinigameState()
        {
            // Check Current game state in frame
            switch (currentState)
            {

                case (MinigameState.Intro):
                    break;

                case (MinigameState.Gameplay):

                    // Return 

                    int roundDefeatedFighter = CheckForStateFighter(FighterState.Dying);

                    // If result is not neutral, End Round
                    if (roundDefeatedFighter != -1)
                    {
                        int roundVictorID;

                        // Victory for Player 2
                        if (roundDefeatedFighter == 1) roundVictorID = 2;

                        // Victory for Player 1
                        else roundVictorID = 1;

                        // If Round Ends the Battle
                        StartEndRoundSequence(roundVictorID);
                    }
                    break;

                case (MinigameState.EndRound):


                    int roundResult = CheckForStateFighter(FighterState.Dead);

                    // End Sequence when one fighter is in state dead
                    if (roundResult != -1)
                    {

                        if ((roundID == maxRound) || (CheckForWinner() != -1))
                        {
                            // Change Canvas to End Screen / End Game

                            int battleVictorID = -1;

                            if (fighter1Score > fighter2Score) battleVictorID = 1; else battleVictorID = 2;

                            if (battleVictorID != -1) StartEndBattleSequence(battleVictorID);

                        }

                        // Next Round
                        else StartNewRoundSequence();
                    }
                    break;

                case (MinigameState.EndBattle):
                    break;
            }
        }

        void ProcessCurrentInput()
        {
            // Check Current game state in frame
            switch (currentState)
            {

                case (MinigameState.Intro):

                    // Check to Game to Start
                    if (Input.GetButtonDown("Submit")) StartGameplaySequence();

                    break;

                case (MinigameState.Gameplay):
                    // Empty
                    break;

                case (MinigameState.EndRound):
                    break;

                case (MinigameState.EndBattle):

                    // Check to Return to Main Menu
                    if (Input.GetButtonDown("Cancel"))
                    {
                        // Return to Main Menu
                        ChangeToMainMenuScene();
                    }

                    // Reset Minigame
                    if (Input.GetButtonDown("Submit"))
                    {
                        ResetCurrentLevel();
                    }

                    break;
            }
        }


        #endregion

        #region Start Sequence Methods

        void StartIntroSequence()
        {
            // Define current state
            currentState = MinigameState.Intro;

            // Change music - Intro music
            levelMusic.IntroMusic();

            // Deactivate Player Controllers
            ConfigFighterControllers(false);

            // Deactivate Pause Menu
            PauseCanvas.SetActive(false);

            // Config Canvas
            ChangeCanvas(currentState);
        }

        void StartGameplaySequence()
        {

            // Activate Pause Menu
            PauseCanvas.SetActive(true);

            // Reset Round Counter
            roundID = 0;

            // Start New Round
            StartNewRoundSequence();

            //Change to Gameplay canvas
            ChangeCanvas(currentState);
        }

        void StartNewRoundSequence()
        {

            // Change music - Start music
            levelMusic.PlayStandardMusic();

            // Recover full Health & Position
            ResetAllFightersStats();

            // Activate Player Controllers
            ConfigFighterControllers(true);

            //Update Round Counter & Update UI
            roundID += 1;

            // UI Update
            UpdateRoundDisplay();

            //Update Background
            UpdateBackground();

            // Change State
            currentState = MinigameState.Gameplay;

        }

        void StartEndRoundSequence(int victorID)
        {
            // Victory to player2
            if (victorID == 1) fighter1Score += 1;
            // Victory to player1
            else if (victorID == 2) fighter2Score += 1;

            // Disable Music
            levelMusic.Stop();

            // Update Display
            UpdateFightersScore();

            // Deactivate Player Controllers
            ConfigFighterControllers(false);

            // Change Current State / Display Victory
            currentState = MinigameState.EndRound;

        }

        void StartEndBattleSequence(int victorID)
        {

            // Deactivate Pause Menu
            PauseCanvas.SetActive(false);

            // Change To End Song
            levelMusic.PlayEndMusic();


            // Change State
            currentState = MinigameState.EndBattle;

            ChangeCanvas(currentState, victorID);
        }

        #endregion

        #region Meta Methods

        // Check Public Defined Variables to Reassure the Right Assigment
        bool PublicVariablesAvailable()
        {
            // End Function
            if (fighter1 == null || fighter2 == null)
            {
                Debug.LogAssertion("No Fighters Selected !!!");
                return false;
            }

            if (introControl == null || gameControl == null)
            {
                Debug.LogAssertion("No Canvas Selected !!!");
                return false;
            }

            //Dont end Function
            return true;
        }

        #endregion

        #region Action Methods 

        void ConfigFighterControllers(bool controllersEnabled)
        {
            // Get Controller Components

            SideScrollerController controller1 = fighter1.GetComponent<SideScrollerController>();
            SideScrollerController controller2 = fighter2.GetComponent<SideScrollerController>();


            if (controller1 == null || controller2 == null)
            {
                Debug.LogAssertion(" No Controlers Found ");
                return;
            }

            // Deactivate Players for Intro Sequence (bool)

            controller1.enabled = controllersEnabled;
            controller2.enabled = controllersEnabled;

            // Set Fighters to Stand Still
            controller1.SetCharacterToStandStill();
            controller2.SetCharacterToStandStill();
        }

        void ChangeCanvas(MinigameState to, int victorID = -1)
        {
            switch (to)
            {
                case (MinigameState.Intro):

                    // Deactivate
                    gameControl.StopGameplay();
                    endControl.StopEnd();

                    // Activate
                    introControl.PlayIntro();

                    break;

                case (MinigameState.Gameplay):

                    // Deactivate
                    introControl.StopIntro();
                    endControl.StopEnd();

                    // Activate
                    gameControl.StartGameplay();

                    break;

                case (MinigameState.EndBattle):

                    // Deactivate
                    introControl.StopIntro();
                    gameControl.StopGameplay();

                    // Activate
                    endControl.StartEndingCutscene (victorID);

                    break;
            }
        }

        void ChangeCurrentMinigameState(MinigameState newState)
        {
            currentState = newState;
        }

        void ChangeToMainMenuScene()
        {
            SceneManager.LoadScene("Main Menu1");
        }

        void ResetAllFightersStats() { // Recover Health and Position of Player

            fighter1.GetComponent<Health>().FullRecovery();
            fighter2.GetComponent<Health>().FullRecovery();

            fighter1.ResetFighterPosition();
            fighter2.ResetFighterPosition();

            fighter1.ResetAnimator();
            fighter2.ResetAnimator();

            fighter1.ChangeState(FighterState.Idle);
            fighter2.ChangeState(FighterState.Idle);
        }

        void ResetCurrentLevel()
        {
            SceneManager.LoadScene("2D_Fighter");
        }

        void UpdateBackground()
        {
            if (roundID == maxRound)
            {
                sceneryAnimator.SetTrigger("Final Stage");
            }
        }

        void UpdateRoundDisplay()
        {
            // Update Round Conter
            roundDisplay.SetText(roundID.ToString());

            // Display Final Mark; if is final round
            finalRoundDisplay.gameObject.SetActive(roundID == maxRound);
        }

        void UpdateFightersScore()
        {
            fighter1ScoreDisplay.SetText(fighter1Score.ToString());
            fighter2ScoreDisplay.SetText(fighter2Score.ToString());
        }

        #endregion

        #region Check Methods

        int CheckForStateFighter(FighterState targetState) /// Return fighter in current state; -1 if no one is found ///
        {
            if (fighter1.currentState == targetState)
            {
                return 1;
            }

            if (fighter2.currentState == targetState)
            {
                return 2;
            }

            /// No Victor ///
            return -1;
        }

        int CheckForWinner()
        {
            int victoryMin = Mathf.CeilToInt((float)maxRound / 2f);

            if (fighter1Score >= victoryMin) return 1;
            if (fighter2Score >= victoryMin) return 2;

            return -1;
        }

        #endregion

    }
}
