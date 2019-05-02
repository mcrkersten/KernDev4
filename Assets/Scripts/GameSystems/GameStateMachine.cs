using System;
using System.Collections.Generic;

    public enum ProcessState {
        SearchingFase,
        CountDownFase,
        PlacingFace,
        GameFace,
        ClientPause,

        GameEndFase,
        Exit,
    }

    public enum Command {    
        Exit,
        EndGame,
        StartPlacingFase,
        StartGameCountdown,
        StartGameFase,
        FoundPlayer,
        Pause,
        UnPause,
    }

public class GameStateMachine {
    class StateTransition {
        readonly ProcessState CurrentState;
        readonly Command Command;

        public StateTransition(ProcessState currentState, Command command) {
            CurrentState = currentState;
            Command = command;
        }

        public override int GetHashCode() {
            return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
        }

        public override bool Equals(object obj) {
            StateTransition other = obj as StateTransition;
            return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
        }
    }

    Dictionary<StateTransition, ProcessState> transitions;
    public ProcessState CurrentState { get; private set; }

    public GameStateMachine() {
        CurrentState = ProcessState.SearchingFase;
        transitions = new Dictionary<StateTransition, ProcessState>
        {
            #region SearchingFase
            { new StateTransition(ProcessState.SearchingFase, Command.FoundPlayer), ProcessState.CountDownFase },
            { new StateTransition(ProcessState.SearchingFase, Command.Pause), ProcessState.ClientPause },
            { new StateTransition(ProcessState.ClientPause, Command.FoundPlayer), ProcessState.CountDownFase },
            { new StateTransition(ProcessState.ClientPause, Command.UnPause), ProcessState.SearchingFase },
            #endregion

            //TransitionPhase from Countdown to ship-placement.
            { new StateTransition(ProcessState.CountDownFase, Command.StartPlacingFase), ProcessState.PlacingFace },

            #region PlacingFase
            { new StateTransition(ProcessState.PlacingFace, Command.Pause), ProcessState.ClientPause },
            { new StateTransition(ProcessState.ClientPause, Command.UnPause), ProcessState.PlacingFace },
            { new StateTransition(ProcessState.PlacingFace, Command.StartGameCountdown), ProcessState.CountDownFase },
            #endregion

            //TransitionPhase from Countdown to GameFase.
            { new StateTransition(ProcessState.CountDownFase, Command.StartPlacingFase), ProcessState.GameFace },

            #region GameFase
            { new StateTransition(ProcessState.GameFace, Command.Pause), ProcessState.ClientPause },
            { new StateTransition(ProcessState.ClientPause, Command.UnPause), ProcessState.GameFace },
            { new StateTransition(ProcessState.GameFace, Command.EndGame), ProcessState.GameEndFase },
            { new StateTransition(ProcessState.ClientPause, Command.EndGame), ProcessState.GameEndFase },
            #endregion

            //Game is over, view score, exit with Menu.
            { new StateTransition(ProcessState.GameEndFase, Command.Pause), ProcessState.ClientPause },
            { new StateTransition(ProcessState.ClientPause, Command.UnPause), ProcessState.GameEndFase },
        };
    }

    public ProcessState ChangeFase(Command command) {
        StateTransition transition = new StateTransition(CurrentState, command);
        ProcessState nextState;
        if (!transitions.TryGetValue(transition, out nextState))
            throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
        return nextState;
    }

    public ProcessState NexFase(Command command) {
        CurrentState = ChangeFase(command);
        return CurrentState;
    }
}

// GameStateMachine p = new GameStateMachine();