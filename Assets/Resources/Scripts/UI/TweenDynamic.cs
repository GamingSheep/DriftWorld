using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class TweenDynamic: MonoBehaviour 
{
	#region Public Attributes
	[Header("Component Type")]
	public bool hasText;
	public bool hasImage;
	
	[Header("Fade Attributes")]
	public Fade pauseFade;
	public Fade endFade;
	public float speed;
	public float pauseDuration;
	#endregion
	
	#region Private Attributes
	public int state = -1; // -1 = Nothing to do, 0 = start In, 1 = end In, 2 = start Out
	private Color textColor;
	private float counter;
	private Vector3 initPosition;
	#endregion
	
	#region References
	private Transform tweenTransform;
	private Text tweenLabel;
	private Image tweenImage;
	#endregion
	
	#region Events
	[Header("Events")]
	public TweenEvent onTweenFinish = new TweenEvent();
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		tweenTransform = transform;
		initPosition = tweenTransform.localPosition;
		
		if(hasText)
		{
			tweenLabel = GetComponent<Text>(); 
		}
		
		if(hasImage)
		{
			tweenImage = GetComponent<Image>();
		}
	}
	
	private void Update () 
	{
		switch(state)
		{
			case 0:
			{
				if(pauseFade.tweenPosition)
				{
					tweenTransform.localPosition = Vector3.Lerp(tweenTransform.localPosition, pauseFade.fadePosition, Time.deltaTime * speed);
				}
				
				if(pauseFade.tweenRotation)
				{
					tweenTransform.localRotation = Quaternion.Lerp (tweenTransform.localRotation, Quaternion.Euler (pauseFade.fadeRotation), Time.deltaTime * speed);
				}
				
				if(pauseFade.tweenAlpha)
				{
					if(hasText)
					{
						textColor = tweenLabel.color;
					textColor.a = Mathf.Lerp (textColor.a, pauseFade.fadeAlpha, Time.deltaTime * speed);
						tweenLabel.color = textColor;
					}
					
					if(hasImage)
					{
						textColor = tweenImage.color;
					textColor.a = Mathf.Lerp (textColor.a, pauseFade.fadeAlpha, Time.deltaTime * speed);
						tweenImage.color = textColor;
					}
				}
				
				counter -= Time.deltaTime;
				if(counter <= 0)
				{
					counter = pauseDuration;
					state = 1;
				}
				
				if(!pauseFade.tweenPosition && !pauseFade.tweenRotation && !pauseFade.tweenAlpha)
				{
					counter = pauseDuration;
					state = 1;
				}
				break;
			}
			case 1:
			{
				counter -= Time.deltaTime;
				if(counter <= 0)
				{
					counter = endFade.fadeDuration;
					state = 2;
				}
				break;
			}
			case 2:
			{
				if(endFade.tweenPosition)
				{
					tweenTransform.localPosition = Vector3.Lerp(tweenTransform.localPosition, endFade.fadePosition, Time.deltaTime * speed);
				}
				
				if(endFade.tweenRotation)
				{
					tweenTransform.localRotation = Quaternion.Lerp (tweenTransform.localRotation, Quaternion.Euler (endFade.fadeRotation), Time.deltaTime * speed);
				}
				
				if(endFade.tweenAlpha)
				{
					if(hasText)
					{
						textColor = tweenLabel.color;
						textColor.a = Mathf.Lerp (textColor.a, endFade.fadeAlpha, Time.deltaTime * speed);
						tweenLabel.color = textColor;
					}
					
					if(hasImage)
					{
						textColor = tweenImage.color;
						textColor.a = Mathf.Lerp (textColor.a, endFade.fadeAlpha, Time.deltaTime * speed);
						tweenImage.color = textColor;
					}
				}
				
				counter -= Time.deltaTime;
				if(counter <= 0)
				{
					onTweenFinish.Invoke ();
					tweenTransform.localPosition = initPosition;
					
					if(endFade.tweenAlpha)
					{
						if(hasText)
						{
							textColor = tweenLabel.color;
							textColor.a = endFade.fadeAlpha;
							tweenLabel.color = textColor;
						}
						
						if(hasImage)
						{
							textColor = tweenImage.color;
							textColor.a = endFade.fadeAlpha;
							tweenImage.color = textColor;
						}
					}
					
					state = -1;
				}
				
				if(!endFade.tweenPosition && !endFade.tweenRotation && !endFade.tweenAlpha)
				{
					onTweenFinish.Invoke ();
					tweenTransform.localPosition = initPosition;
					state = -1;
				}
				break;
			}
		}
	}
	#endregion
	
	#region Tween Dynamic Methods
	public void SetTweenState(int value)
	{
		state = value;
		
		switch(state)
		{
			case 0:
			{
				counter = pauseFade.fadeDuration;
				break;
			}
			case 1:
			{
				counter = pauseDuration;
				break;
			}
			case 2:
			{
				counter = endFade.fadeDuration;
				break;
			}
		}
	}
	#endregion
	
	#region Serializables Attributes
	[Serializable]
	public class Fade
	{
		#region Fade Attributes
		[SerializeField]
		private bool _tweenPosition;
		[SerializeField]
		private bool _tweenRotation;
		[SerializeField]
		private bool _tweenAlpha;
		[SerializeField]
		private Vector3 _fadePosition;
		[SerializeField]
		private Vector3 _fadeRotation;
		[SerializeField]
		private float _fadeAlpha;
		[SerializeField]
		private float _fadeDuration;
		#endregion
		
		#region Properties
		public bool tweenPosition
		{
			get { return _tweenPosition; }
			set { _tweenPosition = value; }
		}
		
		public bool tweenRotation
		{
			get { return _tweenRotation; }
			set { _tweenRotation = value; }
		}
		
		public bool tweenAlpha
		{
			get { return _tweenAlpha; }
			set { _tweenAlpha = value; }
		}
		
		public Vector3 fadePosition
		{
			get { return _fadePosition; }
			set { _fadePosition = value; }
		}
		
		public Vector3 fadeRotation
		{
			get { return _fadeRotation; }
			set { _fadeRotation = value; }
		}
		
		public float fadeDuration
		{
			get { return _fadeDuration; }
			set { _fadeDuration = value; }
		}
		
		public float fadeAlpha
		{
			get { return _fadeAlpha; }
			set { _fadeAlpha = value; }
		}
		#endregion
	}
	
	[Serializable]
	public class TweenEvent : UnityEvent { }
	#endregion
}