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
	private float gameLength = 15;
	
	//how many seconds per choice?
	private int choiceLength = 15;
	
	//how many choices
	private int numChoices = 3;
	
	//Measured in steps from the edge -- just like jumperDist
	int successDist = 11;
	int failDist = 0;
	
	//what is the current distance from the edge? Initialized here for every game
	private int jumperDist = 5;
	public int JumperDist {
		get{return jumperDist;}
	}
	
	//max allowable impact of a choice
	private int maxImpact = 10;
	
	//--------------------------TIMER VARIABLES--------------------------------//
	//when does the user start the game?
	private float gameStartTime;
	
	//Has the game started yet?
	private bool gameStarted = false;
	public bool GameStarted {
		get { return gameStarted;}
	}
	
	private bool gameEnded = false;
	public bool GameEnded {
		get {return GameEnded;}
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
	public int amountToThrottle = 0;
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
				
		if (randomScenes.Length > 0)
			tensionManager.setThrottle(true);
		
		tensionManager.init(gameLength, "tensionLevels.txt", failDist, successDist);
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
			else if (tensionManager.checkIfThrottlingNecessary(amountToThrottle))
			{
				updateGameStatusThrottle();
			}
			else {
				//updateGameTimeRemaining();
				updateGameStatus();
			}
		}
	}
	
	//Start the game -- initialize timers
	void startGame ()
	{
		sceneText = sceneText.Remove(sceneText.IndexOf(begin),begin.Length);
		choiceTimeRemaining = choiceLength;
		gameStarted = true;
		gameStartTime = Time.timeSinceLevelLoad;
		choiceStartTime = Time.timeSinceLevelLoad;
		updateGameTimeRemaining();
		setChoices();//initialize the choices
		updateGameStatus (); //initialize the game status
		//seed the random number generator
		UnityEngine.Random.seed = (int)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		gui.activateGameGUI();//start up the GUI
	}

	//determine which class of choices to draw the choice text from method
	private int determineChoiceClass (float determine)
	{
		if (determine >= 0.6)
			return 5;
		else if (determine < 0.6 && determine >= 0.5)
			return 4;
		else if (determine < 0.5 && determine >= 0.4)
			return 3;
		else if (determine < 0.4 && determine >= 0.3)
			return 2;
		else if (determine < 0.3 && determine >= 0.2)
			return 1;
		else
			return 0;
	}
	
	//set the current choices
	void setChoices() {
		
		//if the tensionManager has determined that a random event should take place to get
		//tension to a desired level
		if (tension.randomEvent.Key)
		{
			jumperDist += Convert.ToInt32(Math.Round(tension.randomEvent.Value));//calculate new jumper distance
			tension.randomEvent = new KeyValuePair<bool,float>(false,0);	//reset the randomEvent	
		}
		
		tensionManager.updateTension(gameTimeRemaining, jumperDist, tension);
		
		//determine what class of choices should be available to the user
		for (int i = 0; i < numChoices; i++)
		{		
			if (tension.successImpacts[i] >= 5 || tension.failureImpacts[i] >= 5)
				currentChoiceClass[i] = 5;
			else{
				if ( Convert.ToInt32(tension.successImpacts[i]) > Convert.ToInt32(tension.failureImpacts[i]) )
					currentChoiceClass[i] = determineChoiceClass(tension.successImpacts[i]/maxImpact);
				else
					currentChoiceClass[i] = determineChoiceClass(tension.failureImpacts[i]/maxImpact);
			}
		}
		
		List<choiceNode>[] choice = new List<choiceNode>[3];	//the three choices that will be inserted into the choice buttons
		
		try{
			
		for (int i = 0; i < numChoices; i++) 
		{
			//ensure that no duplicate choices appear to the user by checking if a chosen class has already been used in 
			//a previous choice butotn
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
			
			//choices set for the actual buttons
			choices[i] = new choiceNode();
			int randomChoice = UnityEngine.Random.Range(0, choice[i].Count);
			
			choices[i].label = choice[i][randomChoice].label;
			choices[i].challengeRate = choice[i][randomChoice].challengeRate;
			choices[i].impactAmount = choice[i][randomChoice].impactAmount;
			choices[i].description = choice[i][randomChoice].description;
			
				//debug info:
			/*choices[i].label += "Success:" + tension.successImpacts[i] + " Failure:" 
					+ tension.failureImpacts[i] + " Challenge:" + tension.challengeLevels[i];*/
				
			choices[i].label += "Success: " + Math.Ceiling(tension.successImpacts[i]) +
					(Math.Ceiling(tension.successImpacts[i]) == 1 ? " step": " steps") + " toward safety. Failure: " +
					Math.Ceiling(tension.failureImpacts[i]) + 
						(Math.Ceiling(tension.successImpacts[i]) == 1 ? " step": " steps") 
						+ " toward demise. Liklihood of success is " + (tension.challengeLevels[i]*100).ToString ("#") + "%";
						
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
			if (Input.anyKeyDown)
			{
				sceneText = scenes[3];
				gameTimeRemaining = 0;
				choiceTimeRemaining = 0;	
			}		
		}
		else if (jumperDist < 1) {
			gui.setGameStatus ("The man has reached the edge of the ledge.");
			if (Input.anyKeyDown)
			{
				sceneText = scenes[2];
				gameTimeRemaining = 0;
				choiceTimeRemaining = 0;	
			}
		}
		else if (gameTimeRemaining <= 0) {
			gui.setGameStatus ("Time is up.");
			if (Input.anyKeyDown)
			{
				sceneText = scenes[1];
				gameTimeRemaining = 0;
				choiceTimeRemaining = 0;	
			}
		}
		else {
			gui.setGameStatus ("The man is " + jumperDist + " steps from the edge.");
		}
	}
	
	private void updateGameStatusThrottle ()
	{
		int i = UnityEngine.Random.Range(0,randomScenes.Length);
		sceneText = randomScenes[i].description;
		jumperDist -= amountToThrottle;
		gui.setGameStatus ("The man is " + jumperDist + " steps from the edge.");
	}
	
	void updateGameTimeRemaining() {
		
		float timePassed = Time.timeSinceLevelLoad - gameStartTime;
		//if (gameTimeRemaining != 0)
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
			//if (gameTimeRemaining > 0)
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
			jumperDist += Convert.ToInt32(Math.Round(tension.successImpacts[gui.getChosenID()]));
			sceneText += "\n" + choice.successText;
		}
		else {
			jumperDist -= Convert.ToInt32(Math.Round(tension.failureImpacts[gui.getChosenID()]));
			sceneText += "\n" + choice.failureText;
		}
						
		if (tension.randomEvent.Key)
			sceneText += "\n\n" + randomScenes[UnityEngine.Random.Range(0,randomScenes.Length-1)];	
		
		if (jumperDist < 1 || jumperDist > 10 || gameTimeRemaining <= 0)
		{
			sceneText += "\n\n Press any key to see the ending";
			for (int i = 0; i < numChoices ; i++)
				choices[i].label = "";
			setGUIChoiceStrings();
		}
		else	
			sceneText += "\nMake your move.";
		
		updateGameStatus();//tell the GUI to update the game status
		setChoices();//updates the choice list
	}
}
