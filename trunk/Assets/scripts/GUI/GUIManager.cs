using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	
	SceneText sceneDescription; //the main description
	SceneText timer; //the overall game timer
	SceneText gameStatus; //the win status of the game
	SceneText choiceTimer; //the timer before a choice is selected
	
	//the logic manager
	public LogicManager logic;
		
	//the user choices
	ChoiceButtons choices;
	
	void Start () {
		//initialize the components
		SceneText[] texts = gameObject.GetComponents<SceneText>();
		sceneDescription = texts[0];
		timer = texts[1];
		gameStatus = texts[2];
		choiceTimer = texts[3];
		choices = gameObject.GetComponent<ChoiceButtons>();
	}
	
	
	// Update is called once per frame
	void Update () {
		sceneDescription.Text = logic.scenes[0];
		//update the game timer
		timer.Text = getGameTime();
		choiceTimer.Text = "Choice Selected in: \n" + logic.ChoiceTimeRemaining.ToString();
	}
	
	//set the choices in the buttons
	public void setChoices(string[] choiceStrings) {
		choices.ChoiceStrings = choiceStrings;
	}
	
	public void setGameStatus(string status) {
		gameStatus.Text = status;
	}
	
	//return the ID of the selected choice
	public int getChosenID() {
		return choices.ChoiceSelected;
	}
		
	//returns the game time in a pretty clock-like string
	string getGameTime() {
		int time = (int)logic.GameTimeRemaining;
		
		int minutes = time/60;
		int seconds = time % 60;
		return ("Time Remaining\n" + minutes + ": " + seconds);
	}
}
