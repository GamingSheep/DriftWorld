using UnityEngine;
using System.Collections;

public class SparksManager: MonoBehaviour 
{
	#region Public Attributes
	
	#endregion
	
	#region Private Attributes

	#endregion
	
	#region References
	private GameObject[] sparks;
	#endregion
	
	#region Main Methods
	private void Start()
	{
		// Initialize values
		sparks = new GameObject[transform.childCount];
		for(int i = 0; i < sparks.Length; i++)
		{
			sparks[i] = transform.GetChild (i).gameObject;
		} 
	}
	#endregion
	
	#region Pool Methods
	public void ActiveSpark(Vector3 destinyPosition, Quaternion destinyRotation)
	{
		for(int i = 0; i < sparks.Length; i++)
		{
			if(!sparks[i].activeSelf)
			{
				sparks[i].SetActive (true);
				sparks[i].transform.position = destinyPosition;
				sparks[i].transform.rotation = destinyRotation;
				break;
			}
		}
	}
	#endregion
}