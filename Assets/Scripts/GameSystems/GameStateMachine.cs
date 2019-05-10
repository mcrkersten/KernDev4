using System;
using System.Collections.Generic;

    public enum ProcessFase {
        SearchingFase,
        CountDownFase1,
        CountDownFase2,
        PlacingFase,
        GameFase,
        PlayerTurn,
        EnemyTurn,
        ClientPause,

        GameEndFase,
        Exit,
    }

    public enum Command {    
        ExitGame = 0,
        EndGame,
        StartPlacingFase,
        StartGameFaseCountdown,
        StartGameFase,
        FoundPlayers,
        ChangeTurnPlayer,
        ChangeTurnEnemy,
        Pause,
        UnPause,
    }

public class GameStateMachine {
    //On state change throw Delegate
    public delegate void ChangeFaseCall(ProcessFase fase);
    /// <summary>
    /// Throws call when fase is changed, gets captured in waveGenerator.cs
    /// </summary>
    public static event ChangeFaseCall OnFaseChange;

    class StateTransition {
        readonly ProcessFase CurrentFase;
        readonly Command Command;

        public StateTransition(ProcessFase currentFase, Command command) {
            CurrentFase = currentFase;
            Command = command;
        }

        public override int GetHashCode() {
            return 17 + 31 * CurrentFase.GetHashCode() + 31 * Command.GetHashCode();
        }

        public override bool Equals(object obj) {
            StateTransition other = obj as StateTransition;
            return other != null && this.CurrentFase == other.CurrentFase && this.Command == other.Command;
        }
    }

    Dictionary<StateTransition, ProcessFase> transitions;
    public ProcessFase CurrentState { get; private set; }

    public GameStateMachine() {
        CurrentState = ProcessFase.SearchingFase;
        transitions = new Dictionary<StateTransition, ProcessFase>
        {
            //Transition from Searching to Countdown1.
            { new StateTransition(ProcessFase.SearchingFase, Command.FoundPlayers), ProcessFase.CountDownFase1 },

            //Transition from Countdown1 to Ship-placement.
            { new StateTransition(ProcessFase.CountDownFase1, Command.StartPlacingFase), ProcessFase.PlacingFase },

            //Transition from Ship-placement to countdown2.
            { new StateTransition(ProcessFase.PlacingFase, Command.StartGameFaseCountdown), ProcessFase.CountDownFase2 },

            //Transition from Countdown2 to GameFase.
            { new StateTransition(ProcessFase.CountDownFase2, Command.StartGameFase), ProcessFase.GameFase },
            { new StateTransition(ProcessFase.CountDownFase2, Command.EndGame), ProcessFase.GameEndFase },

            //Transition from Gamefase to end.
            { new StateTransition(ProcessFase.GameFase, Command.EndGame), ProcessFase.GameEndFase },

            #region ClientOnly
            //CHANGE FROM GAMEFASE TO A TURN
            { new StateTransition(ProcessFase.GameFase, Command.ChangeTurnEnemy), ProcessFase.EnemyTurn },
            { new StateTransition(ProcessFase.GameFase, Command.ChangeTurnPlayer), ProcessFase.PlayerTurn },

            //CHAMGE FROM PLAYERTURN TO ENEMYTURN OR ENDGAME
            { new StateTransition(ProcessFase.PlayerTurn, Command.ChangeTurnEnemy), ProcessFase.EnemyTurn },
            { new StateTransition(ProcessFase.PlayerTurn, Command.EndGame), ProcessFase.GameEndFase },

            //CHAMGE FROM ENEMYTURN TO PLAYERTURN OR ENDGAME
            { new StateTransition(ProcessFase.EnemyTurn, Command.ChangeTurnPlayer), ProcessFase.PlayerTurn },
            { new StateTransition(ProcessFase.EnemyTurn, Command.EndGame), ProcessFase.GameEndFase },
            #endregion

            //If Game is allready ended but we got a new end game command.
            { new StateTransition(ProcessFase.GameEndFase, Command.EndGame), ProcessFase.GameEndFase },
        };
    }

    private ProcessFase ChangeFaseLocal(Command command) {
        StateTransition transition = new StateTransition(CurrentState, command);
        ProcessFase nextState;
        if (!transitions.TryGetValue(transition, out nextState))
            throw new Exception("Invalid transition: " + CurrentState + " -> " + command);

        //Throw FaseChange
        OnFaseChange?.Invoke(nextState);
        return nextState;
    }

    public ProcessFase ChangeFase(Command command) {
        CurrentState = ChangeFaseLocal(command);

        return CurrentState;
    }
}