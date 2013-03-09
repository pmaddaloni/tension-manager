using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public struct choiceNode {
		public string label;
		public string description;
		public string successText;
		public string failureText;
		public int impactAmount;
		public float challengeRate;
	}

public struct randomEventNode {
	public string description;
	public int impactAmount;
}

public class LogicManager : MonoBehaviour
{
	
	//Da GUI
	public GUIManager gui;
	
	//The Tension Manager
	TensionManager tM;
	
	//-----------------------------CONFIGURABLE PARAMETERS----------------------//
	
	//How many seconds in the game?
	private float gameLength = 10;
	
	//how many seconds per choice?
	private int choiceLength = 5;
	
	float minTension = 0;
	float maxTension = 100;
	
	//how many choices
	private int numChoices = 3;
	
	//what is the current distance from the edge? Initialized here for every game
	private int jumperDist = 5;
	public int JumperDist {
		get{return jumperDist;}
	}
	
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
	
	private string begin = "Click to begin the game.";
	
	//-------------------------CHOICE MEMBERS---------------------------//
	
	//the array of choice options 

	private choiceNode[] choices;
	public string[] scenes = new string[20];
	public randomEventNode[] randomScenes = new randomEventNode[5];
	public List<choiceNode>[] choiceList = new List<choiceNode>[6];
	public string sceneText; //the scene description
	
	private int currentChoiceClass = 0;

	//---------------------------METHODS-----------------------------------------//	
	void Start () {
		choiceStartTime = Time.timeSinceLevelLoad; //initialize the choice timer
		choices = new choiceNode[numChoices];
		Parser parser = gameObject.AddComponent<Parser>();
		parser.parseScenes("Scenes.txt", scenes);
		parser.parseChoices("Choices.txt", choiceList);
		parser.parseRandomEvents("RandomEvents.txt", randomScenes);
	
		sceneText = scenes[0] + "\n" + begin;
		
		tM = gameObject.AddComponent<TensionManager>();
		tM.init(gameLength, "tensionLevels.txt", minTension, maxTension);
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
		tM.getImportanceLevel(gameTimeRemaining);
		handleChoiceTimer ();
	}
	
	//Start the game -- initialize timers
	void startGame ()
	{
		sceneText = sceneText.Remove(sceneText.IndexOf(begin),begin.Length);
		gameStarted = true;
		gameStartTime = Time.timeSinceLevelLoad;
		choiceStartTime = Time.timeSinceLevelLoad;
		setChoices();//initialize the choices
		updateGameStatus (); //initialize the game status
		//seed the random number generator
		UnityEngine.Random.seed = (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		gui.activateGameGUI();//start up the GUI
	}
	
	//set the current choices
	void setChoices() {
		List<choiceNode> tmp = new List<choiceNode>(choiceList[currentChoiceClass]);
		for (int i = 0; i < numChoices; i++) 
		{
			choices[i] = new choiceNode();
			int choice = UnityEngine.Random.Range(0, tmp.Count);
			
			choices[i].label = tmp[choice].label;
			choices[i].challengeRate = tmp[choice].challengeRate;
			choices[i].impactAmount = tmp[choice].impactAmount;
			choices[i].description = tmp[choice].description;
			choices[i].successText = tmp[choice].successText;
			choices[i].failureText = tmp[choice].failureText;
			tmp.RemoveAt(choice);
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
		gui.setGameStatus ("The man is " + jumperDist + " steps from the edge.");
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
			sceneText += "\n" + choice.successText;
		}
		else {
			jumperDist -= choice.impactAmount;
			sceneText += "\n" + choice.failureText;
		}
		
		sceneText += "\n\n Make your move.";
		
		updateGameStatus();//tell the GUI to update the game status
		
		setChoices();//updates the choice list
	}
}
