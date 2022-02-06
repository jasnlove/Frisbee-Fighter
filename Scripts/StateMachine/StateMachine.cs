namespace States
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public StateMachine(State startingState)
        {
            CurrentState = startingState;
            CurrentState.InitializeState();
        }

        public void RunStateMachine()
        {
            CurrentState.OnUpdate();
            State tmp = CurrentState.QueryStateChange();
            if(tmp != CurrentState)
            {
                ChangeState(tmp);
            }
        }

        private void ChangeState(State nextState)
        {
            if (CurrentState == nextState || nextState == null)
            {
                return;
            }
            CurrentState.OnExit();
            CurrentState = nextState;
            CurrentState.InitializeState();
        }
    }
}
