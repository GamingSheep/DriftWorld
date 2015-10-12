#pragma warning disable 0414
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Networking;
using System.Collections;

public class CarController : MonoBehaviour 
{
	#region Public Attributes
	[Header("Main Attributes")]
	private bool isPlayer;
	public float maxAngle;
	public float respawnTempInit;
	
	[Header("Car References")]
	public GameObject sparks;
	public bool recentlyAvoid;
	public MeshRenderer[] renderers;
    public Animator chasisAnimator;
	
	[Header("Gamepad Attributes")]
	public float rumbleIntensity;
	public float rumbleTempInit;
	public Transform targetTransform;
	public float lookScale;
	
	[Header("Visual Attributes")]
	public GameObject[] lowObjects;
	public GameObject[] highObjects;
	#endregion
	
	#region Private Attributes
	private float steerTime			 = 0.05f;
	private bool isGrounded			 = false;
	private bool backwards			 = false;
	private float basicMaxRPM;
	private bool canMove			= true;
	private float moveDelay			= 0.2f;
	private bool rumbling           = false;
	private float rumbleTemp;
	private Vector3 targetPosition;
	private bool lookBack;
	private bool respawning;
	private float respawnTemp;
	#endregion
	
	#region References
	private CarEngine engine;
	private CameraRig cameraRig;
	private CarWheel[] wheels;
	private CarSetup carSetup;
	private LightsSetup	lightsSetup;
	private AudioSource audioSource;
	private CarInput carInput;
	private bool wasGrounded;
	private Vector3 rot;
	private GameObject auxObject;
	private DataManager dataManager;
	private SparksManager sparksManager;
	private Rigidbody carRigidbody;
	private NetworkIdentity netIdentity;
	#endregion
	
	#region Main Methods
	private void Start () 
	{ 
		// Initialize values
		engine 			= GetComponent<CarEngine>();		
		carSetup 		= GetComponent<CarSetup>();
		wheels 			= carSetup.Wheels;
		basicMaxRPM 	= carSetup.EngineMaxRPM;
		lightsSetup		= GetComponent<LightsSetup>();
		audioSource     = GetComponent<AudioSource>();
		cameraRig 	 	= GetComponent<CameraRig>();
		carRigidbody	= GetComponent<Rigidbody>();
		carInput        = CarInput.Instance;
		rumbling		= false;
		rumbleTemp		= rumbleTempInit;
		auxObject		= null;
		dataManager		= DataManager.Instance;
		sparksManager	= GameObject.Find ("SparksManager").GetComponent<SparksManager>();
		lookBack		= false;
		respawnTemp		= respawnTempInit;
		isPlayer		= false;
		
		if(GetComponent<NetworkIdentity>())
		{
			netIdentity		= GetComponent<NetworkIdentity>();
			isPlayer		= netIdentity.isLocalPlayer;
		}
		else
		{
			isPlayer		= true;
		}
		
		// Set up visual car
        switch(Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsWebPlayer:
			{
				for(int i = 0; i < lowObjects.Length; i++)
                {
                    lowObjects[i].SetActive (false);
                }
                for(int i = 0; i < highObjects.Length; i++)
                {
                    highObjects[i].SetActive (true);
                }
				break;
			}
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.BlackBerryPlayer:
			case RuntimePlatform.Android:
			{
				for(int i = 0; i < lowObjects.Length; i++)
                {
                    lowObjects[i].SetActive (true);
                }
                for(int i = 0; i < highObjects.Length; i++)
                {
                    highObjects[i].SetActive (false);
                }
				break;
			}
			default:
			{
				for(int i = 0; i < lowObjects.Length; i++)
                {
                    lowObjects[i].SetActive (true);
                }
                for(int i = 0; i < highObjects.Length; i++)
                {
                    highObjects[i].SetActive (false);
                }
				break;
			}
		}
		
		renderers = GetComponentsInChildren<MeshRenderer>();
	}
	
	private void OnDisable()
	{
		// Avoid rumble value freeze
		if(carInput != null)
		{
			carInput.Rumble(0.0f);
		}
	}

	private void Update () 
	{ 
		if(isPlayer)
		{
			if(recentlyAvoid) recentlyAvoid = false;
			
			if(rumbling)
			{
				rumbleTemp -= Time.deltaTime;
				if(rumbleTemp <= 0)
				{
					rumbling = false;
				}
			}
			else
			{
				carInput.Rumble (0.0f);
			}
			
			// Restart engine values
			engine.throttle  = 0;
			engine.handbrake = 0;
			engine.brake = 0;
			
			if(engine.steer < 0)
			{
				if(!carInput.Left ())
				{
					if(engine.steer < -0.05f)
					{
						engine.steer += steerTime;
					}
					else
					{
						engine.steer = 0.0f;
					}
				}
			}
			else if(engine.steer > 0) 	
			{
				if(!carInput.Right ())
				{
					if(engine.steer > 0.05f)
					{
						engine.steer -= steerTime;
					}
					else
					{
						engine.steer = 0.0f;
					}
				}
			}
			
			// Respawn logic
			if(respawning)
			{
				if(carRigidbody.isKinematic)
				{
					carRigidbody.isKinematic = false;
				}
				
				respawnTemp -= Time.deltaTime;
				if(respawnTemp <= 0)
				{
					for(int i = 0; i < renderers.Length; i++)
					{
						renderers[i].gameObject.SetActive (true);
					}
					
					CancelInvoke ("RespawnCarVisual");
					respawning = false;
				}
			}
			
			// Acceleration input
			if(carInput.ThrottleForward())
			{
				lightsSetup.SetBreakLights(false);
				lightsSetup.SetReverseLights(false);
                chasisAnimator.SetInteger("state", 0);
                backwards = false;
				
				if(engine.Gear == -1) 
				{
					engine.throttle = -carInput.ForwardIntensity();
				}
				else 
				{
					engine.throttle = carInput.ForwardIntensity();
				}
			}
			
			// Brake and Reverse input
			if(carInput.ThrottleBackward())
			{
				if(engine.SpeedAsKM < 15 || backwards)
				{
					lightsSetup.SetBreakLights(false);
					lightsSetup.SetReverseLights(true);
                    chasisAnimator.SetInteger("state", 2);
                    backwards = true;
					
					if(engine.Gear != -1)
					{
						engine.throttle = -carInput.BackwardIntensity();
					}
					else
					{
						engine.throttle = carInput.BackwardIntensity();
					}
				} 
				else 
				{
					lightsSetup.SetBreakLights(true);
					lightsSetup.SetReverseLights(false);
                    chasisAnimator.SetInteger("state", 1);
                    engine.brake = 1;
					engine.throttle = 0;
				}
			}
			else
			{
				lightsSetup.SetBreakLights(false);
                chasisAnimator.SetInteger("state", 0);
            }
			
			// Hand Brake input
			if(carInput.HandBrake())
			{
				lightsSetup.SetBreakLights(true);
				lightsSetup.SetReverseLights(false);
                chasisAnimator.SetInteger("state", 1);
                engine.handbrake = 1;
			}
			else
			{
				if(!carInput.ThrottleBackward())
				{
					lightsSetup.SetBreakLights (false);
                    chasisAnimator.SetInteger("state", 0);
                }
				
				engine.handbrake = 0;
			}
			
			if(engine.handbrake != 1 && engine.Gear != -1 && engine.brake != 1)
			{
				lightsSetup.ResetEmission();
                chasisAnimator.SetInteger("state", 0);
            }
			else
			{
				if(engine.Gear == -1)
				{
					if(carInput.ThrottleForward())
					{
						lightsSetup.SetBreakLights(true);
                        chasisAnimator.SetInteger("state", 1);
                    }
				}
			}
			
			
			// Steer input
			if(carInput.Left())
			{
				if(engine.steer > -0.4f)
				{
					if(dataManager.isGamepad)
					{
						engine.steer = carInput.HorizontalIntensity() * 0.5f;
					}
					else
					{
						engine.steer -= steerTime;
					}
				}
			}
			else if(carInput.Right())
			{
				if(engine.steer < 0.4f)
				{
					if(dataManager.isGamepad)
					{
						engine.steer = carInput.HorizontalIntensity() * 0.5f;
					}
					else
					{
						engine.steer += steerTime;
					}
				}
			}
				
			// Nitro input
			if(carInput.Nitrous())
			{
				engine.UseNitro = true;
			}
			else
			{
				engine.UseNitro = false;
			}
			
			// Gears input
			if(carSetup.Automatic == false)
			{
				if(carInput.GearUp())
				{
					engine.ShiftUp();
				}
				else if(carInput.GearDown())
				{
					engine.ShiftDown();
				}
			}
			
			// Camera change input
			if(carInput.Camera())
			{
				cameraRig.ChangeCamera();
			}
			
			// Look back input
			if(carInput.LookBack())
			{
				if(!lookBack)
				{
					lookBack = true;
					cameraRig.SetBackCamera(true);
				}
			}
			else
			{
				if(lookBack)
				{
					lookBack = false;
					cameraRig.SetBackCamera(false);
				}
			}
			
			// Respawn input
			if(carInput.Respawn())
			{
				if(!respawning)
				{
					respawning = true;
					respawnTemp = respawnTempInit;
					carRigidbody.isKinematic = true;
					transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
					transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
					carRigidbody.velocity = Vector3.zero;
					carRigidbody.angularVelocity = Vector3.zero;
					InvokeRepeating ("RespawnCarVisual", 0.2f, 0.2f);
				}
			}			
			
			// Camera rotation input
			targetPosition = targetTransform.localPosition;
			targetPosition.x = carInput.Look () * lookScale;
			
			if(targetPosition.x > -0.2 && targetPosition.x < 0.2f)
			{
				targetPosition.x = 0.0f;
			}
			
			targetTransform.localPosition = targetPosition;
		}
		
		// Last fixes logic
		GroundHit();
	}
	#endregion
	
	#region Detection Methods
	private void OnCollisionEnter(Collision other)
	{
		//Detect hit with other cars
		if(other.gameObject.layer == LayerMask.NameToLayer("Car") && !other.gameObject.name.Contains ("Wheel") && !other.gameObject.name.Contains ("Cone"))
		{			
			if(isPlayer) 
			{
				engine.recentlyDamage = true;
				carInput.Rumble(rumbleIntensity);
				rumbling = true;
				rumbleTemp = rumbleTempInit;
			}
			
			// Play contact audio
			if(!audioSource.isPlaying)
			{
				audioSource.Play ();
			}
			
			if(other.contacts.Length > 0) 
			{
				Sparks(other.contacts[0]);
			}		
		}
		
		//Detect hit with walls or random stuff
		else if(other.gameObject.layer == LayerMask.NameToLayer("Obstacle") && !other.gameObject.name.Contains ("Wheel") && !other.gameObject.name.Contains ("Cone"))
		{
			if(isPlayer) 
			{
				engine.recentlyDamage = true;
				carInput.Rumble(rumbleIntensity);
				rumbling = true;
				rumbleTemp = rumbleTempInit;
			}
			
			// Play contact audio
			if(!audioSource.isPlaying && audioSource.gameObject.activeSelf)
			{
				audioSource.Play ();
			}
			
			if(other.contacts.Length > 0)
			{
				if(Vector3.Distance(other.contacts[0].point, transform.position) < 5)
				{
					Sparks(other.contacts[0]);
				}
			}
		}
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			if(!engine.recentlyDamage)
			{
				Debug.Log ("CarController: obstacle recently avoided");
				recentlyAvoid = true;
			}
		}
	}
	#endregion
	
	#region Other Methods
	private void RespawnCarVisual()
	{
		if(renderers[0].gameObject.activeSelf)
		{
			for(int i = 0; i < renderers.Length; i++)
			{
				renderers[i].gameObject.SetActive (false);
			}
		}
		else
		{
			for(int i = 0; i < renderers.Length; i++)
			{
				renderers[i].gameObject.SetActive (true);
			}
		}
	}
	
	private void GroundHit()
	{
		wasGrounded = isGrounded;	
		
		foreach(CarWheel wheel in wheels)
		{
			if(!wheel.HitToSurface.collider)
			{
				isGrounded = false;
			}
			else
			{
				isGrounded = true;
			}
		}
		
		if(!wasGrounded && isGrounded)
		{
			if(isPlayer)
			{
				carInput.Rumble(0.0f);
				rumbling = true;
				rumbleTemp = rumbleTempInit;
			}
			
			// Play ground contact audio
		}
	}
	
	private void Sparks(ContactPoint contact)
	{
		if(sparks != null)
		{
			rot  = transform.rotation.eulerAngles;
			rot.y += 180;
			
			// Old Method
			/*auxObject = (GameObject)Instantiate(sparks, contact.point, Quaternion.Euler(rot));
			Destroy (auxObject, 1.0f);*/
			
			// New method
			sparksManager.ActiveSpark(contact.point, Quaternion.Euler (rot));
		}
	}
	#endregion
	
	#if UNITY_EDITOR
	#region Editor Methods
	private void OnDrawGizmos()
	{
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc(transform.position ,Vector3.up, 27f);
	}
	#endregion
	#endif
	
	#region Properties	
	public float Speed 
	{
		get { return engine.SpeedAsKM; }	
	}
	
	public CameraRig CameraRig
	{
		get { return cameraRig; }
	}
	#endregion
}