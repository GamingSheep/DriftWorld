using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleAnimation: MonoBehaviour 
{
	#region Public Attributes
	[Header("Group Attributes")]
	public float fadeSpeed;
	public float maxScale;
	public float minScale;
	#endregion
	
	#region Private Attributes
	private int actualToggle;
	private Vector3 actualScale;
	private Toggle[] toggles;
	private bool result;
	#endregion
	
	#region References
	[Header("Toggles References")]
	public Transform[] togglesTransform;
	public SettingType[] settingsTypes;
	private AudioSource audioSource;
	#endregion
	
	#region Main Methods
	private void Start()
	{
		if(GetComponent<AudioSource>())
		{
			audioSource = GetComponent<AudioSource>();
		}
		
		for(int i = 0; i < togglesTransform.Length; i++)
		{
			switch(settingsTypes[i])
			{
				case SettingType.QUALITY:
				{
					if(i == DataManager.Instance.quality)
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = true;
					}
					else
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = false;
					}
					break;
				}
				case SettingType.ANTIALIASING:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.antialiasing;
					break;
				}
				case SettingType.BLOOM:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.bloom;
					break;
				}
				case SettingType.MOTIONBLUR:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.cameraMotionBlur;
					break;
				}
				case SettingType.DEPTHOFFIELD:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.depthOfField;
					break;
				}
				case SettingType.VIGNETTE:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.vignette;
					break;
				}
				case SettingType.SSAO:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.ambientOcclusion;
					break;
				}
				case SettingType.COLOREFFECT:
				{
					togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.colorEffect;
					break;
				}
				case SettingType.CONTROLLER:
				{
					if(i == 0)
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.isGamepad;
					}
					else if(i == 1)
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = !DataManager.Instance.isGamepad;
					}
					break;
				}
				case SettingType.TRACTION:
				{
					if(i == 2)
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = DataManager.Instance.isAutomatic;
					}
					else if(i == 3)
					{
						togglesTransform[i].GetChild(0).GetComponent<Toggle>().isOn = !DataManager.Instance.isAutomatic;
					}
					break;
				}
			}
		}
	}
	
	private void OnEnable () 
	{ 
		actualToggle = -1;
		toggles = new Toggle[togglesTransform.Length];
		
		for(int i = 0; i < togglesTransform.Length; i++)
		{
			toggles[i] = togglesTransform[i].GetChild(0).GetComponent<Toggle>();
		}
	}
	
	private void Update () 
	{
		for(int i = 0; i < togglesTransform.Length; i++)
		{
			UpdateToggle(i);
		}
		
		if(actualToggle != -1)
		{
			if(!IsSomeToggleActive(toggles))
			{
				actualToggle = -1;
			}
		}
	}
	#endregion
	
	#region Toggles Methods
	private void UpdateToggle(int value)
	{
		if(EventSystem.current.currentSelectedGameObject == toggles[value].gameObject)
		{
			if(actualToggle != value)
			{
				if(audioSource != null)
				{
					audioSource.Play ();
				}
				actualToggle = value;
			}
			if(togglesTransform[value].localScale.x < maxScale)
			{
				actualScale = togglesTransform[value].localScale;
				actualScale += Vector3.one * Time.deltaTime * fadeSpeed;
				togglesTransform[value].localScale = actualScale;
			}
			else
			{
				actualScale = togglesTransform[value].localScale;
				actualScale = Vector3.one * maxScale;
				togglesTransform[value].localScale = actualScale;	
			}
		}
		else
		{
			if(togglesTransform[value].localScale.x > minScale)
			{
				actualScale = togglesTransform[value].localScale;
				actualScale -= Vector3.one * Time.deltaTime * fadeSpeed;
				togglesTransform[value].localScale = actualScale;
			}
			else
			{
				actualScale = togglesTransform[value].localScale;
				actualScale = Vector3.one * minScale;
				togglesTransform[value].localScale = actualScale;	
			}
		}
	}
	
	private bool IsSomeToggleActive(Toggle[] toggles)
	{
		result = false;
		
		for(int i = 0; i < toggles.Length; i++)
		{
			if(EventSystem.current.currentSelectedGameObject == toggles[i].gameObject)
			{
				result = true;
				break;
			}
		}
		
		return result;
	}
	#endregion
}