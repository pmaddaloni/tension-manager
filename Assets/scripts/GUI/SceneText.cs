using UnityEngine;
using System.Collections;

public class SceneText : GuiObject {
	
	//the text string
	private string text;
	public string Text {
		get {return text;}
		set {text = value;}
	}
	
	// Use this for initialization
	protected override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	protected override void handleGUI ()
	{
		GUI.TextArea(position, text);
	}
}
