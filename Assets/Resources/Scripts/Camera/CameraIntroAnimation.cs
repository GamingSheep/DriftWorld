using UnityEngine;
using System.Collections;

public class CameraIntroAnimation: MonoBehaviour 
{
	#region Public Attributes
	public Vector3 wantedPosition1;
	public Vector3 wantedRotation1;
	public Vector3 wantedPosition2;
	public Vector3 wantedRotation2;
	#endregion
	
	#region Private Attributes
	private int _state = 0;
	private Vector3 cameraPosition;
	private Vector3 cameraRotation;
	
	private float scaleValue = 0.3f;
	private float lerpTime = 10f;
	private float currentLerpTime;
	
	private Vector3 startPos1;
	private Vector3 endPos1;	
	private Quaternion startRot1;
	private Quaternion endRot1;
	
	private Vector3 startPos2;
	private Vector3 endPos2;	
	private Quaternion startRot2;
	private Quaternion endRot2;
	
	private float t;
	#endregion
	
	#region References
	private Transform cameraTransform;
	private CarDrifting carDrifting;
	private MultiplayerManager multiplayerManager;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Get references
		cameraTransform = Camera.main.transform;
		carDrifting = GetComponent<CarDrifting>();
		
		if(GameObject.FindWithTag("MultiplayerManager"))
		{
			multiplayerManager = GetComponent<MultiplayerManager>();
		}
		
		// Initialize values
		scaleValue = 0.3f;
		lerpTime = 5f;
		currentLerpTime = 0.0f;
		t = 0.0f;
		startPos1 = cameraTransform.position;
		startRot1 = cameraTransform.localRotation;
		endPos1 = wantedPosition1;
		endRot1 = Quaternion.Euler (wantedRotation1);
		startPos2 = cameraTransform.position;
		startRot2 = cameraTransform.localRotation;
		endPos2 = wantedPosition2;
		endRot2 = Quaternion.Euler (wantedRotation2);
		SetAnimation(1);
	}
	
	private void OnLevelWasLoaded(int level)
	{
		if(DataManager.Instance.isGamepad)
		{
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
		}
	}
	
	private void Update () 
	{
		if(_state == 1)
		{
			//increment timer once per frame
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime) 
			{
				currentLerpTime = lerpTime;
			}
			
			t = currentLerpTime / lerpTime;
			t = t * t * (3f - 2f*t);
			
			cameraTransform.position = Vector3.Lerp(startPos1, endPos1, t);
			cameraTransform.localRotation = Quaternion.Lerp (startRot1, endRot1, t);
			
			if(cameraTransform.position == wantedPosition1)
			{
				_state = 0;
				
				if(multiplayerManager == null)
				{
					// Check for gamepad before active main menu (ActiveMainMenu())
					carDrifting.CheckGamepad();
				}
				else
				{
					multiplayerManager.ActiveMainMenu();
				}
			}
		}
		else if(_state == 2)
		{
			if(Mathf.Sin (Time.time * scaleValue) > 0)
			{
				currentLerpTime += Time.deltaTime;
				if (currentLerpTime > lerpTime) 
				{
					currentLerpTime = lerpTime;
				}
				
				t = currentLerpTime / lerpTime;
				t = t * t * (3f - 2f*t);
				
				cameraTransform.position = Vector3.Lerp(startPos2, endPos2, t);
				cameraTransform.localRotation = Quaternion.Lerp (startRot2, endRot2, t);
			}
			else
			{
				//increment timer once per frame
				currentLerpTime += Time.deltaTime;
				if (currentLerpTime > lerpTime) 
				{
					currentLerpTime = lerpTime;
				}
				
				float t = currentLerpTime / lerpTime;
				t = t * t * (3f - 2f*t);
				
				cameraTransform.position = Vector3.Lerp(startPos2, endPos2, t);
				cameraTransform.localRotation = Quaternion.Lerp (startRot2, endRot2, t);
			}
			
			if(cameraTransform.position == wantedPosition2)
			{
				_state = 0;
				
				if(carDrifting != null)
				{
					carDrifting.ActivePlayer();
				}
				else
				{
					if(GetComponent<MultiplayerManager>())
					{
						GetComponent<MultiplayerManager>().ActivePlayer();
					}
				}
			}
		}
	}
	#endregion
	
	#region Animation Methods
	public void SetAnimation(int value)
	{
		if(value != 1)
		{
			startPos2 = cameraTransform.position;
			startRot2 = cameraTransform.localRotation;
			scaleValue = 0.25f;
			lerpTime = 10f;
			currentLerpTime = 0.0f;
		}
		
		UnityEngine.Cursor.visible = false;
		_state = value;
	}
	#endregion
	
	#region Properties
	public int state
	{
		get { return _state; }
	}
	#endregion
}