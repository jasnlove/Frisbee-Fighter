using System;
using System.Collections.Generic;

namespace States
{
    /* How to use:
     * 
     * The only thing you need to really know, outside of how to construct it
     * is that the transitions list is order-important, so order things in terms
     * of their importance.
     * 
     */

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

        public State QueryStateChange() //Queries all transitions attached to this state, if any func<bool> result in true, the first one found is returned
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
