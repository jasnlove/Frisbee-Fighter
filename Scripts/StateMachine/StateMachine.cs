namespace States
{
    /* How to use:
     * 
     * Once you have set up the statemachine either by meticulously constructing it, or using the builder I wrote
     * Just run RunStateMachine() in the Update() method, or call it whenever, it mostly does its own managing and logic
     * it doesn't really care when
     * 
     */

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

        private void ChangeState(State nextState) //Calls exit methods and initialization methods of new machine
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
