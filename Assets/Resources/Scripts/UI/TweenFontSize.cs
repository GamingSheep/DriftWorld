using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TweenFontSize: MonoBehaviour 
{
	#region Enums
	public enum TweenType { ALWAYS, ONCE };
	#endregion
	
	#region Public Attributes
	[Header("Tween Attributes")]
	public TweenType tweenType;
	public int speed;
	public int minValue;
	public int maxValue;
	#endregion
	
	#region Private Attributes
	private bool state;
	private int work = -1;
	private float auxValue;
	#endregion
	
	#region References
	private Text textLabel;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Initialize values
		textLabel = GetComponent<Text>();
		state = false;
		work = -1;
		auxValue = textLabel.fontSize;
	}
	
	private void Update () 
	{
		switch(tweenType)
		{
			case TweenType.ALWAYS:
			{
				if(state)
				{
					if(textLabel.fontSize < maxValue)
					{
						auxValue += speed * Time.deltaTime;
						textLabel.fontSize = Mathf.RoundToInt(auxValue);
					}
					else
					{
						auxValue = maxValue;
						textLabel.fontSize = maxValue;
						state = false;
					}
				}
				else
				{
					if(textLabel.fontSize > minValue)
					{
						auxValue -= speed * Time.deltaTime;
						textLabel.fontSize = Mathf.RoundToInt(auxValue);
					}
					else
					{
						auxValue = minValue;
						textLabel.fontSize = minValue;
						state = true;
					}
				}
				break;
			}
			case TweenType.ONCE:
			{
				switch(work)
				{
					case 0:
					{
						if(textLabel.fontSize < maxValue)
						{
							auxValue += speed * Time.deltaTime;
							textLabel.fontSize = Mathf.RoundToInt(auxValue);
						}
						else
						{
							auxValue = maxValue;
							textLabel.fontSize = maxValue;
							work = 1;
						}
						break;
					}
					case 1:
					{
						if(textLabel.fontSize > minValue)
						{
							auxValue -= speed * Time.deltaTime;
							textLabel.fontSize = Mathf.RoundToInt(auxValue);
						}
						else
						{
							auxValue = minValue;
							textLabel.fontSize = minValue;
							work = -1;
						}
						break;
					}
				}
				break;
			}
		}
	}
	#endregion
	
	#region Tween Methods
	public void StartTween()
	{
		work = 0;
	}
	#endregion
}