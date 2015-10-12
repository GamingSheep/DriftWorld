using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionUI: MonoBehaviour 
{
	#region Enums
	public enum ResolutionType { _800x600, _1024x768, _1280x720, _1280x1024, _1366x768, _1440x900, _1680x1050, _1920x1080 };
	public ResolutionType resolutionType;
	#endregion
	
	#region Public Attributes	
	[Header("Input Attributes")]
	public float moveTemp;
	
	[Header("Fade Attributes")]
	public float fadeSpeed;
	public float maxScale;
	public float minScale;
	#endregion
	
	#region Private Attributes
	private bool canMove;
	private bool actualButton;
	private Vector3 actualScale;
	#endregion
	
	#region References
	public GameObject resolutionButton;
	public Text textButton;
	public GameObject[] resolutionArrows;
	public SettingsManager settingsManager;
	private CarInput carInput;
	private AudioSource audioSource;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Initialize 
		carInput = CarInput.Instance;
		actualButton = false;
		canMove = true;
		resolutionType = (ResolutionType)DataManager.Instance.resolution;
		
		switch(resolutionType)
		{
			case ResolutionType._800x600:
			{
				textButton.text = "Resolution: 800x600";
				break;
			}
			case ResolutionType._1024x768:
			{
				textButton.text = "Resolution: 1024x768";
				break;
			}
			case ResolutionType._1280x720:
			{
				textButton.text = "Resolution: 1280x720";
				break;
			}
			case ResolutionType._1280x1024:
			{
				textButton.text = "Resolution: 1280x1024";
				break;
			}
			case ResolutionType._1366x768:
			{
				textButton.text = "Resolution: 1366x768";
				break;
			}
			case ResolutionType._1440x900:
			{
				textButton.text = "Resolution: 1440x900";
				break;
			}
			case ResolutionType._1680x1050:
			{
				textButton.text = "Resolution: 1680x1050";
				break;
			}
			case ResolutionType._1920x1080:
			{
				textButton.text = "Resolution: 1920x1080";
				break;
			}
		}
		
		// Get references
		if(settingsManager == null)
		{
			settingsManager = transform.root.GetComponent<SettingsManager>();
		}
		
		if(GetComponent<AudioSource>())
		{
			audioSource = GetComponent<AudioSource>();
		}
	}
	
	private void OnEnable()
	{
		for(int i = 0; i < resolutionArrows.Length; i++)
		{
			resolutionArrows[i].SetActive (false);
		}
	}
	
	private void Update () 
	{
		if(EventSystem.current.currentSelectedGameObject == resolutionButton)
		{
			if(!actualButton)
			{
				for(int i = 0; i < resolutionArrows.Length; i++)
				{
					resolutionArrows[i].SetActive (true);
				}
				
				actualButton = true;
				if(audioSource)
				{
					audioSource.Play ();
				}
			}
			
			if(canMove)
			{
				if(carInput.Left())
				{
					resolutionType--;
					
					if((int)resolutionType < 0)
					{
						resolutionType = (ResolutionType)7;
					}
					
					canMove = false;
				}
				else if(carInput.Right())
				{
					resolutionType++;
					
					if((int)resolutionType > 7)
					{
						resolutionType = (ResolutionType)0;
					}
					
					canMove = false;
				}
			}
			else
			{
				if(!carInput.Left () && !carInput.Right ())
				{
					canMove = true;
				}
			}
			
			switch(resolutionType)
			{
				case ResolutionType._800x600:
				{
					textButton.text = "Resolution: 800x600";
					break;
				}
				case ResolutionType._1024x768:
				{
					textButton.text = "Resolution: 1024x768";
					break;
				}
				case ResolutionType._1280x720:
				{
					textButton.text = "Resolution: 1280x720";
					break;
				}
				case ResolutionType._1280x1024:
				{
					textButton.text = "Resolution: 1280x1024";
					break;
				}
				case ResolutionType._1366x768:
				{
					textButton.text = "Resolution: 1366x768";
					break;
				}
				case ResolutionType._1440x900:
				{
					textButton.text = "Resolution: 1440x900";
					break;
				}
				case ResolutionType._1680x1050:
				{
					textButton.text = "Resolution: 1680x1050";
					break;
				}
				case ResolutionType._1920x1080:
				{
					textButton.text = "Resolution: 1920x1080";
					break;
				}
			}
			
			if(resolutionButton.transform.localScale.x < maxScale)
			{
				actualScale = resolutionButton.transform.localScale;
				actualScale += Vector3.one * Time.deltaTime * fadeSpeed;
				resolutionButton.transform.localScale = actualScale;
			}
			else
			{
				actualScale = resolutionButton.transform.localScale;
				actualScale = Vector3.one * maxScale;
				resolutionButton.transform.localScale = actualScale;	
			}
		}
		else
		{			
			if(actualButton)
			{
				for(int i = 0; i < resolutionArrows.Length; i++)
				{
					resolutionArrows[i].SetActive (false);
				}
				
				settingsManager.SetResolution ((int)resolutionType);
				actualButton = false;
			}
			
			if(resolutionButton.transform.localScale.x > minScale)
			{
				actualScale = resolutionButton.transform.localScale;
				actualScale -= Vector3.one * Time.deltaTime * fadeSpeed;
				resolutionButton.transform.localScale = actualScale;
			}
			else
			{
				actualScale = resolutionButton.transform.localScale;
				actualScale = Vector3.one * minScale;
				resolutionButton.transform.localScale = actualScale;	
			}
		}
		#endregion
	}
}