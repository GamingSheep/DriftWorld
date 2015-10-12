using UnityEngine;
using System.Collections;

public class ParticleRotation: MonoBehaviour 
{
	#region Enums
	public enum RotationAxis { X, Y, Z };
	#endregion
	
	#region Public Attributes
	[Header("Rotation Attributes")]
	public RotationAxis rotationAxis;
	public float rotationSpeed;
	#endregion
	
	#region Private Attributes
	private Vector3 particleRotation;
	#endregion
	
	#region References
	public Transform[] rotationObjects;
	#endregion
	
	#region Main Methods	
	private void Update () 
	{
		for(int i = 0; i < rotationObjects.Length; i++)
		{
			particleRotation = rotationObjects[i].localRotation.eulerAngles;
			
			switch(rotationAxis)
			{
				case RotationAxis.X:
				{
					particleRotation.x += rotationSpeed * Time.deltaTime;
					break;
				}
				case RotationAxis.Y:
				{
					particleRotation.y += rotationSpeed * Time.deltaTime;
					break;
				}
				case RotationAxis.Z:
				{
					particleRotation.z += rotationSpeed * Time.deltaTime;
					break;
				}
			}
			
			rotationObjects[i].localRotation = Quaternion.Euler (particleRotation);
		}
	}
	#endregion
}