using UnityEngine;
using System.Collections;

public class CarSwayBar : MonoBehaviour 
{
	#region Main Attributes
	public CarWheel wheel1;
	public CarWheel wheel2;
	public float coefficient = 5000;
	
	private float force;
	#endregion
	
	#region Main Methods	
	private void FixedUpdate () 
	{
		if (wheel1 != null && wheel2 != null && this.enabled)
		{
			force = (wheel1.compression - wheel2.compression) * coefficient;
			wheel1.suspensionForceInput = +force;
			wheel2.suspensionForceInput = -force;
		}
	}
	#endregion
}