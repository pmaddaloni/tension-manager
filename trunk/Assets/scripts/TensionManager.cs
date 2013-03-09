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
	
	bool initialized = false;
	
	public void Start() {
	}
	
	public void Update() {
		if(!initialized) {
			Debug.Log ("Tension Manager not initialized");
		}
	}
	
	/*returns the impact level necessary to match the desired tension
	 * timePassed: how much time has passed in this arc duration?
	 * */
	public int getImportanceLevel(float timeRemaining) {
		float currDesiredTensionPercent = getCurrDesiredTension(arcDuration-timeRemaining)/maxTension;
		
		Debug.Log("current desired tension percent: " + currDesiredTensionPercent);
		return 0;
	}
	
	//interpolates the current tension value for this amount of time passed
	//assumes an equal amount of time per graph tension level
	private float getCurrDesiredTension(float timePassed) {
		
		if (timePassed > arcDuration) {
			Debug.LogError("Tried to calculate desired tension for timePassed > arcDuration");
			return 0;
		}
		float percentComplete = timePassed/arcDuration;
		
		//based on percent complete, which index from the list should the tension be at?
		float rawIndex = percentComplete * (tensionLevels.Count-1);
		int lowIndex = (int)Math.Floor(rawIndex);
		int highIndex = (int)Math.Ceiling(rawIndex);
		float betweenPercent = rawIndex - lowIndex;
		float lowValue = tensionLevels[lowIndex];
		float highValue = tensionLevels[highIndex];
		return lowValue + (highValue-lowValue)*betweenPercent;
	}
	
	public void init(float duration, string tensionFileName, float min, float max) {
		
		tensionLevels = new List<float>();
		
		if(initialized) {
			Debug.LogError("Tried to re-initialize a tensionManager");
			return;
		}
		
		initialized  = true;
		arcDuration = duration;
		minTension = min;
		maxTension = max;
				
		initTensionLevels(tensionFileName);
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
