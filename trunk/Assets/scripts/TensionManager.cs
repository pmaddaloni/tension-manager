using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class TensionManager : MonoBehaviour {
		
	private const float BASE_CHALLENGE = .5f;
		
	private float arcDuration;//how long the experience should be, in seconds
	private List<float> tensionLevels; //a list of tension levels, to be evenly distributed over the arcDuration
	
	//the currently desired tension percent
	private float desiredTensionPercent;
		
	//the current, success and fail state values on the spectrum
	//assumes that successStateVal is higher than failStateVal
	private int failStateVal;
	private int successStateVal;
	
	//the size of the overall space and half the space, for reasoning about percent distance from fail/success
	//private float spaceSize;
	//private float halfSpaceSize;
	
	private int currStateVal;
	
	private bool initialized = false;
	private bool throttling = false;
	
	//which end of the choice to increase
	private enum impacts {
		success, fail, neither, SIZE
	};
		
	public void Start() {
	}
	
	public void Update() {
		if(!initialized) {
			Debug.Log ("Tension Manager not initialized");
		}
	}
	
	public void setThrottle(bool setToThrottle)
	{
		throttling = setToThrottle;
	}
		
	public bool checkIfThrottlingNecessary(int amountToThrottle)
	{
		amountToThrottle = 0;
		return false;
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
		
		//set the desired tension percent from the graph
		updateDesiredTension(arcDuration-timeRemaining);
		
		//build the choices
		buildChoices(tension);
	}
	
	void buildChoices(tensionStruct tension) {
		
		//how far is current state from success/failure?
		float distFromSuccess = Math.Abs (successStateVal-currStateVal);
		float distFromFailure = Math.Abs (currStateVal-failStateVal);
		
		//the maxExtraPercent a choice can push past the avgImpact
		float remainingPercent = 1 - desiredTensionPercent;
		float maxExtraPercent = desiredTensionPercent*remainingPercent;
		
		int numChoices = tension.challengeLevels.Length;
		
		//set the impacts for each choice in the array
		for (int i = 0; i < numChoices; i++) {
			//how much extra impact will the choice allow?
			float extraPercent = UnityEngine.Random.Range(0, maxExtraPercent);
			
			//what percent of the desired tension does this addition grant?
			float percentAboveAverage = extraPercent/desiredTensionPercent;
			
			if (i % (int)impacts.SIZE == (int)impacts.success) {
				//the increased impact
				tension.successImpacts[i] = (desiredTensionPercent+extraPercent)*distFromSuccess;
				//average impact
				tension.failureImpacts[i] = desiredTensionPercent*distFromFailure;
				//add the additional percent to the challenge to compensate for greater reward
				tension.challengeLevels[i] = BASE_CHALLENGE + percentAboveAverage;
			} else if (i % (int)impacts.SIZE == (int)impacts.fail) {
				//the increased impact
				tension.failureImpacts[i] = (desiredTensionPercent+extraPercent)*distFromFailure;
				//average impact
				tension.successImpacts[i] = desiredTensionPercent*distFromSuccess;
				//remove the additional percent from the challenge to compensate for greater punishment
				tension.challengeLevels[i] = BASE_CHALLENGE - percentAboveAverage;
			} else {//neither - average impact, base challenge
				tension.successImpacts[i] = desiredTensionPercent*distFromSuccess;
				tension.failureImpacts[i] = desiredTensionPercent*distFromFailure;
				tension.challengeLevels[i] = BASE_CHALLENGE;
			}
		}
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
		
		desiredTensionPercent = rawTension/100;
	}
	
	/*
	//returns the post choice percent-distance-from--each-end-state based on desired tension
	//reasons about distance using the halfSpaceSize so the lowest-importance choice will leave you at the midpoint
	//assumes desiredTensionPercent is updated
	public float getPostChoiceDist() {
		
		//size of the success-failure space
		return halfSpaceSize - (desiredTensionPercent*halfSpaceSize);
	}*/
	
	
	
	public void init(float duration, string tensionFileName, int failVal, int succVal) {
		
		tensionLevels = new List<float>();
		
		if(initialized) {
			Debug.LogError("Tried to re-initialize a tensionManager");
			return;
		}
		
		arcDuration = duration;
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
						if (newNum < 0) Debug.LogError("tension file has value less than 0 at line: " + lineNum);
						else if (newNum > 100) Debug.LogError("tension file has value greater than 100 at line: " + lineNum);
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
