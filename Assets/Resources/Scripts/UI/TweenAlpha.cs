using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TweenAlpha: MonoBehaviour 
{
	#region Enums
	public enum TweenLogic { ONCE, LOOP };
	public TweenLogic tweenLogic;
	#endregion
	
	#region Public Attributes
	[Header("Tween Attributes")]
	public float speed;
	public float minValue;
	public float maxValue;
	
	[Header("Custom Attributes")]
	public bool doStart;
	public bool disableOnEnd;
	public bool onEnable;
	public int initValue;
	public float initAlpha;
	#endregion
	
	#region Private Attributes
	private int work = 0;
	private Color auxColor;
	#endregion
	
	#region References
	[Header("References")]
	public Image image;
	public Text text;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Initialize values
		if(!image && GetComponent<Image>())
		{
			image = GetComponent<Image>();
		}
		
		if(!text && GetComponent<Text>())
		{
			text = GetComponent<Text>();
		}
		
		if(doStart && !onEnable)
		{
			if(image)
			{
				auxColor = image.color;
				auxColor.a = initAlpha;
				image.color = auxColor;
			}
			
			if(text)
			{
				auxColor = text.color;
				auxColor.a = initAlpha;
				text.color = auxColor;
			}
			
			work = initValue;
		}
	}
	
	private void OnEnable()
	{
		if(onEnable)
		{
			if(image)
			{
				auxColor = image.color;
				auxColor.a = initAlpha;
				image.color = auxColor;
			}
			
			if(text)
			{
				auxColor = text.color;
				auxColor.a = initAlpha;
				text.color = auxColor;
			}
			
			work = initValue;
		}
	}
	
	private void Update () 
	{
		switch(work)
		{
			case 1:
			{
				if(image != null)
				{
					if(image.color.a < maxValue)
					{
						auxColor = image.color;
						auxColor.a += speed * Time.deltaTime;
						image.color = auxColor;
					}
				}
				
				if(text != null)
				{
					if(text.color.a < maxValue)
					{
						auxColor = text.color;
						auxColor.a += speed * Time.deltaTime;
						text.color = auxColor;
					}
					else
					{
						switch(tweenLogic)
						{
							case TweenLogic.ONCE:
							{
								work = 0;
								break;
							}
							case TweenLogic.LOOP:
							{
								work = 2;
								break;
							}
						}
						
						if(disableOnEnd)
						{
							gameObject.SetActive (false);
						}
					}
				}
				break;
			}
			case 2:
			{
				if(image != null)
				{
					if(image.color.a > minValue)
					{
						auxColor = image.color;
						auxColor.a -= speed * Time.deltaTime;
						image.color = auxColor;
					}
					else
					{
						switch(tweenLogic)
						{
						case TweenLogic.ONCE:
						{
							work = 0;
							break;
						}
						case TweenLogic.LOOP:
						{
							work = 1;
							break;
						}
						}
						
						if(disableOnEnd)
						{
							gameObject.SetActive (false);
						}
					}
				}
				
				if(text != null)
				{
					if(text.color.a > minValue)
					{
						auxColor = text.color;
						auxColor.a -= speed * Time.deltaTime;
						text.color = auxColor;
					}
					else
					{
						switch(tweenLogic)
						{
							case TweenLogic.ONCE:
							{
								work = 0;
								break;
							}
							case TweenLogic.LOOP:
							{
								work = 1;
								break;
							}
						}
						
						if(disableOnEnd)
						{
							gameObject.SetActive (false);
						}
					}
				}
				break;
			}
		}
	}
	#endregion
	
	#region Tween Methods
	public void SetTween(int value)
	{		
		work = value;
	}
	
	public void ResetTween(bool min)
	{
		if(image != null)
		{
			auxColor = image.color;
			
			if(min)
			{
				auxColor.a = minValue;
			}
			else
			{
				auxColor.a = maxValue;
			}
			
			image.color = auxColor;
		}
		
		if(text != null)
		{
			auxColor = text.color;
			
			if(min)
			{
				auxColor.a = minValue;
			}
			else
			{
				auxColor.a = maxValue;
			}
			
			text.color = auxColor;
		}
	}
	#endregion
}