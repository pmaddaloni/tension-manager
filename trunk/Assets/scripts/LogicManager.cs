using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

//holds all the information in a choice
public struct choiceNode {
		public string label;
		public string description;
		public string successText;
		public string failureText;
		public int impactAmount;
		public float challengeRate;
	}

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
	public string[] scenes = new string[20];
	public List<choiceNode>[] choiceList = new List<choiceNode>[6];
	public string sceneText; //the scene description

	
	
	//---------------------------METHODS-----------------------------------------//	
	void Start () {
		choiceStartTime = Time.timeSinceLevelLoad; //initialize the choice timer
		choices = new choiceNode[numChoices];
		Parser parser = gameObject.AddComponent<Parser>();
		parser.parseScenes("Scenes.txt", scenes);
		parser.parseChoices("Choices.txt", choiceList);
	
		sceneText = scenes[0];
		
		TensionManager tension = gameObject.AddComponent<TensionManager>();
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
		setChoices();//initialize the choices
		updateGameStatus (); //initialize the game status
		gui.activateGameGUI();//start up the GUI
		//seed the random number generator
		UnityEngine.Random.seed = (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
	}
	
	//set the current choices
	void setChoices() {
		for (int i = 0; i < numChoices; i++) {
			choices[i] = new choiceNode();
			
			//just using dummy data - the logic for pulling these from the doc and setting levels 
			//should hapen here
			choices[i].label = ("choiceNode " + i);
			choices[i].challengeRate = .5f;
			choices[i].impactAmount = 1;
			choices[i].description = "You valiantly try to do thing " + i + "\n";
			choices[i].successText = "You succeed at thing " + i + ".\n The crazy mofo moves " 
				+ choices[i].impactAmount + " steps away from the ledge.\n";
			choices[i].failureText = "Ah, crap. You fail miserably at thing " + i + ".\n The crazy mofo moves " 
				+ choices[i].impactAmount + " steps towards from the ledge.\n";
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
	void updateGameStatus ()
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
		float attempt = UnityEngine.Random.Range(0f,1f);
		bool success = attempt >= choice.challengeRate;
		
		//the description for the next scene
		sceneText = choice.description;
		
		if (success) {
			jumperDist += choice.impactAmount;
			sceneText += choice.successText;
		}
		else {
			jumperDist -= choice.impactAmount;
			sceneText += choice.failureText;
		}
		
		sceneText += "\n\n More Stuff happens. What do you want to do? \n";
		
		updateGameStatus();//tell the GUI to update the game status
		
		setChoices();//updates the choice list
	}
}
