using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LogicManager : MonoBehaviour
{
	
	//Da GUI
	public GUIManager gui;
	
	//-----------------------------CONFIGURABLE PARAMETERS----------------------//
	
	//How many seconds in the game?
	private float gameLength = 300;
	
	//how many seconds per choice?
	private int choiceLength = 5;
	
	//--------------------------TIMER VARIABLES--------------------------------//
	//when does the user start the game?
	private float gameStartTime;
	
	//Has the game started yet?
	private bool gameStarted = false;
	public bool GameStarted {
		get { return gameStarted;}
	}
	
	//when did the current choice start?
	private float choiceStartTime;
		
	//the time the game has been running
	private float gameTimeRemaining;
	public float GameTimeRemaining {
		get { return gameTimeRemaining;}
	}
	
	//how long until the current choice is selected?
	private int choiceTimeRemaining;
	public int ChoiceTimeRemaining {
		get { return choiceTimeRemaining;}
	}
	
	//-------------------------GAME STATE VARIABLES---------------------------//
	
	//how many choices
	private int numChoices = 3;
	
	//what is the current distance from the edge?
	private int jumperDist = 5;
	public int JumperDist {
		get{return jumperDist;}
	}

	//-------------------------CHOICE MEMBERS---------------------------//
	
	//the array of choice options 

	private choiceNode[] choices;
	public struct choiceNode {
		public string label;
		public string description;
		public string successText;
		public string failureText;
		public int impactAmount;
		public int challengeRate;
	}
			
	public string[] scenes = new string[20];
	public IList[] choiceList = new IList[5];
	
	
	//---------------------------METHODS-----------------------------------------//	
	void Start () {
		choiceStartTime = Time.timeSinceLevelLoad; //initialize the choice timer
		choices = new choiceNode[numChoices];
						
		try {			
			// Create an instance of StreamReader to read from a file. 
            // The using statement also closes the StreamReader. 
			
			//Read Scenes
            using (StreamReader sr = new StreamReader("Scenes.txt")) 
            {
                string line;
				int sceneCounter = 0;
                // Read and display lines from the file until the end of  
                // the file is reached. 
                while ((line = sr.ReadLine()) != null) 
                {
                    //Debug.Log(line);
					if ( (line.Trim()).Equals("<Intro>") )
					{
						line = sr.ReadLine();
						while( !(line.Trim()).Equals("<End>") )
						{
							scenes[sceneCounter] += line + '\n';
							line = sr.ReadLine();
						}
						sceneCounter++;
					}
					
					else {
						line = sr.ReadLine();						
						if ((line.Trim()).Equals("<Scene" + sceneCounter + ">"))
						{
	
							line = sr.ReadLine();
							while( !(line.Trim()).Equals("<End>") && sceneCounter < 20)
							{
								print(line);
								scenes[sceneCounter] += line;
								line = sr.ReadLine();
							}
							sceneCounter++;
						}			
					}
                }
            }
			
			//Read Choices
			/*using (StreamReader sr = new StreamReader("Choices.txt")) 
            {
                string line;
                // Read and display lines from the file until the end of  
                // the file is reached. 
                while ((line = sr.ReadLine()) != null) 
                {
                    //Debug.Log(line);
					if ( (line.Trim()).Equals("<Intro>") )
					{
						line = sr.ReadLine();
						while( !(line.Trim()).Equals("<End>") )
						{
							scenes[sceneCounter] += line + '\n';
							line = sr.ReadLine();
						}
						sceneCounter++;
					}
					
					else {
						line = sr.ReadLine();						
						if ((line.Trim()).Equals("<Scene" + sceneCounter + ">"))
						{
	
							line = sr.ReadLine();
							while( !(line.Trim()).Equals("<End>") && sceneCounter < 20)
							{
								print(line);
								scenes[sceneCounter] += line;
								line = sr.ReadLine();
							}
							sceneCounter++;
						}			
					}
                }
            }
		}*/
		}
		catch (Exception e)
		{
            // Let the user know what went wrong.
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!gameStarted) {
			//start the game!
			if (Input.anyKeyDown)
				startGame ();
			return;
		}
		
		updateGameTimeRemaining();
		handleChoiceTimer ();
	}
	
	//Start the game -- initialize timers
	void startGame ()
	{
		gui.activateGameGUI();//start up the GUI
		gameStarted = true;
		gameStartTime = Time.timeSinceLevelLoad;
		choiceStartTime = Time.timeSinceLevelLoad;
		setChoices();//initialize the choices
		setGameStatus (); //initialize the game status
		//seed the random number generator
		UnityEngine.Random.seed = (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
	}
	
	//set the current choices
	void setChoices() {
		for (int i = 0; i < numChoices; i++) {
			choices[i] = new choiceNode();
			choices[i].label = ("choiceNode " + i);
		}
		setGUIChoiceStrings();
	}
	
	
	//set the choice strings in the GUI
	void setGUIChoiceStrings ()
	{
		//set the strings for the array buttons
		string[] choiceStrings = new string[numChoices];
		for (int i = 0; i < numChoices; i++) {
			choiceStrings[i] = choices[i].label;
		}
		gui.setChoiceStrings(choiceStrings);//update the choices in the gui
	}
	
	//Set the game status in the gui
	void setGameStatus ()
	{
		gui.setGameStatus ("The crazy mofo is " + jumperDist + " steps from the edge");
	}
	
	void updateGameTimeRemaining() {
		
		float timePassed = Time.timeSinceLevelLoad - gameStartTime;
		gameTimeRemaining = gameLength - timePassed;
	}
	
	/*
	 * update the choice time
	 * If time's up, handle the choice
	 * */
	void handleChoiceTimer ()
	{
		float currTime = Time.timeSinceLevelLoad;
		float choiceTimePassed = currTime - choiceStartTime;
		
		if (choiceTimePassed >= choiceLength) {
			choiceTimeRemaining = choiceLength;
			choiceStartTime = currTime;
			handleChoice ();
		} else
			choiceTimeRemaining = choiceLength - (int)choiceTimePassed;
	}
	
	//handle a user choice
	void handleChoice ()
	{
		//set new choices here after dealing with repercussions
		choiceNode choice = choices[gui.getChosenID()];
		bool success = UnityEngine.Random.Range(0,1) >= choice.challengeRate;
		if (success) print ("success!");
			else print ("failure!");
		//handle repercussions of success/failure
		//update game state, etc.
		setChoices();//updates the choice list
	}
}