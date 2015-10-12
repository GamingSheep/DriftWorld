using UnityEngine;
using System.Collections;

public class DriftPickup: MonoBehaviour 
{
	#region Enums
	public enum RotationAxis { NONE, X, Y, Z };
	public enum PickupType { MULTIPLIER, TIME, NITRO };
	#endregion
	
	#region Public Attributes
	[Header("Pickup Attributes")]
	public PickupType pickupType;
	public float amount;
	public float delayTime;	// -1 = unlimited
	
	[Header("Transform Animation Attributes")]
	public Transform animationPivot;
	public bool levitate;
	public float levitationSpeed;
	public float positionScale;
	public RotationAxis rotationAxis;
	public float rotationSpeed;
	
	[Header("Material Animation Attributes")]
	public bool animateMaterial;
	public MeshRenderer[] meshRenderer;
	public float fadeSpeed;
	public float maxValue;
	public float minValue;
	
	[Header("Helper Attributes")]
	public MeshRenderer helper;
	public float minDistanceToFade;
	public float helperMaxValue;
	public float helperMinValue;
	#endregion
	
	#region Private Attributes
	private Vector3 pickupPosition;
	private Vector3 initPosition;
	private Vector3 pickupRotation;
	private float materialMultiplier;
	private bool ascending;
	private Vector3 playerDistance;
	private Color helperColor;
    #endregion

    #region References
    private CarDrifting carDrifting;
    private MultiplayerManager multiplayerManager;
    private AudioSource audioSource;
    private Transform playerTransform;

    [Header("Destroy Attributes")] 
	public GameObject destroyChild;
	#endregion
	
	#region Main Methods
	private void Start () 
	{		
		if(GameObject.Find ("DriftManager"))
		{
			carDrifting = GameObject.Find ("DriftManager").GetComponent<CarDrifting>();
		}
        else if (GameObject.Find("MultiplayerManager"))
        {
            multiplayerManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
        }

        audioSource = GetComponent<AudioSource>();
		initPosition = animationPivot.localPosition;
		ascending = true;
	}
	
	private void Update () 
	{
		if(playerTransform == null && GameObject.FindWithTag ("Player"))
		{
			playerTransform = GameObject.FindWithTag ("Player").transform;
			if(GameObject.Find ("DriftManager"))
			{
				carDrifting = GameObject.Find ("DriftManager").GetComponent<CarDrifting>();
			}
            else if (GameObject.Find("MultiplayerManager"))
            {
                multiplayerManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
            }
        }
		
		if(playerTransform != null)
		{
			switch(rotationAxis)
			{
				case RotationAxis.NONE:
				{
					break;
				}
				case RotationAxis.X:
				{
					pickupRotation = animationPivot.localRotation.eulerAngles;
					pickupRotation.x += rotationSpeed * Time.deltaTime;
					animationPivot.localRotation = Quaternion.Euler (pickupRotation);
					break;	
				}
				case RotationAxis.Y:
				{
					pickupRotation = animationPivot.localRotation.eulerAngles;
					pickupRotation.y += rotationSpeed * Time.deltaTime;
					animationPivot.localRotation = Quaternion.Euler (pickupRotation);
					break;	
				}
				case RotationAxis.Z:
				{
					pickupRotation = animationPivot.localRotation.eulerAngles;
					pickupRotation.z += rotationSpeed * Time.deltaTime;
					animationPivot.localRotation = Quaternion.Euler (pickupRotation);
					break;	
				}
			}
			
			if(levitate)
			{
				pickupPosition = animationPivot.localPosition;
				pickupPosition.y = initPosition.y + Mathf.Sin (Time.time * levitationSpeed) * positionScale;
				animationPivot.localPosition = pickupPosition;
			}
			
			if(helper != null && playerTransform != null)
			{
				playerDistance = playerTransform.position - transform.position;
				helperColor = helper.material.GetColor ("_TintColor");		
					
				if((Mathf.Abs(playerDistance.sqrMagnitude) * 0.001f) > helperMaxValue)
				{
					helperColor.a = helperMaxValue;
				}
				else
				{
					helperColor.a = Mathf.Abs (playerDistance.sqrMagnitude) * 0.001f/4;
				}
				
				helper.material.SetColor ("_TintColor", helperColor);
			}	
			
			if(animateMaterial)
			{
				for(int i = 0; i < meshRenderer.Length; i++)
				{
					materialMultiplier = meshRenderer[i].material.GetFloat ("_RimMultiplier");
					
					if(ascending)
					{
						if(materialMultiplier < maxValue - 1.95f)
						{
							materialMultiplier = Mathf.Lerp (materialMultiplier, maxValue, Time.deltaTime * fadeSpeed * 0.6f);
						}
						else
						{
							ascending = false;
						}
					}
					else
					{
						if(materialMultiplier > minValue + 0.05f)
						{
							materialMultiplier = Mathf.Lerp (materialMultiplier, minValue, Time.deltaTime * fadeSpeed);
						}
						else
						{
							ascending = true;
						}
					}
					
					meshRenderer[i].material.SetFloat("_RimMultiplier", materialMultiplier);
				}
			}
		}
	}
	#endregion
	
	#region Value Animation Methods
	private static float Oscillate(float input, float min, float max)
	{
		float range = max - min ;
		return min + Mathf.Abs(((input + range) % (range * 2)) - range);
	}
	#endregion
	
	#region Detection Methods
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			if(carDrifting != null)
			{
				// Add values
				switch(pickupType)
				{
					case PickupType.MULTIPLIER:
					{
						carDrifting.AddMultiplier((int)amount);
						audioSource.Play ();
						break;
					}
					case PickupType.TIME:
					{
						carDrifting.AddTime(amount);
						audioSource.Play ();
						break;
					}
					case PickupType.NITRO:
					{
						carDrifting.AddNitro(amount);
						audioSource.Play ();
						break;
					}
				}
			}
            else if (multiplayerManager != null)
            {
                // Add values
                switch (pickupType)
                {
                    case PickupType.MULTIPLIER:
                    {
                        multiplayerManager.AddMultiplier((int)amount);
                        audioSource.Play();
                        break;
                    }
                    case PickupType.NITRO:
                    {
                        multiplayerManager.AddNitro(amount);
                        audioSource.Play();
                        break;
                    }
                }
            }

            // Play sound
            audioSource.Play ();
			
			DisablePickup();
		}		
	}
	#endregion
	
	#region Pickup Methods
	public void DisablePickup()
	{
		// Disable all child ands enable destroyed pickup object
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild (i).gameObject.SetActive (false);
		}
		
		destroyChild.SetActive (true);
		GetComponent<BoxCollider>().enabled = false;
		
		Invoke("DisableDestroy", 4.0f);
		
		if(delayTime != -1)
		{
			Invoke ("EnablePickup", delayTime);
		}
	}
	
	private void DisableDestroy()
	{
		destroyChild.SetActive (false);
	}
	
	public void DisablePickupForMinimap()
	{
		// Disable all child ands enable destroyed pickup object
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild (i).gameObject.SetActive (false);
		}
		
		GetComponent<BoxCollider>().enabled = false;
		
		if(delayTime != -1)
		{
			Invoke ("EnablePickup", delayTime);
		}
	}
	
	public void EnablePickup()
	{
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild (i).gameObject.SetActive (true);
		}
		
		destroyChild.SetActive (false);
		
		GetComponent<BoxCollider>().enabled = true;
	}
	#endregion
}