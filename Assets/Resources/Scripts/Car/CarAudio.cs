using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CarAudio : MonoBehaviour 
{
	#region Public Attributes
	[Header("Main Attributes")]
	public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
	public float MinDistance = 1;
	public float MaxDistance = 100f;
	public float volumeScale;
	
	[Header("Audio Clips")]
	public AudioClip TurboPopHigh;
	public AudioClip TurboWaste;
	public AudioClip Skid;
	public AudioClip SpeedHiss;
	public AudioClip BrakeDisk;
	public AudioClip BackfireShort;
	public AudioClip BackfireLong;
	public AudioClip Nitro;
	public AudioClip GearChange;
	#endregion
	
	#region Private Attributes
	private AudioSource AudioChannel1;
	private AudioSource AudioChannel2;
	private AudioSource AudioChannelTurbo;
	private AudioSource AudioChannelHiss;
	private AudioSource AudioChannelSkid;
	private AudioSource AudioChannelBackfire;
	private AudioSource AudioChannelNos;
	private AudioSource AudioChannelGearChange;
	private AudioSource AudioChannelBrakeDisk;
	private float EngineThrootle = 0;
	private float EngineRPM = 0f;
	private RPMLevelStage[] rpmLevelStages = new RPMLevelStage[2];
	private bool isPaused;
	private AudioSource[] sources;
	
	private RPMLevelStage rpmLevelStage;
	private RPMLevel rpmLevel;
	
	private float currentRPMDiff;
	private float levelRPMDiff;
	private float rpmTime;
	private float pitchDiff;
	private float stageRPMCoef;
	
	private RPMLevelStage levelStage1;
	private RPMLevelStage levelStage2;
	private GameObject auxObject;
	
	private bool blnIsTurboInRange;
	#endregion
	
	#region RPM Attributes
	[Header("RPM Attributes")]
	[SerializeField]
	private RPMLevel[] RPMLevels;
	
	[SerializeField]
	private AnimationCurve RevUpVolume;
	[SerializeField]
	private AnimationCurve RevDownVolume;
	#endregion
	
	#region References
	private CarEngine CarEngine;
	private CarSetup CarSetup;
	#endregion
	
	#region Main Methods
	public void Awake()
	{
		// Pre-Initialize values
		CarEngine = GetComponent(typeof(CarEngine)) as CarEngine;
		CarSetup = GetComponent(typeof(CarSetup)) as CarSetup;
		
		SetupAudioSources();
		
		for (int i = 0; i < this.RPMLevels.Length; i++)
		{
			this.RPMLevels[i].Source = ((i % 2) != 0) ? this.AudioChannel2 : this.AudioChannel1;
		}
		this.rpmLevelStages[0] = new RPMLevelStage();
		this.rpmLevelStages[1] = new RPMLevelStage();
		
		if (this.AudioChannel1 != null)
		{
			this.AudioChannel1.loop = true;
			this.AudioChannel1.volume = 0f;
		}
		if (this.AudioChannel2 != null)
		{
			this.AudioChannel2.loop = true;
			this.AudioChannel2.volume = 0f;
		}
		
		isPaused = false;
		blnIsTurboInRange = false;
		
		rpmLevelStage = null;
		rpmLevel = null;
		
		currentRPMDiff = 0.0f;
		levelRPMDiff = 0.0f;
		rpmTime = 0.0f;
		pitchDiff = 0.0f;
		stageRPMCoef = 0.0f;
		
		levelStage1 = null;
		levelStage2 = null;
		
		auxObject = null;
		
		sources = GetComponentsInChildren<AudioSource>();
	}
	
	private void Update()
	{
		AudioChannelSkid.volume = Mathf.Clamp01(Mathf.Abs(CarEngine.TotalSlip) * 0.2f - 0.3f) * 0.2f;
	}
	
	public void LateUpdate()
	{
		EngineThrootle = CarEngine.throttle;
		this.EngineRPM = CarEngine.RPM;
		
		for (int i = 0; i < this.RPMLevels.Length; i++)
		{
			
			if ((this.EngineRPM >= this.RPMLevels[i].RpmLow) && (this.EngineRPM < this.RPMLevels[i].RpmHigh))
			{
				if ((i < (this.RPMLevels.Length - 1)) && (this.EngineRPM >= this.RPMLevels[i + 1].RpmLow))
				{
					this.rpmLevelStages[0].RPMLevel = this.RPMLevels[i];
					this.rpmLevelStages[0].RPMLevel.Source.mute = false;
					this.rpmLevelStages[1].RPMLevel = this.RPMLevels[i + 1];
					this.rpmLevelStages[1].RPMLevel.Source.mute = false;
					
				}
				else
				{
					this.rpmLevelStages[0].RPMLevel = this.RPMLevels[i];
					this.rpmLevelStages[1].RPMLevel = null;
					if ((this.rpmLevelStages[0].RPMLevel.Source == this.AudioChannel1) && this.AudioChannel2.isPlaying)
					{
						this.AudioChannel1.mute = false;
						this.AudioChannel2.mute = true;
					}
					else if ((this.rpmLevelStages[0].RPMLevel.Source == this.AudioChannel2) && this.AudioChannel1.isPlaying)
					{
						this.AudioChannel1.mute = true;
						this.AudioChannel2.mute = false;
					}
				}
				break;
			}
		}
		
		for (int j = 0; j < this.rpmLevelStages.Length; j++)
		{
			rpmLevelStage = this.rpmLevelStages[j];
			rpmLevel = rpmLevelStage.RPMLevel;
			
			if (rpmLevel != null)
			{
				currentRPMDiff = Mathf.Clamp(this.EngineRPM, rpmLevel.RpmLow, rpmLevel.RpmHigh);
				levelRPMDiff = rpmLevel.RpmHigh - rpmLevel.RpmLow;
				rpmTime = (currentRPMDiff - rpmLevel.RpmLow) / levelRPMDiff;
				pitchDiff = rpmLevel.PitchMax - rpmLevel.PitchMin;
				
				rpmLevelStage.Pitch = rpmLevel.PitchMin + (pitchDiff * rpmLevel.PitchCurve.Evaluate(rpmTime));
				
				if (EngineThrootle > 0)
				{ 
					rpmLevel.Source.clip = rpmLevel.OnClip; 
				}
				else
				{ 
					rpmLevel.Source.clip = rpmLevel.OffClip; 
				}
				
				rpmLevelStage.Volume = 1f;
				
				if (this.EngineThrootle > 0f)
				{
					rpmLevelStage.Volume = (rpmLevelStage.Volume * this.RevUpVolume.Evaluate(this.EngineRPM / CarSetup.EngineMaxRPM)) * volumeScale;
				}
				else
				{
					rpmLevelStage.Volume = (rpmLevelStage.Volume * this.RevDownVolume.Evaluate(this.EngineRPM / CarSetup.EngineMaxRPM)) * volumeScale;
				}
				
				if (!rpmLevel.Source.isPlaying)
				{
					if(!isPaused)
					{
						rpmLevel.Source.Play();
					}
				}
			}
		}
		
		if ((this.rpmLevelStages[0].RPMLevel != null) && (this.rpmLevelStages[1].RPMLevel != null))
		{
			levelRPMDiff = this.rpmLevelStages[0].RPMLevel.RpmHigh - this.rpmLevelStages[1].RPMLevel.RpmLow;
			stageRPMCoef = (this.EngineRPM - this.rpmLevelStages[1].RPMLevel.RpmLow) / levelRPMDiff;
			this.rpmLevelStages[0].RPMLevel.CurrentFade = this.rpmLevelStages[0].RPMLevel.FadeCurve.Evaluate(1f - stageRPMCoef);
			this.rpmLevelStages[1].RPMLevel.CurrentFade = this.rpmLevelStages[0].RPMLevel.FadeCurve.Evaluate(stageRPMCoef);
			levelStage1 = this.rpmLevelStages[0];
			levelStage1.Volume *= this.rpmLevelStages[0].RPMLevel.CurrentFade;
			levelStage2 = this.rpmLevelStages[1];
			levelStage2.Volume *= this.rpmLevelStages[1].RPMLevel.CurrentFade;
		}
		
		this.rpmLevelStages[0].Update();
		this.rpmLevelStages[1].Update();
		ApplyHiss();
	}
	#endregion
	
	#region Audio Methods		
	public void EnableSources()
	{
		for(int i = 0; i < sources.Length; i++)
		{
			sources[i].enabled = true;
		}
		
		isPaused = false;
	}
	
	public void EnableSources(float scale)
	{
		for(int i = 0; i < sources.Length; i++)
		{
			sources[i].volume = sources[i].volume * scale;
		}
		
		isPaused = false;
	}
	
	public void DisableSources()
	{
		for(int i = 0; i < sources.Length; i++)
		{
			sources[i].enabled = false;
		}
		
		isPaused = true;
	}
	
	private void SetupAudioSources()
	{
		auxObject = new GameObject("audio_channel1");
		auxObject.transform.parent = transform;
		auxObject.transform.localPosition = Vector3.zero;
		auxObject.transform.localRotation = Quaternion.identity;
		auxObject.AddComponent(typeof(AudioSource));
		auxObject.GetComponent<AudioSource>().loop = true;
		auxObject.GetComponent<AudioSource>().volume = 1;
		AudioChannel1 = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
		AudioChannel1.playOnAwake = false;
		AudioChannel1.rolloffMode = RolloffMode;
		AudioChannel1.minDistance = MinDistance;
		AudioChannel1.maxDistance = MaxDistance;
		
		auxObject = new GameObject("audio_channel2");
		auxObject.transform.parent = transform;
		auxObject.transform.localPosition = Vector3.zero;
		auxObject.transform.localRotation = Quaternion.identity;
		auxObject.AddComponent(typeof(AudioSource));
		auxObject.GetComponent<AudioSource>().loop = true;
		auxObject.GetComponent<AudioSource>().volume = 1;
		AudioChannel2 = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
		AudioChannel2.playOnAwake = false;
		AudioChannel2.rolloffMode = RolloffMode;
		AudioChannel2.minDistance = MinDistance;
		AudioChannel2.maxDistance = MaxDistance;
		
		
		if (TurboPopHigh != null)
		{
			auxObject = new GameObject("audio_turbo");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = false;
			auxObject.GetComponent<AudioSource>().volume = 1;
			AudioChannelTurbo = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelTurbo.playOnAwake = false;
			AudioChannelTurbo.rolloffMode = RolloffMode;
			AudioChannelTurbo.minDistance = MinDistance;
			AudioChannelTurbo.maxDistance = MaxDistance;
		}
		
		if (SpeedHiss != null)
		{
			auxObject = new GameObject("audio_hiss");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = true;
			auxObject.GetComponent<AudioSource>().volume = 0;
			auxObject.GetComponent<AudioSource>().pitch = 0;
			auxObject.GetComponent<AudioSource>().clip = SpeedHiss;
			AudioChannelHiss = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelHiss.playOnAwake = true;
			AudioChannelHiss.Play();
			AudioChannelHiss.rolloffMode = RolloffMode;
			AudioChannelHiss.minDistance = MinDistance;
			AudioChannelHiss.maxDistance = MaxDistance;
		}
		
		auxObject = new GameObject("audio_skid");
		auxObject.transform.parent = transform;
		auxObject.transform.localPosition = Vector3.zero;
		auxObject.transform.localRotation = Quaternion.identity;
		auxObject.AddComponent(typeof(AudioSource));
		auxObject.GetComponent<AudioSource>().loop = true;
		auxObject.GetComponent<AudioSource>().volume = 0;
		auxObject.GetComponent<AudioSource>().clip = Skid;
		AudioChannelSkid = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
		AudioChannelSkid.playOnAwake = true;
		AudioChannelSkid.Play();
		AudioChannelSkid.rolloffMode = RolloffMode;
		AudioChannelSkid.minDistance = MinDistance;
		AudioChannelSkid.maxDistance = MaxDistance;
		
		if (BackfireShort != null || BackfireLong != null)
		{
			auxObject = new GameObject("audio_backfire");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = false;
			auxObject.GetComponent<AudioSource>().volume = 1;
			auxObject.GetComponent<AudioSource>().clip = BackfireShort;
			AudioChannelBackfire = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelBackfire.playOnAwake = false;
			AudioChannelBackfire.rolloffMode = RolloffMode;
			AudioChannelBackfire.minDistance = MinDistance;
			AudioChannelBackfire.maxDistance = MaxDistance;
		}
		
		if (Nitro != null)
		{
			auxObject = new GameObject("audio_nitro");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = true;
			auxObject.GetComponent<AudioSource>().volume = 0.35f;
			auxObject.GetComponent<AudioSource>().clip = Nitro;
			AudioChannelNos = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelNos.playOnAwake = false;
			AudioChannelNos.rolloffMode = RolloffMode;
			AudioChannelNos.minDistance = MinDistance;
			AudioChannelNos.maxDistance = MaxDistance;
		}
		
		if (GearChange != null)
		{
			auxObject = new GameObject("audio_gearchange");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = false;
			auxObject.GetComponent<AudioSource>().volume = 1;
			AudioChannelGearChange = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelGearChange.playOnAwake = false;
			AudioChannelGearChange.rolloffMode = RolloffMode;
			AudioChannelGearChange.minDistance = MinDistance;
			AudioChannelGearChange.maxDistance = MaxDistance;
		}
		
		if (BrakeDisk != null)
		{
			auxObject = new GameObject("audio_brakedisk");
			auxObject.transform.parent = transform;
			auxObject.transform.localPosition = Vector3.zero;
			auxObject.transform.localRotation = Quaternion.identity;
			auxObject.AddComponent(typeof(AudioSource));
			auxObject.GetComponent<AudioSource>().loop = true;
			auxObject.GetComponent<AudioSource>().volume = 1;
			auxObject.GetComponent<AudioSource>().clip = BrakeDisk;
			AudioChannelBrakeDisk = auxObject.GetComponent(typeof(AudioSource)) as AudioSource;
			AudioChannelBrakeDisk.playOnAwake = false;
			AudioChannelBrakeDisk.rolloffMode = RolloffMode;
			AudioChannelBrakeDisk.minDistance = MinDistance;
			AudioChannelBrakeDisk.maxDistance = MaxDistance;
		}
		
	}
	
	private void ApplyHiss()
	{
		if (SpeedHiss != null)
		{
			if (CarEngine.SpeedAsKM > 70)
			{
				AudioChannelHiss.mute = false;
				AudioChannelHiss.volume = CarEngine.SpeedAsKM / 150;
				
				if (AudioChannelHiss.volume < 0.4) AudioChannelHiss.volume = 0.4f;
				{
					AudioChannelHiss.pitch = CarEngine.SpeedAsKM / 150;
				}
				if (AudioChannelHiss.pitch < 0.5f) 
				{
					AudioChannelHiss.pitch = 0.5f;
				}
				if (AudioChannelHiss.pitch > 1.5f) 
				{
					AudioChannelHiss.pitch = 1.5f;
				}
			}
			else
			{
				AudioChannelHiss.mute = true;
			}
		}
	}
	
	public void PopTurbo(float TurboValue)
	{
		if (TurboPopHigh != null && TurboWaste != null)
		{
			blnIsTurboInRange = true;
			
			if (TurboValue > 0.9f)
			{
				AudioChannelTurbo.GetComponent<AudioSource>().clip = TurboPopHigh;
			}
			else if (TurboValue >= 0.5f && TurboValue < 0.9f)
			{
				AudioChannelTurbo.GetComponent<AudioSource>().clip = TurboWaste;
			}
			else
			{
				blnIsTurboInRange = false;
			}
			
			if (!AudioChannelTurbo.isPlaying && blnIsTurboInRange)
			{
				if(!isPaused)
				{
					AudioChannelTurbo.Play();
				}
			}
		}
	}
	
	public void PopGear()
	{
		if (GearChange != null && AudioChannelGearChange!=null)
		{
			
			AudioChannelGearChange.GetComponent<AudioSource>().clip = GearChange; 
			
			if(!isPaused)
			{
				AudioChannelGearChange.Play();
			}
		}
		
	}
		
	public void ApplyBrakeDisk(float Brake)
	{
		if (AudioChannelBrakeDisk != null)
		{
			AudioChannelBrakeDisk.volume = Brake;
			
			if (!AudioChannelBrakeDisk.isPlaying && Brake > 0)
			{
				AudioChannelBrakeDisk.Play();
			}
			else if (Brake == 0) 
			{
				AudioChannelBrakeDisk.Stop();
			}
		}
		
	}
		
	public void ApplyNitro(bool IsUsing)
	{
		if (AudioChannelNos != null)
		{
			if (IsUsing && !AudioChannelNos.isPlaying)
			{
				if(!isPaused)
				{
					AudioChannelNos.Play();
				}
			}
			else if(!IsUsing)
			{
				if (AudioChannelNos.isPlaying)
				{
					AudioChannelNos.Stop();
				}
			}
		}
	}
		
	public bool PopBackfire(Boolean IsLong)
	{
		if (AudioChannelBackfire != null)
		{
			if (IsLong)
			{
				AudioChannelBackfire.clip = BackfireLong;
			}
			else
			{
				AudioChannelBackfire.clip = BackfireShort;
			}
			
			if (!AudioChannelBackfire.isPlaying) 
			{
				if(!isPaused)
				{
					AudioChannelBackfire.Play();
				}
			}
			return AudioChannelBackfire.isPlaying;
		}
		else
		{
			return false;
		}
		
	}
		
	public void UpdateSkidSound()
	{
		if(!isPaused)
		{
			AudioChannelSkid.GetComponent<AudioSource>().clip = Skid;
			if (!AudioChannelSkid.isPlaying) 
			{
				if(!isPaused)
				{
					AudioChannelSkid.Play();
				}
			}
		}
	}
	#endregion
	
	#region Serializables Attributes	
	public class RPMLevelStage
	{
		#region Serializable Methods
		public void Update()
		{
			if (this.RPMLevel != null)
			{
				this.RPMLevel.Source.volume = this.Volume;
				this.RPMLevel.Source.pitch = this.Pitch;
			}
		}
		#endregion
		
		#region Serializable Properties
		public float Pitch { get; set; }
		
		public float Volume { get; set; }
		
		public CarAudio.RPMLevel RPMLevel { get; set; }
		#endregion
	}
		
	[Serializable]
	public class RPMLevel
	{
		#region Serializable Attributes
		[SerializeField]
		private AudioClip offSound;
		[SerializeField]
		private AudioClip onSound;
		[SerializeField]
		private float rpmLow;
		[SerializeField]
		private float rpmHigh;
		
		[SerializeField]
		private AnimationCurve fadeCurve;
		[SerializeField]
		private float currentFade;
		
		[SerializeField]
		private AnimationCurve pitchCurve;
		[SerializeField]
		private float pitchMin;
		[SerializeField]
		private float pitchMax;
		#endregion
		
		#region Serializable Properties
		public AnimationCurve FadeCurve
		{
			get
			{
				return this.fadeCurve;
			}
		}
		
		public float CurrentFade
		{
			get
			{
				return this.currentFade;
			}
			set
			{
				this.currentFade = value;
			}
		}
		
		public AudioClip OffClip
		{
			get
			{
				return this.offSound;
			}
		}
		
		public AudioClip OnClip
		{
			get
			{
				return this.onSound;
			}
		}
		
		public AnimationCurve PitchCurve
		{
			get
			{
				return this.pitchCurve;
			}
		}
		
		public float PitchMin
		{
			get
			{
				return this.pitchMin;
			}
		}
		
		public float PitchMax
		{
			get
			{
				return this.pitchMax;
			}
		}
		
		public float RpmLow
		{
			get
			{
				return this.rpmLow;
			}
		}
		
		public float RpmHigh
		{
			get
			{
				return this.rpmHigh;
			}
		}
		
		public AudioSource Source { set; get; }
		#endregion
	}
	#endregion
}