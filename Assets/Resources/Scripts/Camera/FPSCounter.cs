using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
	#region Private Attributes
	private float deltaTime;
	private int w;
	private int h;
	private float msec;
	private float fps;
	private string text;
	private Rect rect;
	private GUIStyle style;
	#endregion
	
	#region Main Attributes
	private void Start()
	{
		deltaTime = 0.0f;
		msec = 0.0f;
		fps = 0.0f;
		w = Screen.width;
		h = Screen.height;		
		text = "";
		
		style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (1.0f, 0.0f, 0.0f, 1.0f);
		
		rect = new Rect(0, 0, w, h * 2 / 100);
	}
	
	private void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		
	}
	#endregion
	
	#region GUI Methods
	private void OnGUI()
	{
		msec = deltaTime * 1000.0f;
		fps = 1.0f / deltaTime;
		text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
	#endregion
}