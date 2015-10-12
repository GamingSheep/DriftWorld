using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ManualButtonAnimation: MonoBehaviour 
{
	#region Public Attributes
	[Header("Group Attributes")]
	public float fadeSpeed;
	public float maxScale;
	public float minScale;
	#endregion
	
	#region Private Attributes
	private int actualButton;
	private int auxActualButton;
	private Vector3 actualScale;
	private Button[] buttons;
	#endregion
	
	#region References
	[Header("Buttons References")]
	public Transform[] buttonsTransform;
	public GameObject[] menusObject;
	private AudioSource audioSource;
	#endregion
	
	#region Auxiliar Attributes
	private bool result;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		if(GetComponent<AudioSource>())
		{
			audioSource = GetComponent<AudioSource>();
		}
	}
	
	private void OnEnable()
	{
		buttons = new Button[buttonsTransform.Length];
		for(int i = 0; i < buttonsTransform.Length; i++)
		{
			buttons[i] = buttonsTransform[i].GetComponent<Button>();
		}
	}
	
	private void Update () 
	{
		for(int i = 0; i < buttonsTransform.Length; i++)
		{
			UpdateButton(buttonsTransform[i], i);
		}
		
		if(!menusObject[actualButton].activeSelf)
		{
			for(int i = 0; i < menusObject.Length; i++)
			{
				menusObject[i].SetActive (false);
			}
			
			menusObject[actualButton].SetActive (true);
		}
		
		if(auxActualButton != -1)
		{
			if(!IsSomeButtonActive(buttons))
			{
				auxActualButton = -1;
			}
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
				actualButton = number;
			}
			
			if(auxActualButton != number)
			{
				if(audioSource != null)
				{
					audioSource.Play ();
				}
				
				auxActualButton = number;
			}
		}
		
		if(actualButton == number)
		{
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
	
	private bool IsSomeButtonActive(Button[] buttons)
	{
		result = false;
		
		for(int i = 0; i < buttons.Length; i++)
		{
			if(EventSystem.current.currentSelectedGameObject == buttons[i].gameObject)
			{
				result = true;
				break;
			}
		}
		
		return result;
	}
	#endregion
}