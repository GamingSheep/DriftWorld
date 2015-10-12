using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OrbitCamera : MonoBehaviour 
{
	#region Public Attributes
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	
	public float distanceMin = 0.5f;
	public float distanceMax = 15f;
	#endregion
	
	#region Private Attributes
	private Rigidbody cameraRigidbody;
	private float x = 0.0f;
	private float y = 0.0f;
	private Quaternion actualRotation;
	private Vector3 negDistance;
	private Vector3 actualPosition;
	private Vector3 angles;
	private GameObject[] players;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		
		cameraRigidbody = GetComponent<Rigidbody>();
		
		// Make the rigid body not change rotation
		if (cameraRigidbody != null)
		{
			cameraRigidbody.freezeRotation = true;
		}
		
		if(GameObject.FindObjectOfType<NetworkManager>())
		{
			players = GameObject.FindGameObjectsWithTag("Player");
			for(int i = 0; i < players.Length; i++)
			{
				if(players[i].GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					target = players[i].transform;
					break;
				}
			}
		}
		else
		{
			target = GameObject.FindWithTag ("Player").transform;
		}
	}
	
	private void LateUpdate () 
	{
		if (target) 
		{
			x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			
			actualRotation = Quaternion.Euler(y, x, 0);
			
			distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
			
			negDistance = new Vector3(0.0f, 0.0f, -distance);
			actualPosition = actualRotation * negDistance + target.position;
			
			transform.rotation = actualRotation;
			transform.position = actualPosition;
		}
	}
	#endregion
	
	#region Calculation Attributes
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
		{
			angle += 360F;
		}
		
		if (angle > 360F)
		{
			angle -= 360F;
		}
		
		return Mathf.Clamp(angle, min, max);
	}
	#endregion
}