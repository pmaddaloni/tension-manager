using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class TensionManager : MonoBehaviour {
		
	private float arcDuration;//how long the experience should be, in seconds
	private List<float> tensionLevels; //a list of tension levels, to be evenly distributed over the arcDuration
	
	//the min and max tension levels, set by the user in init
	private float minTension;
	private float maxTension;
	
	//the currently desired tension percent
	private float desiredTensionPercent;
	
	//the current, success and fail state values on the spectrum
	//assumes that successStateVal is higher than failStateVal
	private int failStateVal;
	private int successStateVal;
	private int currStateVal;
	
	
	
	bool initialized = false;
	
	public void Start() {
	}
	
	public void Update() {
		if(!initialized) {
			Debug.Log ("Tension Manager not initialized");
		}
	}
	
	/*returns the impact level and challenge necessary to match the desired tension
	 * timeRemaining: how much time is remaining in the arc duration?
	 * currentState: what is the current state between success and fail?
	 * */
	public void updateTension(float timeRemaining, int currentState, tensionStruct tension) {
		if(successStateVal <= currentState || currentState <= failStateVal) {
			Debug.LogError("TensionManager: Tried to getChoices for a state that's already failed or succeeded");
			//return tension;
		} else if (timeRemaining <= 0) {
			Debug.LogError("TensionManager: Tried to calculate desired tension for timeRemaining <= 0");
			//return tension;
		}
		
		//set the current state
		currStateVal = currentState;
		
		//set the desired tension from the graph
		updateDesiredTension(arcDuration-timeRemaining);
		
		//how far should the state be from an end-state after the choice resolves?
		float postChoiceDist = getPostChoiceDist();
		
		//impact values are the difference between the current state and the desired fail or success state
		float maxSuccessImpact = (successStateVal-postChoiceDist) - currentState;
		float maxFailImpact =  currentState - (failStateVal + postChoiceDist);
		float challenge = .5f;//dummy
		
		//set the impacts for each choice in the array
		for (int i = 0; i < tension.challengeLevels.Length; i++) {
			tension.successImpacts[i] = maxSuccessImpact;
			tension.failureImpacts[i] = maxFailImpact;
			tension.challengeLevels[i] = challenge;
		}
		
		//need to check if that should go towards success or failure or both -- right now I'm using both
		//then round to equal actual int distances
		//then correct for rounding with challenge setting
		//find the tension discrepancy created by rounding the impact
		//calc the tension to compensate -- based on whether it's success or failure important
		
		//return tension;//should return the new tension
	}
	
	//interpolates the current tension value for this amount of time passed
	//assumes an equal amount of time per graph tension level
	private void updateDesiredTension(float timePassed) {
		
		float percentComplete = timePassed/arcDuration;
		
		//based on percent complete, which index from the list should the tension be at?
		float rawIndex = percentComplete * (tensionLevels.Count-1);
		int lowIndex = (int)Math.Floor(rawIndex);
		int highIndex = (int)Math.Ceiling(rawIndex);
		float betweenPercent = rawIndex - lowIndex;
		float lowValue = tensionLevels[lowIndex];
		float highValue = tensionLevels[highIndex];
		float rawTension = lowValue + (highValue-lowValue)*betweenPercent;
		desiredTensionPercent = rawTension/maxTension;
	}
	
	//returns the post choice distance-from-end-state based on desired tension
	//assumes desiredTensionPercent is updated
	public float getPostChoiceDist() {
		//size of the success-failure space
		float spaceSize = Math.Abs(successStateVal-failStateVal);
		return desiredTensionPercent*spaceSize;
	}
	
	
	
	public void init(float duration, string tensionFileName, float min, float max, int failVal, int succVal) {
		
		tensionLevels = new List<float>();
		
		if(initialized) {
			Debug.LogError("Tried to re-initialize a tensionManager");
			return;
		}
		
		arcDuration = duration;
		minTension = min;
		maxTension = max;
		successStateVal = succVal;
		failStateVal = failVal;
				
		initTensionLevels(tensionFileName);
		initialized  = true;
	}
	
	//read the tension levels from the file into the list
	private void initTensionLevels(string tensionFileName) {
		//read the tension levels into the list
		try {			
			using (StreamReader sr = new StreamReader(tensionFileName)) 
            {
				string numString;
				int lineNum = 1;
				float newNum;
				
                // Read and display lines from the file until the end of  
                // the file is reached. 
                while ((numString = sr.ReadLine()) != null) 
                {
					if(float.TryParse(numString, out newNum)) {
						if (newNum < minTension) Debug.LogError("tension file has value less than minTension at line: " + lineNum);
						else if (newNum > maxTension) Debug.LogError("tension file has value greater than maxTension at line: " + lineNum);
						else {
							tensionLevels.Add(newNum);
						}
						lineNum++;
					} else {
						Debug.LogError("tension file parse failed at line: " + lineNum);
						break;
					}			
                }
            }
		} catch (Exception e) {
			// Let the user know what went wrong.
			Debug.LogError ("In Tension Manager initTensionLevels:" + e.Message);
		}
	}
	
	
}
