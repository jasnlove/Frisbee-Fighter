using System;
using System.Collections.Generic;

namespace States
{
    public class State
    {
        public string Name { get; private set; }
        private Action enterState;
        private Action exitState;
        private Action duringState;

        private List<Transition> transitions;

        public State(string name, Action enter, Action during, Action exit)
        {
            this.Name = name;
            this.enterState = enter;
            this.duringState = during;
            this.exitState = exit;
            this.transitions = new List<Transition>();
        }

        public void AssignTransition(State toState, Func<bool> condition)
        {
            Transition t = new Transition(condition, toState);
            transitions.Add(t);
        }

        public void InitializeState()
        {
            enterState?.Invoke();
        }

        public void OnExit()
        {
            exitState?.Invoke();
        }

        public void OnUpdate()
        {
            duringState?.Invoke();
        }

        public State QueryStateChange()
        {
            foreach(Transition t in transitions)
            {
                if (t.Check())
                {
                    return t.NextState;
                }
            }
            return this;
        }
    }
}
