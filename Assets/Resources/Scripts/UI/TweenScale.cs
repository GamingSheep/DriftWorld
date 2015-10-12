using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class TweenScale: MonoBehaviour 
{
	#region Enums
	public enum ScaleAxis { X, Y, Z , ALL};
	#endregion
	
	#region Public Attributes
	[Header("Tween Attributes")]
	public AnimationSlot[] animations;
	#endregion
	
	#region Private Attributes
	private Vector3 auxScale;
	private float auxValue;
	private int currentAnimation;
	#endregion
	
	#region References
	private RectTransform auxTransform;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		auxValue = 0.0f;
		currentAnimation = 0;
		auxTransform = GetComponent<RectTransform>();
	}
	
	private void Update () 
	{
		auxValue += Time.deltaTime;
		auxScale = auxTransform.localScale;
		
		switch(animations[currentAnimation].axis)
		{
			case ScaleAxis.X:
			{
				auxScale.x = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case ScaleAxis.Y:
			{
				auxScale.y = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case ScaleAxis.Z:
			{
				auxScale.z = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case ScaleAxis.ALL:
			{
				auxScale.x = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				auxScale.y = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				auxScale.z = Mathf.Lerp (animations[currentAnimation].minScale, animations[currentAnimation].maxScale, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
		}
		
		auxTransform.localScale = auxScale;
		
		if(auxValue >= animations[currentAnimation].animationDuration)
		{
			animations[currentAnimation].onAnimationFinish.Invoke();
			auxValue = 0.0f;
			currentAnimation++;
			
			if(currentAnimation >= animations.Length)
			{
				currentAnimation = 0;
			}
		}
	}
	#endregion
	
	#region Serializable
	[Serializable]
	public class AnimationSlot
	{
		#region Serializable Attributes
		[SerializeField]
		private ScaleAxis _axis;
		[SerializeField]
		private AnimationCurve _animationCurve;
		[SerializeField]
		private float _minScale;
		[SerializeField]
		private float _maxScale;
		[SerializeField]
		private float _animationDuration;
		[SerializeField]
		private TweenEvent _onAnimationFinish = new TweenEvent();
		#endregion
		
		#region Properties
		public ScaleAxis axis
		{
			get { return _axis; }
			set { _axis = value; }
		}
		
		public AnimationCurve animationCurve
		{
			get { return _animationCurve; }
			set { _animationCurve = value; }
		}
		
		public float minScale
		{
			get { return _minScale; }
			set { _minScale = value; }
		}
		
		public float maxScale
		{
			get { return _maxScale; }
			set { _maxScale = value; }
		}
		
		public float animationDuration
		{
			get { return _animationDuration; }
			set { _animationDuration = value; }
		}
		
		public TweenEvent onAnimationFinish
		{
			get { return _onAnimationFinish; }
			set { _onAnimationFinish = value; }
		}
		#endregion
	}
	
	[Serializable]
	public class TweenEvent : UnityEvent { }
	#endregion
}