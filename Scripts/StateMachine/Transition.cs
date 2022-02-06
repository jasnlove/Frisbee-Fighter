using System;

namespace States
{
    public class Transition
    {
        public State NextState { get; private set; }
        private Func<bool> condition;

        public Transition(Func<bool> condition, State nextState)
        {
            this.condition = condition;
            this.NextState = nextState;
        }

        public bool Check()
        {
            return condition.Invoke();
        }
    }
}
