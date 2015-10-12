using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataManager: ScriptableObject 
{
	#region Static Attributes
	private static DataManager instance;
	#endregion
	
	#region Public Attributes
	
	#endregion
	
	#region Private Attributes
	private int _bestDrift;
	private int _resolution;
	private int _quality;
	private bool _antialiasing;
	private bool _bloom;
	private bool _cameraMotionBlur;
	private bool _depthOfField;
	private bool _vignette;
	private bool _ambientOcclusion;
	private bool _colorEffect;
	private float _generalVolume;
	private float _musicVolume;
	private bool _isGamepad;
	private bool _isAutomatic;
	private bool _freeTutorial;
	private bool _challengeTutorial;
	private bool[] _achievements = new bool[2];
	#endregion
	
	#region References
	
	#endregion
	
	#region Singleton Methods
	public DataManager () 
	{
		// Initialize values
		if(PlayerPrefs.HasKey ("bestDrift"))
		{
			_bestDrift = PlayerPrefs.GetInt ("bestDrift");
		}
		else
		{
			_bestDrift = 0;
			PlayerPrefs.SetInt ("bestDrift", _bestDrift);
		}
		
		if(PlayerPrefs.HasKey ("resolution"))
		{
			resolution = PlayerPrefs.GetInt ("resolution");
		}
		else
		{
			_resolution = 0;
			PlayerPrefs.SetInt ("resolution", _resolution);
		}
		
		if(PlayerPrefs.HasKey ("quality"))
		{
			_quality = PlayerPrefs.GetInt ("quality");
		}
		else
		{
			_quality = 0;
			PlayerPrefs.SetInt ("quality", _quality);
		}
		
		if(PlayerPrefs.HasKey ("antialiasing"))
		{
			int aux1 = PlayerPrefs.GetInt ("antialiasing");
			_antialiasing = (aux1 > 0) ? true : false;
		}
		else
		{
			_antialiasing = false;
			PlayerPrefs.SetInt ("antialiasing", Convert.ToInt32(_antialiasing));
		}
		
		if(PlayerPrefs.HasKey ("bloom"))
		{
			int aux2 = PlayerPrefs.GetInt ("bloom");
			_bloom = (aux2 > 0) ? true : false;
		}
		else
		{
			_bloom = false;
			PlayerPrefs.SetInt ("bloom", Convert.ToInt32(_bloom));
		}
		
		if(PlayerPrefs.HasKey ("cameraMotionBlur"))
		{
			int aux3 = PlayerPrefs.GetInt ("cameraMotionBlur");
			_cameraMotionBlur = (aux3 > 0) ? true : false;
		}
		else
		{
			_cameraMotionBlur = false;
			PlayerPrefs.SetInt ("cameraMotionBlur", Convert.ToInt32(_cameraMotionBlur));
		}
		
		if(PlayerPrefs.HasKey ("depthOfField"))
		{
			int aux4 = PlayerPrefs.GetInt ("depthOfField");
			_depthOfField = (aux4 > 0) ? true : false;
		}
		else
		{
			_depthOfField = false;
			PlayerPrefs.SetInt ("depthOfField", Convert.ToInt32(_depthOfField));
		}
		
		if(PlayerPrefs.HasKey ("vignette"))
		{
			int aux5 = PlayerPrefs.GetInt ("vignette");
			_vignette = (aux5 > 0) ? true : false;
		}
		else
		{
			_vignette = false;
			PlayerPrefs.SetInt ("vignette", Convert.ToInt32(_vignette));
		}
		
		if(PlayerPrefs.HasKey ("ambientOcclusion"))
		{
			int aux6 = PlayerPrefs.GetInt ("ambientOcclusion");
			_ambientOcclusion = (aux6 > 0) ? true : false;
		}
		else
		{
			_ambientOcclusion = false;
			PlayerPrefs.SetInt ("ambientOcclusion", Convert.ToInt32(_ambientOcclusion));
		}
		
		if(PlayerPrefs.HasKey ("colorEffect"))
		{
			int aux7 = PlayerPrefs.GetInt ("colorEffect");
            colorEffect = (aux7 > 0) ? true : false;
		}
		else
		{
            _colorEffect = false;
			PlayerPrefs.SetInt ("colorEffect", Convert.ToInt32(_colorEffect));
		}
		
		if(PlayerPrefs.HasKey ("generalVolume"))
		{
			_generalVolume = PlayerPrefs.GetFloat ("generalVolume");
		}
		else
		{
			_generalVolume = 1.0f;
			PlayerPrefs.SetFloat ("generalVolume", _generalVolume);
		}
		
		if(PlayerPrefs.HasKey ("musicVolume"))
		{
			_musicVolume = PlayerPrefs.GetFloat ("musicVolume");
		}
		else
		{
			_musicVolume = 0.236f;
			PlayerPrefs.SetFloat ("musicVolume", _musicVolume);
		}
		
		if(PlayerPrefs.HasKey ("isGamepad"))
		{
			int aux8 = PlayerPrefs.GetInt ("isGamepad");
			_isGamepad = (aux8 > 0) ? true : false;
		}
		else
		{
			_isGamepad = false;
			PlayerPrefs.SetInt ("isGamepad", Convert.ToInt32(_isGamepad));
		}
		
		if(PlayerPrefs.HasKey ("isAutomatic"))
		{
			int aux9 = PlayerPrefs.GetInt ("isAutomatic");
			_isAutomatic = (aux9 > 0) ? true : false;
		}
		else
		{
			_isAutomatic = true;
			PlayerPrefs.SetInt ("isAutomatic", Convert.ToInt32(_isAutomatic));
		}
		
		if(PlayerPrefs.HasKey ("freeTutorial"))
		{
			int aux10 = PlayerPrefs.GetInt ("freeTutorial");
			_freeTutorial = (aux10 > 0) ? true : false;
		}
		else
		{
			_freeTutorial = false;
			PlayerPrefs.SetInt ("freeTutorial", Convert.ToInt32(_freeTutorial));
		}
		
		if(PlayerPrefs.HasKey ("challengeTutorial"))
		{
			int aux11 = PlayerPrefs.GetInt ("challengeTutorial");
			_challengeTutorial = (aux11 > 0) ? true : false;
		}
		else
		{
			_challengeTutorial = false;
			PlayerPrefs.SetInt ("challengeTutorial", Convert.ToInt32(_challengeTutorial));
		}
		
		for(int i = 0; i < _achievements.Length; i++)
		{
			if(PlayerPrefs.HasKey ("achievements" + i))
			{
				int achievementAux = PlayerPrefs.GetInt ("achievements" + i);
				_achievements[i] = (achievementAux > 0) ? true : false;
			}
			else
			{
				_achievements[i] = false;
				PlayerPrefs.SetInt ("achievements" + i, Convert.ToInt32 (_achievements[i]));
			}
		}
		
		// Set up frame rate
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}
	
	static DataManager()
	{
		// Initialize singleton instance
		instance = ScriptableObject.CreateInstance<DataManager>();
	}
	#endregion
	
	#region Data Manager Methods
	public void SaveData()
	{
		switch(Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsWebPlayer:
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.BlackBerryPlayer:
			case RuntimePlatform.Android:
			{
				PlayerPrefs.SetInt ("bestDrift", _bestDrift);
                PlayerPrefs.SetInt("resolution", _resolution);
                PlayerPrefs.SetInt("quality", _quality);
                PlayerPrefs.SetInt("antialiasing", Convert.ToInt32(_antialiasing));
                PlayerPrefs.SetInt("bloom", Convert.ToInt32(_bloom));
                PlayerPrefs.SetInt("cameraMotionBlur", Convert.ToInt32(_cameraMotionBlur));
                PlayerPrefs.SetInt("depthOfField", Convert.ToInt32(_depthOfField));
                PlayerPrefs.SetInt("vignette", Convert.ToInt32(_vignette));
                PlayerPrefs.SetInt("ambientOcclusion", Convert.ToInt32(_ambientOcclusion));
                PlayerPrefs.SetInt("colorEffect", Convert.ToInt32(_colorEffect));
                PlayerPrefs.SetFloat ("generalVolume", _generalVolume);
                PlayerPrefs.SetFloat ("musicVolume", _musicVolume);
                PlayerPrefs.SetInt("isGamepad", Convert.ToInt32(_isGamepad));
                PlayerPrefs.SetInt("isAutomatic", Convert.ToInt32(_isAutomatic));
                PlayerPrefs.SetInt("freeTutorial", Convert.ToInt32(_freeTutorial));
                PlayerPrefs.SetInt("challengeTutorial", Convert.ToInt32(_challengeTutorial));
                
                for(int i = 0; i < _achievements.Length; i++)
                {
                	PlayerPrefs.SetInt ("achievements" + i, Convert.ToInt32 (_achievements[i]));
                }
				break;
			}
		}

        // Show log
		Debug.Log ("DataManager: all data saved successful");
	}
	
	public void DeleteData()
	{
		_freeTutorial = false;
		_challengeTutorial = false;
		
		for(int i = 0; i < _achievements.Length; i++)
		{
			_achievements[i] = false;
		}
		
		SaveData ();
	}
	#endregion
	
	#region Properties
	public static DataManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new DataManager();
			}
			
			return instance; 
		}
	}
	
	public int bestDrift
	{
		get { return _bestDrift; }
		set { _bestDrift = value; }
	}
	
	public int resolution
	{
		get { return _resolution; }
		set { _resolution = value; }
	}
	
	public int quality
	{
		get { return _quality; }
		set { _quality = value; }
	}
	
	public bool antialiasing
	{
		get { return _antialiasing; }
		set { _antialiasing = value; }
	}
	
	public bool bloom
	{
		get { return _bloom; }
		set { _bloom = value; }
	}
	
	public bool cameraMotionBlur
	{
		get { return _cameraMotionBlur; }
		set { _cameraMotionBlur = value; }
	}
	
	public bool depthOfField
	{
		get { return _depthOfField; }
		set { _depthOfField = value; }
	}
	
	public bool vignette
	{
		get { return _vignette; }
		set { _vignette = value; }
	}
	
	public bool ambientOcclusion
	{
		get { return _ambientOcclusion; }
		set { _ambientOcclusion = value; }
	}
	
	public bool colorEffect
    {
		get { return _colorEffect; }
		set { _colorEffect = value; }
	}
	
	public float generalVolume
	{
		get { return _generalVolume; }
		set { _generalVolume = value; }
	}
	
	public float musicVolume
	{
		get { return _musicVolume; }
		set { _musicVolume = value; }
	}
	
	public bool isGamepad
	{
		get { return _isGamepad; }
		set { _isGamepad = value; }
	}
	
	public bool isAutomatic
	{
		get { return _isAutomatic; }
		set { _isAutomatic = value; }
	}
	
	public bool freeTutorial
	{
		get { return _freeTutorial; }
		set { _freeTutorial = value; }
	}
	
	public bool challengeTutorial
	{
		get { return _challengeTutorial; }
		set { _challengeTutorial = value; }
	}
	
	public bool[] achievements
	{
		get { return _achievements; }
		set { _achievements = value; }
	}
	#endregion
}