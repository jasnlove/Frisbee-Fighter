using System.Collections.Generic;
using System;
using System.Linq;

namespace States
{

    /* How to use:
     * 
     * Start with a StateMachine x = new StateMachineBuilder()
     * Define a state using .WithState( stateName : string)
     * Define the states actions using .WithOnEnter, .WithOnRun, .WithOnExit
     * Define the states transition(s) using .WithTransition(nextState : string, Func<bool>)
     * Defining transitions for states that don't exist will not error, but will just produce nothing
     * Continue this pattern, define the next state with .WithState... and do this until there are no more states.
     * call .Build()
     * EXAMPLE:
     *             StateMachine x = new StateMachineBuilder()
     *                          .WithState("X")
     *                          .WithOnEnter( () => { foo(); } )
     *                          .WithTransition("Y", () => { return bar(); } )
     *                          .WithState("Y")
     *                          .WithOnEnter( () => { foobar(); } )
     *                          .Build()
     */



    public class StateMachineBuilder
    {
        private readonly List<string> _states = new List<string>();
        private readonly Dictionary<string, Action> _onEnter = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> _onExit = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> _onRun = new Dictionary<string, Action>();
        private readonly List<(string from, string to, Func<bool> condition)> _transitions = new List<(string from, string to, Func<bool> condition)>();
        private readonly List<(string to, Func<bool> condition)> _anyStates = new List<(string to, Func<bool> condition)>();
        private string _currentState;

        public StateMachineBuilder WithState(string name)
        {
            if (_states.Contains(name)) throw new ArgumentException($"State {name} already exists.");
            _states.Add(name);
            _currentState = name;
            return this;
        }

        public StateMachineBuilder WithTransitionFromAnyState(Func<bool> condition)
        {
            _anyStates.Add((_currentState,condition));
            return this;
        }

        public StateMachineBuilder WithOnEnter(Action onEnter)
        {
            _onEnter.Add(_currentState, onEnter);
            return this;
        }

        public StateMachineBuilder WithOnExit(Action onExit)
        {
            _onExit.Add(_currentState, onExit);
            return this;
        }

        public StateMachineBuilder WithOnRun(Action onRun)
        {
            _onRun.Add(_currentState, onRun);
            return this;
        }

        public StateMachineBuilder WithTransition(string nextState, Func<bool> condition)
        {
            _transitions.Add((_currentState, nextState, condition));
            return this;
        }

        public StateMachine Build()
        {
            var statesLookup = _states.Select(name => new State(name, GetOnEnter(name), GetOnRun(name), GetOnExit(name)))
              .ToDictionary(x => x.Name, x => x);
            foreach (var (from, to, condition) in _transitions.Select(t => (statesLookup[t.from], statesLookup[t.to], t.condition)))
            {
                from.AssignTransition(to, condition);
            }

            StateMachine sm = new StateMachine(statesLookup.Values.First());
            foreach(var (to, condition) in _anyStates.Select(t=> (statesLookup[t.to], t.condition)))
            {
                sm.AddAnyState(new Transition(condition, to));
            }

            return sm;

            Action GetOnRun(string key) => GetAction(_onRun, key);
            Action GetOnExit(string key) => GetAction(_onExit, key);
            Action GetOnEnter(string key) => GetAction(_onEnter, key);
            Action GetAction(Dictionary<string, Action> collection, string key) =>
              collection.TryGetValue(key, out var value) ? value : null;
        }
    }
}
