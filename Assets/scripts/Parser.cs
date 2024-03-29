using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Parse the text files for scenes and choices
public class Parser : MonoBehaviour
{
	public void parseScenes (string fileName, string[] scenes)
	{
		try {			
			// Create an instance of StreamReader to read from a file. 
			// The using statement also closes the StreamReader. 
			
			//Read Scenes
			using (StreamReader sr = new StreamReader(fileName)) {
				string line;
				int sceneCounter = 0;
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((line = sr.ReadLine()) != null) {
					//Debug.Log(line);
					if ((line.Trim ().ToUpper ()).Equals ("<INTRO>")) {
						line = sr.ReadLine ();
						while (!(line.Trim().ToUpper()).Equals("<END>")) {
							scenes [sceneCounter] += line + '\n';
							line = sr.ReadLine ();
						}
						sceneCounter++;
					} else {
						line = sr.ReadLine ();						
						if ((line.Trim ().ToUpper ()).Equals ("<SCENE" + sceneCounter + ">")) {
	
							line = sr.ReadLine ();
							while (!(line.Trim().ToUpper()).Equals("<END>") && sceneCounter < 20) {
								scenes [sceneCounter] += line;
								line = sr.ReadLine ();
							}
							sceneCounter++;
						}			
					}
				}
			}
		} catch (Exception e) {
			// Let the user know what went wrong.
			Debug.Log (e.Message);
		}
	}
		
	public void parseChoices (string fileName, List<choiceNode>[] choiceList)
	{
		try {
			//Read Choices
			using (StreamReader sr = new StreamReader(fileName)) {
				int classCounter = 0;
				string line;
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((line = sr.ReadLine()) != null) {
					if ((line.Trim ()).Length == 0) {	
						continue;
					}
					
					if ((line.Trim ()).ToUpper ().Equals ("<CLASS" + classCounter + ">")) {
						choiceList [classCounter] = new List<choiceNode> ();			
						while ((line = sr.ReadLine()) != null && !(line.Trim()).ToUpper().Equals("<ENDCLASS>")) {
							choiceNode temp = new choiceNode ();
							
							if ((line.Trim ()).Length == 0) {	
								continue;
							}
							
							while ((line = sr.ReadLine()) != null && !(line.Trim()).ToUpper().Equals("<ENDCHOICE>")) {			
								if ((line.Trim ()).Length == 0) {	
									continue;
								}
								switch ((line.Trim ()).ToUpper ()) {
									
								case "<LABEL>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.label += line.Trim ();
										line = sr.ReadLine ();
									}
									break;
								case "<DESCRIPTION>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.description += line.Trim () + '\n';
										line = sr.ReadLine ();
									}
									break;
								case "<SUCCESS>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.successText += line.Trim () + '\n';
										line = sr.ReadLine ();
									}
									break;
								case "<FAILURE>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.failureText += line.Trim () + '\n';
										line = sr.ReadLine ();
									}
									break;
								case "<IMPACT>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.impactAmount = Convert.ToInt32 (line.Trim ());
										line = sr.ReadLine ();
									}
									break;
								case "<CHALLENGE>":
									line = sr.ReadLine ();
									while (!(line.Trim()).ToUpper().Equals("<END>")) {
										temp.challengeRate = Convert.ToInt32 (line.Trim ());
										line = sr.ReadLine ();
									}
									break;
								default:
									break;
								}
							}
							choiceList [classCounter].Add (temp);
						}
						classCounter++; 
					}//end if			
					
				}//end CLASS while loop
			}//end using
		} catch (Exception e) {
			// Let the user know what went wrong.
			Debug.Log (e.Message);
		}
	}
	
	public void parseRandomEvents (string fileName, List<randomEventNode> randomEvents)
	{
		try {			
			// Create an instance of StreamReader to read from a file. 
			// The using statement also closes the StreamReader. 
			
			//Read Scenes
			using (StreamReader sr = new StreamReader(fileName)) {
				string line;
				int randomEventCounter = 0;
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((line = sr.ReadLine()) != null) {
					//Debug.Log(line);
					if ((line.Trim ().ToUpper ()).Equals ("<RANDOMSCENE" + randomEventCounter + ">")) {
						randomEventNode temp = new randomEventNode ();
						while ((line = sr.ReadLine()) != null && !(line.Trim()).ToUpper().Equals("<ENDSCENE>")) {			
							if ((line.Trim ()).Length == 0) {	
								continue;
							}
							switch ((line.Trim ()).ToUpper ()) {
							case "<DESCRIPTION>":
								line = sr.ReadLine ();
								while (!(line.Trim()).ToUpper().Equals("<END>")) {
									temp.description += line.Trim () + '\n';
									line = sr.ReadLine ();
								}
								break;
							case "<POSITIVE>":
								line = sr.ReadLine ();
								while (!(line.Trim()).ToUpper().Equals("<END>")) {
									temp.positiveEvent += line.Trim () + '\n';
									line = sr.ReadLine ();
								}
								break;
							case "<NEGATIVE>":
								line = sr.ReadLine ();
								while (!(line.Trim()).ToUpper().Equals("<END>")) {
									temp.negativeEvent += line.Trim () + '\n';
									line = sr.ReadLine ();
								}
								break;
							case "<IMPACT>":
								line = sr.ReadLine ();
								while (!(line.Trim()).ToUpper().Equals("<END>")) {
									temp.impactAmount = Convert.ToInt32 (line.Trim ());
									line = sr.ReadLine ();
								}
								break;
							default:
								break;
							}
						}
						randomEvents.Add (temp);
						randomEventCounter++;
					}
				}
			}
		} catch (Exception e) {
			// Let the user know what went wrong.
			Debug.Log (e.Message);
		}
	}
}

