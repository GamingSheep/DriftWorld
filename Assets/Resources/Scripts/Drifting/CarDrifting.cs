using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;
using System;

public class CarDrifting: MonoBehaviour 
{
	#region Enums
	public enum GameType { FREE, TIME, TUTORIAL };
	#endregion
	
	#region Public Attributes
	[Header("Drift Attributes")]
	public GameType gameType;
	public float driftValue;
	public float minDriftRatio;
	
	[Header("Time Challenge Attributes")]
	public float leftTimeInit;
	public float challengeLabelSpeed;
	
	[Header("Extra Attributes")]
	public int extraNitro;
	public int extraAvoid;
	public float avoidMinSpeed;
	public float avoidTempInit;
	
	[Header("Temp Attributes")]
	public float multiplierTempInit;
	public float collisionTempInit;
	
	[Header("UI Attributes")]
	public bool started;
	public bool pause;
	public bool ended;
	public bool settings;
	public float fadeSpeed;
	public float initAlpha;
	public Color initColor;
	public Color resetColor;
	public float speedUI;
	
	[Header("UI Sounds")]
	public AudioClip pauseClip;
	public AudioClip selectClip;
	
	[Header("Tutorial Attributes")]
	public Camera[] freeTutorialCameras;
	public GameObject[] freeTutorialLabels;
	public Camera[] challengeTutorialCameras;
	public GameObject[] challengeTutorialLabels;
	public float stateDuration;
	public bool tutorial;
	public bool canAdvanceTutorial;
	
	[Header("Drift Tutorial Attributes")]
	public bool driftTutorial;
	public GameObject[] driftTutorialLabels;
	
	#endregion
	
	#region Private Attributes
	private Vector3 rigidbodyVelocity;
	private float leftTime;
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
	private bool pressedExit;
	private bool checkingGamepad;
	private Text extraTweenText;
	private TweenFontSize challengeLabelTween;
	private Rigidbody carRigidbody;
	
	// Time challenge
	private int timePickupsCount;
	private int currentTimePickup;
	private GameObject[] timePickups;
	
	// Tutorials
	private int tutorialState;
	private float tutorialTemp;
	
	// Drift tutorial
	private int driftTutorialState;
	private bool driftTutorialDriftDone;
	private bool driftTutorialObjective;
	private float previousTimeScale;
	
	// Online
	private string playerName;
	
	// Debug
	private bool disabledUI;
	#endregion
	
	#region References	
	private CarEngine carEngine;
	private CarSetup carSetup;
	private CarController carController;
	private CarAudio carAudio;
	private SettingsManager settingsManager;
	private CameraIntroAnimation introAnimation;
	private int challengeTextState = 0;
	private Bloom bloomUI;
	
	// Time challenge
	[Header("Time Challenge References")]
	public GameObject timePickupRoot;
	
	[Header("Main UI References")]
	public GameObject startUI;
	public GameObject gamepadUI;
	public GameObject tutorialUI;
	public GameObject gameplayUI;
	public GameObject timeChallengeUI;
	public GameObject driftTutorialUI;
	public GameObject endUI;
	public GameObject pauseUI;
	public GameObject settingsUI;
	public GameObject exitUI;
	public AudioSource audioSource;
	public AudioManager audioManager;
	
	[Header("UI Text References")]
	public Text driftLabel;
	public Text maxDriftLabel;
	public Text totalLabel;
	public Text multiplierLabel;
	public Text challengeLabel;
	public Text endLabel;
	public Text[] labelsUI;
	
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
	
	[Header("Time Challenge References")]
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
	private void Start () 
	{
		volumeFadeIn = true;
		AudioListener.volume = 0.0f;
		
		// Initialize values
		pauseAnimation = false;
		pause = false;
		started = false;
		settings = false;
		isDrifting = false;
		canContinue = false;
		canDrift = true;
		canShowExtra = true;
		avoiding = false;
		pressedExit = false;
		tutorial = false;
		canAdvanceTutorial = true;
		driftTutorial = false;
		driftTutorialObjective = false;
		driftTutorialDriftDone = false;
		checkingGamepad = false;
		disabledUI = false;
		totalScore = 0;
		driftScore = 0;
		maxDriftScore = 0;
		multiplier = 0;
		challengeTextState = 0;
		tutorialState = 0;
		driftTutorialState = 0;
		playerName = "";
		previousTimeScale = Time.timeScale;
		rigidbodyVelocity = Vector3.zero;
		multiplierTemp = multiplierTempInit;
		collisionTemp = collisionTempInit;
		avoidTemp = avoidTempInit;
		initPosition = new Vector3[labelsUI.Length];
		dataManager = DataManager.Instance;
		carInput = CarInput.Instance;
		
		// Initialize time challenge values
		leftTime = leftTimeInit;
		timePickupsCount = timePickupRoot.transform.childCount;
		currentTimePickup = 0;
		
		timePickups = new GameObject[timePickupsCount];
		
		for(int i = 0; i < timePickups.Length; i++)
		{
			timePickups[i] = timePickupRoot.transform.GetChild (i).gameObject;
		}
		
		for(int i = 0; i < timePickups.Length; i++)
		{
			timePickups[i].GetComponent<DriftPickup>().DisablePickupForMinimap();
		}
		
		timeChallengeUI.SetActive (false);
		
		for(int i = 0; i < freeTutorialLabels.Length; i++)
		{
			freeTutorialCameras[i].enabled = false;
			freeTutorialLabels[i].SetActive (false);	
		}
		
		for(int i = 0; i < challengeTutorialCameras.Length; i++)
		{
			challengeTutorialCameras[i].enabled = false;
			challengeTutorialLabels[i].SetActive (false);	
		}
		
		// Get player references
		carController = GameObject.FindWithTag ("Player").GetComponent<CarController>();
		carEngine = carController.GetComponent<CarEngine>();
		carSetup = carController.GetComponent<CarSetup>();
		carAudio = carController.GetComponent<CarAudio>();
		carRigidbody = carController.GetComponent<Rigidbody>();
		introAnimation = GetComponent<CameraIntroAnimation>();
		settingsManager = GetComponent<SettingsManager>();
		extraTweenText = extraTween.GetComponent<Text>();
		challengeLabelTween = challengeLabel.GetComponent<TweenFontSize>();
		bloomUI = GameObject.Find ("UICamera").GetComponent<Bloom>();
		
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
		
		pauseUI.SetActive (false);
		settingsUI.SetActive (false);
		gameplayUI.SetActive (false);
		startUI.SetActive(false);
		bloomUI.enabled = false;
		
		carController.enabled = false;
		carEngine.enabled = false;
        carSetup.enabled = false;
        carRigidbody.isKinematic = true;
        //carRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        // Disable car audio
        carAudio.EnableSources(0.5f);
		
		EventSystem.current.SetSelectedGameObject(null);
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
		// Debug inputs
		if(Input.GetKeyDown (KeyCode.F2))
		{
			dataManager.DeleteData();
		}
		
		if(Input.GetKeyDown (KeyCode.F3))
		{
			disabledUI = !disabledUI;
			gameplayUI.SetActive (disabledUI);
		}
		
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
				carRigidbody.velocity = Vector3.zero;
				
				if(pressedExit)
				{
					if(carInput.Pause () || carInput.Cancel ())
					{
						// Back from exit menu
						pressedExit = false;
						exitUI.SetActive (false);
						ActiveMainMenu ();
						
						// Play pause sound
						audioSource.clip = pauseClip;
						if(!audioSource.isPlaying)
						{
							audioSource.Play ();
						}
					}
					
					if(carInput.Submit ())
					{
						// Play pause sound
						audioSource.clip = selectClip;
						if(!audioSource.isPlaying)
						{
							audioSource.Play ();
						}
					}
				}
				else
				{
					if(introAnimation.state == 0)
					{
						if(checkingGamepad)
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
								if(carInput.Pause ())
								{
									// Go to settings from main menu
									SetSettings ();
									
									// Play pause sound
									audioSource.clip = selectClip;
									if(!audioSource.isPlaying)
									{
										audioSource.Play ();
									}
								}
								
								if(carInput.Camera())
								{
									SetCredits();
									
									// Play pause sound
									audioSource.clip = selectClip;
									if(!audioSource.isPlaying)
									{
										audioSource.Play ();
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if(driftTutorial)
				{
					if(driftTutorialState == 0 && fade.GetComponent<Image>().color.a < 0.1f)
					{
						// Skip drift film
						if(carInput.Submit ())
						{
							canAdvanceTutorial = false;
							if(IsInvoking ("FinishFilm"))
							{
								CancelInvoke ("FinishFilm");
								FinishFilm();
								
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
						if(pause)
						{
							if(carInput.Pause () || carInput.Cancel ())
							{
								if(settings)
								{
									// Go to pause menu from settings menu in drift tutorial
									SetPause (true);
									
									// Play pause sound
									audioSource.clip = pauseClip;
									if(!audioSource.isPlaying)
									{
										audioSource.Play ();
									}
								}
								else
								{
									// Go to gameplay from drift tutorial pause menu
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
							if(carInput.Pause ())
							{
								// Set pause in drift tutorial from gameplay
								SetPause (true);
								
								// Play pause sound
								audioSource.clip = pauseClip;
								if(!audioSource.isPlaying)
								{
									audioSource.Play ();
								}
							}
						}
					}
				}
				else
				{
					if(tutorial)
					{
						if(!carRigidbody.isKinematic)
						{
							carRigidbody.isKinematic = true;
						}
						
						if(carInput.Submit ())
						{
							// Skip next state in tutorial
							if(canAdvanceTutorial && fade.GetComponent<Image>().color.a < 0.1f)
							{
								canAdvanceTutorial = false;
								tutorialState++;
								SetTutorial ();
								Debug.Log ("CarDrifting: skip to next tutorial state");
								
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
								Debug.Log ("CarDrifting: you cannot pause when you finished time challenge");
							}
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
			if(tutorial)
			{
				tutorialTemp -= Time.deltaTime;
				if(tutorialTemp <= 0)
				{
					tutorialState++;
					SetTutorial ();
				}
			}
			else
			{
				if(driftTutorial)
				{
					switch(driftTutorialState)
					{
						case 1:
						{
							if(carEngine.SpeedAsKM > 30)
							{
								driftTutorialState++;
								Time.timeScale = 0.5f;
								SetDriftTutorial ();
							}
							break;
						}
						case 2:
						{
							if(isDrifting)
							{
								driftTutorialDriftDone = true;
								driftTutorialState++;
								Time.timeScale = 0.5f;
								SetDriftTutorial ();
							}
							break;
						}
						case 3:
						{
							if(!isDrifting && !canContinue)
							{
								if(driftTutorialDriftDone)
								{
									driftTutorialState++;
									Time.timeScale = 1.0f;
									SetDriftTutorial ();
								}
							}
							break;
						}
						case 4:
						{
							if(!IsInvoking("LastDriftTutorialObjective"))
							{
								Invoke ("LastDriftTutorialObjective", 5.0f);
							}
							break;
						}
						case 5:
						{
							if(multiplier >= 10)
							{
								driftTutorialObjective = true;
							}
							
							if(!isDrifting && !canContinue)
							{
								if(driftTutorialObjective)
								{
									driftTutorialState++;
									SetDriftTutorial ();
								}
							}
							break;
						}
					}
				}
							
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
					
					totalLabel.text = "Total: " + totalScore.ToString ();
					maxDriftLabel.text = "Best Drift: " + maxDriftScore.ToString ();
				}
			}
		}
		#endregion
		
		#region Time Challenge Logic
		if(gameType == GameType.TIME)
		{
			if(started)
			{
				if(!pause && !tutorial)
				{
					if(ended)
					{
						if(carInput.Cancel ())
						{
							SetRestart ();
						}
						
						if(carInput.Pause ())
						{
							SetRestart ();
						}
						
						// Update label value
						leftTime = 0.0f;
						challengeTextState = 0;
						challengeLabelTween.enabled = false;
						challengeLabel.color = Color.red;
						challengeLabel.text = "Time: " + FormatTime(leftTime, true, 2);
					}
					else
					{
						leftTime -= Time.deltaTime;
						
						switch(challengeTextState)
						{
							case 1:
							{
								auxColor = challengeLabel.color;
							
								if(challengeLabel.color.g > 0)
								{
									auxColor.g -= Time.deltaTime * challengeLabelSpeed;
									auxColor.b -= Time.deltaTime * challengeLabelSpeed;
								}
								else
								{
									auxColor.g = 0.0f;
									auxColor.b = 0.0f;
									auxColor.r = 1.0f;
									challengeTextState = 2;
								}
								
								challengeLabel.color = auxColor;
								break;
							}
							case 2:
							{
								auxColor = challengeLabel.color;
								
								if(challengeLabel.color.g < 1.0f)
								{
									auxColor.g += Time.deltaTime * challengeLabelSpeed;
									auxColor.b += Time.deltaTime * challengeLabelSpeed;
								}
								else
								{
									auxColor.g = 1.0f;
									auxColor.b = 1.0f;
									auxColor.r = 1.0f;
									challengeTextState = 1;
								}
								
								challengeLabel.color = auxColor;
								break;
							}
						}
						
						if(leftTime < 10)
						{
							if(challengeTextState < 1)
							{
								leftTimeSource.enabled = true;
								challengeLabelTween.enabled = true;
								challengeTextState = 1;
							}
						}
						else
						{
							if(challengeTextState > 0)
							{
								leftTimeSource.enabled = false;
								challengeTextState = 0;
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
	
	#region Gameplay Methods
	public void CheckGamepad()
	{
		if(!carInput.PlayerIndexSet)
		{
			if(carInput.CheckForGamepad())
			{
				if(!dataManager.isGamepad)
				{
					Debug.Log ("CarDrifting: gamepads found, enabling gamepad setting menu");
					checkingGamepad = true;
					gamepadUI.SetActive(true);
					EventSystem.current.SetSelectedGameObject(gamepadUI.transform.GetChild(0).gameObject);
				}
				else
				{
					Debug.Log ("CarDrifting: input configuration is already set to gamepad");
					ActiveMainMenu ();
				}
			}
			else
			{
				if(dataManager.isGamepad)
				{
					Debug.Log ("CarDrifting: input saved data was for gamepad and there was any found, switching to keyboard");
					settingsManager.SetController(false);
					dataManager.SaveData();
				}
				
				Debug.Log ("CarDrifting: no gamepads found, enabling main menu");
				ActiveMainMenu();
			}
		}
		else
		{
			Debug.Log ("CarDrifting: it is not first time, avoid gamepad message");
			ActiveMainMenu();
		}
	}
	
	public void SetGamepad()
	{
		settingsManager.SetController(true);
		dataManager.SaveData();
		ActiveMainMenu();
		Debug.Log ("CarDrifting: switching configuration for gamepad and enabling main menu");
	}
	
	public void ActiveMainMenu()
	{
		settings = false;
		checkingGamepad = false;
		gamepadUI.SetActive (false);
		settingsUI.SetActive (false);
		startUI.SetActive(true);
		backgroundTween.SetTween(1);
		bloomUI.enabled = true;
		EventSystem.current.SetSelectedGameObject(startUI.transform.GetChild(0).gameObject);
		
		// Fix objects active state
		carSetup.NosFireObject.SetActive (false);
		carSetup.ExhaustBackfire.SetActive (false);
		
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
	}
	
	public void SetStart(int type)
	{
		pause = false;
		settings = false;
		startUI.SetActive (false);
		backgroundTween.SetTween(2);
		introAnimation.SetAnimation(2);
		UnityEngine.Cursor.visible = false;
		bloomUI.enabled = false;
		
		gameType = (GameType)type;
		
		audioManager.SetGameplayMusic();
		
		// Play pause sound
		audioSource.clip = selectClip;
		if(!audioSource.isPlaying)
		{
			audioSource.Play ();
		}
		
		if(gameType == GameType.TIME)
		{
			timeChallengeUI.SetActive (true);
			InitializeChallengePickups();
		}
		else
		{
			timeChallengeUI.SetActive (false);
			
			for(int i = 0; i < timePickups.Length; i++)
			{
				timePickups[i].GetComponent<DriftPickup>().DisablePickupForMinimap();
			}
			
			if(gameType == GameType.TUTORIAL)
			{
				Debug.Log ("CarDrifting: drift tutorial selected");
			}
		}	
	}
	
	public void SetPause(bool isPaused)
	{
		pause = isPaused;
		
		if(isPaused)
		{
			// Save previous time scale for drift tutorial
			previousTimeScale = Time.timeScale;
			Time.timeScale = 1.0f;
			
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
			// Restore previous time scale
			Time.timeScale = previousTimeScale;
						
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
		
		switch(gameType)
		{
			case GameType.FREE:
			{
				if(!dataManager.freeTutorial)
				{
					tutorial = true;
					SetTutorial ();
					tutorialUI.SetActive(true);
					driftTutorialUI.SetActive (false);
					tutorialUI.transform.GetChild(0).gameObject.SetActive(true);
					Debug.Log ("CarDrifting: the player didn't see the tutorial, start it");
				}
				else
				{
					driftTutorialUI.SetActive (false);
					tutorialUI.SetActive (false);
					UnfreezePlayer ();
				}
				break;
			}
			case GameType.TIME:
			{
				if(!dataManager.challengeTutorial)
				{
					tutorial = true;
					SetTutorial ();
					tutorialUI.SetActive(true);
					driftTutorialUI.SetActive (false);
					tutorialUI.transform.GetChild(1).gameObject.SetActive(true);
					Debug.Log ("CarDrifting: the player didn't see the tutorial, start it");
				}
				else
				{
					driftTutorialUI.SetActive (false);
					tutorialUI.SetActive (false);
					UnfreezePlayer ();
				}
				break;
			}
			case GameType.TUTORIAL:
			{
				driftTutorial = true;
				driftTutorialUI.SetActive (true);
				tutorialUI.SetActive (false);
				SetDriftTutorial();
				Debug.Log ("CarDrifting: starting drift tutorial");
				break;
			}
		}
	}
	
	public void UnfreezePlayer()
	{
        carSetup.enabled = true;
        carEngine.enabled = true;
		carController.enabled = true;
		tutorialUI.SetActive (false);
		gameplayUI.SetActive (true);
		bloomUI.enabled = false;
		
		carRigidbody.isKinematic = false;
        //carRigidbody.constraints = RigidbodyConstraints.None;

        if (gameType == GameType.TUTORIAL)
		{
			driftTutorialUI.SetActive (true);
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
		// Disable main scripts
		pause = true;
		carController.enabled = false;
		carEngine.enabled = false;
		carEngine.GetComponent<Rigidbody>().isKinematic = true;
		//pauseUI.SetActive (true);
		gameplayUI.SetActive (false);
		backgroundTween.SetTween(1);
		EventSystem.current.SetSelectedGameObject(null);
		bloomUI.enabled = true;
		
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
	
	public void SetCredits()
	{
		// Disable main scripts
		pause = true;
		carController.enabled = false;
		carEngine.enabled = false;
		carEngine.GetComponent<Rigidbody>().isKinematic = true;
		//pauseUI.SetActive (true);
		gameplayUI.SetActive (false);
		backgroundTween.SetTween(1);
		EventSystem.current.SetSelectedGameObject(null);
		bloomUI.enabled = true;
		
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
		Invoke ("GoCredits", 1 / fade.speed);	
	}
	
	public void SetMultiplayer()
	{
		// Disable main scripts
		pause = true;
		carController.enabled = false;
		carEngine.enabled = false;
		carEngine.GetComponent<Rigidbody>().isKinematic = true;
		//pauseUI.SetActive (true);
		gameplayUI.SetActive (false);
		backgroundTween.SetTween(1);
		EventSystem.current.SetSelectedGameObject(null);
		bloomUI.enabled = true;
		
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
		Invoke ("GoMultiplayer", 1 / fade.speed);	
	}
	
	public void SetExit(int value)
	{
		switch(value)
		{
			case -1:
			{
				pressedExit = false;
				exitUI.SetActive(false);
				ActiveMainMenu();
				EventSystem.current.SetSelectedGameObject(startUI.transform.GetChild(0).gameObject);
				break;
			}
			case 0:
			{
				pressedExit = true;
				startUI.SetActive (false);
				exitUI.SetActive(true);
				EventSystem.current.SetSelectedGameObject(exitUI.transform.GetChild(1).gameObject);
				break;
			}
			case 1:
			{
				dataManager.SaveData ();
				EventSystem.current.SetSelectedGameObject(null);
				
				fade.gameObject.SetActive(true);
				fade.SetTween (1);
					
				Invoke ("ExitGame", 3.5f);
				break;
			}	
		}	
	}
	
	private void ExitGame()
	{
		Application.Quit();
	}
	
	private void ChangeLevel()
	{
		Application.LoadLevel (Application.loadedLevel);
	}
	
	private void GoMultiplayer()
	{
		// Load Credits scene
		Application.LoadLevel ("OnlineScene");
	}
	
	private void GoCredits()
	{
		// Load Credits scene
		Application.LoadLevel ("Credits");
	}
	#endregion
	
	#region Tutorial Methods
	private void SetTutorial()
	{
		fade.gameObject.SetActive (true);
		tutorialTemp = stateDuration;
		bloomUI.enabled = true;
		switch(gameType)
		{
			case GameType.FREE:
			{
				if(tutorialState < freeTutorialCameras.Length)
				{
					fade.SetTween (1);
					Invoke ("ResetFade", 4.0f);
					Invoke ("ChangeFreeTutorialCamera", 2.0f);
				}
				else
				{
					fade.SetTween (1);
					Invoke ("ResetFade", 4.0f);
					Invoke ("CallFinishFreeTutorial", 2.0f);
				}
				break;
			}
			case GameType.TIME:
			{
				if(tutorialState < challengeTutorialCameras.Length)
				{
					fade.SetTween (1);
					Invoke ("ResetFade", 4.0f);
					Invoke ("ChangeChallengeTutorialCamera", 2.0f);
				}
				else
				{
					fade.SetTween (1);
					Invoke ("ResetFade", 4.0f);
					Invoke ("CallFinishChallengeTutorial", 2.0f);
				}
				break;
			}
		}
		
		// Disable car audio
		carAudio.DisableSources();
	}
	
	private void ChangeFreeTutorialCamera()
	{
		for(int i = 0; i < freeTutorialLabels.Length; i++)
		{
			freeTutorialCameras[i].enabled = false;
			freeTutorialLabels[i].SetActive (false);	
		}
		
		freeTutorialCameras[tutorialState].enabled = true;
		freeTutorialLabels[tutorialState].SetActive (true);
	}
	
	private void CallFinishFreeTutorial()
	{
		for(int i = 0; i < freeTutorialLabels.Length; i++)
		{
			freeTutorialCameras[i].enabled = false;
			freeTutorialLabels[i].SetActive (false);	
		}
		
		FinishTutorial ((int)gameType);
	}
	
	private void ChangeChallengeTutorialCamera()
	{
		for(int i = 0; i < challengeTutorialCameras.Length; i++)
		{
			challengeTutorialCameras[i].enabled = false;
			challengeTutorialLabels[i].SetActive (false);	
		}
		
		challengeTutorialCameras[tutorialState].enabled = true;
		challengeTutorialLabels[tutorialState].SetActive (true);
	}
	
	private void CallFinishChallengeTutorial()
	{
		for(int i = 0; i < challengeTutorialCameras.Length; i++)
		{
			challengeTutorialCameras[i].enabled = false;
			challengeTutorialLabels[i].SetActive (false);	
		}
		
		FinishTutorial ((int)gameType);
	}
	
	private void FinishTutorial(int type)
	{
		if(type == 0)
		{
			dataManager.freeTutorial = true;
			Debug.Log ("CarDrifting: free tutorial finished, unfreeze player");
		}
		else if(type == 1)
		{
			dataManager.challengeTutorial = true;
			Debug.Log ("CarDrifting: free tutorial finished, unfreeze player");
		}
		
		canAdvanceTutorial = true;
		tutorial = false;
		bloomUI.enabled = false;
		
		dataManager.SaveData();
		UnfreezePlayer();
	}
	
	private void ResetFade()
	{
		canAdvanceTutorial = true;
		fade.SetTween (2);
	}
	#endregion
	
	#region Drift Tutorial Methods
	private void LastDriftTutorialObjective()
	{
		driftTutorialState++;
		Time.timeScale = 1.0f;
		SetDriftTutorial ();
	}
	
	private void SetDriftTutorial()
	{
		if(driftTutorialState < driftTutorialLabels.Length)
		{
			if(driftTutorialState == 0)
			{
				fade.gameObject.SetActive (true);
				fade.SetTween (1);
				Invoke ("StartDriftFilm", 2.0f);
				Invoke ("ResetFade", 4.0f);
			}
			else
			{
				if(!carController.enabled)
				{
					driftTutorialUI.SetActive (true);
					UnfreezePlayer();
				}
				
				for(int i = 0; i < driftTutorialLabels.Length; i++)
				{
					driftTutorialLabels[i].SetActive (false);	
				}
				
				driftTutorialLabels[driftTutorialState].SetActive (true);
			}
		}
		else
		{
			Debug.Log ("CarDrifting: drift tutorial finished");
			SetRestart ();
		}
	}
	
	private void StartDriftFilm()
	{
		Debug.Log ("CarDrifting: enabling tutorial film");
		Invoke ("FinishFilm", 10.0f);
	}
	
	private void FinishFilm()
	{
		fade.gameObject.SetActive (true);
		fade.SetTween (1);
		Invoke ("NextFilm", 2.0f);
		Invoke ("ResetFade", 4.0f);
	}
	
	private void NextFilm()
	{
		driftTutorialState++;
		SetDriftTutorial();
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
		totalLabel.GetComponent<TweenFontSize>().StartTween();
		
		if(driftScore > maxDriftScore)
		{
			maxDriftScore = driftScore;
			dataManager.bestDrift = maxDriftScore;
			PlayerPrefs.SetInt ("bestDrift", maxDriftScore);
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
		timeOverSource.enabled = true;
		timeOverSource.Play ();
		canContinue = false;
		totalScore += driftScore;
		
		if(driftScore > maxDriftScore)
		{
			maxDriftScore = driftScore;
			dataManager.bestDrift = maxDriftScore;
			PlayerPrefs.SetInt ("bestDrift", maxDriftScore);
		}
		
		driftScore = 0;
		multiplier = 0;
		
		// Change game state
		ended = true;
				
		gameplayUI.SetActive (false);
		
		actualCamera = carEngine.GetComponent<CameraRig>().cameraTransform[carEngine.GetComponent<CameraRig>().activeCamera].GetComponent<BlurOptimized>();
		actualCamera.enabled = true;
		pauseAnimation = true;
		
		endLabel.text = totalScore.ToString ();
		
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
		//gameplayUI.SetActive (false);
		pauseUI.SetActive (false);
		endUI.SetActive (true);
		endUI.transform.GetChild(0).gameObject.SetActive(true);
		endUI.transform.GetChild(1).gameObject.SetActive(false);
		EventSystem.current.SetSelectedGameObject(endUI.transform.GetChild(0).transform.GetChild (3).transform.GetChild (1).gameObject);
	}
	
	public void NextTimePickup()
	{
		int auxCurrent = currentTimePickup;
		
		timePickups[currentTimePickup].GetComponent<DriftPickup>().DisablePickupForMinimap();
		
		while(auxCurrent == currentTimePickup)
		{
			currentTimePickup = (int)UnityEngine.Random.Range (0, timePickupsCount - 1);
		}
		
		timePickups[currentTimePickup].GetComponent<DriftPickup>().EnablePickup();
	}
	
	public void InitializeChallengePickups()
	{
		for(int i = 0; i < timePickups.Length; i++)
		{
			timePickups[i].GetComponent<DriftPickup>().DisablePickupForMinimap();
		}
		
		timePickups[currentTimePickup].GetComponent<DriftPickup>().EnablePickup();
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
	public void UpdatePlayerName(string name)
	{
		playerName = name;
		GameObject.Find ("EndUI").GetComponent<OnlineManager>().playerName = name;
	}
	
	public void SendTimeChallenge()
	{
		if(playerName != "")
		{
			endUI.GetComponent<OnlineManager>().CallPostScores();
			endUI.transform.GetChild (0).gameObject.SetActive (false);
			endUI.transform.GetChild (1).gameObject.SetActive (true);
			EventSystem.current.SetSelectedGameObject(endUI.transform.GetChild (1).transform.GetChild (0).gameObject);	
			Invoke ("GetTimeChallenge", 1.0f);
			
			audioSource.clip = selectClip;
			if(!audioSource.isPlaying)
			{
				audioSource.Play ();
			}
		}
		else
		{
			audioSource.clip = pauseClip;
			if(!audioSource.isPlaying)
			{
				audioSource.Play ();
			}
		}
	}
	
	private void GetTimeChallenge()
	{
		endUI.GetComponent<OnlineManager>().CallGetScores();
	}
	#endregion
	
	#region Pickup Methods
	public void AddMultiplier(int amount)
	{
		multiplier += amount;
	}
	
	public void AddTime(float amount)
	{
		if(gameType == GameType.TIME)
		{
			if(started && !ended)
			{
				leftTime += amount;
				
				if(leftTime > leftTimeInit)
				{
					leftTime = leftTimeInit;
				}
				
				timeExtraTween.SetTweenState(0);
				
				NextTimePickup();
			}
		}
	}
	
	public void AddTimeAnimation()
	{
		challengeLabel.GetComponent<TweenFontSize>().enabled = true;
		challengeLabel.GetComponent<TweenFontSize>().tweenType = TweenFontSize.TweenType.ONCE;
		challengeLabel.GetComponent<TweenFontSize>().StartTween();
		Invoke ("ResetTimeAnimation", 1.0f);
	}
	
	private void ResetTimeAnimation()
	{
		challengeLabel.GetComponent<TweenFontSize>().tweenType = TweenFontSize.TweenType.ALWAYS;
		
		if(challengeTextState < 1)
		{
			challengeLabel.GetComponent<TweenFontSize>().enabled = false;
			challengeLabel.fontSize = challengeLabel.GetComponent<TweenFontSize>().minValue;
		}	
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
	#endregion
}