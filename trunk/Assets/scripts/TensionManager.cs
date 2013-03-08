using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TensionManager : MonoBehaviour {
	
	private float arcDuration;//how long the experience should be, in seconds
	private List<float> tensionLevels; //a list of tension levels, to be evenly distributed over the arcDuration
	
	bool initialized = false;
	
	public void Start() {
		
	}
	
	public void Update() {
		if(!initialized) {
			Debug.Log ("Tension Manager not initialized");
		}
	}
	
	public void init(float duration, string tensionFileName) {
		if(initialized) {
			Debug.Log("Tried to re-initialize a tensionManager");
			return;
		}
		
		initialized  = true;
		arcDuration = duration;
	}
}
