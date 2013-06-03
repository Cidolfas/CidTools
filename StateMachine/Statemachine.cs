using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
/*   State Template
 * 
	// IDLE STATE
	bool enterIDLE ()
	{
		return true;
	}
	
	bool updateIDLE ()
	{
		float x = Input.GetAxisRaw("Horizontal");
		if(x != 0)
			stateMachine.ChangeState(enterWALK, updateWALK, exitWALK);
		 
		return false;
	}
	
	bool exitIDLE ()
	{
		return true;
	}
 
*/

/*
 * Lovingly copy-pasted from:
 * https://gist.github.com/SteveSwink/5120612
 * Credit goes to Steve Swink and Mike Stevenson
 * Changes have been made from there
*/
 
// Defines the required parameters and return value type for a StateMachine state.
// Returns a bool representing whether or not the state has finished running.
public delegate bool StateDelegate ();
 
// To use, create an instance of StateMachine inside of a MonoBehaviour, load it up with
// references to state methods with ChangeState(), then call its Execute() method during the
// MonoBehaviour's Update cycle. An example MonoBehaviour is included at the bottom of this file.
public class StateMachine
{
	// Keep track of the currently running state
	enum MState {
		None,
		Entering,
		Updating,
		Exiting
	}
	
	MState mState = MState.None;
 	
	//  List<State> stateList = new List<State>();
	//  Hashtable<int, State> stateList = new Hashtable<int, State>();
	Dictionary<int, State> stateList = new Dictionary<int, State>();
	//	IDictionary<int, State> stateList = new IDictionary<int, State>(); 
 
	// These states will be cached when calling ChangeState(). They'll be copied into
	// currentStateMethod in succession as each state finishes running. The Execute()
	// method will execute currentStateMethod on each update, running whatever method
	// is stored there.
	StateDelegate enter;
	StateDelegate update;
	StateDelegate exit;
 
	// After being called by the Execute() method, the current state will be replaced with
	// the next most appropriate state if the current state returns 'true', signifying that
	// it has finished running.
	StateDelegate currentStateMethod;
	
	// This controls whether or not the state machine will immediately exit the previous state and
	// enter the new state upon a change state call
	public bool immediateMode = true;
 
	public void AddState(State newState){
		stateList.Add(newState.key, newState);
	}
	
	// A single state may be stored at any given time. If you need to queue more than
	// one state, you could conceivably replace ChangeState() with AddState() and append
	// the three state parameters to a list. As its currently written, changing the state
	// immediatley calls the current 'exit' state, then overwrites the cached enter/run/exit
	// states with these new states.
	public void ChangeState (StateDelegate enter, StateDelegate update, StateDelegate exit)
	{
		bool isStateCurrentlyRunning = currentStateMethod != null;
 
		// If a state is currently running, it should be allowed to gracefully exit
		// before the next state takes over
		if (isStateCurrentlyRunning) {
			SwitchCurrentState (MState.Exiting);
		}
 
		// Cache the given state values
		this.enter = enter;
		this.update = update;
		this.exit = exit;
 
		// If a state isn't currently running, we can immediately switch to our entering
		// state using the state delegates we cached a few lines above
		if (!isStateCurrentlyRunning) {
			SwitchCurrentState (MState.Entering);
		}
		
		if (immediateMode) {
			Execute();
		}
	}
	
	public void ChangeState(State state){
		ChangeState(state.enter, state.update, state.exit);
	}
	
	public void ChangeState(int state) {
		if (!stateList.ContainsKey(state) || stateList[state] == null) {
			Debug.LogError("A statemachine is trying to access an unassigned state by number!");
			return;
		}
		
		ChangeState(stateList[state]);
	}

	const int maxLoops = 100;
	private bool _shouldQuit = false;
	
	// Call this during
	public void Execute ()
	{
		if (currentStateMethod == null)
			return;
 		
		_shouldQuit = false;
		
		int loops = 0;
		
		while(!_shouldQuit) {
			// Execute the current state method
			bool finished = currentStateMethod ();
	 
			// If we've reached the end of the current enter/run/exit, advance to the next one
			if (finished && !_shouldQuit) {
				switch (mState) {
				case MState.None:
					SwitchCurrentState (MState.Entering);
					break;
				case MState.Entering:
					SwitchCurrentState (MState.Updating);
					enter = null;
					_shouldQuit = true;
					break;
				case MState.Updating:
					SwitchCurrentState (MState.Exiting);
					update = null;
					break;
				case MState.Exiting:
					// If an Enter behavior exists, it must have been added by ChangeState. We should
					// start running again from the top instead of coming to a halt.
					if (enter != null) {
						SwitchCurrentState (MState.Entering);
					} else {
						SwitchCurrentState (MState.None);
						exit = null;
						_shouldQuit = true;
					}
					break;
				}
			} else {
				_shouldQuit = true;
			}
			
			loops++;
			if (loops >= maxLoops) {
				_shouldQuit = true; // This keeps us from freezing due to an infinite exit/enter cycle
				Debug.LogError("A statemachine has reached the maximum loop count!");
			}
			
			if (!immediateMode) {
				_shouldQuit = true;
			}
		}
	}
 
	// Utility method for performing the state delegate swapping logic based on an enum value
	void SwitchCurrentState (MState state)
	{
		this.mState = state;
		switch (mState) {
		case MState.None:
			currentStateMethod = null;
			break;
		case MState.Entering:
			currentStateMethod = this.enter;
			break;
		case MState.Exiting:
			currentStateMethod = this.exit;
			break;
		case MState.Updating:
			currentStateMethod = this.update;
			break;
		}
	}
}