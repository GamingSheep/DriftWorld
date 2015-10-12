using UnityEngine;
using System.Collections;

public class DetailManager: MonoBehaviour 
{
	#region Public Attributes
	[Header("Manager Attributes")]
	public float changeDistance;
	
	[Header("References")]
	public GameObject[] lowObjects;
	public GameObject[] highObjects;
	#endregion
	
	#region Private Attributes
	private bool automatic;
	private Vector3 distanceVector;
	private float currentDistance;
	private bool isInRange; // false = low, true = high
	private bool initDone;
	#endregion
	
	#region References
	private Transform playerTransform;
	#endregion
	
	#region Main Methods
	private void Awake()
	{
		initDone = false;
		
		for(int i = 0; i < lowObjects.Length; i++)
		{
			lowObjects[i].SetActive (true);
		}
		
		for(int i = 0; i < highObjects.Length; i++)
		{
			highObjects[i].SetActive (true);
		}
	}
	private void Start () 
	{		
		// Check object references
		if(lowObjects.Length != highObjects.Length)
		{
			Debug.Log ("DetailManager: the low objects count and high objects count don't match");
			this.enabled = false;
		}
	}
	
	private void LateUpdate () 
	{
		if(playerTransform == null && GameObject.FindWithTag ("MainCamera"))
		{			
			// Get references
			playerTransform = GameObject.FindWithTag ("MainCamera").transform;
		}
		else
		{
			if(!initDone)
			{
				initDone = true;
				Invoke ("SetUpObjects", 0.25f);
			}
		}
		
		if(automatic && playerTransform != null)
		{
			distanceVector = transform.position - playerTransform.position;
			
			currentDistance = Mathf.Abs (distanceVector.sqrMagnitude);
			
			if(currentDistance < changeDistance)
			{
				if(!isInRange)
				{
					SetHighObjects();
				}
			}
			else
			{
				if(isInRange)
				{
					SetLowObjects();
				}
			}
		}
	}
	#endregion
	
	#region Detail Methods
	private void SetUpObjects()
	{
		switch(Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsWebPlayer:
			{
				automatic = true;
				break;
			}
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.BlackBerryPlayer:
			case RuntimePlatform.Android:
			{
				automatic = false;
				break;
			}
			default:
			{
				automatic = false;
				break;
			}
		}
		
		SetLowObjects();
	}
	
	private void SetHighObjects()
	{
		isInRange = true;
		for(int i = 0; i < lowObjects.Length; i++)
		{
			lowObjects[i].SetActive (false);
		}
		
		for(int i = 0; i < highObjects.Length; i++)
		{
			highObjects[i].SetActive (true);
		}
	}
	
	private void SetLowObjects()
	{
		isInRange = false;
		for(int i = 0; i < highObjects.Length; i++)
		{
			highObjects[i].SetActive (false);
		}
		
		for(int i = 0; i < lowObjects.Length; i++)
		{
			lowObjects[i].SetActive (true);
		}
	}
	#endregion
}