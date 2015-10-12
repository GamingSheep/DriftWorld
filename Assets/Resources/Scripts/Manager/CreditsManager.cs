using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreditsManager: MonoBehaviour
{
	#region Public Attributes
	[Header("Credits Attributes")]
	public Vector3 initPosition;
	public Vector3 endPosition;
	public float creditsSpeed;
	
	[Header("Multimedia Attributes")]
	public MovieTexture creditsMovie;
	#endregion
	
	#region Private Attributes
	private Vector3 auxPosition;
	private int index;
	private int state;
	#endregion
	
	#region References
	private CarInput input;
	private AudioSource source;
	private Color fadeColor;
	
	[Header("Credits References")]
	public RawImage rawImage;
	public GameObject skipButton;
	public RawImage fade;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		input = CarInput.Instance;
		index = 0;
		state = 0;
		transform.localPosition = initPosition;
		source = GetComponent<AudioSource>();
		creditsMovie.Play ();
		source.Play ();
		
		fadeColor = fade.color;
		fadeColor.a = 1.0f;
		fade.color = fadeColor;
	}
	
	private void Update () 
	{
		if(input.Submit ())
		{
			switch(index)
			{
				case 0:
				{
					index = 1;
					skipButton.SetActive(true);
					break;
				}
				case 1:
				{
					skipButton.SetActive(false);
					EndCredits();
					break;
				}
			}		
		}
		
		switch(state)
		{
			case 0:
			{
				fadeColor = fade.color;
				if(fadeColor.a > 0.0f)
				{
					fadeColor.a -= Time.deltaTime;
					fade.color = fadeColor;
				}
				else
				{
					fadeColor.a = 0.0f;
					fade.color = fadeColor;
					state = 1;
				}
				break;
			}
			case 2:
			{
				fadeColor = fade.color;
				if(fadeColor.a < 1.0f)
				{
					fadeColor.a += Time.deltaTime;
					fade.color = fadeColor;
				}
				else
				{
					fadeColor.a = 1.0f;
					fade.color = fadeColor;
					state = 0;
					ChangeScene ();
				}
				break;
			}
		}
		
		if(index == 2)
		{
			if(source.volume > 0.0f)
			{
				source.volume -= Time.deltaTime;
			}
		}	
		
		if(transform.localPosition.y > endPosition.y)
		{
			EndCredits();
		}
		else
		{
			auxPosition = transform.localPosition;
			auxPosition.y += creditsSpeed * Time.deltaTime;
			transform.localPosition = auxPosition;
		}
	}
	#endregion
	
	#region Credits Methods
	private void EndCredits()
	{
		creditsMovie.Stop ();
		index = 2;
		state = 2;	
	}
	
	private void ChangeScene()
	{
		creditsMovie.Stop ();
		Application.LoadLevel("DemoScene");
	}
	#endregion
}