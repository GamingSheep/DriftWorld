#pragma warning disable 0414
#pragma warning disable 0219
using UnityEngine;
using System.Collections;

public class CarSetup : MonoBehaviour 
{
	#region Enums
	public enum DriveEnum
	{
		FWD = 0,
		RWD = 1,
		AWD = 2,
		Trailer = 3,
	}
	#endregion
	
	#region Main Attributes
	[Header("Main Attributes")]
	public bool realTimeEdit;
	private Rigidbody auxRigidbody;
	public bool RigidbodySleepOnAwake = true;
	public float CarMass = 1000f;
	public float CarInteria = 1f;
	public Transform CenterOfMass;
	public DriveEnum Drive = DriveEnum.RWD;
	public CarWheel[] Wheels;
	#endregion
	
	#region Engine Attributes
	[Header("Engine Attributes")]
	public float EngineMinRPM = 1000;
	public float EngineMaxRPM = 8000;
	public float GearDownRPM = 4500;
	public float GearUpRPM = 7000;
	public float ClutchTime = 0.20f;
	public float EngineMaxTorque = 350;
	public float EngineTorqueRPM = 4000;
	public float EngineMaxPowerKw = 75;
	public float EnginePowerRPM = 6500;
	public float EngineInteria = 1f;
	public float EngineFriction = 10f;
	public float EngineRPMFriction = 1f;
	public bool  LimitEngineSpeed = false;
	public float LimitSpeedTo = 100f;
	
	public float[] Gears = { -4f, 0, 4f, 2.5f, 1.78f, 1.36f, 1.1f,0.89f };
	public float GearFinalRatio = 5.75f;
	public bool Automatic = true;
	public float DifferentialLock = 0f;
	public bool AutoReverse = true;
	#endregion
	
	#region Wheels Attributes
	[Header("Wheels Attributes")]
	public float FrontBrakeTorque = 2000;
	public float RearBrakeTorque = 3500;
	public float TrailerBrakeTorque = 3500f;
	
	public float HandBrakeTorque = 5000;
	public float HandBrakeTorqueFront = 1000;
	public float TrailerHandBrakeTorque = 5000;
	
	public float MaxSteeringAngle = 30;
	public float FrontWheelGrip = 1.5f;
	public float FrontWheelSideGrip = 1.5f;
	public float RearWheelGrip = 1.6f;
	public float RearWheelSideGrip = 1.6f;
	public float TrailerWheelGrip = 1.5f;
	public float TrailerWheelSideGrip=1.5f;
	
	public float FrontWheelRadius = 0.32f;
	public float RearWheelRadius = 0.32f;
	public float TrailerWheelRadius = 0.32f;
	
	public float WheelInteria = 1f;
	public float FrictionTorque = 10f;
	public float MassFriction = 0.25f;
	
	private bool NoSuspension = false;
	public float SuspensionHeight = 0.25f;
	public float SuspensionStiffness = 5000;
	public float SuspensionReleaseCoef = 50;
	public Transform WheelBase;
	public float WheelBaseAlignment = 0f;
	
	public float BlurSwtichVelocity = 20f;
	private bool BurnoutStart = false;
	private float BurnoutStartDuration = 1f;
	#endregion
	
	#region Lights Attributes
	[Header("Lights Attributes")]
	public Transform[] HeadlightsLightObjects;
	public Light[] HeadlightsLights;
	public Transform[] BrakelightsLightObjects;
	public Light[] BrakelightsLights;
	public Transform[] ReverseLightLightObjects;
	public Light[] ReverselightLights;
	
	public float DefaultBrakeLightIntensity = 0.7f;
	public bool LightsOn = false;
	#endregion
	
	#region AeroDynamics Attributes
	[Header("AeroDynamics Attributes")]
	public bool EnableWing = true;
	public float WingDownforceCoef = -0.5f;
	public bool EnableAirFriction = true;
	public Vector3 AirFrictionCoef = new Vector3(0.5f, 0.5f, 0.05f);
	#endregion
	
	#region Effects Attributes
	[Header("Effects Attributes")]
	public GameObject ExhaustBackfire;
	public float BackfireSeconds = 0.5f;
	public float BackfireBlockingSeconds = 5;
	#endregion
	
	#region Nitro Attributes
	[Header("Nitro Attributes")]
	public GameObject NosFireObject;
	public bool NitroEnable = false;
	public bool AutoFill = true;
	public float RefillSpeed = 1f;
	public float NitroInitialAmount = 5f;
	public float NitroLeft = 0;
	public float ForceBalance= 1f;
	public float ForceAdd = 25f;
	#endregion
	
	#region Other Attributes
	[Header("Other Attributes")]
	public bool UseEarthFX;
	public string SkidmarkObjectName;
	private GameObject SkidMarks;
	#endregion
	
	#region Auxiliar Attributes
	private float wingDownEff;
	private Vector3 localVelo;
	private Vector3 absLocalVelo;
	private Vector3 airResistance;
	#endregion
	
	#region Main Methods
	private void Awake()
	{
		auxRigidbody = transform.GetComponent<Rigidbody>();
		
		if (RigidbodySleepOnAwake)
		{
			auxRigidbody.Sleep();
		}

        // Initialize values
        NitroLeft = NitroInitialAmount;

        if (CenterOfMass != null)
        {
            GetComponent<Rigidbody>().centerOfMass = CenterOfMass.localPosition;
        }

        if (WheelBase != null)
        {
            WheelBase.localPosition = new Vector3(0, WheelBaseAlignment);
        }

        if (SkidmarkObjectName != "")
        {
            if (GameObject.Find(SkidmarkObjectName))
            {
                SkidMarks = GameObject.Find(SkidmarkObjectName);
                Debug.Log(SkidMarks.name);
            }
        }

        if (CarInteria < 0.01f)
        {
            CarInteria = 0.01f;
        }

        GetComponent<Rigidbody>().inertiaTensor *= CarInteria;

        updateVehicleConfig();

        NosFireObject.SetActive(false);
    }
	
	private void FixedUpdate () 
	{
		if(realTimeEdit)
		{
			updateVehicleConfig();
		}
		// Apply win force
		if (auxRigidbody != null && EnableWing)
		{
			wingDownEff = WingDownforceCoef * auxRigidbody.velocity.sqrMagnitude;
			auxRigidbody.AddForceAtPosition(wingDownEff * transform.up, transform.position);
		}
		
		// Apply air friction
		if (auxRigidbody != null && EnableAirFriction)
		{
			localVelo = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
			absLocalVelo = new Vector3(Mathf.Abs(localVelo.x), Mathf.Abs(localVelo.y), Mathf.Abs(localVelo.z));
			airResistance = Vector3.Scale(Vector3.Scale(localVelo, absLocalVelo), -2 * AirFrictionCoef);
			auxRigidbody.AddForce(transform.TransformDirection(airResistance));
		}
		
		// Fix left nitro amount value
		if(NitroLeft > NitroInitialAmount)
		{
			NitroLeft = NitroInitialAmount;
		}
	}
	#endregion
	
	#region Vehicle Methods
	private void updateVehicleConfig()
	{
		if (CenterOfMass != null)
		{
			GetComponent<Rigidbody>().centerOfMass = CenterOfMass.localPosition;
		}
		if (WheelBase != null)
		{
			WheelBase.localPosition = new Vector3(0, WheelBaseAlignment);
		}
		
		if (CarMass<1f) 
		{
			CarMass=1f;
		}
		
		auxRigidbody.mass = CarMass;
		
		foreach (CarWheel CarWheel in Wheels)
		{
			if (CarWheel.WheelLocation == WheelLocationEnum.Front)
			{
				if (Drive == DriveEnum.FWD || Drive == DriveEnum.AWD) 
				{
					CarWheel.isPowered = true;
				}
				
				CarWheel.maxSteeringAngle = MaxSteeringAngle;
				CarWheel.suspensionHeight = SuspensionHeight;
				CarWheel.suspensionStiffness = SuspensionStiffness;
				CarWheel.brakeTorque = FrontBrakeTorque;
				CarWheel.handbrakeTorque = HandBrakeTorqueFront;
				CarWheel.definedGrip = FrontWheelGrip;
				CarWheel.definedSideGrip= FrontWheelSideGrip;
				CarWheel.radius = FrontWheelRadius;
			}
			
			if (CarWheel.WheelLocation == WheelLocationEnum.Rear)
			{
				if (Drive == DriveEnum.RWD || Drive == DriveEnum.AWD) 
				{
					CarWheel.isPowered = true;
				}
				
				CarWheel.maxSteeringAngle = 0;
				CarWheel.suspensionHeight = SuspensionHeight;
				CarWheel.suspensionStiffness = SuspensionStiffness;
				CarWheel.brakeTorque = RearBrakeTorque;
				CarWheel.handbrakeTorque = HandBrakeTorque;
				CarWheel.definedGrip = RearWheelGrip;
				CarWheel.radius = RearWheelRadius;
				CarWheel.definedSideGrip = RearWheelSideGrip;
			}
			
			if (CarWheel.WheelLocation == WheelLocationEnum.Trailer)
			{
				CarWheel.isPowered = false;
				CarWheel.maxSteeringAngle = 0;
				CarWheel.suspensionHeight = SuspensionHeight;
				CarWheel.suspensionStiffness = SuspensionStiffness;
				CarWheel.brakeTorque = TrailerBrakeTorque;
				CarWheel.handbrakeTorque = TrailerHandBrakeTorque;
				CarWheel.definedGrip = TrailerWheelGrip;
				CarWheel.radius = TrailerWheelRadius;
				CarWheel.definedSideGrip = TrailerWheelSideGrip;
			}
			
			CarWheel.suspensionRelease = SuspensionReleaseCoef;
			CarWheel.inertia = WheelInteria;
			CarWheel.frictionTorque = FrictionTorque;
			CarWheel.massFraction = MassFriction;
			CarWheel.blurSwitchVelocity = BlurSwtichVelocity;
			
			if(CarWheel.SkidmarkObject != null)
			{
				CarWheel.SkidmarkObject = SkidMarks;
			}
		}
	}
	#endregion
}