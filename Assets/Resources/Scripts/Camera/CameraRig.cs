using UnityEngine;
using System.Collections;

public class CameraRig: MonoBehaviour 
{
	#region Public Attributes
	public Transform backCamera;
	public int totalCameras;
	public int activeCamera;
	#endregion
	
	#region Private Attributes
	public Transform[] cameraTransform;
	private Vector3[] positionInit;
	private Quaternion[] rotationInit;
	private bool backState;
	private Vector3 backPositionInit;
	private Quaternion backRotationInit;
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	private void Awake () 
	{
		cameraTransform[0] = Camera.main.transform;
		backState = false;
		
		positionInit = new Vector3[totalCameras];
		rotationInit = new Quaternion[totalCameras];
		
		for(int i = 1; i < totalCameras; i++)
		{
			positionInit[i] = cameraTransform[i].localPosition;
			rotationInit[i] = cameraTransform[i].localRotation;
			cameraTransform[i].GetComponent<Camera>().enabled = false;
		}
		
		backPositionInit = backCamera.localPosition;
		backRotationInit = backCamera.localRotation;
		backCamera.GetComponent<Camera>().enabled = false;
		
		cameraTransform[activeCamera].GetComponent<Camera>().enabled = true;
		
		// Enable car test with no game logic
		if(!GameObject.Find("DriftManager") && !GameObject.FindWithTag ("MultiplayerManager"))
		{
			Debug.Log ("CameraRig: no game manager, enabling camera logic for tests");
			cameraTransform[0].GetComponent<OrbitCamera>().enabled = true;
			cameraTransform[0].GetComponent<OrbitCamera>().target = GameObject.FindWithTag ("Player").transform;
			DataManager.Instance.isGamepad = false;
		}
	}
	
	private void Update () 
	{
		if(activeCamera > 0)
		{
			cameraTransform[activeCamera].localPosition = positionInit[activeCamera];
			cameraTransform[activeCamera].localRotation = rotationInit[activeCamera];
		}
		
		if(backState)
		{
			backCamera.localPosition = backPositionInit;
			backCamera.localRotation = backRotationInit;
		}
	}
	#endregion
	
	#region Camera Methods
	public void ChangeCamera()
	{
		activeCamera++;
		
		if(activeCamera == totalCameras)
		{
			activeCamera = 0;
		}
		
		// Enable first the new camera
		cameraTransform[activeCamera].GetComponent<Camera>().enabled = true;
		
		// Disable then the unused cameras
		for(int i = 0; i < totalCameras; i++)
		{
			if(i != activeCamera)
			{
				cameraTransform[i].GetComponent<Camera>().enabled = false;
			}
		}		
	}
	
	public void SetBackCamera(bool state)
	{
		// Update values
		backState = state;
			
		if(state)
		{
			// Enable first the new camera
			backCamera.GetComponent<Camera>().enabled = true;
			
			// Disable then the unused cameras
			for(int i = 0; i < totalCameras; i++)
			{
				if(i != activeCamera)
				{
					cameraTransform[i].GetComponent<Camera>().enabled = false;
				}
			}
		}
		else
		{	
			// Enable first the new camera
			cameraTransform[activeCamera].GetComponent<Camera>().enabled = true;
			
			// Disable then the unused cameras
			for(int i = 0; i < totalCameras; i++)
			{
				if(i != activeCamera)
				{
					cameraTransform[i].GetComponent<Camera>().enabled = false;
				}
			}
			
			backCamera.GetComponent<Camera>().enabled = false;
		}		
	}
	#endregion
}