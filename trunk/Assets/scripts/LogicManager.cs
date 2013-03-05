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
	private string[] choiceStrings;
	public string[] scenes = new string[20];
	public IList[] choices = new IList[20];
	
	
	//---------------------------METHODS-----------------------------------------//	
	void Start ()
	{
		choiceStrings = new string[numChoices];
				
		try {			
			// Create an instance of StreamReader to read from a file. 
			// The using statement also closes the StreamReader. 
			using (StreamReader sr = new StreamReader("TheStorySoFar.txt")) {
				string line;
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((line = sr.ReadLine()) != null) {
					//Debug.Log(line);
					if ((line.Trim ()).Equals ("Intro:")) {
						line = sr.ReadLine ();
						while (!(line.Trim()).Equals("<End Of Scene>")) {
							scenes [0] += line;
							line = sr.ReadLine();
						}
					}
				}
			}
		} catch (Exception e) {
			// Let the user know what went wrong.
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
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
		gameStarted = true;
		gameStartTime = Time.timeSinceLevelLoad;
		choiceStartTime = Time.timeSinceLevelLoad;
		setChoiceStrings ();//initialize the choice strings
		setGameStatus (); //initialize the game status
	}
	
	void setChoiceStrings ()
	{
		//Insert logic for choosing the choice strings here
		for (int i = 0; i < numChoices; i++) {
			choiceStrings [i] = ("logicChoice " + i);
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
		Debug.Log (gui.getChosenID ());
	}
}
