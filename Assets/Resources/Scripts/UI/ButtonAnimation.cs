#pragma warning disable 0414
#pragma warning disable 0219
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnimation: MonoBehaviour 
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
	[Header("Buttons References")]
	public Transform[] buttonsTransform;
	private AudioSource audioSource;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		if(GetComponent<AudioSource>())
		{
			audioSource = GetComponent<AudioSource>();
		}
	}
	
	private void Update () 
	{
		for(int i = 0; i < buttonsTransform.Length; i++)
		{
			UpdateButton(buttonsTransform[i], i);
		}
	}
	#endregion
	
	#region Buttons Methods
	private void UpdateButton(Transform button, int number)
	{
		if(EventSystem.current.currentSelectedGameObject == button.gameObject)
		{
			if(actualButton != number)
			{
				if(audioSource != null)
				{
					audioSource.Play ();
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