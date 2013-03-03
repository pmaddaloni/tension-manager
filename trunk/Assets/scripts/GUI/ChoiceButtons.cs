using UnityEngine;
using System.Collections;

public class ChoiceButtons : GuiObject
{
	
	//A 2d list containing text choices, by level
	private ArrayList choiceLevels = new ArrayList ();
	private int numChoices = 3;
	
	//Which choice is selected in the grid?
	private int choiceSelected = 0;
	private string[] choiceText;
	
	// Use this for initialization
	protected override void Start ()
	{
		base.Start();
		choiceText = new string[numChoices];
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	//manage displaying this object
	protected override void handleGUI() {
		//build string array
		for (int i = 0; i < numChoices; i++) {
			choiceText [i] = "Choice " + i;		
		}
		choiceSelected = GUI.SelectionGrid (position, choiceSelected, choiceText, 1);
	}
}
