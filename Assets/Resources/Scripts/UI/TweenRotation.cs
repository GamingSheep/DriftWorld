using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class TweenRotation: MonoBehaviour 
{
	#region Enums
	public enum RotationAxis { X, Y, Z , ALL};
	#endregion
	
	#region Public Attributes
	[Header("Tween Attributes")]
	public AnimationSlot[] animations;
	#endregion
	
	#region Private Attributes
	private Vector3 auxRotation;
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
		auxRotation = auxTransform.localRotation.eulerAngles;
		
		switch(animations[currentAnimation].axis)
		{
			case RotationAxis.X:
			{
				auxRotation.x = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case RotationAxis.Y:
			{
				auxRotation.y = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case RotationAxis.Z:
			{
				auxRotation.z = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
			case RotationAxis.ALL:
			{
				auxRotation.x = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				auxRotation.y = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				auxRotation.z = Mathf.Lerp (animations[currentAnimation].minRotation, animations[currentAnimation].maxRotation, animations[currentAnimation].animationCurve.Evaluate (auxValue / animations[currentAnimation].animationDuration));
				break;
			}
		}
		
		auxTransform.localRotation = Quaternion.Euler (auxRotation);
		
		if(auxValue >= animations[currentAnimation].animationDuration)
		{
			animations[currentAnimation].onAnimationFinish.Invoke ();
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
		private RotationAxis _axis;
		[SerializeField]
		private AnimationCurve _animationCurve;
		[SerializeField]
		private float _minRotation;
		[SerializeField]
		private float _maxRotation;
		[SerializeField]
		private float _animationDuration;
		[SerializeField]
		private TweenEvent _onAnimationFinish = new TweenEvent();
		#endregion
		
		#region Properties
		public RotationAxis axis
		{
			get { return _axis; }
			set { _axis = value; }
		}
		
		public AnimationCurve animationCurve
		{
			get { return _animationCurve; }
			set { _animationCurve = value; }
		}
		
		public float minRotation
		{
			get { return _minRotation; }
			set { _minRotation = value; }
		}
		
		public float maxRotation
		{
			get { return _maxRotation; }
			set { _maxRotation = value; }
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