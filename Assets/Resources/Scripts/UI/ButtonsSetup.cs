using UnityEngine;
using System.Collections;

public class ButtonsSetup: MonoBehaviour 
{
	#region Public Attributes
	
	#endregion
	
	#region Private Attributes
	
	#endregion
	
	#region References
	private DataManager dataManager;
	#endregion
	
	#region Main Methods
	private void Start () 
	{ 
		dataManager = DataManager.Instance;
	}
	
	private void LateUpdate () 
	{
		transform.GetChild (0).gameObject.SetActive (!dataManager.isGamepad);
		transform.GetChild (1).gameObject.SetActive (dataManager.isGamepad);
	}
	#endregion
}