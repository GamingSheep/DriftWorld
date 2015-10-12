using UnityEngine;
using System.Collections;

public class CarEngine : MonoBehaviour 
{
	#region Main Attributes
	private CarSetup CarSetup;
	private CarAudio CarAudio;
	private Vector3 engineOrientation = Vector3.right;
	#endregion
	
	#region Inputs
	[Header("Inputs Attributes")]
	public float throttle = 0;	
	public float brake = 0;
	public float handbrake = 0;
	public float steer;
	public int CurrentGear = 2;
	public int Gear = 0;
	public string GearAuto = "D";

	public bool isClutch = false;
	private float clutchInteral = 0;
	private int gearToChange = 0;
	
	public float RPM;
	public float SpeedAsKM;
	public float SpeedAsMile;
	public float TurboFill = 0;
	private float TurboDiff = 0;
	
	public float slipRatio = 0.0f;
	public float engineAngularVelo;
	
	public float totalSlip = 0.0f;
	
	private bool isShifting = false;
	private float ShiftInternalCounter = 1;
	private bool isBackfire = true;
	private bool isBackfireBlocked = false;
	private float BackfireBlockingInternal = 0;
	private float BackfireInternalCounter;
	private bool isBackfireAudioPlaying = false;
	public float limitedTimeEngineVelo = 0;
	private bool m_useNitro = false;
	public bool brakeFill;
	private float Sqr(float x) { return x * x; }
	public bool recentlyDamage;
	#endregion
	
	#region Auxiliar Attributes
	private float ratio;
	private float inertia;
	private float engineFrictionTorque;
	private float engineTorque;
	private bool PoweredWheelsOnGround;
	private float engineAngularAcceleration;
	private Rigidbody auxRigidbody;
	private int iPoweredCount;
	private float drivetrainFraction;
	private float averageAngularVelo;
	private float PoweredWheelsOnGroundCount;
	private float minClutchRPM;
	private float rpmWiggle;
	private int iGear;
	private float maxPowerTorque;
	private float aproxFactor;
	private float torque;
	#endregion
	
	#region Other Attributes
	[Header("Other Attributes")]
	public float WaitForReverse = 2f;
	private float lockingTorque;
	public float intThrottle = 0;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Initialize values
		CarSetup = GetComponent(typeof(CarSetup)) as CarSetup;
		CarAudio = GetComponent(typeof(CarAudio)) as CarAudio;
		auxRigidbody = GetComponent<Rigidbody>();
		recentlyDamage = false;
		ResetNos();
		
		CarSetup.NosFireObject.SetActive (false);
	}
	
	private void FixedUpdate()
	{
		if(recentlyDamage) recentlyDamage = false;
		
		if (isClutch)
		{
			CurrentGear = 1;
			intThrottle = 0;
		}
		else
		{
			intThrottle = throttle;
		}
		
		ratio = CarSetup.Gears[CurrentGear] * CarSetup.GearFinalRatio;
		inertia = CarSetup.EngineInteria * Sqr(ratio);
		engineFrictionTorque = CarSetup.EngineFriction + RPM * CarSetup.EngineRPMFriction;
		
		engineTorque = 0;
		PoweredWheelsOnGround = true;
		
		if (!isClutch && PoweredWheelsOnGround)
		{
			engineTorque = (CalcEngineTorque() + Mathf.Abs(engineFrictionTorque)) * intThrottle;
		}
		
		slipRatio = 0.0f;
		
		if (ratio == 0)
		{
			// Neutral gear logic
			engineAngularAcceleration = (engineTorque - engineFrictionTorque) / CarSetup.EngineInteria;
			engineAngularVelo += engineAngularAcceleration * Time.deltaTime * 2;
			
			// Apply torque to car body
			auxRigidbody.AddTorque(-engineOrientation * engineTorque);
		}
		else
		{
			iPoweredCount = 0;
			
			foreach (CarWheel CarWheel in CarSetup.Wheels)
			{
				if (CarWheel.isPowered) iPoweredCount++;
			}
			
			drivetrainFraction = 0.8f / iPoweredCount;
			averageAngularVelo = 0;
			
			foreach (CarWheel CarWheel in CarSetup.Wheels)
			{
				if (CarWheel.isPowered) averageAngularVelo += CarWheel.angularVelocity * drivetrainFraction;
			}
			
			
			PoweredWheelsOnGroundCount = 0;
			
			foreach (CarWheel CarWheel in CarSetup.Wheels)
			{
				if (CarWheel.isPowered)
				{
					if (!isClutch)
					{
						if (CarWheel.angularVelocity * CarSetup.GearFinalRatio *CarSetup.Gears[CurrentGear] * (25 / Mathf.PI) > CarSetup.EngineMaxRPM)
						{
							CarWheel.angularVelocity = CarSetup.EngineMaxRPM / (CarSetup.GearFinalRatio * CarSetup.Gears[CurrentGear] * (25 / Mathf.PI));
						}
						
						if (CarSetup.LimitEngineSpeed && SpeedAsKM >= CarSetup.LimitSpeedTo)
						{
							if (CarWheel.angularVelocity * CarSetup.GearFinalRatio * CarSetup.Gears[CurrentGear] * (25 / Mathf.PI) > limitedTimeEngineVelo)
							{
								CarWheel.angularVelocity = limitedTimeEngineVelo / (CarSetup.GearFinalRatio * CarSetup.Gears[CurrentGear] * (25 / Mathf.PI));
							}
						}
						
						lockingTorque = (averageAngularVelo - CarWheel.angularVelocity) * CarSetup.DifferentialLock;
						CarWheel.drivetrainInertia = inertia * drivetrainFraction;
						CarWheel.driveFrictionTorque = engineFrictionTorque * Mathf.Abs(ratio) * drivetrainFraction;
						CarWheel.driveTorque = engineTorque * ratio * drivetrainFraction + lockingTorque;
					}
					
					slipRatio += CarWheel.slipRatio * drivetrainFraction;
					
					if (CarWheel.onGround)
					{
						PoweredWheelsOnGroundCount++;
					}
				}
			}
			
			PoweredWheelsOnGround = (PoweredWheelsOnGroundCount == iPoweredCount) ? true : false;
			
			// Update engine angular velocity
			engineAngularVelo = averageAngularVelo * ratio;
		}
		
		// Limit engine speed based engine velocity
		if (CarSetup.LimitEngineSpeed && SpeedAsKM >= CarSetup.LimitSpeedTo)
		{
			RPM = limitedTimeEngineVelo;
		}
		else
		{
			RPM = engineAngularVelo * (60.0f / (2 * Mathf.PI));
			limitedTimeEngineVelo=RPM;
		}
		
		// very simple simulation of clutch - just pretend we are at a higher rpm.
		minClutchRPM = CarSetup.EngineMinRPM;
		
		if (CurrentGear == 2 || CurrentGear == 0)
		{
			minClutchRPM += intThrottle * UnityEngine.Random.Range(3000, 3250);			
		}
		
		if (RPM < minClutchRPM)
		{
			RPM = minClutchRPM;
		}
		
		if (RPM > CarSetup.EngineMaxRPM)
		{
			RPM = CarSetup.EngineMaxRPM;
		}
		
		TurboDiff = CarSetup.EnginePowerRPM - CarSetup.EngineTorqueRPM;
		
		if (brake > 0) TurboFill = 0;
		if (intThrottle > 0) FillTurbo(RPM);
		
		rpmWiggle = (CarSetup.EngineMaxRPM * (0.5f + 0.5f * intThrottle));
		
		if (RPM >= rpmWiggle - 2000 && CurrentGear > 0 && intThrottle > 0)
		{
			RPM = RPM + UnityEngine.Random.Range(20, 100);
		}
		else if (RPM >= rpmWiggle - 1500 && CurrentGear > 0 && intThrottle > 0)
		{
			RPM = RPM + UnityEngine.Random.Range(100, 250);
		}
		else if ((RPM >= rpmWiggle - 1000 && CurrentGear > 0 && intThrottle > 0))
		{
			RPM = RPM + UnityEngine.Random.Range(50, 350);
		}
		
		// Automatic gear shifting. Bases shift points on throttle input and rpm.
		if (CarSetup.Automatic)
		{
			if (!isClutch)
			{
				if (CarSetup.AutoReverse)
				{
					if (CurrentGear == 2 && brake > 0 && SpeedAsKM < 10)
					{
						CurrentGear = 1;
						ShiftDown();
						
					}
					else if (CurrentGear == 0 && brake > 0 && SpeedAsKM < 10)
					{
						CurrentGear = 1;
						ShiftUp();
					}
				}
				
				if (RPM >= (CarSetup.GearUpRPM) && CurrentGear > 0 && intThrottle > 0 && CurrentGear < CarSetup.Gears.Length - 1)
				{
					ShiftUp();
					
					if (CarAudio != null && CurrentGear > 2)
					{
						CarAudio.PopTurbo(TurboFill);
					}
					
					TurboFill = 0;
					isClutch = true;
				}
				else if (RPM <= CarSetup.GearDownRPM && CurrentGear > 2)
				{
					ShiftDown();
					isClutch = true;
				}
				
				if (intThrottle < 0 && RPM <= CarSetup.EngineMinRPM)
				{
					CurrentGear = (CurrentGear == 0 ? 2 : 0);
				}
			}			
		}
		
		// Correct the gear for GUI usage
		iGear = CurrentGear - 1;
		Gear = iGear; // gear conversation to normal
		
		if (iGear == 0)
		{
			GearAuto = "N";
		}
		if (iGear > 0)
		{
			GearAuto = "D";
		}
		if (iGear < 0)
		{
			GearAuto = "R";
		}
		
		this.SpeedAsKM = auxRigidbody.velocity.magnitude * 3.6f;
		this.SpeedAsMile = this.SpeedAsKM * 0.6214f;
		
		if (CarSetup.ExhaustBackfire != null)
		{
			if ((brake > 0 || handbrake > 0) && !isBackfire && !isBackfireBlocked && CurrentGear > 2)
			{
				isBackfire = true;
				BackfireInternalCounter = UnityEngine.Random.Range(0.1f, 0.6f);
				
				if (!isBackfireBlocked)
				{
					BackfireBlockingInternal = 0;
				}
			}
			
			setBeckfire();
		}
		
		if (CarAudio != null) 
		{
			if (brake > 0)
			{
				CarAudio.ApplyBrakeDisk(brake);
			}
			else
			{
				CarAudio.ApplyBrakeDisk(0);
			}
		}
		
		foreach (CarWheel CarWheel in CarSetup.Wheels)
		{
			CarWheel.brake = brake;
			CarWheel.steering = steer;
			CarWheel.handbrake = handbrake;
		}
		
		if (isClutch) 
		{
			Clutch();
		}
		
		if (throttle > 0 && Gear > 0 )
		{
			UseNitroInternal();
		}
		
		if (brakeFill)
		{
			if((brake > 0 || handbrake > 0) && SpeedAsKM > 10.0f)
			{
				FillNitroInternal();
			}
		}
	}
	#endregion
	
	#region Calculation Methods
	// Calculate engine torque for current rpm and throttle values.
	private float CalcEngineTorque()
	{
		float result;
		
		if (RPM < CarSetup.EngineTorqueRPM)
		{
			result = CarSetup.EngineMaxTorque * (-Sqr(RPM / CarSetup.EngineTorqueRPM - 1) + 1);
		}
		else
		{
			maxPowerTorque = (CarSetup.EngineMaxPowerKw * 1000) / (CarSetup.EnginePowerRPM * 2 * Mathf.PI / 60);
			aproxFactor = (CarSetup.EngineMaxTorque - maxPowerTorque) / (2 * CarSetup.EngineTorqueRPM * CarSetup.EnginePowerRPM - Sqr(CarSetup.EnginePowerRPM) - Sqr(CarSetup.EngineTorqueRPM));
			torque = aproxFactor * Sqr(RPM - CarSetup.EngineTorqueRPM) + CarSetup.EngineMaxTorque;
			result = torque > 0 ? torque : 0;
		}
		
		if (RPM > CarSetup.EngineMaxRPM)
		{
			result *= 1 - ((RPM - CarSetup.EngineMaxRPM) * 0.006f);
			
			if (result < 0)
			{
				result = 0;
			}
		}
		
		if (RPM < 0)
		{
			result = 0;
		}
		
		return result;
	}
	#endregion
	
	#region Gears Methods
	private void Clutch()
	{
		if (isClutch)
		{
			clutchInteral -= Time.deltaTime;
			
			CurrentGear = 1;
			if (clutchInteral <= 0f)
			{
				isClutch = false;
				clutchInteral = CarSetup.ClutchTime;
				CurrentGear = gearToChange;
			}
		}
	}
	
	public void ShiftUp()
	{
		if (CurrentGear < CarSetup.Gears.Length - 1)
		{
			if (!isClutch)
			{
				clutchInteral = CarSetup.ClutchTime;
				gearToChange = CurrentGear + 1;
				isClutch = true;
				if (CarAudio != null)
				{ 
					CarAudio.PopGear(); 
				}
			}
		}
	}
	
	public void ShiftDown()
	{
		if (CurrentGear > 0)
		{
			if (!isClutch)
			{
				clutchInteral = CarSetup.ClutchTime;
				gearToChange = CurrentGear - 1;
				isClutch = true;
				if (CarAudio != null)
				{ 
					CarAudio.PopGear(); 
				}
			}
		}
	}
	
	public void ShiftDone()
	{
		if (isShifting)
		{
			ShiftInternalCounter -= Time.deltaTime;
			
			if (ShiftInternalCounter < 0f)
			{
				isShifting = false;
				ShiftInternalCounter = 1;
			}
		}
	}
	#endregion
	
	#region Shift Fire Methods
	private void setBeckfire()
	{		
		if (isBackfireBlocked)
		{
			BackfireBlockingInternal += Time.deltaTime;
			
			if (BackfireBlockingInternal >= CarSetup.BackfireBlockingSeconds) 
			{ 
				BackfireBlockingInternal = 0; 
				isBackfireBlocked = false; 
			}
		}
		
		if (isBackfire)
		{
			BackfireInternalCounter -= Time.deltaTime;
			
			if (BackfireInternalCounter >= 0f)
			{
				CarSetup.ExhaustBackfire.SetActive(true);
				
				if (CarAudio != null && !isBackfireAudioPlaying)
				{
					if (BackfireInternalCounter > CarSetup.BackfireSeconds)
					{
						isBackfireAudioPlaying = CarAudio.PopBackfire(true);
					}
					else
					{
						isBackfireAudioPlaying = CarAudio.PopBackfire(false);
					}
				}
				
				isBackfireBlocked = true;
			}
			else
			{
				isBackfire = false;
				isBackfireAudioPlaying = false;
				resetBackFire();
			}
		}
		else
		{
			resetBackFire();
		}
	}
	
	private void resetBackFire()
	{
		if (CarSetup.ExhaustBackfire.activeSelf) 
		{
			CarSetup.ExhaustBackfire.SetActive(false);
		}
	}
	
	private void FillTurbo(float eRPM)
	{
		
		if (eRPM >= CarSetup.EngineTorqueRPM && eRPM <= CarSetup.EnginePowerRPM)
		{
			TurboFill += (((RPM * TurboDiff) / CarSetup.EnginePowerRPM) / TurboDiff) * Time.deltaTime;
			
			if (TurboFill > 1) 
			{
				TurboFill = 1;
			}
		}
	}
	#endregion
	
	#region Properties
	public float TotalSlip
	{
		get
		{
			totalSlip = 0;
			
			if (CarSetup != null)
			{
				foreach (CarWheel CarWheel in CarSetup.Wheels)
				{
					totalSlip += CarWheel.slipVelo / CarSetup.Wheels.Length;
				}
			}
			return totalSlip;
		}
	}
	
	public bool UseNitro
	{
		get { return m_useNitro; }
		set
		{
			m_useNitro = value;
			
			if (value==false)
			{
				ResetNos(); 
			}
		}
	}
	public void UseNitroInternal()
	{
		if (m_useNitro)
		{
			if (CarSetup.NitroLeft > 0)
			{
				auxRigidbody.AddForce(auxRigidbody.transform.forward * CarSetup.ForceAdd * Mathf.Clamp01((CarSetup.ForceBalance / 10)), ForceMode.Acceleration);
				CarSetup.NitroLeft -= Time.deltaTime * Mathf.Clamp01((CarSetup.ForceBalance / 10)) * 2;
				
				if (CarSetup.NosFireObject != null)
				{
					if (!CarSetup.NosFireObject.activeSelf) CarSetup.NosFireObject.SetActive(true);
				}
				
				if (CarAudio != null)
				{
					CarAudio.ApplyNitro(true);
				}
			}
			else
			{
				ResetNos();
			}
		}
		else
		{
			if(!brakeFill)
			{
				FillNitroInternal();
			}
		}
	}
	
	private void FillNitroInternal()
	{
		ResetNos();
		
		if (CarSetup.AutoFill)
		{
			if (CarSetup.NitroLeft < CarSetup.NitroInitialAmount)
			{
				CarSetup.NitroLeft += Time.deltaTime / 2;
			}
		}
	}
	
	private void ResetNos()
	{
		if(CarSetup == null)
		{
			CarSetup = GetComponent<CarSetup>();
		}
		
		if (CarSetup.NosFireObject != null)
		{
			if (CarSetup.NosFireObject.activeSelf) 
			{
				CarSetup.NosFireObject.SetActive(false);
			}
		}
		
		if (CarAudio != null)
		{
			CarAudio.ApplyNitro(false);
		}
	}
	#endregion
}