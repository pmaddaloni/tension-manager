using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class TensionManager : MonoBehaviour
{
		
	private const float BASE_CHALLENGE = .5f;
	private float arcDuration;//how long the experience should be, in seconds
	private float percentComplete;//how far along on the arc should we be?
	
	private List<float> tensionLevels; //a list of tension levels, to be evenly distributed over the arcDuration
	
	//the currently desired tension percent
	private float desiredTensionPercent;
	
	//the maximum additional pecent wiggle room to amplify choices
	private float maxExtraPercent;
	
	//extra impact choices must yield at least this much wiggle room
	//if it can't provide at least this, will throttle play
	private float minExtraPercent = .07f;
		
	//the current, success and fail state values on the spectrum
	//assumes that successStateVal is higher than failStateVal
	private float failStateVal;
	private float successStateVal;
	
	//current state and distance from the ends - updated in updateTension()
	private float currStateVal;
	private float distFromSuccess;
	private float distFromFailure;
	
	//the size of the success-fail space
	private float spaceSize;
	
	//How much time must be past before allowing a game end?
	private float minEndTimePercent;
	private bool initialized = false;
	
	//which end of the choice to increase
	private enum impacts
	{
		success,
		fail,
		neither,
		SIZE
	};
	
	//capped impact values which are used if the tension graph would allow early victory
	float cappedSuccessImpact;
	float cappedFailImpact;
	bool cappedImpact = false;
	bool cappedExtraPercent = false;
	
	private enum throttleType
	{
		positive,
		negative,
		handled
	};
	
	/**
	 * Initialize the tension manager
	 * duration: game length
	 * tensionFileName: the txt file holding the desired tension graph
	 * failVal: if state reaches this, the game ends in failure
	 * succVal: if state reaches this, the game ends in success
	 * bool throttle: should throttling be allowed
	 * minPercentTimeToEnd: how much of the duration must have been passed to allow a win/fail state?
	 * */
	public void init (float duration, string tensionFileName, int failVal, int succVal, float minPercentTimeToEnd)
	{
		
		tensionLevels = new List<float> ();
		
		if (initialized) {
			Debug.LogError ("Tried to re-initialize a tensionManager");
			return;
		}
		
		arcDuration = duration;
		successStateVal = succVal;
		failStateVal = failVal;
		spaceSize = Mathf.Abs (successStateVal - failStateVal);
				
		initTensionLevels (tensionFileName);
		initialized = true;
		
		minEndTimePercent = minPercentTimeToEnd;
	}
		
	public void Start ()
	{
	}
	
	public void Update ()
	{
		if (!initialized) {
			Debug.Log ("Tension Manager not initialized");
		}
	}
	
	/*returns the impact level and challenge necessary to match the desired tension
	 * timeRemaining: how much time is remaining in the arc duration?
	 * currentState: what is the current state between success and fail?
	 * */
	public void updateTension (float timeRemaining, int currentState, tensionStruct tension)
	{
		if (successStateVal <= currentState || currentState <= failStateVal) {
			Debug.LogError ("TensionManager: Tried to getChoices for a state that's already failed or succeeded");
			return;
		} else if (timeRemaining <= 0) {
			Debug.LogError ("TensionManager: Tried to calculate desired tension for timeRemaining <= 0");
			return;
		}
		
		//set the current state
		currStateVal = currentState;
		
		//reset the throttling booleans
		cappedImpact = false;
		cappedExtraPercent = false;
		
		//reset the random throttle event
		//tension.randomEvent = new KeyValuePair<bool, float>(false, 0);
		tension.randomEventNeeded[0] = false;
		tension.randomEventImpact[0] = 0;
		
		//how far into the arc are we?
		percentComplete = (arcDuration - timeRemaining) / arcDuration;
		
		//set the desired tension percent from the graph
		updateDesiredTension ();
	
		//build the choices
		buildChoices (tension);
	}
	
	void buildChoices (tensionStruct tension)
	{
		//how far is current state from success/failure?
		distFromSuccess = Math.Abs (successStateVal - currStateVal);
		distFromFailure = Math.Abs (currStateVal - failStateVal);
		
		//set based on the desired tension -- will throttle progress if there's no play for additional impace percent
		updateMaxExtraImpactPercentAndThrottle (tension);
		
		int numChoices = tension.challengeLevels.Length;
		
		//set the impacts for each choice in the array
		for (int i = 0; i < numChoices; i++) {
			
			//impact has been hard capped to prevent end states, which the raw desired tension would have allowed
			if(cappedImpact) {
				tension.successImpacts [i] = cappedSuccessImpact;
				tension.failureImpacts [i] = cappedFailImpact;
				tension.challengeLevels [i] = BASE_CHALLENGE;
			} else {
			
				//how much extra impact will the choice allow?
				float extraPercent;
				if (cappedExtraPercent) extraPercent = UnityEngine.Random.Range (0, maxExtraPercent);//extra percent was throttled
				else extraPercent = UnityEngine.Random.Range (minExtraPercent, maxExtraPercent);//use at least min extra percent
				
				if (i % (int)impacts.SIZE == (int)impacts.success) {//boost the success impact
					//the increased impact
					tension.successImpacts [i] = (desiredTensionPercent + extraPercent) * distFromSuccess;
					//average impact
					tension.failureImpacts [i] = desiredTensionPercent * distFromFailure;
					//the percent, out of the spaceSize that the extra percent boosts the success impact
					float gainPercent = (extraPercent * distFromSuccess) / spaceSize;
					//add the additional percent to the challenge to compensate for greater reward
					tension.challengeLevels [i] = BASE_CHALLENGE + gainPercent * BASE_CHALLENGE;
					
				} else if (i % (int)impacts.SIZE == (int)impacts.fail) {//boost the fail impact
					//the increased impact
					tension.failureImpacts [i] = (desiredTensionPercent + extraPercent) * distFromFailure;
					//average impact
					tension.successImpacts [i] = desiredTensionPercent * distFromSuccess;
					
					//the percent, out of the space size, that the extra percent boosts the failure impact
					float gainPercent = (extraPercent * distFromFailure) / spaceSize;
					//remove the additional percent from the challenge to compensate for greater punishment
					tension.challengeLevels [i] = BASE_CHALLENGE - gainPercent * BASE_CHALLENGE;
					
				} else {//neither - average impact, base challenge
					tension.successImpacts [i] = desiredTensionPercent * distFromSuccess;
					tension.failureImpacts [i] = desiredTensionPercent * distFromFailure;
					tension.challengeLevels [i] = BASE_CHALLENGE;
				}
			} //print ("TensionManager" + tension.challengeLevels [i]);
		}
	}
	
	//set the max extra percent that can be added to the choices
	//will throttle the player if there aren't interesting options
	void updateMaxExtraImpactPercentAndThrottle (tensionStruct tension)
	{
		//the default extra percent is fine, because winning is acceptable
		if (percentComplete >= minEndTimePercent) {
			//if winning is acceptable at this point, allow moves to use the full percent space
			maxExtraPercent = 1 - desiredTensionPercent;
			return;
		}
		
		//the max percent movement we can allow without permitting an end state
		float maxSuccessMovePercent = (distFromSuccess - 1) / distFromSuccess;
		float maxFailMovePercent = (distFromFailure - 1) / distFromFailure;
		
		throttleType throttle = getThrottleType (maxSuccessMovePercent, maxFailMovePercent);
		
		//Tension is just too high before winning should be allowed, or the minExtraPercent would allow winning
		//for these cases, getThrottleType handles setting the values and the respective global booleans
		if (throttle == throttleType.handled) {
			return;
		}
		
		//set the throttle and then reset max extra impact and add any new throttles if necessary
		if (throttle == throttleType.positive) {
			setThrottle(tension, throttleType.positive);
			
			//need to recheck based on the post-throttled state
			updateMaxExtraImpactPercentAndThrottle(tension);
		} else  {
			setThrottle(tension, throttleType.negative);
			
			//need to recheck based on the post-throttled state
			updateMaxExtraImpactPercentAndThrottle(tension);
		}
	}
	
	//set a positive or negative throttle that'll move the current state 50-100% to the center of the space
	private void setThrottle(tensionStruct tension, throttleType throttle) {
		float maxThrottle;
		
		
		//throttle the state to as much as half way to the other extreme
		if(throttle == throttleType.positive) {
			maxThrottle = distFromSuccess/2;
		} else {//negative throttle
			
			maxThrottle = distFromFailure/2;
		}
		
		//throttle the state at least halfway to maxThrottle
		float throttleAmt = UnityEngine.Random.Range(0, maxThrottle); 
		
		if(throttle == throttleType.negative){
			throttleAmt *= -1;
		}
		
		//update state variables
		currStateVal += Mathf.Round(throttleAmt);
		distFromSuccess = Math.Abs (successStateVal - currStateVal);
		distFromFailure = Math.Abs (currStateVal - failStateVal);
		
		/*Debug.Log ("throttle type: " + throttle);
		Debug.Log("new amt: " + throttleAmt);
		Debug.Log ("currState: " + currStateVal);*/
		//there was a pre-existing throttle to be taken into account, add it to the new throttle
		if (tension.randomEventNeeded[0]) throttleAmt += tension.randomEventImpact[0];
		//tension.randomEvent = new KeyValuePair<bool, float>(true, throttleAmt);
		tension.randomEventNeeded[0] = true;
		tension.randomEventImpact[0] = throttleAmt;
		//print ("Tension Manager" + tension.randomEventNeeded[0] + " " + tension.randomEventImpact[0]);
		//Debug.Log ("throttle true: " + throttleAmt);
	}
	
	//returns the type of throttling necessary based on the max move percents in the effort of providing interesting choices
	private throttleType getThrottleType (float maxSuccessMovePercent, float maxFailMovePercent)
	{
		//throttle check parameters
		bool throttleNegative = false;
		bool throttlePositive = false;
		
		//how much wiggle room there is to add extra impact without allowing end states
		float remainingNonWinSuccessPercent = maxSuccessMovePercent - desiredTensionPercent;
		float remainingNonWinFailPercent = maxFailMovePercent - desiredTensionPercent;
		/*Debug.Log("nonWinSuccessPerc: " + remainingNonWinSuccessPercent);
		Debug.Log ("nonWinFailPerc: " + remainingNonWinFailPercent);
		Debug.Log ("desired tension: " + desiredTensionPercent);*/
			
		//If we're not able to allow extra impact percent of the specified minimum value
		if (remainingNonWinSuccessPercent < minExtraPercent)
			throttleNegative = true;
		else if (remainingNonWinFailPercent < minExtraPercent)
			throttlePositive = true;
				
		if (throttleNegative && throttlePositive) {//need to throttle both
			
			//the tension level would permit winning too early, so hard cap the impacts
			//This would be a good place to instead introduce tension on a different axis (i.e. - less choice time)
			if(remainingNonWinFailPercent < 0 || remainingNonWinSuccessPercent < 0) {
				cappedImpact = true;
				cappedSuccessImpact = Mathf.Abs (currStateVal - (distFromSuccess-1));
				cappedFailImpact = Mathf.Abs( currStateVal - (distFromFailure-1));
			} else {
			
				//cap the extra percent to the greatest point where winning won't be possible
				maxExtraPercent = Mathf.Min(remainingNonWinFailPercent, remainingNonWinSuccessPercent);
				cappedExtraPercent = true;
			}
				return throttleType.handled;
		}
		else if (!throttleNegative && !throttlePositive) {
			//set the maxExtra percent based on smaller remainingNonWin Percent & desired tension
			//float tensionBasedExtra = desiredTensionPercent*Mathf.Min(remainingNonWinFailPercent, remainingNonWinSuccessPercent);
			float tensionBasedExtra = Mathf.Min(remainingNonWinFailPercent, remainingNonWinSuccessPercent);
			maxExtraPercent = Mathf.Max(tensionBasedExtra, minExtraPercent);
			return throttleType.handled;
		} else if (throttlePositive)
			return throttleType.positive;
		else
			return throttleType.negative;
	}
	
	
	//interpolates the current tension value for this amount of time passed
	//assumes an equal amount of time per graph tension level
	private void updateDesiredTension ()
	{
		//based on percent complete, which index from the list should the tension be at?
		float rawIndex = percentComplete * (tensionLevels.Count - 1);
		int lowIndex = (int)Math.Floor (rawIndex);
		int highIndex = (int)Math.Ceiling (rawIndex);
		float betweenPercent = rawIndex - lowIndex;
		float lowValue = tensionLevels [lowIndex];
		float highValue = tensionLevels [highIndex];
		float rawTension = lowValue + (highValue - lowValue) * betweenPercent;
		
		desiredTensionPercent = rawTension / 100;
	}
	
	//read the tension levels from the file into the list
	private void initTensionLevels (string tensionFileName)
	{
		//read the tension levels into the list
		try {			
			using (StreamReader sr = new StreamReader(tensionFileName)) {
				string numString;
				int lineNum = 1;
				float newNum;
				
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((numString = sr.ReadLine()) != null) {
					if (float.TryParse (numString, out newNum)) {
						if (newNum < 0)
							Debug.LogError ("tension file has value less than 0 at line: " + lineNum);
						else if (newNum > 100)
							Debug.LogError ("tension file has value greater than 100 at line: " + lineNum);
						else {
							tensionLevels.Add (newNum);
						}
						lineNum++;
					} else {
						Debug.LogError ("tension file parse failed at line: " + lineNum);
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
