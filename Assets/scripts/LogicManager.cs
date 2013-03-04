using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class LogicManager : MonoBehaviour {
	
	public string[] scenes = new string[20];
	
	
	void Start () {
		try {			
			// Create an instance of StreamReader to read from a file. 
            // The using statement also closes the StreamReader. 
            using (StreamReader sr = new StreamReader("TheStorySoFar.txt")) 
            {
                string line;
                // Read and display lines from the file until the end of  
                // the file is reached. 
                while ((line = sr.ReadLine()) != null) 
                {
                    //Debug.Log(line);
					if ( (line.Trim()).Equals("Intro:") )
					{
						line = sr.ReadLine();
						while( !(line.Trim()).Equals("<End Of Scene>") )
						{
							scenes[0] += line;
							line = sr.ReadLine();
						}
					}
                }
            }
		}
		catch (Exception e)
		{
            // Let the user know what went wrong.
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
