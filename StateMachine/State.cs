using UnityEngine;
using System.Collections;

public class State
{
 
	public int key;
	public StateDelegate enter;
	public StateDelegate update;
	public StateDelegate exit;
 
	public State (int enumIndex, StateDelegate enterD, StateDelegate updateD, StateDelegate exitD)
	{
		key = enumIndex;  
		enter = enterD; 
		update = updateD; 
		exit = exitD; 
	}
}

