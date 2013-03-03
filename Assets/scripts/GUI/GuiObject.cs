using UnityEngine;
using System.Collections;

//A general GUI Object
public class GuiObject : MonoBehaviour
{
	protected Rect position; //position & size on screen
	
	public string guiName;
	
	//where in the screen should the object appear?
	public float xPercentScreen;
	public float yPercentScren;
	
	//how much of the screen should the object take up?
	public float widthPercentScreen;
	public float heightPercentScreen;

	// Use this for initialization
	protected virtual void Start ()
	{
		setPositionAndSize();
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	protected virtual void handleGUI() {
		Debug.Log("Calling handleGUI in GuiObject!");
	}
	
	void setPositionAndSize ()
	{
		float width = widthPercentScreen * Screen.width;
		float height = heightPercentScreen * Screen.height;
		float x = xPercentScreen * Screen.width;
		float y = yPercentScren * Screen.height;
		
		position = new Rect (x,y,width,height);
	}
	
	//handle the object display
	void OnGUI() {
		handleGUI();
	}
}

