using UnityEngine;
using System.Collections;

public class ChoiceButtons : GuiObject
{
	//the choices to display
	private string[] choiceStrings;
	public string[] ChoiceStrings {
		set {choiceStrings = value;}
	}
	
	//Which choice is selected in the grid?
	private int choiceSelected = 0;
	public int ChoiceSelected {
		get{return choiceSelected;}
	}
	
	// Use this for initialization
	protected override void Start ()
	{
		base.Start();
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	//manage displaying this object
	protected override void handleGUI() {
		//build string array
		choiceSelected = GUI.SelectionGrid (position, choiceSelected, choiceStrings, 1);
	}
}
