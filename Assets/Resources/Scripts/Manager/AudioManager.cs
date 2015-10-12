using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class AudioManager: MonoBehaviour 
{
	#region Public Attributes
	[Header("Audio Attributes")]
	public AudioSlot[] musicClips;
	public float fadeRange;
	public float fadeSpeed;
	
	[Header("UI Attributes")]
	public GameObject audioUI;
	public Text audioName;
	public Text authorName;
	#endregion
	
	#region Private Attributes
	private int currentClip;
	private float playedValue;
	private float playedValueLength;
	private AudioClip[] streamAudioClip;
	private bool hasExternal;
	private bool _showing;
	#endregion
	
	#region Streaming Attributes
	private string folderPath;
	private string fileType = "*.ogg";
	private string[] filesInfo;
	#endregion
	
	#region References
	[Header("Audio References")]
	public AudioSource audioSource;
	public AudioSource menuAudioSource;
	public AudioSource audioUIsound;
	public SettingsManager settingsManager;
	private CarInput carInput;
	public CarDrifting carDrifting;
	public MultiplayerManager multiplayerManager;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		if(audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		
		if(settingsManager == null)
		{
			settingsManager = transform.root.GetComponent<SettingsManager>();
		}
		
		if(carDrifting == null)
		{
			carDrifting = settingsManager.GetComponent<CarDrifting>();
		}
		
		if(multiplayerManager == null)
		{
			multiplayerManager = settingsManager.GetComponent<MultiplayerManager>();
		}
		
		if(carInput == null)
		{
			carInput = CarInput.Instance;
		}
		
		_showing = false;
		menuAudioSource.enabled = true;
		audioSource.enabled = true;
		
		hasExternal = false;
		folderPath = Application.dataPath + "/Music/";
		
		#if !UNITY_EDITOR
		StartCoroutine(GetStreamMusic());
		#endif
	}
	
	private void Update () 
	{
		// Automatic audio playing logic
		if(audioSource.isPlaying)
		{
			playedValue += Time.deltaTime;
			
			if(hasExternal)
			{
				if(playedValue < fadeRange)
				{
					if(audioSource.volume < 1.0f * settingsManager.GetMusicVolume()) // 
					{
						audioSource.volume += fadeSpeed * Time.deltaTime;
					}
				}
				else
				{
					if(playedValue > playedValueLength - fadeRange)
					{
						if(audioSource.volume > 0.0f)
						{
							audioSource.volume -= fadeSpeed * Time.deltaTime;
						}
					}
				}
			}
			else
			{
				if(playedValue < fadeRange)
				{
					if(audioSource.volume < musicClips[currentClip].musicVolume * settingsManager.GetMusicVolume()) // 
					{
						audioSource.volume += fadeSpeed * Time.deltaTime;
					}
				}
				else
				{
					if(playedValue > playedValueLength - fadeRange)
					{
						if(audioSource.volume > 0.0f)
						{
							audioSource.volume -= fadeSpeed * Time.deltaTime;
						}
					}
				}
			}
		}
		else
		{
			if(!menuAudioSource.enabled)
			{
				if(hasExternal)
				{
					SetMusic (UnityEngine.Random.Range (0, streamAudioClip.Length));
				}
				else
				{
					SetMusic (UnityEngine.Random.Range (0, musicClips.Length));
				}
			}
		}
		
		// Input logic
		if(carInput.Music())
		{
			if(carDrifting != null)
			{
				if(carDrifting.started)
				{
					if(hasExternal)
					{
						SetMusic (UnityEngine.Random.Range (0, streamAudioClip.Length));
					}
					else
					{
						SetMusic (UnityEngine.Random.Range (0, musicClips.Length));
					}
				}
				else
				{
					Debug.Log ("AudioManager: can not change music during main menu");
				}
			}
			
			if(multiplayerManager != null)
			{
				if(multiplayerManager.started)
				{
					if(hasExternal)
					{
						SetMusic (UnityEngine.Random.Range (0, streamAudioClip.Length));
					}
					else
					{
						SetMusic (UnityEngine.Random.Range (0, musicClips.Length));
					}
				}
				else
				{
					Debug.Log ("AudioManager: can not change music during main menu");
				}
			}
		}
	}
	#endregion
	
	#region Audio Methods
	private void SetMusic(int slot)
	{
		if(hasExternal)
		{
			if(slot == currentClip && streamAudioClip.Length > 1)
			{
				SetMusic (UnityEngine.Random.Range (0, streamAudioClip.Length));
			}
			else
			{
				audioSource.clip = streamAudioClip[currentClip];
				audioSource.volume = 1.0f;
				audioSource.Play ();
				
				//SetUI(true);
				//Invoke ("PlaySoundUI", 1.0f);
				
				playedValue = 0.0f;
				playedValueLength = audioSource.clip.length;
				
				Debug.Log ("AudioSource: change music to: " + streamAudioClip[currentClip].name);
			}
		}
		else
		{
			if(slot == currentClip && musicClips.Length > 1)
			{
				SetMusic (UnityEngine.Random.Range (0, musicClips.Length));
			}
			else
			{
				currentClip = slot;	
				
				audioSource.clip = musicClips[currentClip].musicClip;
				audioSource.volume = 0.0f;
				audioSource.Play ();
				
				SetUI(true);
				Invoke ("PlaySoundUI", 1.5f);
				
				playedValue = 0.0f;
				playedValueLength = audioSource.clip.length;
				
				Debug.Log ("AudioSource: change music to " + musicClips[currentClip].musicAuthor + " - " + musicClips[currentClip].musicName);
			}
		}
	}
	#endregion
	
	#region UI Methods
	public void SetUI(bool state)
	{
		if(state)
		{
			_showing = true;
			audioName.text = musicClips[currentClip].musicName;
			authorName.text = musicClips[currentClip].musicAuthor;
			audioUI.SetActive (true);
		}
		else
		{
			_showing = false;
			audioName.text = "";
			authorName.text = "";
			audioUI.SetActive (false);
		}
	}
	
	public void SetGameplayMusic()
	{
		menuAudioSource.enabled = false;
		audioSource.enabled = true;
		SetMusic (UnityEngine.Random.Range (0, musicClips.Length));
	}
	
	private void PlaySoundUI()
	{
		audioUIsound.Play ();
	}
	#endregion
	
	#region Stream Methods
	private IEnumerator GetStreamMusic()
	{
		filesInfo = Directory.GetFiles(folderPath, fileType);
		
		yield return filesInfo;
		
		StartCoroutine(SaveStreamMusic(filesInfo));
	}
	
	private IEnumerator SaveStreamMusic(string[] path)
	{
		streamAudioClip = new AudioClip[path.Length];
		
		for(int i = 0; i < filesInfo.Length; i++)
		{
			WWW request = new WWW("file://" + path[i]);
			
			yield return request;
			
			streamAudioClip[i] = request.GetAudioClip(false, true);
			streamAudioClip[i].name = "external_audio_" + i;
			
			Debug.Log ("AudioManager: loaded external file path: " + path[i]);
		}
		
		if(streamAudioClip.Length > 0)
		{
			hasExternal = true;
		}
		else
		{
			hasExternal = false;
		}
	}
	#endregion
	
	#region Serializable
	[Serializable]
	public class AudioSlot
	{
		#region Serializable Attributes
		[SerializeField]
		private AudioClip _musicClip;
		[SerializeField]
		private float _musicVolume;
		[SerializeField]
		private string _musicName;
		[SerializeField]
		private string _musicAuthor;
		#endregion
		
		#region Properties
		public AudioClip musicClip
		{
			get { return _musicClip; }
			set { _musicClip = value; }
		}
		
		public float musicVolume
		{
			get { return _musicVolume; }
			set { _musicVolume = value; }
		}
		
		public string musicName
		{
			get { return _musicName; }
			set { _musicName = value; }
		}
		
		public string musicAuthor
		{
			get { return _musicAuthor; }
			set { _musicAuthor = value; }
		}
		#endregion
	}
	#endregion
	
	#region Properties
	public bool showing
	{
		get { return _showing; }
		set { _showing = value; }
	}
	#endregion
}