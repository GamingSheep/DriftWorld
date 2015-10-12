using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Minimap: MonoBehaviour 
{
	#region Public Attributes
	[Header("Minimap Attributes")]
	public float minimapScale;
	public float minimapRadius;
	private Transform minimapPlayer;
	private Transform[] minimapNitro;
	private Transform[] minimapMultiplier;
	private Transform[] minimapTime;
	private Transform[] minimapPlayers;
	#endregion
	
	#region Private Attributes
	private Vector3 playerRotation;
	private Vector3 pickupPosition;
	private float auxAngle;
	private Vector3 finalVector = new Vector3(0, 0, -1);
    private bool isOnline;
	#endregion
	
	#region References
	[Header("Minimap References")]
	public GameObject pickupPrefab;
	private Transform playerTransform;
	private GameObject[] nitroObjects;
	private GameObject[] multiplierObjects;
	private GameObject[] timeObjects;
	private GameObject[] playerObjects;
	#endregion
	
	#region Auxiliar Attributes
	private GameObject auxObject;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
        // Initialize world values
        if (GameObject.FindWithTag("MultiplayerManager"))
        {
            isOnline = true;
        }
        else
        {
            isOnline = false;
        }

        if(isOnline)
        {
			playerObjects = GameObject.FindGameObjectsWithTag("Player");
			for(int i = 0; i < playerObjects.Length; i++)
			{
				if(playerObjects[i].GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					playerTransform = playerObjects[i].transform;
					break;
				}
			}
		}
		else
		{
			playerTransform = GameObject.FindWithTag("Player").transform;
		}
		
		nitroObjects = GameObject.FindGameObjectsWithTag("NitroPickup");
		multiplierObjects = GameObject.FindGameObjectsWithTag("MultiplierPickup");
		timeObjects = GameObject.FindGameObjectsWithTag("TimePickup");
		
		// Initialize Player values
		minimapPlayer = transform;
		
		// Initialize nitro pickups
		minimapNitro = new Transform[nitroObjects.Length];
		
		for(int i = 0; i < nitroObjects.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapNitro[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(0, 223, 255, 255);	// Blue
		}
		
		// Initialize multiplier pickups
		minimapMultiplier = new Transform[multiplierObjects.Length];
		
		for(int i = 0; i < multiplierObjects.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapMultiplier[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(236, 197, 48, 255);	// Yellow
		}
		
		// Initialize time pickups
		minimapTime = new Transform[timeObjects.Length];
		
		for(int i = 0; i < minimapTime.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapTime[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(63, 231, 86, 255);	// Green
		}

        // Initialize player pickups
        if (isOnline)
        {
            minimapPlayers = new Transform[playerObjects.Length];

            for (int i = 0; i < minimapPlayers.Length; i++)
            {
                if (playerObjects[i].transform != playerTransform)
                {
                    auxObject = (GameObject)Instantiate(pickupPrefab, Vector3.zero, Quaternion.identity);
                    auxObject.transform.SetParent(transform);
                    auxObject.transform.localScale = Vector3.one;
                    minimapPlayers[i] = auxObject.transform;

                    // Setup color
                    auxObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255);    // Red
                }
            }
        }
	}
	
	private void Update () 
	{
		// Update minimap scale based on car speed
		//minimapScale = playerTransform.GetComponent<CarEngine>().SpeedAsKM * 0.5f;
		
		// Update minimap rotation based on car rotation
		playerRotation = playerTransform.localRotation.eulerAngles;
		minimapPlayer.localRotation = Quaternion.Euler (new Vector3(minimapPlayer.localRotation.eulerAngles.x, minimapPlayer.localRotation.eulerAngles.y, playerRotation.y));
		
		// Update nitro pickups based on world nitro pickups
		for(int i = 0; i < minimapNitro.Length; i++)
		{
			if(i < nitroObjects.Length)
			{				
				// Update pickups UI position based on world position
				if(nitroObjects[i].GetComponent<BoxCollider>().enabled)
				{
					if(!minimapNitro[i].GetComponent<Image>().enabled)
					{
						minimapNitro[i].GetComponent<Image>().enabled = true;
					}
					
					pickupPosition = nitroObjects[i].transform.position - playerTransform.position;
					
					if(pickupPosition.sqrMagnitude < minimapRadius * minimapScale)
					{
						finalVector.x = pickupPosition.x * minimapScale;
						finalVector.y = pickupPosition.z * minimapScale;
						minimapNitro[i].localPosition = finalVector;
					}
					else
					{
						auxAngle = Mathf.Atan2(pickupPosition.z, pickupPosition.x);
						finalVector.x = Mathf.Cos (auxAngle) * minimapRadius/2 * minimapScale/2;
						finalVector.y = Mathf.Sin (auxAngle) * minimapRadius/2 * minimapScale/2;
						minimapNitro[i].localPosition = finalVector;
					}
				}
				else
				{
					if(minimapNitro[i].GetComponent<Image>().enabled)
					{
						minimapNitro[i].GetComponent<Image>().enabled = false;
					}
				}
			}
			else
			{
				if(minimapNitro[i].GetComponent<Image>().enabled)
				{
					Debug.Log ("Minimap: disabled UI nitro pickup because it has not a world references");
					minimapNitro[i].GetComponent<Image>().enabled = false;
				}
			}
		}
		
		// Update multiplier pickups based on world multiplier pickups
		for(int i = 0; i < minimapMultiplier.Length; i++)
		{
			if(i < multiplierObjects.Length)
			{				
				// Update pickups UI position based on world position
				if(multiplierObjects[i].GetComponent<BoxCollider>().enabled)
				{
					if(!minimapMultiplier[i].GetComponent<Image>().enabled)
					{
						minimapMultiplier[i].GetComponent<Image>().enabled = true;
					}
					
					pickupPosition = multiplierObjects[i].transform.position - playerTransform.position;
					
					if(pickupPosition.sqrMagnitude < minimapRadius * minimapScale)
					{
						finalVector.x = pickupPosition.x * minimapScale;
						finalVector.y = pickupPosition.z * minimapScale;
						minimapMultiplier[i].localPosition = finalVector;
					}
					else
					{
						auxAngle = Mathf.Atan2(pickupPosition.z, pickupPosition.x);
						finalVector.x = Mathf.Cos (auxAngle) * minimapRadius/2 * minimapScale/2;
						finalVector.y = Mathf.Sin (auxAngle) * minimapRadius/2 * minimapScale/2;
						minimapMultiplier[i].localPosition = finalVector;
					}
				}
				else
				{
					if(minimapMultiplier[i].GetComponent<Image>().enabled)
					{
						minimapMultiplier[i].GetComponent<Image>().enabled = false;
					}
				}
			}
			else
			{
				if(minimapMultiplier[i].GetComponent<Image>().enabled)
				{
					Debug.Log ("Minimap: disabled UI multiplier pickup because it has not a world references");
					minimapMultiplier[i].GetComponent<Image>().enabled = false;
				}
			}
		}
		
		// Update time pickups based on world time pickups
		for(int i = 0; i < minimapTime.Length; i++)
		{
			if(i < timeObjects.Length)
			{				
				// Update pickups UI position based on world position
				if(timeObjects[i].GetComponent<BoxCollider>().enabled)
				{
					if(!minimapTime[i].GetComponent<Image>().enabled)
					{
						minimapTime[i].GetComponent<Image>().enabled = true;
					}
					
					pickupPosition = timeObjects[i].transform.position - playerTransform.position;
					
					if(pickupPosition.sqrMagnitude < minimapRadius * minimapScale)
					{
						finalVector.x = pickupPosition.x * minimapScale;
						finalVector.y = pickupPosition.z * minimapScale;
						minimapTime[i].localPosition = finalVector;
					}
					else
					{
						auxAngle = Mathf.Atan2(pickupPosition.z, pickupPosition.x);
						finalVector.x = Mathf.Cos (auxAngle) * minimapRadius/2 * minimapScale/2;
						finalVector.y = Mathf.Sin (auxAngle) * minimapRadius/2 * minimapScale/2;
						minimapTime[i].localPosition = finalVector;
					}
				}
				else
				{
					if(minimapTime[i].GetComponent<Image>().enabled)
					{
						minimapTime[i].GetComponent<Image>().enabled = false;
					}
				}
			}
			else
			{
				if(minimapTime[i].GetComponent<Image>().enabled)
				{
					Debug.Log ("Minimap: disabled UI time pickup because it has not a world references");
					minimapTime[i].GetComponent<Image>().enabled = false;
				}
			}
		}

        // Update players based on world time 
        if (isOnline)
        {
            for (int i = 0; i < minimapPlayers.Length; i++)
            {
                if (i < playerObjects.Length)
                {
                    // Update pickups UI position based on world position
                    if (playerObjects[i] != null && minimapPlayers[i] != null)
                    {
                        if (!minimapPlayers[i].GetComponent<Image>().enabled)
                        {
                            minimapPlayers[i].GetComponent<Image>().enabled = true;
                        }

                        pickupPosition = playerObjects[i].transform.position - playerTransform.position;

                        if (pickupPosition.sqrMagnitude < minimapRadius * minimapScale)
                        {
                            finalVector.x = pickupPosition.x * minimapScale;
                            finalVector.y = pickupPosition.z * minimapScale;
                            minimapPlayers[i].localPosition = finalVector;
                        }
                        else
                        {
                            auxAngle = Mathf.Atan2(pickupPosition.z, pickupPosition.x);
                            finalVector.x = Mathf.Cos(auxAngle) * minimapRadius / 2 * minimapScale / 2;
                            finalVector.y = Mathf.Sin(auxAngle) * minimapRadius / 2 * minimapScale / 2;
                            minimapPlayers[i].localPosition = finalVector;
                        }
                    }
                    else
                    {
                        if (minimapPlayers[i] != null)
                        {
                            if (minimapPlayers[i].GetComponent<Image>().enabled)
                            {
                                minimapPlayers[i].GetComponent<Image>().enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    if (minimapPlayers[i].GetComponent<Image>().enabled)
                    {
                        Debug.Log("Minimap: disabled UI player pickup because it has not a world references");
                        minimapPlayers[i].GetComponent<Image>().enabled = false;
                    }
                }
            }
        }
	}
	#endregion
	
	#region Minimap Methods
	public void UpdateMinimap()
	{
		// Initialize world values
		if(isOnline)
		{
			playerObjects = GameObject.FindGameObjectsWithTag("Player");
			for(int i = 0; i < playerObjects.Length; i++)
			{
				if(playerObjects[i].GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					playerTransform = playerObjects[i].transform;
					break;
				}
			}
		}
		else
		{
			playerTransform = GameObject.FindWithTag("Player").transform;
		}
		
		for(int i = 1; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild (i).gameObject);
		}
		
		nitroObjects = GameObject.FindGameObjectsWithTag("NitroPickup");
		multiplierObjects = GameObject.FindGameObjectsWithTag("MultiplierPickup");
		timeObjects = GameObject.FindGameObjectsWithTag("TimePickup");
		
		// Initialize Player values
		minimapPlayer = transform;
		
		// Initialize nitro pickups
		minimapNitro = new Transform[nitroObjects.Length];
		
		for(int i = 0; i < nitroObjects.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapNitro[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(0, 223, 255, 255);	// Blue
		}
		
		// Initialize multiplier pickups
		minimapMultiplier = new Transform[multiplierObjects.Length];
		
		for(int i = 0; i < multiplierObjects.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapMultiplier[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(236, 197, 48, 255);	// Yellow
		}
		
		// Initialize time pickups
		minimapTime = new Transform[timeObjects.Length];
		
		for(int i = 0; i < minimapTime.Length; i++)
		{
			auxObject = (GameObject)Instantiate (pickupPrefab, Vector3.zero, Quaternion.identity);
			auxObject.transform.SetParent(transform);
			auxObject.transform.localScale = Vector3.one;
			minimapTime[i] = auxObject.transform;
			
			// Setup color
			auxObject.GetComponent<Image>().color = new Color32(63, 231, 86, 255);	// Green
		}

        // Initialize player pickups
        if (isOnline)
        {
            minimapPlayers = new Transform[playerObjects.Length];

            for (int i = 0; i < minimapPlayers.Length; i++)
            {
                if (playerObjects[i].transform != playerTransform)
                {
                    auxObject = (GameObject)Instantiate(pickupPrefab, Vector3.zero, Quaternion.identity);
                    auxObject.transform.SetParent(transform);
                    auxObject.transform.localScale = Vector3.one;
                    minimapPlayers[i] = auxObject.transform;

                    // Setup color
                    auxObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255);    // Red
                }
            }
        }
	}
	#endregion
}