using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LogicManager : MonoBehaviour {
	
	public GUIManager gui;
	
	//How many seconds in the game?
	private float gameLength = 300;
	
	//how many seconds per choice?
	private int choiceLength = 15;
	
	//when did the current choice start?
	private float choiceStartTime;
		
	//the time the game has been running
	private float gameTimeRemaining;
	public float GameTimeRemaining{
		get {return gameTimeRemaining;}
	}
	
	//how long until the current choice is selected?
	private int choiceTimeRemaining;
	public int ChoiceTimeRemaining {
		get {return choiceTimeRemaining;}
	}
	
	//how many choices
	private int numChoices = 3;
	
	//the array of choice options 
	private string[] choiceStrings;	
	
	public string[] scenes = new string[20];
	public IList[] choices = new IList[20];
	
	void Start () {
		choiceStartTime = Time.timeSinceLevelLoad; //initialize the choice timer
		choiceStrings = new string[numChoices];
		initChoiceStrings();//initialize the choice strings
		
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
		gameTimeRemaining = gameLength - Time.timeSinceLevelLoad;
		handleChoiceTimer();
	}
	
	void initChoiceStrings() {
		for (int i = 0; i < numChoices; i++) {
			choiceStrings[i] = ("logicChoice " + i);
		}
		gui.setChoices(choiceStrings);//set the choices in the gui
	}
	
	/*
	 * update the choice time
	 * If time's up, handle the choice
	 * */
	void handleChoiceTimer() {
		float currTime = Time.timeSinceLevelLoad;
		float choiceTimePassed = currTime - choiceStartTime;
		
		if(choiceTimePassed >= choiceLength) {
			choiceTimeRemaining = choiceLength;
			choiceStartTime = currTime;
			handleChoice();
		} else choiceTimeRemaining = choiceLength - (int)choiceTimePassed;
	}
	
	//handle a user choice
	void handleChoice() {
		//set new choices here after dealing with repercussions
	}
}
