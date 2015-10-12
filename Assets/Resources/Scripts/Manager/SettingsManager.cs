using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class SettingsManager: MonoBehaviour 
{
	#region Public Attributes
	public GameObject audioManager;
	public KeyboardInputModule standaloneModule;
	#endregion
	
	#region Private Attributes
	private Transform[] cameras;
	private AudioSource[] musicSource;
	#endregion
	
	#region References
	private CarController carController;
	private DataManager dataManager;
	private GameObject[] players;
	#endregion
	
	#region Main Methods
	private void Awake()
	{
		dataManager = DataManager.Instance;
		
		SetController (dataManager.isGamepad);

        standaloneModule = GameObject.Find("EventSystem").GetComponent<KeyboardInputModule>();
	}
	
	private void Start () 
	{
		if(GameObject.FindWithTag ("MultiplayerManager"))
		{
			players = GameObject.FindGameObjectsWithTag("Player");
			for(int i = 0; i < players.Length; i++)
			{
				if(players[i].GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					carController = players[i].GetComponent<CarController>();
					cameras = carController.GetComponent<CameraRig>().cameraTransform;
					break;
				}
			}
		}
		else
		{
			carController = GameObject.FindWithTag ("Player").GetComponent<CarController>();
			cameras = carController.GetComponent<CameraRig>().cameraTransform;
		}
		
		dataManager = DataManager.Instance;
		musicSource = audioManager.GetComponents<AudioSource>();
		
		// Initialize values
		if(!carController)
		{
			GameObject.FindWithTag("Player").GetComponent<CarController>();
		}
		
		// Setup settings
		Invoke ("SetupSettings", 0.01f);
	}
	#endregion
	
	#region Invoke Methods
	private void SetupSettings()
	{		
		SetResolution(dataManager.resolution);
		SetQualitySettings(dataManager.quality);
		SetAntialiasing(dataManager.antialiasing);
		SetCameraMotionBlur(dataManager.cameraMotionBlur);
		SetDepthOfField(dataManager.depthOfField);
		SetVignette(dataManager.vignette);
		SetAmbientOcclusion(dataManager.ambientOcclusion);
		SetColorEffect(dataManager.colorEffect);
		SetGeneralVolumeInit(dataManager.generalVolume);
		SetMusicVolumeInit(dataManager.musicVolume);
		SetController (dataManager.isGamepad);
		SetTraction(dataManager.isAutomatic);
	}
	#endregion
	
	#region Quality Settings Methods
	public void SetResolution(int value)
	{
		switch(value)
		{
			case 0:
			{
				Screen.SetResolution(800, 600, true);
				break;
			}
			case 1:
			{
				Screen.SetResolution(1024, 768, true);
				break;
			}
			case 2:
			{
				Screen.SetResolution(1280, 720, true);
				break;
			}
			case 3:
			{
				Screen.SetResolution(1280, 1024, true);
				break;
			}
			case 4:
			{
				Screen.SetResolution(1366, 768, true);
				break;
			}
			case 5:
			{
				Screen.SetResolution(1440, 900, true);
				break;
			}
			case 6:
			{
				Screen.SetResolution(1680, 1050, true);
				break;
			}
			case 7:
			{
				Screen.SetResolution(1920, 1080, true);
				break;
			}
		}
		
		dataManager.resolution = value;
		PlayerPrefs.SetInt("resolution", value);
	}	
	
	public void SetQualitySettings(int value)
	{
		QualitySettings.SetQualityLevel(value);
		dataManager.quality = value;
		PlayerPrefs.SetInt("quality", value);
	}	
	#endregion
	
	#region Camera Effects Methods
	public void SetAntialiasing(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<Antialiasing>().enabled = active;
		}
		
		dataManager.antialiasing = active;
		PlayerPrefs.SetInt("antialiasing", active ? 1 : 0);
	}
	
	public void SetBloom(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<Bloom>().enabled = active;
		}
		
		dataManager.bloom = active;
		PlayerPrefs.SetInt("bloom", active ? 1 : 0);
	}
	
	public void SetCameraMotionBlur(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<CameraMotionBlur>().enabled = active;
		}
		
		dataManager.cameraMotionBlur = active;
		PlayerPrefs.SetInt("cameraMotionBlur", active ? 1 : 0);
	}
	
	public void SetDepthOfField(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<DepthOfField>().enabled = active;
		}
		
		dataManager.depthOfField = active;
		PlayerPrefs.SetInt("depthOfField", active ? 1 : 0);
	}
	
	public void SetVignette(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<CC_FastVignette>().enabled = active;
		}
		
		dataManager.vignette = active;
		PlayerPrefs.SetInt("vignette", active ? 1 : 0);
	}
	
	public void SetAmbientOcclusion(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<ScreenSpaceAmbientOcclusion>().enabled = active;
		}
		
		dataManager.ambientOcclusion = active;
		PlayerPrefs.SetInt("ambientOcclusion", active ? 1 : 0);
	}
	
	public void SetColorEffect(bool active)
	{
		for(int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<CC_Vintage>().enabled = active;
		}
		
		dataManager.colorEffect = active;
		PlayerPrefs.SetInt("colorEffect", active ? 1 : 0);
	}
	#endregion
	
	#region Audio Methods
	public void SetGeneralVolume(float value)
	{
		AudioListener.volume = value;
		dataManager.generalVolume = value;
		PlayerPrefs.SetFloat("generalVolume", value);
	}
	
	public void SetGeneralVolumeInit(float value)
	{
		dataManager.generalVolume = value;
		PlayerPrefs.SetFloat("generalVolume", value);
	}
	
	public void SetMusicVolume(float value)
	{
		dataManager.musicVolume = value;
		
		for(int i = 0; i < musicSource.Length; i++)
		{
			musicSource[i].volume = value;
		}
		
		PlayerPrefs.SetFloat("musicVolume", value);
	}
	
	public void SetMusicVolumeInit(float value)
	{
		dataManager.musicVolume = value;
		PlayerPrefs.SetFloat("musicVolume", value);
	}
	
	public float GetMusicVolume()
	{
		return DataManager.Instance.musicVolume;
	}
	#endregion
	
	#region Controls Methods
	public void SetController(bool isGamepad)
	{	
		dataManager.isGamepad = isGamepad;
		PlayerPrefs.SetInt("isGamepad", isGamepad ? 1 : 0);
        
		if(isGamepad)
		{
			standaloneModule.horizontalAxis = "Horizontal360";
			standaloneModule.verticalAxis = "Vertical360";
			standaloneModule.submitButton = "Submit360";
			standaloneModule.cancelButton = "Cancel360";
			
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			standaloneModule.horizontalAxis = "Horizontal";
			standaloneModule.verticalAxis = "Vertical";
			standaloneModule.submitButton = "Submit";
			standaloneModule.cancelButton = "Cancel";
			
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
	}
	
	public void SetTraction(bool isAutomatic)
	{
		if(carController != null)
		{
			carController.GetComponent<CarSetup>().Automatic = isAutomatic;
		}
		
		dataManager.isAutomatic = isAutomatic;
		PlayerPrefs.SetInt("isAutomatic", isAutomatic ? 1 : 0);
	}
	#endregion
}