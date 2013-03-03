using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	
	SceneText sceneDescription; //the main description
	SceneText timer;
	SceneText gameStatus;
	
	//the logic manager
	public LogicManager logic;
		
	//the user choices
	ChoiceButtons choices;
	
	void Start () {
		SceneText[] texts = gameObject.GetComponents<SceneText>();
		sceneDescription = texts[0];
		timer = texts[1];
		gameStatus = texts[2];
		initText();//Initialize the text areas
		
		choices = gameObject.GetComponent<ChoiceButtons>();
		
		
	}
	
	void initText() {
		sceneDescription.Text = "scene desc";
		timer.Text = "timer";
		gameStatus.Text = "gameStatus";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
