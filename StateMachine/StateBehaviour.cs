using UnityEngine;
using System.Collections;

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

public abstract class StateBehaviour : MonoBehaviour {
	
	protected StateMachine sm = new StateMachine();
	
	// Update is called once per frame
	void Update ()
	{
		// ALL UPDATES MUST BE DONE FROM THE STATE OR THE PreStateUpdate/PostStateUpdate FUNCTIONS!
		PreStateUpdate ();
		sm.Execute ();
		PostStateUpdate ();
	}
	
	protected virtual void PreStateUpdate ()
	{
		
	}
	
	protected virtual void PostStateUpdate()
	{
		
	}
}
