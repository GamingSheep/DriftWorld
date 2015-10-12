using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;
using System;
using UnityEngine.Networking;

public class MultiplayerManager: NetworkBehaviour 
{
	#region Enums

	#endregion
	
	#region Public Attributes
	[Header("Drift Attributes")]
	// Score logic values while drifting
	public float driftValue;
	public float minDriftRatio;
	
	[Header("Multiplayer Gameplay Attributes")]
	// Multiplayer time left logic and animation
	public bool itIsServer;
	public float leftTimeInit;
	public float timeLabelSpeed;
	public int timeLabelState;
	
	[Header("Extra Attributes")]
	// Extra score logic and animations
	public int extraNitro;
	public int extraAvoid;
	public float avoidMinSpeed;
	public float avoidTempInit;
	
	[Header("Extra Temp Attributes")]
	// Temp offset values
	public float multiplierTempInit;
	public float collisionTempInit;
	
	[Header("UI Attributes")]
	// Gameplay state values
	public bool started;
	public bool client;
	public bool pause;
	public bool ended;
	public bool settings;
	
	// Fade values
	public float fadeSpeed;
	public float initAlpha;
	
	// Drift UI values
	public Color initColor;
	public Color resetColor;
	public float speedUI;
	
	[Header("UI Sounds")]
	// UI sounds
	public AudioClip pauseClip;
	public AudioClip selectClip;	
	#endregion
	
	#region Private Attributes
	private Vector3 rigidbodyVelocity;
    public float leftTime = 300;
	private bool isDrifting;
	private bool canContinue;
	private bool canDrift;
	private int totalScore;
	private int driftScore;
	private int maxDriftScore;
	private int multiplier;
	private float multiplierTemp;
	private float collisionTemp;
	private Color auxColor;
	private Vector3[] initPosition;
	private bool pauseAnimation;
	private BlurOptimized actualCamera;
	private DataManager dataManager;
	private CarInput carInput;
	private bool canShowExtra;
	private float avoidTemp;
	private bool avoiding;
	private Text extraTweenText;
	private TweenFontSize challengeLabelTween;
	private Rigidbody carRigidbody;
	private GameObject[] players;
	private KeyboardInputModule standaloneModule;
	#endregion
	
	#region References	
	private CarEngine carEngine;
	private CarSetup carSetup;
	private CarController carController;
	private CarAudio carAudio;
	private SettingsManager settingsManager;
	private CameraIntroAnimation introAnimation;
	private Bloom bloomUI;
	private	CustomNetworkManager network;
	
	[Header("Main UI References")]
	// Gameplay references
	public GameObject mainTitle;
	public GameObject startUI;
	public GameObject clientUI;
	public GameObject gameplayUI;
	public GameObject endUI;
	public GameObject pauseUI;
	public GameObject settingsUI;
	public AudioSource audioSource;
	public AudioManager audioManager;
	
	[Header("UI Text References")]
	public Text driftLabel;
	public Text maxDriftLabel;
	public Text multiplierLabel;
	public Text challengeLabel;
	public Text[] labelsUI;
    public StandManager stand;
	
	[Header("UI Field References")]
	public InputField adressField;
	
	[Header("UI Color References")]
	public Color defaultColor;
	public Color goodColor;
	public Color proColor;
	public Color insaneColor;
	
	[Header("UI Animation References")]
	public TweenAlpha backgroundTween;
	public TweenAlpha fade;
	public TweenDynamic extraTween;
	public TweenDynamic timeExtraTween;
	
	[Header("Multiplayer References")]
	public Minimap minimap;
	public AudioSource leftTimeSource;
	public AudioSource timeOverSource;
	
	[Header("Achievements References")]
	public AchievementsManager achievementsManager;
	#endregion
	
	#region Auxiliar Attributes
	private string strReturn;
	private bool volumeFadeIn;
	#endregion
	
	#region Main Methods
	public override void OnStartClient ()
	{
		base.OnStartClient ();
		minimap.UpdateMinimap();
	}
	private void Start ()
	{
		volumeFadeIn = true;
		AudioListener.volume = 0.0f;
		
		// Initialize values
		pauseAnimation = false;
		pause = false;
		started = false;
		client = false;
		settings = false;
		isDrifting = false;
		canContinue = false;
		canDrift = true;
		canShowExtra = true;
		avoiding = false;
		timeLabelState = 0;
		totalScore = 0;
		driftScore = 0;
		maxDriftScore = 0;
		multiplier = 0;
		rigidbodyVelocity = Vector3.zero;
		multiplierTemp = multiplierTempInit;
		collisionTemp = collisionTempInit;
		avoidTemp = avoidTempInit;
		initPosition = new Vector3[labelsUI.Length];
		dataManager = DataManager.Instance;
		carInput = CarInput.Instance;
		
		if(!standaloneModule)
		{
			standaloneModule = GameObject.Find ("EventSystem").GetComponent<KeyboardInputModule>();
		}
		
		if(dataManager.isGamepad)
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
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Load data
		maxDriftScore = dataManager.bestDrift;
		
		for(int i = 0; i < labelsUI.Length; i++)
		{
			labelsUI[i].color = initColor;
			auxColor = labelsUI[i].color;
			auxColor.a = initAlpha;
			labelsUI[i].color = auxColor;
			initPosition[i] = labelsUI[i].transform.position;
		}
		
		network = GetComponent<CustomNetworkManager>();
		introAnimation = GetComponent<CameraIntroAnimation>();
		settingsManager = GetComponent<SettingsManager>();
		extraTweenText = extraTween.GetComponent<Text>();
		challengeLabelTween = challengeLabel.GetComponent<TweenFontSize>();
		bloomUI = GameObject.Find ("UICamera").GetComponent<Bloom>();
		
		pauseUI.SetActive (false);
		settingsUI.SetActive (false);
		gameplayUI.SetActive (false);
		startUI.SetActive(false);
		bloomUI.enabled = false;		
		
		EventSystem.current.SetSelectedGameObject(null);
		
		// Enable match maker by default
		network.StartMatchMaker();
		network.matchMaker.SetProgramAppID((UnityEngine.Networking.Types.AppID)338802);
		network.SetMatchHost("mm.unet.unity3d.com", 443, true);
		network.matchSize = 5;
		network.matchName = adressField.text;
	}
	
	private void OnLevelWasLoaded(int level)
	{		
		if(DataManager.Instance.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
	}
	
	private void Update () 
	{
		#region Input Logic
		if(!IsInvoking ("ChangeLevel"))
		{
			if(volumeFadeIn)
			{
				if(AudioListener.volume < dataManager.generalVolume)
				{
					AudioListener.volume += Time.deltaTime * 0.5f;
				}
				else
				{
					volumeFadeIn = false;
					AudioListener.volume = dataManager.generalVolume;
				}
			}
			
			if(!started)
			{			
				if(introAnimation.state == 0)
				{
					if(settings)
					{
						if(carInput.Pause () || carInput.Cancel ())
						{
							// Back to main menu
							ActiveMainMenu ();
							
							// Play pause sound
							audioSource.clip = pauseClip;
							if(!audioSource.isPlaying)
							{
								audioSource.Play ();
							}
						}
					}
					else
					{
						if(client)
						{
							if(carInput.Pause () || carInput.Cancel ())
							{
								// Go to client menu
								ActiveMainMenu();
								
								// Play pause sound
								audioSource.clip = selectClip;
								if(!audioSource.isPlaying)
								{
									audioSource.Play ();
								}
							}
							
							if(network.matches != null)
							{
								clientUI.GetComponent<MultiplayerList>().enabled = true;
							}
						}
					}
				}
				
				if(GameObject.FindWithTag ("Player"))
				{
                    Debug.Log(GameObject.FindWithTag("Player").gameObject.name);

                    clientUI.SetActive(false);
                    mainTitle.SetActive(false);
                    startUI.SetActive(false);
                    backgroundTween.SetTween(2);
                    introAnimation.SetAnimation(2);
                    UnityEngine.Cursor.visible = false;
                    bloomUI.enabled = false;
                    audioManager.SetGameplayMusic();

                    started = true;
					StartGamePlayer();
					settingsManager.enabled = true;

                    Invoke("StartMultiplayerFade", 5.0f);

                    Debug.Log ("MultiplayerManager: connected to network succesful");
				}
			}
			else
			{
				if(pause)
				{
					if(carInput.Pause () || carInput.Cancel ())
					{
						if(settings)
						{
							// Go to pause menu from settings in race
							SetPause(true);
							
							// Play pause sound
							audioSource.clip = pauseClip;
							if(!audioSource.isPlaying)
							{
								audioSource.Play ();
							}
						}
						else
						{
							// Set gameplay from pause menu
							SetPause (false);
							
							// Play pause sound
							audioSource.clip = selectClip;
							if(!audioSource.isPlaying)
							{
								audioSource.Play ();
							}
						}
					}
				}
				else
				{
					if(!ended)
					{
						if(carInput.Pause ())
						{
							// Go to pause from gameplay when not ended
							SetPause(true);
						}
					}
					else
					{
                        if (carInput.Pause() || carInput.Cancel())
                        {
                            // Play pause sound
                            audioSource.clip = selectClip;
                            if (!audioSource.isPlaying)
                            {
                                audioSource.Play();
                            }

                            QuitGame();
                        }
					}
				}
			}
		}
		else
		{
			if(AudioListener.volume > 0.0f)
			{
				AudioListener.volume -= Time.deltaTime * 0.5f;
			}
			else
			{
				AudioListener.volume = 0.0f;
			}
		}
		#endregion
		
		#region Pause Animation
		if(pauseAnimation)
		{
			if(pause || ended)
			{
				if(actualCamera.blurIterations < 2)
				{
					actualCamera.blurIterations++;
				}
				
				if(actualCamera.downsample < 1)
				{
					actualCamera.downsample++;
				}
				
				if(actualCamera.blurSize < 3)
				{
					actualCamera.blurSize++;
				}
				
				if(actualCamera.blurIterations == 2 && actualCamera.downsample == 1 && actualCamera.blurSize == 3)
				{
					pauseAnimation = false;
				}
			}
			else
			{
				if(actualCamera.blurIterations > 0)
				{
					actualCamera.blurIterations--;
				}
				
				if(actualCamera.downsample > 0)
				{
					actualCamera.downsample--;
				}
				
				if(actualCamera.blurSize > 0)
				{
					actualCamera.blurSize--;
				}
				
				if(actualCamera.blurIterations == 0 && actualCamera.downsample == 0 && actualCamera.blurSize == 0)
				{
					pauseAnimation = false;
					actualCamera.enabled = false;
					pauseUI.SetActive (false);
					settingsUI.SetActive (false);
					gameplayUI.SetActive (true);
					
					// Enable car audio
					carAudio.EnableSources();
				}
			}
		}
		#endregion
		
		#region Drift Logic
		if(started)
		{				
			if(!pause)
			{
				if(isDrifting && !ended)
				{
					if(carEngine.slipRatio < minDriftRatio)
					{
						FinishDrift();
					}
					else
					{
						if(!carEngine.recentlyDamage)
						{
							driftScore += (int)(driftValue * multiplier * Time.deltaTime);
							
							// Extra message
							if(carInput.Nitrous())
							{
								if(canShowExtra)
								{
									canShowExtra = false;
									driftScore += (int)extraNitro;
									extraTweenText.text = "Nitro +" + extraNitro;
									extraTween.SetTweenState(0);
								}
							}
							
							if(carController.recentlyAvoid)
							{
								// Start to count and cancel if damaged
								avoidTemp = avoidTempInit;
								avoiding = true;
							}
							
							if(avoiding)
							{
								avoidTemp -= Time.deltaTime;
								if(avoidTemp <= 0)
								{
									avoiding = false;
									GiveExtraAvoid();
								}
							}
						}
						else
						{
							avoiding = false;
						}
					}
				}
				else
				{
					if(canContinue)
					{
						if(avoiding)
						{
							avoidTemp -= Time.deltaTime;
							if(avoidTemp <= 0)
							{
								avoiding = false;
								GiveExtraAvoid();
							}
						}
						
						multiplierTemp -= Time.deltaTime;
						if(multiplierTemp <= 0)
						{
							ResetMultiplier();
						}
					}
					
					if(canDrift)
					{
						if(carEngine.slipRatio > minDriftRatio)
						{
							StartDrift();
						}
					}
					else
					{
						collisionTemp -= Time.deltaTime;
						if(collisionTemp <= 0)
						{
							RestoreDrift();
						}
					}
				}
				
				if(carEngine.recentlyDamage)
				{
					SetUIColor(resetColor);
					ResetDrift();
				}
				
				// Update UI
				if(canContinue)// > 0.0f)
				{
					ResetPosition();
					FadeIn ();
				}
				else
				{
					FadeOut ();
					
					if(!canDrift)
					{
						MoveDown();
					}
				}
				
				if(canDrift && canContinue)
				{
					driftLabel.text = driftScore.ToString ();
					multiplierLabel.text = "X" + multiplier;
					
					float auxAlpha = multiplierLabel.color.a;
					
					if(multiplier < 10)
					{
						multiplierLabel.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, auxAlpha);
					}
					else if(multiplier >= 10 && multiplier < 30)
					{
						multiplierLabel.color = new Color(goodColor.r, goodColor.g, goodColor.b, auxAlpha);
					}
					else if(multiplier >= 30 && multiplier < 50)
					{
						multiplierLabel.color = new Color(proColor.r, proColor.g, proColor.b, auxAlpha);
					}
					else if(multiplier >= 50)
					{
						multiplierLabel.color = new Color(insaneColor.r, insaneColor.g, insaneColor.b, auxAlpha);
					}
				}
				
				maxDriftLabel.text = "Best Drift: " + maxDriftScore.ToString ();
			}
		}
		#endregion
		
		#region Multiplayer Logic
		if(started)
		{
			if(!pause)
			{
				if(ended)
				{
					challengeLabel.text = "Time: " + FormatTime(leftTime, true, 2);
				}
				else
				{
					if(itIsServer)
					{
						leftTime -= Time.deltaTime;
					}
					
					switch(timeLabelState)
					{
						case 1:
						{
							auxColor = challengeLabel.color;
							
							if(challengeLabel.color.g > 0)
							{
								auxColor.g -= Time.deltaTime * timeLabelSpeed;
							auxColor.b -= Time.deltaTime * timeLabelSpeed;
							}
							else
							{
								auxColor.g = 0.0f;
								auxColor.b = 0.0f;
								auxColor.r = 1.0f;
								timeLabelState = 2;
							}
							
							challengeLabel.color = auxColor;
							break;
						}
						case 2:
						{
							auxColor = challengeLabel.color;
							
							if(challengeLabel.color.g < 1.0f)
							{
								auxColor.g += Time.deltaTime * timeLabelSpeed;
								auxColor.b += Time.deltaTime * timeLabelSpeed;
							}
							else
							{
								auxColor.g = 1.0f;
								auxColor.b = 1.0f;
								auxColor.r = 1.0f;
								timeLabelState = 1;
							}
							
							challengeLabel.color = auxColor;
							break;
						}
					}
					
					if(leftTime < 10)
					{
						if(timeLabelState < 1)
						{
							leftTimeSource.enabled = true;
							challengeLabelTween.enabled = true;
							timeLabelState = 1;
						}
					}
					else
					{
						if(timeLabelState > 0)
						{
							leftTimeSource.enabled = false;
							timeLabelState = 0;
							challengeLabel.color = new Color(initColor.r, initColor.g, initColor.b, 255);
							challengeLabelTween.enabled = false;
							challengeLabel.fontSize = challengeLabelTween.minValue;
						}	
					}
					
					// Update label value
					challengeLabel.text = "Time: " + FormatTime(leftTime, true, 2);
					
					if(leftTime <= 0)
					{
						FinishChallenge();
					}
				}
			}
		}
		#endregion
		
		#region End Volume Logic
		if(IsInvoking("ExitGame"))
		{
			AudioListener.volume = 1.0f - fade.image.color.a;
		}
		#endregion
	}
	#endregion
	
	#region Multiplayer Methods
    private void StartMultiplayerFade()
    {
        fade.gameObject.SetActive(true);
        fade.SetTween(1);
        fade.disableOnEnd = false;
    }

    private void EndMultiplayerFade()
    {
        fade.SetTween(2);
        fade.disableOnEnd = true;
    }

    public void ActiveMainMenu()
	{
		client = false;
		settings = false;
		mainTitle.SetActive (true);
		clientUI.SetActive (false);
		settingsUI.SetActive (false);
		startUI.SetActive(true);
		backgroundTween.SetTween(1);
		bloomUI.enabled = true;
		EventSystem.current.SetSelectedGameObject(startUI.transform.GetChild(0).gameObject);
		
		if(carSetup != null)
		{
			// Fix objects active state
			carSetup.NosFireObject.SetActive (false);
			carSetup.ExhaustBackfire.SetActive (false);
		}
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = true;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
	}
	
	public void CreateMatch()
	{
		network.matchName = adressField.text;
		network.matchMaker.CreateMatch(network.matchName, network.matchSize, true, "", network.OnMatchCreate);

		// Play pause sound
		audioSource.clip = selectClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
	}
	
	public void JoinMatch()
	{
        // Play pause sound
        audioSource.clip = selectClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
    }
	
	public void ActiveMultiplayerMenu()
	{
		network.matchMaker.ListMatches(0,9, "", network.OnMatchList);
		client = true;
		startUI.SetActive(false);
		clientUI.SetActive (true);
		EventSystem.current.SetSelectedGameObject(clientUI.transform.GetChild(0).gameObject);
	}	
	
	public void StartGamePlayer ()
	{
		// Get player references
		players = GameObject.FindGameObjectsWithTag("Player");
		for(int i = 0; i < players.Length; i++)
		{
			if(players[i].GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				carController = players[i].GetComponent<CarController>();
				carEngine = carController.GetComponent<CarEngine>();
				carSetup = carController.GetComponent<CarSetup>();
				carAudio = carController.GetComponent<CarAudio>();
				carRigidbody = carController.GetComponent<Rigidbody>();		
				carController.enabled = false;
				carEngine.enabled = true;
				// Disable car audio
				carAudio.EnableSources(0.5f);
				Debug.Log ("MultiplayerManager: player initialization complete");
				break;
			}
		}		
	}


	
	public void QuitGame()
	{
		EventSystem.current.SetSelectedGameObject(null);
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Play pause sound
		audioSource.clip = pauseClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		fade.gameObject.SetActive (true);
		fade.SetTween(1);
		
        if(itIsServer)
        {
            Invoke("QuitHost", 1 / fade.speed);
        }
        else
        {
            Invoke("QuitClient", 1 / fade.speed);
        }
	}
	
	public void QuitLobby()
	{
		EventSystem.current.SetSelectedGameObject(null);
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Play pause sound
		audioSource.clip = pauseClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		fade.gameObject.SetActive (true);
		fade.SetTween(1);
		
		Invoke ("QuitClient", 1 / fade.speed);
	}

	private void QuitHost()
	{
        // Call match killer and stop host when completed
        network.matchMaker.DestroyMatch(network.networkID, network.CancelMatch);
	}
	
	private void QuitClient()
	{
        // Call connection drop and stop client when completed
        network.DropConnection();
	}

    public void ResetMatch()
    {
        // Fade out
        fade.gameObject.SetActive(true);
        fade.SetTween(1);
        Invoke("ResetFadeIn", 1 / fade.speed);
    }

    private void ResetFadeIn()
    {
        // Reset values
        ended = false;
        leftTime = leftTimeInit;
        totalScore = 0;
        carSetup.NitroLeft = carSetup.NitroInitialAmount;
        carRigidbody.isKinematic = true;
        carEngine.enabled = false;
        carController.enabled = false;
        carRigidbody.transform.position = new Vector3(0, 0.0f, 5.0f * carRigidbody.GetComponent<NetworkIdentity>().playerControllerId);
        carEngine.enabled = true;
        carController.enabled = true;
        carRigidbody.isKinematic = false;

        // Update UI
        UnfreezePlayer();
        backgroundTween.SetTween(2);
        actualCamera = carEngine.GetComponent<CameraRig>().cameraTransform[carEngine.GetComponent<CameraRig>().activeCamera].GetComponent<BlurOptimized>();
        actualCamera.enabled = false;

        endUI.SetActive(false);
        gameplayUI.SetActive(true);

        // Fade in
        fade.gameObject.SetActive(true);
        fade.SetTween(2);
    }
	#endregion
	
	#region Connection Methods
	public void UpdateMatchData()
	{
        Invoke ("CallUpdateMinimap", 1.0f);
    }
	
	private void CallUpdateMinimap()
	{
		minimap.UpdateMinimap ();
	}
	#endregion
	
	#region Gameplay Methods
	public void SetPause(bool isPaused)
	{
		pause = isPaused;
		
		if(isPaused)
		{			
			// Disable main scripts
			settings = false;
			carController.enabled = false;
			carEngine.enabled = false;
			bloomUI.enabled = true;
			
			for(int i = 0; i < carSetup.Wheels.Length; i++)
			{
				carSetup.Wheels[i].enabled = false;
			}
			
			rigidbodyVelocity = carEngine.GetComponent<Rigidbody>().velocity;
			carEngine.GetComponent<Rigidbody>().isKinematic = true;
			pauseUI.SetActive (true);
			gameplayUI.SetActive (false);
			settingsUI.SetActive(false);
			backgroundTween.SetTween(1);
			EventSystem.current.SetSelectedGameObject(pauseUI.transform.GetChild(0).gameObject);
			
			if(dataManager.isGamepad)
			{
				#if !UNITY_EDITOR
				UnityEngine.Cursor.visible = false;
				UnityEngine.Cursor.lockState = CursorLockMode.Locked;
				#endif
			}
			else
			{
				#if !UNITY_EDITOR
				UnityEngine.Cursor.visible = false;
				UnityEngine.Cursor.lockState = CursorLockMode.None;
				#endif
			}
			
			// Enable camera effects
			if(carEngine.GetComponent<CameraRig>().activeCamera == 0)
			{
				Camera.main.GetComponent<CarCamera>().enabled = false;
			}
			
			// Play pause sound
			audioSource.clip = pauseClip;
			if(!audioSource.isPlaying)
			{
				audioSource.Play ();
			}
			
			// Disable car audio
			carAudio.DisableSources();
			
			actualCamera = carEngine.GetComponent<CameraRig>().cameraTransform[carEngine.GetComponent<CameraRig>().activeCamera].GetComponent<BlurOptimized>();
			actualCamera.enabled = true;
			pauseAnimation = true;
		}
		else
		{			
			// Disable main scripts
			settings = false;
			bloomUI.enabled = false;
			
			for(int i = 0; i < carSetup.Wheels.Length; i++)
			{
				carSetup.Wheels[i].enabled = true;
			}
			
			carController.enabled = true;
			carEngine.enabled = true;
			carEngine.GetComponent<Rigidbody>().isKinematic = false;
			carEngine.GetComponent<Rigidbody>().velocity = rigidbodyVelocity;
			pauseUI.SetActive (false);
			gameplayUI.SetActive (true);
			settingsUI.SetActive(false);
			backgroundTween.SetTween(2);
			EventSystem.current.SetSelectedGameObject(null);
			
			if(dataManager.isGamepad)
			{
				#if !UNITY_EDITOR
				UnityEngine.Cursor.visible = false;
				UnityEngine.Cursor.lockState = CursorLockMode.Locked;
				#endif
			}
			else
			{
				#if !UNITY_EDITOR
				UnityEngine.Cursor.visible = false;
				UnityEngine.Cursor.lockState = CursorLockMode.None;
				#endif
			}
			
			// Enable camera effects
			if(carEngine.GetComponent<CameraRig>().activeCamera == 0)
			{
				Camera.main.GetComponent<CarCamera>().enabled = true;
			}
			
			// Play pause sound
			audioSource.clip = selectClip;
			if(!audioSource.isPlaying)
			{
				audioSource.Play ();
			}
			
			actualCamera = carEngine.GetComponent<CameraRig>().cameraTransform[carEngine.GetComponent<CameraRig>().activeCamera].GetComponent<BlurOptimized>();
			actualCamera.enabled = true;
			pauseAnimation = true;
		}
	}
	
	public void ActivePlayer()
	{
		started = true;
		UnfreezePlayer ();
	}
	
	public void UnfreezePlayer()
	{
        Invoke("EndMultiplayerFade", 3.0f);

        carController.enabled = true;
		gameplayUI.SetActive (true);
		bloomUI.enabled = false;
		carRigidbody.isKinematic = false;
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Enable camera logic
		if(carEngine.GetComponent<CameraRig>().activeCamera == 0)
		{
			Camera.main.GetComponent<CarCamera>().enabled = true;
		}
		
		// Disable car audio
		carAudio.EnableSources();
	}
	
	public void SetGameplay()
	{
		// Enable main scripts
		pause = false;
		backgroundTween.SetTween(2);
		bloomUI.enabled = false;
		
		// Play select sound
		audioSource.clip = selectClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		pauseAnimation = true;
	}
	
	public void SetSettings()
	{
		// Enable main scripts	
		settings = true;	
		pauseUI.SetActive (false);
		startUI.SetActive (false);
		settingsUI.SetActive(true);
		EventSystem.current.SetSelectedGameObject(settingsUI.transform.GetChild(0).gameObject);
		bloomUI.enabled = true;
		
		// Play select sound
		audioSource.clip = selectClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
	}
	
	public void SetRestart()
	{
		EventSystem.current.SetSelectedGameObject(null);
		
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Play pause sound
		audioSource.clip = pauseClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		fade.gameObject.SetActive (true);
		fade.SetTween(1);
		Invoke ("RestartLevel", 1 / fade.speed);		
	}
	
	public void SetExit()
	{
		EventSystem.current.SetSelectedGameObject(null);
				
		if(dataManager.isGamepad)
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			#endif
		}
		else
		{
			#if !UNITY_EDITOR
			UnityEngine.Cursor.visible = false;
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			#endif
		}
		
		// Play pause sound
		audioSource.clip = pauseClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		fade.gameObject.SetActive (true);
		fade.SetTween(1);
		Invoke ("ChangeLevel", 1 / fade.speed);		
	}
	
	private void RestartLevel()
	{
		Application.LoadLevel (Application.loadedLevel);
	}
	
	private void ChangeLevel()
	{
		Application.LoadLevel ("DemoScene");
	}
	#endregion
	
	#region Drift Methods
	public void StartDrift()
	{
		// Enable drifting and initialize drift score
		isDrifting = true;
		canContinue = true;
		multiplier++;
		multiplierTemp = multiplierTempInit;
	}
	
	public void ResetDrift()
	{
		// Reset score and avoid to add score to total
		isDrifting = false;
		canContinue = false;
		canDrift = false;
		driftScore = 0;
		multiplier = 0;
		collisionTemp = collisionTempInit;
	}
	
	public void FinishDrift()
	{
		// Reset score and add score to total
		isDrifting = false;
	}
	
	public void ResetMultiplier()
	{
		if(driftScore > 100000)
		{
			achievementsManager.UnlockAchievement(1);
		}
		
		if(multiplier > 50)
		{
			achievementsManager.UnlockAchievement(0);
		}
		
		canContinue = false;
		totalScore += driftScore;
		
		if(driftScore > maxDriftScore)
		{
			maxDriftScore = driftScore;
			dataManager.bestDrift = maxDriftScore;
			PlayerPrefs.SetFloat ("bestDrift", maxDriftScore);
			maxDriftLabel.GetComponent<TweenFontSize>().StartTween();
		}
		
		driftScore = 0;
		multiplier = 0;
	}
	
	public void RestoreDrift()
	{
		canDrift = true;
		collisionTemp = collisionTempInit;
	}
	
	public void ResetExtraMessage()
	{
		canShowExtra = true;
	}
	
	public void GiveExtraAvoid()
	{
		if(canShowExtra)
		{
			if(carEngine.SpeedAsKM > avoidMinSpeed)
			{
				canShowExtra = false;
				driftScore += (int)extraAvoid;
				extraTween.GetComponent<Text>().text = "That was close! +" + extraAvoid;
				extraTween.SetTweenState(0);
			}
		}
	}
	#endregion
	
	#region UI Methods
	public void FadeIn()
	{
		for(int i = 0; i < labelsUI.Length; i++)
		{
			auxColor = labelsUI[i].color;
			
			if(auxColor.a < 1.0f)
			{
				auxColor.a += fadeSpeed * Time.deltaTime;
			}
			else
			{
				auxColor.a = 1.0f;
			}
			
			labelsUI[i].color = auxColor;
		}
	}
	
	public void FadeOut()
	{
		for(int i = 0; i < labelsUI.Length; i++)
		{
			auxColor = labelsUI[i].color;
			
			if(auxColor.a > 0.0f)
			{
				auxColor.a -= fadeSpeed * Time.deltaTime;
			}
			else
			{
				auxColor.a = 0.0f;
				SetUIColor (initColor);
			}
			
			labelsUI[i].color = auxColor;
		}
	}
	
	public void MoveDown()
	{
		for(int i = 0; i < labelsUI.Length; i++)
		{
			labelsUI[i].transform.Translate (0, -Time.deltaTime * speedUI, 0);
		}
	}
	
	public void ResetPosition()
	{
		for(int i = 0; i < labelsUI.Length; i++)
		{
			labelsUI[i].transform.position = initPosition[i];
		}
	}
	
	public void SetUIColor(Color inputColor)
	{		
		for(int i = 0; i < labelsUI.Length; i++)
		{
			Color auxColor = inputColor;
			auxColor.a = labelsUI[i].color.a;
			labelsUI[i].color = auxColor;
		}
	}		
	#endregion
	
	#region Time Challenge Methods
	private void FinishChallenge()
	{
		canContinue = false;
		totalScore += driftScore;
		
		if(driftScore > maxDriftScore)
		{
			maxDriftScore = driftScore;
			dataManager.bestDrift = maxDriftScore;
			PlayerPrefs.SetFloat ("bestDrift", maxDriftScore);
		}
		
		driftScore = 0;
		multiplier = 0;
		
		// Change game state
		ended = true;
		
		gameplayUI.SetActive (false);
		
		actualCamera = carEngine.GetComponent<CameraRig>().cameraTransform[carEngine.GetComponent<CameraRig>().activeCamera].GetComponent<BlurOptimized>();
		actualCamera.enabled = true;
		pauseAnimation = true;
		
		// Disable Player controller
		carController.enabled = false;
		carEngine.brake = 1;
		carEngine.throttle = 0;
		carEngine.steer = 0;
		
		Invoke ("EnableEndUI", 2.0f);
	}
	
	private void EnableEndUI()
	{
		// Disable car audio
		carAudio.DisableSources();
		
		// Enable end UI
		bloomUI.enabled = true;
		backgroundTween.SetTween(1);
		gameplayUI.SetActive (false);
		pauseUI.SetActive (false);
		endUI.SetActive (true);

        timeOverSource.enabled = true;
        timeOverSource.Play();
    }
	
	private string FormatTime(float TimeValue, bool ShowFraction, float FractionDecimals, bool FractionMinutes = false)
	{		
		strReturn = "00:00:00";
		
		if (!ShowFraction) strReturn = "00:00";
		
		if (TimeValue > 0)
		{
			TimeSpan tTime = TimeSpan.FromSeconds(TimeValue);
			
			float minutes = tTime.Minutes;
			
			if(minutes > 9 && FractionMinutes) minutes = 9;
			float seconds = tTime.Seconds;
			
			float fractions = (TimeValue * 100) % 100;
			if (fractions >= 99) fractions = 0;
			
			if (ShowFraction)
			{
				if (FractionDecimals == 2)
				{
					strReturn = String.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, fractions);
				}
				else
				{ 
					strReturn = String.Format("{0:00}:{1:00}:{2:0}", minutes, seconds, fractions);
				}
			}
			else
			{
				strReturn = String.Format("{0:00}.{1:00}", minutes, seconds);
			}
		} 
		return strReturn;
	}
	#endregion
	
	#region Online Methods
	
	#endregion
	
	#region Pickup Methods
	public void AddMultiplier(int amount)
	{
		multiplier += amount;
	}
	
	public void AddNitro(float amount)
	{
		carSetup.NitroLeft += amount;
	}
	#endregion
	
	#region Properties
	public int TotalScore
	{
		get { return totalScore; }
	}
	
	public CustomNetworkManager Network
	{
		get { return network; }
	}
	
	public float LeftTime
	{
		get { return leftTime; }
		set { leftTime = value; }
	}
	#endregion
}