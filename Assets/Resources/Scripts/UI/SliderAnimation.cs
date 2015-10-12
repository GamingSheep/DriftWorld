#pragma warning disable 0414
#pragma warning disable 0219
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderAnimation: MonoBehaviour 
{
	#region Public Attributes
	[Header("Group Attributes")]
	public float fadeSpeed;
	public float maxScale;
	public float minScale;
	#endregion
	
	#region Private Attributes
	private int actualButton;
	private Vector3 actualScale;
	#endregion
	
	#region References
	[Header("Sliders References")]
	public Transform[] slidersTransform;
	public SettingType[] settingsTypes;
	private AudioSource audioSource;
	#endregion
	
	#region Main Methods
	private void Start()
	{
		if(!audioSource)
		{
			audioSource = GetComponent<AudioSource>();
		}
		
		for(int i = 0; i < slidersTransform.Length; i++)
		{
			switch(settingsTypes[i])
			{
				case SettingType.GENERALVOLUME:
				{
					slidersTransform[i].GetChild(0).GetComponent<Slider>().value = DataManager.Instance.generalVolume;
					break;
				}
				case SettingType.MUSICVOLUME:
				{
					slidersTransform[i].GetChild(0).GetComponent<Slider>().value = DataManager.Instance.musicVolume;
					break;
				}
			}
		}
	}
	
	private void Update () 
	{
		for(int i = 0; i < slidersTransform.Length; i++)
		{
			UpdateButton(slidersTransform[i], i);
		}
	}
	#endregion
	
	#region Buttons Methods
	private void UpdateButton(Transform button, int number)
	{
		if(EventSystem.current.currentSelectedGameObject == button.GetChild (0).gameObject)
		{
			if(actualButton != number)
			{
				if(audioSource)
				{
					audioSource.Play();
				}
				
				actualButton = number;
			}
				
			if(button.localScale.x < maxScale)
			{
				actualScale = button.localScale;
				actualScale += Vector3.one * Time.deltaTime * fadeSpeed;
				button.localScale = actualScale;
			}
			else
			{
				actualScale = button.localScale;
				actualScale = Vector3.one * maxScale;
				button.localScale = actualScale;	
			}
		}
		else
		{
			if(button.localScale.x > minScale)
			{
				actualScale = button.localScale;
				actualScale -= Vector3.one * Time.deltaTime * fadeSpeed;
				button.localScale = actualScale;
			}
			else
			{
				actualScale = button.localScale;
				actualScale = Vector3.one * minScale;
				button.localScale = actualScale;	
			}
		}
	}
	#endregion
}