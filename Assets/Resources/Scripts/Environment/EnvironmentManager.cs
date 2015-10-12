using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class EnvironmentManager: MonoBehaviour 
{
	#region Public Attributes
	[Header("Lights Attributes")]
	public bool staticLighting;
	public int staticLightState;
	
	[Header("Sun Attributes")]
	public Transform sunObject;
	public bool moveSun;
	[Tooltip("If enabled, the sun will move as real life")]
	public bool realLife;
	[Tooltip("If realLife is enabled, this values does not matter")]
	public Vector3 moveSpeed;
	
	[Header("Manager Attributes")]
	public Transform[] limitsCollider;	// order: up-left, up-right, down-left, down-right
	#endregion
	
	#region Private Attributes
	private int currentState;
	private Light[] allLights;
	private Light[] containerLights;
	private int containerCounter;
	private Vector3 auxRotation;
	private bool lightsOn;
	private Camera[] sceneCameras;
	private int containerFinalCounter;
	private MeshRenderer[] allMarks;
	private Transform[] driftMarks;
	private int driftMarksCounter;
	private Vector3 driftMarksRotation;
	private GameObject[] players;
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Initialize values		
		if(staticLighting)
		{
			currentState = staticLightState;
		}
		else
		{
			// Set up a random light
			currentState = Random.Range ((int)-1, (int)4);
		}
		
		// Get all cameras
		sceneCameras = GameObject.FindObjectsOfType<Camera>();
		
		// Get all scene lights
		containerCounter = 0;
		allLights = GameObject.FindObjectsOfType<Light>();
		
		// Filter container lights
		containerLights = new Light[allLights.Length];
		for(int i = 0; i < allLights.Length; i++)
		{
			if(allLights[i].gameObject.name == "ContainerLight" || allLights[i].gameObject.name == "LightVisual")
			{
				if(allLights[i].transform.position.x > limitsCollider[0].position.x && allLights[i].transform.position.x < limitsCollider[1].position.x && allLights[i].transform.position.z > limitsCollider[0].position.z && allLights[i].transform.position.z < limitsCollider[2].position.z)
				{
					containerLights[containerCounter] = allLights[i];
					containerCounter++;
				}
				else
				{
					allLights[i].transform.parent.transform.gameObject.SetActive (false);
				}
			}
		}
		
		// Fix array length
		Light[] auxLights = containerLights;
		containerLights = new Light[containerCounter];
		for(int i = 0; i < containerLights.Length; i++)
		{
			containerLights[i] = auxLights[i];
		}
		
		// Check sun object reference
		if(sunObject == null)
		{
			moveSun = false;
			Debug.Log ("EnvironmentManager: there is no sun reference, disabling move sun attribute");
		}
		
		// Get all mesh renderers
		driftMarksCounter = 0;
		allMarks = GameObject.FindObjectsOfType<MeshRenderer>();
		
		// Filter drift marks
		driftMarks = new Transform[allMarks.Length];
		for(int i = 0; i < allMarks.Length; i++)
		{
			if(allMarks[i].gameObject.name == "DriftMarks")
			{
				driftMarks[driftMarksCounter] = allMarks[i].transform;
				driftMarksCounter++;
			}
		}
		
		// Fix array length
		Transform[] auxMarks = driftMarks;
		driftMarks = new Transform[driftMarksCounter];
		for(int i = 0; i < driftMarks.Length; i++)
		{
			driftMarks[i] = auxMarks[i];
		}
		
		// Apply changes to all environment objects
		SetUpSun();
		SetUpLights();
		SetUpDriftMarks();
	}
	
	private void Update ()
	{
		if(moveSun)
		{			
			if(realLife)
			{
				sunObject.Rotate(Vector3.right * (360f * Time.deltaTime) / 60 / 60 / 24, Space.Self);
			}
			else
			{
				sunObject.Rotate(moveSpeed * Time.deltaTime, Space.Self);
			}
			
			auxRotation = sunObject.localRotation.eulerAngles;
			
			if(auxRotation.z == 0.0f && auxRotation.y == 0.0f)
			{
				if(auxRotation.x > 90.0f && auxRotation.x < 350.0f)
				{
					if(!lightsOn)
					{
						SetLights(true);
					}
				}
				else
				{
					if(lightsOn)
					{
						SetLights(false);
					}
				}
			}
			else if(auxRotation.z == 180.0f && auxRotation.y == 180.0f)
			{
				if(auxRotation.x > 270)
				{
					if(!lightsOn)
					{
						SetLights(true);
					}
				}
				else
				{
					if(lightsOn)
					{
						SetLights(false);
					} 
				}
			}
		}
	}
	#endregion
	
	#region Lights Methods
	private void SetUpSun()
	{
		switch(currentState)
		{
			case 0:
			{
				sunObject.localRotation = Quaternion.Euler (34.98499f, 0, 0);
				break;
			}
			case 1:
			{
				sunObject.localRotation = Quaternion.Euler (64.46765f, 0, 0);
				break;
			}
			case 2:
			{
				sunObject.localRotation = Quaternion.Euler (130f, 0, 0);
				break;
			}
			case 3:
			{
				sunObject.localRotation = Quaternion.Euler (270f, 0, 0);
				break;
			}
		}
	}
	
	private void SetUpLights()
	{
		// Check if it is night state
		if(currentState != 3)
		{
			for(int i = 0; i < sceneCameras.Length; i++)
			{
				sceneCameras[i].renderingPath = RenderingPath.Forward;
				
				if(sceneCameras[i].GetComponent<CameraMotionBlur>())
				{
					sceneCameras[i].GetComponent<CameraMotionBlur>().filterType = CameraMotionBlur.MotionBlurFilter.ReconstructionDX11;
				}
			}
			
			lightsOn = false;
			Debug.Log ("EnvironmentManager: disabling container lights because it is not night");
			for(int i = 0; i < containerLights.Length; i++)
			{
				containerLights[i].enabled = false;
			}
		}
		else
		{
			for(int i = 0; i < sceneCameras.Length; i++)
			{
				sceneCameras[i].renderingPath = RenderingPath.DeferredShading;
				
				if(sceneCameras[i].GetComponent<CameraMotionBlur>())
				{
					sceneCameras[i].GetComponent<CameraMotionBlur>().filterType = CameraMotionBlur.MotionBlurFilter.CameraMotion;
				}
			}
			
			lightsOn = true;
		}
		
		players = GameObject.FindGameObjectsWithTag ("Player");
		for(int i = 0; i < players.Length; i++)
		{
            if (players[i] != null)
            {
                players[i].GetComponent<LightsSetup>().SetFrontLights(lightsOn);
            }
		}
	}
	
	private void SetLights(bool active)
	{
		// Check if it is night state
		lightsOn = active;
		for(int i = 0; i < containerLights.Length; i++)
		{
			containerLights[i].enabled = active;
		}
		
		if(active)
		{
			for(int i = 0; i < sceneCameras.Length; i++)
			{
				sceneCameras[i].renderingPath = RenderingPath.DeferredShading;
				
				if(sceneCameras[i].GetComponent<CameraMotionBlur>())
				{
					sceneCameras[i].GetComponent<CameraMotionBlur>().filterType = CameraMotionBlur.MotionBlurFilter.CameraMotion;
				}
			}
		}
		else
		{
			for(int i = 0; i < sceneCameras.Length; i++)
			{
				sceneCameras[i].renderingPath = RenderingPath.Forward;
				
				if(sceneCameras[i].GetComponent<CameraMotionBlur>())
				{
					sceneCameras[i].GetComponent<CameraMotionBlur>().filterType = CameraMotionBlur.MotionBlurFilter.ReconstructionDX11;
				}
			}
		}
		
		players = GameObject.FindGameObjectsWithTag ("Player");
		for(int i = 0; i < players.Length; i++)
		{
			players[i].GetComponent<LightsSetup>().SetFrontLights(active);
		}
	}
	#endregion
	
	#region Drift Marks Methods
	private void SetUpDriftMarks()
	{
		for(int i = 0; i < driftMarks.Length; i++)
		{
			driftMarksRotation = driftMarks[i].localRotation.eulerAngles;
			driftMarksRotation.y = Random.Range (0.0f, 360.0f);
			driftMarks[i].localRotation = Quaternion.Euler (driftMarksRotation);
		}
	}
	#endregion
	
	#region Properties
	public bool LightsOn
	{
		get { return lightsOn; }
	}
	#endregion
}