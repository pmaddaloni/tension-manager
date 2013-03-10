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

public struct tensionStruct {	
	public float[] challengeLevels;
	public float[] successImpacts;
	public float[] failureImpacts;
	public KeyValuePair<bool, float> randomEvent;
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
	TensionManager tensionManager;
	
	//-----------------------------CONFIGURABLE PARAMETERS----------------------//
	
	//How many seconds in the game?
	private float gameLength = 100;
	
	//how many seconds per choice?
	private int choiceLength = 15;
	
	//how many choices
	private int numChoices = 3;
	
	float minTension = 0;
	float maxTension = 100;
	
	//Measured in steps from the edge -- just like jumperDist
	int successDist = 11;
	int failDist = 0;
	
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
	
	private int[] currentChoiceClass = new int[3];
	
	public tensionStruct tension = new tensionStruct();


	//---------------------------METHODS-----------------------------------------//	
	void Start () {
		choiceStartTime = Time.timeSinceLevelLoad; //initialize the choice timer
		choices = new choiceNode[numChoices];
		Parser parser = gameObject.AddComponent<Parser>();
		parser.parseScenes("Scenes.txt", scenes);
		parser.parseChoices("Choices.txt", choiceList);
		parser.parseRandomEvents("RandomEvents.txt", randomScenes);
		
		tension.challengeLevels = new float[3];
		tension.successImpacts = new float[3];
		tension.failureImpacts = new float[3];
		tension.randomEvent = new KeyValuePair<bool, float>(false,0);
	
		sceneText = scenes[0] + "\n" + begin;
		
		tensionManager = gameObject.AddComponent<TensionManager>();
		tensionManager.init(gameLength, "tensionLevels.txt", minTension, maxTension, failDist, successDist);
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
		else {
			if (gameTimeRemaining > 0) {
				updateGameTimeRemaining();
				handleChoiceTimer ();
			}
			else {
				updateGameTimeRemaining();
			}
		}
	}
	
	//Start the game -- initialize timers
	void startGame ()
	{
		sceneText = sceneText.Remove(sceneText.IndexOf(begin),begin.Length);
		gameStarted = true;
		gameStartTime = Time.timeSinceLevelLoad;
		choiceStartTime = Time.timeSinceLevelLoad;
		setChoices();//initialize the choices
		updateGameTimeRemaining();
		updateGameStatus (); //initialize the game status
		//seed the random number generator
		UnityEngine.Random.seed = (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		gui.activateGameGUI();//start up the GUI
	}
	
	//set the current choices
	void setChoices() {
		
		if (tension.randomEvent.Key)
		{
			jumperDist += Convert.ToInt32(tension.randomEvent.Value);
			tension.randomEvent = new KeyValuePair<bool,float>(false,0);	
		}

		tensionManager.updateTension(gameTimeRemaining, jumperDist, tension);
		
		currentChoiceClass[0] = Math.Abs(jumperDist + Convert.ToInt32(tension.successImpacts[0]));
		currentChoiceClass[1] = Math.Abs(jumperDist + Convert.ToInt32(tension.successImpacts[1]));
		currentChoiceClass[2] = Math.Abs(jumperDist + Convert.ToInt32(tension.successImpacts[2]));
		
		List<choiceNode>[] choice = new List<choiceNode>[3];
		
		try{
			
		for (int i = 0; i < numChoices; i++) 
		{
			switch (i)
			{
			case 0:
				print (i + " = " + currentChoiceClass[i]);
				choice[i] = new List<choiceNode>(choiceList[currentChoiceClass[i]]);
				break;
			case 1:
				print (i + " = " + currentChoiceClass[i]);
				if (currentChoiceClass[i] == currentChoiceClass[i-1])
					choice[i] = choice[i-1];
				else
					choice[i] = new List<choiceNode>(choiceList[currentChoiceClass[i]]);
				break;
			case 2:
				print (i + " = " + currentChoiceClass[i]);
				if (currentChoiceClass[i] == currentChoiceClass[i-1])
					choice[i] = choice[i-1];
				else if (currentChoiceClass[i] == currentChoiceClass[i-2])
					choice[i] = choice[i-2];
				else 
					choice[i] = new List<choiceNode>(choiceList[currentChoiceClass[i]]);
				break;
			default:
				choice[i] = new List<choiceNode>(choiceList[currentChoiceClass[i]]);
				break;
			}
			
			choices[i] = new choiceNode();
			int randomChoice = UnityEngine.Random.Range(0, choice[i].Count);
			
			choices[i].label = choice[i][randomChoice].label;
			choices[i].challengeRate = choice[i][randomChoice].challengeRate;
			choices[i].impactAmount = choice[i][randomChoice].impactAmount;
			choices[i].description = choice[i][randomChoice].description;
				
			choices[i].label += "Success:" + tension.successImpacts[i] + " Failure:" 
					+ tension.failureImpacts[i] + " Challenge:" + tension.challengeLevels[i];
						
			choices[i].successText = choice[i][randomChoice].successText;
			choices[i].failureText = choice[i][randomChoice].failureText;
			choice[i].RemoveAt(randomChoice);
		}
		}
		catch(Exception e)
		{
			Debug.Log (e.Message);
			//Application.Quit();
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
		if (jumperDist > 10) {
			gui.setGameStatus ("You've successfully gotten the man to come down off the ledge.");
			sceneText = scenes[3];
			gameTimeRemaining = 0;
			choiceTimeRemaining = 0;			
		}
		else if (jumperDist < 1) {
			gui.setGameStatus ("The man has reached the edge of the ledge.");
			sceneText = scenes[2];
			gameTimeRemaining = 0;
			choiceTimeRemaining = 0;
		}
		else if (gameTimeRemaining <= 0) {
			gui.setGameStatus ("Time is up.");
			sceneText = scenes[1];
			gameTimeRemaining = 0;
			choiceTimeRemaining = 0;
		}
		else {
			gui.setGameStatus ("The man is " + jumperDist + " steps from the edge.");
		}
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
		} 
		else
			choiceTimeRemaining = choiceLength - (int)choiceTimePassed;
	}
	
	//handle a user choice
	void handleChoice ()
	{
		//set new choices here after dealing with repercussions
		choiceNode choice = choices[gui.getChosenID()];
		float attempt = UnityEngine.Random.Range(0f,1f);
				
		bool successful;
		
		switch (gui.getChosenID())
		{
		case 0:
			successful = attempt >= tension.challengeLevels[0];
			break;
		case 1:
			successful = attempt >= tension.challengeLevels[1];
			break;
		case 2:
			successful = attempt >= tension.challengeLevels[2];
			break;
		default:
			successful = false;
			break;
		}
		
		
		//the description for the next scene
		sceneText = choice.description;
		
		if (successful) {
			jumperDist += Convert.ToInt32(tension.successImpacts[gui.getChosenID()]);
			sceneText += "\n" + choice.successText;
		}
		else {
			jumperDist -= Convert.ToInt32(tension.failureImpacts[gui.getChosenID()]);
			sceneText += "\n" + choice.failureText;
		}
						
		if (tension.randomEvent.Key)
			sceneText += "\n\n" + randomScenes[UnityEngine.Random.Range(0,randomScenes.Length-1)];	
			
		sceneText += "\n\nMake your move.";
		
		updateGameStatus();//tell the GUI to update the game status
		
		setChoices();//updates the choice list
	}
}
