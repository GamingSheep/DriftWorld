#pragma warning disable 0414
#pragma warning disable 0219
using UnityEngine;
using System.Collections;

#region Interfaces
public interface IEarthFX
{
	Transform WheelTransform { get; }
	RaycastHit HitToSurface { get; }
	float SlipRatio { get; }
	float SlipVelo { get; }
	float AngularVelocity { get; }
	float Grip { get; set; }
	float SideGrip { get; set; }
	float DefinedGrip { get; set; }
	float DefinedSideGrip { get; set; }
	bool OnGround { get; }
	bool EarthFXEnabled { set; }
	int LastSkid { get; set; }
	int LastTrail { get; set; }
	string PreviousSurface { get; set; }
	bool SurfaceChanged { get; set; }
	GameObject SkidObject { get; set; }
	GameObject TrailObject { get; set; }
	CarSkidmarks SkidMark { get; set; }
	CarSkidmarks TrailMark { get; set; }
	ParticleEmitter SkidSmoke { get; set; }
	ParticleEmitter TrailSmoke { get; set; }
	ParticleEmitter Splatter { get; set; }
}
#endregion

#region Enums
public enum WheelLocationEnum
{ 
	Front = 0,
	Rear = 1,
	Trailer = 2,
}
#endregion

public class CarWheel : MonoBehaviour, IEarthFX
{
	#region Main Attributes
	public GameObject WheelModel;
	public GameObject WheelBlurModel;
	public GameObject DiskModel;
	public Vector3 DiskModelOffset;
	public GameObject CaliperModel;
	public Vector3 CaliperModelOffset;
	#endregion
	
	#region Perfomance Attributes
	public WheelLocationEnum WheelLocation;
	public bool isPowered = false;
	private bool NoSuspension = false;
	public float suspensionHeight = 0.2f; // Wheel suspension travel in meters
	public float suspensionStiffness = 5000;// Damper strength in kg/s
	public float suspensionRelease = 50f; // damper release time
	public float brakeTorque = 4000; // Maximal braking torque (in Nm)
	public float handbrakeTorque = 0;// Maximal handbrake torque (in Nm)
	public float definedGrip = 1.0f;
	public float wheelGrip = 1.0f;
	public float definedSideGrip = 1.0f;
	public float sideGrip = 1.0f;
	public float maxSteeringAngle = 0f;
	public float slipVeloLimiter = 20f;
	public float radius = 0.34f;
	public float blurSwitchVelocity = 20f;
	public bool BurnoutStart = true;
	public float BurnoutStartDuration = 1f;
	// Wheel angular inertia in kg * m^2
	public float inertia = 2.2f;
	// Base friction torque (in Nm)
	public float frictionTorque = 10;
	// Fraction of the car's mass carried by this wheel
	public float massFraction = 0.25f;
	// Pacejka coefficients
	private float[] a = { 1.5f, -40f, 1600f, 2600f, 8.7f, 0.014f, -0.24f, 1.0f, -0.03f, -0.0013f, -0.06f, -8.5f, -0.29f, 17.8f, -2.4f };
	private float[] b = { 1.5f, -80f, 1950f, 23.3f, 390f, 0.05f, 0f, 0.055f, -0.024f, 0.014f, 0.26f };
	#endregion
	
	#region Wheel Input Attributes
	public float driveTorque = 0;
	public float driveFrictionTorque = 0;
	public float brake = 0;
	public float handbrake = 0;
	public float steering = 0;
	public float drivetrainInertia = 0;
	public float suspensionForceInput = 0;
	#endregion
	
	#region Wheel Output Attributes
	public float angularVelocity;
	public float oAngularVelocity;
	public float slipRatio;
	public float slipVelo;
	public float compression = 0.5f;
	#endregion
	
	#region States Attributes
	private float fullCompressionSpringForce;
	public Vector3 wheelVelo;
	public Vector3 localVelo;
	private Vector3 groundNormal;
	private float rotation;
	private float normalForce;
	private Vector3 suspensionForce;
	public Vector3 roadForce;
	private Vector3 up, right, forwardNormal, rightNormal;//,forward;
	private Quaternion localRotation = Quaternion.identity;
	private Quaternion inverseLocalRotation = Quaternion.identity;
	public float slipAngle;
	#endregion
	
	#region Cached Attributes
	Rigidbody body;
	private float maxSlip;
	private float maxAngle;
	private float oldAngle;
	public float totalFrictionTorque;
	public int slipRes;
	private float longitunalSlipVelo;
	private float lateralSlipVelo;
	private float groundCalc;
	private Vector3 force;
	public bool onGround;
	private RaycastHit hit;
	public bool UseEarthFX;
	public int m_LastSkid = -1;
	public int m_lastTrail = -1;
	public GameObject SkidmarkObject;
	public CarSkidmarks m_skidNotEarthFX;
	public GameObject m_SkidObject;
	public GameObject m_TrailObject;
	public CarSkidmarks m_SkidMarks;
	public CarSkidmarks m_TrailMarks;
	private ParticleEmitter m_SkidSmoke;
	private ParticleEmitter m_TrailSmoke;
	private ParticleEmitter m_Splatter;
	public string m_previousSurface = "";
	public bool m_surfaceChanged = false;
	private CarSetup carSetup;
	#endregion
	
	#region Other Attributes
	public float camberAngle;
	public float invSlipRes;
	private float slipFactor = 10f;
	private Vector3 wheelRotation;
	#endregion
	
	#region Auxiliar Attributes	
	// Awake
	private Transform rigidbodyTransform;
	
	// Fixed Update
	private Vector3 pos;
	private RaycastHit[] hits;
	private float staticFrictionForce;
	private float latGravityForce;
	private float totalInertia;
	private float angularDelta;
	private float driveAngularDelta;
	private float auxTotalFrictionTorque;
	private float frictionAngularDelta;
	private Vector3 auxSkidmarkA;
	private Vector3 auxSkidmarkB;
	private float dist;
	
	// CalcLongitudinalForce
	private float FzSquared;
	private float FzFz;
	private float uP;
	private float D;
	private float B;
	private float E;
	private float Sh;
	private float Sv;
	private float Fx;
	
	// CalcLateralForce
	private float FzSquare;
	private Vector3 wheelUp;
	private Vector3 carUp;
	private float uP1;
	private float D1;
	private float B1;
	private float S;
	private float E1;
	private float Sv1;
	private float Fy;
	
	// CombinedForce
	private float unitSlip;
	private float unitAngle;
	private float p;
	private Vector3 forward;
	
	// InitSlipMaxima
	private float auxForce;
	private const float stepSize = 0.001f;
	private const float testNormalForce = 4000f;
	private float newForce;
	
	// SuspensionForce
	private float springForce;
	private float damperForce;
	private const float fullSlipVelo = 4f;
	
	// SlipRatio
	private float wheelRoadVelo;
	private float absRoadVelo;
	private float damping;
	private float wheelTireVelo;
	
	// SlipAngle
	private Vector3 auxLocalVelo;
	private float auxT;
	
	// RoadForce
	private float auxTotalInertia;
	private float auxDriveAngularDelta;
	private float auxFrictionAngularDelta;
	private Vector3 totalForce;
	private float newAngle;
	private float auxF;
	private Vector3 worldForce;
	#endregion
	
	#region Main Methods	
	private void Start()
	{
		rigidbodyTransform = transform;
		
		while (rigidbodyTransform != null && rigidbodyTransform.GetComponent<Rigidbody>() == null)
		{
			rigidbodyTransform = rigidbodyTransform.parent;
		}
		
		if (rigidbodyTransform != null)
		{
			body = rigidbodyTransform.GetComponent<Rigidbody>();
		}
		
		InitSlipMaxima();

        carSetup = transform.root.GetComponent<CarSetup>();

        SkidmarkObject = GameObject.Find(carSetup.SkidmarkObjectName);
		
		if (SkidmarkObject != null)
		{
			m_skidNotEarthFX = SkidmarkObject.GetComponentInChildren(typeof(CarSkidmarks)) as CarSkidmarks;
			m_SkidSmoke = SkidmarkObject.GetComponentInChildren(typeof(ParticleEmitter)) as ParticleEmitter;
		}
		
	}
	
	private void FixedUpdate()
	{
		if (!UseEarthFX)
		{
			wheelGrip = definedGrip;
			sideGrip = definedSideGrip;
		}
		
		if (suspensionHeight < 0.05f) suspensionHeight = 0.05f;
		
		pos = transform.position;
		up = transform.up;
		
		onGround = Physics.Raycast(pos, -up, out hit, suspensionHeight + radius);
		
		if (onGround && hit.collider.isTrigger)
		{
			onGround = false; 
			dist = suspensionHeight + radius;
			hits = Physics.RaycastAll(pos, -up, suspensionHeight + radius);
			foreach (RaycastHit test in hits)
			{
				if (!test.collider.isTrigger && test.distance <= dist)
				{
					hit = test;
					onGround = true;
					dist = test.distance;
				}
			}
		}
		
		fullCompressionSpringForce = body.mass * massFraction * 2.0f * -Physics.gravity.y;
		staticFrictionForce = wheelGrip * fullCompressionSpringForce;
		latGravityForce = fullCompressionSpringForce * Mathf.Cos(Vector3.Angle(right, Vector3.up) * Mathf.Deg2Rad);
		
		if (latGravityForce < staticFrictionForce)
		{
			body.AddForceAtPosition(latGravityForce * -right, WheelModel.transform.position);
			
		}
		
		if (onGround)
		{
			groundNormal = transform.InverseTransformDirection(inverseLocalRotation * hit.normal);
			
			if (!NoSuspension)
			{
				compression = 1.0f - ((hit.distance - radius) / suspensionHeight);
			}
			
			suspensionForce = SuspensionForce();
			
			if (slipVelo < 30)
			{
				wheelVelo = body.GetPointVelocity(pos);
				localVelo = transform.InverseTransformDirection(inverseLocalRotation * wheelVelo);
				roadForce = RoadForce();
				
				body.AddForceAtPosition(suspensionForce + roadForce, pos);
			}
			else
			{
				angularVelocity = 0;
				slipVelo = 0;
			}
		}
		else
		{
			if (slipVelo < 10)
			{
				totalInertia = inertia + drivetrainInertia;
				angularDelta = Time.deltaTime * invSlipRes / totalInertia;
				driveAngularDelta = driveTorque * angularDelta;
				auxTotalFrictionTorque = brakeTorque * brake + handbrakeTorque * handbrake + frictionTorque + driveFrictionTorque;
				frictionAngularDelta = auxTotalFrictionTorque * angularDelta;
				angularVelocity += driveAngularDelta;
				
				if (Mathf.Abs(angularVelocity) > frictionAngularDelta)
				{
					angularVelocity -= frictionAngularDelta * Mathf.Sign(angularVelocity);
				}
				else
				{ angularVelocity = 0; }
			}
			else
			{
				//compression = 0.0f;
				//suspensionForce = Vector3.zero;
				//roadForce = Vector3.zero;
				//angularVelocity = 0;
				//slipRatio = 0;
				//slipVelo = 0;
			}
		}
		
		if (onGround && UseEarthFX)
		{
			//This functions moved to eartfx processor...
		}
		else if (onGround && this.m_skidNotEarthFX != null && Mathf.Abs(this.slipRatio) > 0.15)
		{
			m_LastSkid = m_skidNotEarthFX.AddSkidMark(hit.point, hit.normal, Mathf.Abs(slipRatio) - 0f, m_LastSkid);
			
			if (this.m_SkidSmoke != null)
			{					
				auxSkidmarkA.x =  UnityEngine.Random.Range(-0.1f, 0.1f);
				auxSkidmarkA.y =  UnityEngine.Random.Range(-0.1f, 0.1f);
				auxSkidmarkA.z =  UnityEngine.Random.Range(-0.1f, 0.1f);
				
				auxSkidmarkB.x = this.slipVelo * 0.05f;
				auxSkidmarkB.y = 0.0f;
				auxSkidmarkB.z = 0.0f;
				
				this.m_SkidSmoke.Emit( hit.point + auxSkidmarkA, auxSkidmarkB, UnityEngine.Random.Range(m_SkidSmoke.minSize, m_SkidSmoke.maxSize) * Mathf.Clamp(0.5f, 1f, 1.5f), UnityEngine.Random.Range(m_SkidSmoke.minEnergy, m_SkidSmoke.maxEnergy), Color.white);
			}
		}
		else
		{
			m_LastSkid = -1;
		}
		
		if (!NoSuspension)
		{
			compression = Mathf.Clamp01(compression);
		}
		else
		{
			compression = 0.5f;
		}
		rotation += angularVelocity * Time.deltaTime;
		
		UpdateModels();
		
	}
	
	private float CalcLongitudinalForce(float Fz, float slip)
	{
		Fz *= 0.001f;//convert to kN
		FzSquared = Fz * Fz;
		slip *= 100f; //covert to %
		FzFz = Fz * Fz;
		uP = b[1] * Fz + b[2];
		//float D = uP * Fz;
		D = (b[1] * FzFz + b[2] * Fz);
		//float B = ((b[3] * Fz + b[4]) * Mathf.Exp(-b[5] * Fz)) / (b[0] * uP);
		B = ((b[3] * FzSquared + b[4] * Fz) * Mathf.Exp(-b[5] * Fz)) / (b[0] * D);
		
		E = b[6] * FzSquared + b[7] * Fz + b[8];
		Sh = b[9] * Fz + b[10];
		Sv = 0;
		//float S = slip + b[9] * Fz + b[10];
		
		//float Fx = D * Mathf.Sin(b[0] * Mathf.Atan(S * B + E * (Mathf.Atan(S * B) - S * B)));
		Fx = D * Mathf.Sin(b[0] * Mathf.Atan(B * (1.0f - E) * (slip + Sh) + E * Mathf.Atan(B * (slip + Sh)))) + Sv;
		return Fx;
	}
		
	private float CalcLateralForce(float Fz, float slipAngle)
	{
		Fz *= 0.001f;//convert to kN
		FzSquare = Fz * Fz;

		wheelUp = transform.TransformDirection(Vector3.up);
		carUp = transform.root.transform.TransformDirection(Vector3.up);
		camberAngle = Vector3.Angle(wheelUp, carUp);
		
		slipAngle *= (360f / (2 * Mathf.PI)); //convert angle to deg
		uP1 = a[1] * Fz + a[2];
		//float D = uP * Fz;
		D1 = (a[1] * FzSquare + a[2] * Fz) * sideGrip; /// SIDEWAYSGRIP
		
		//float B = (a[3] * Mathf.Sin(2 * Mathf.Atan(Fz / a[4]))) / (a[0] * peakFrictionCoeff * Fz);
		//      B=(a3*sinf(2*atanf(Fz/a4))*(1-a5*fabs(camber)))/(C*D);                                                        
		
		B1 = (a[3] * Mathf.Sin(2 * Mathf.Atan(Fz / a[4])) * (1 - a[5] * Mathf.Abs(camberAngle))) / (a[0] * D1);
		S = slipAngle + a[9] * Fz + a[10];
		
		E1 = a[6] * Fz + a[7];
		//float Sv = a[12] * Fz + a[13];
		//float Sv = a[12] * Fz + a[13] * -a[11] * Fz;
		Sv1 = (a[11] * Fz + a[12]) * camberAngle * Fz + a[12] * Fz + a[13];
		//Sv = (a111 * Fz + a112) * camber * Fz + a12 * Fz + a13;
		Fy = D1 * Mathf.Sin(a[0] * Mathf.Atan(S * B1 + E1 * (Mathf.Atan(S * B1) - S * B1))) + Sv1;
		return Fy;		
	}
	
	private float CalcLongitudinalForceUnit(float Fz, float slip)
	{
		return CalcLongitudinalForce(Fz, slip * maxSlip);
	}
	
	private float CalcLateralForceUnit(float Fz, float slipAngle)
	{
		return CalcLateralForce(Fz, slipAngle * maxAngle);
	}
	
	private Vector3 CombinedForce(float Fz, float slip, float slipAngle)
	{
		unitSlip = slip / maxSlip;
		unitAngle = slipAngle / maxAngle;
		p = Mathf.Sqrt(unitSlip * unitSlip + unitAngle * unitAngle);
		if (p <= Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		else
		{
			if (slip < -0.8f)
				return -localVelo.normalized * (Mathf.Abs(unitAngle / p * CalcLateralForceUnit(Fz, p)) + Mathf.Abs(unitSlip / p * CalcLongitudinalForceUnit(Fz, p)));
		}
		
		forward = new Vector3(0, -groundNormal.z, groundNormal.y);
		return Vector3.right * unitAngle / p * CalcLateralForceUnit(Fz, p) + forward * unitSlip / p * CalcLongitudinalForceUnit(Fz, p);
	}
	
	private void InitSlipMaxima()
	{
		auxForce = 0;
		
		for (float slip = stepSize; ; slip += stepSize)
		{
			newForce = CalcLongitudinalForce(testNormalForce, slip);
			if (auxForce < newForce)
				auxForce = newForce;
			else
			{
				maxSlip = slip - stepSize;
				break;
			}
		}
		
		auxForce = 0;
		
		for (float slipAngle = stepSize; ; slipAngle += stepSize)
		{
			newForce = CalcLateralForce(testNormalForce, slipAngle);
			if (auxForce < newForce)
				auxForce = newForce;
			else
			{
				maxAngle = slipAngle - stepSize;
				break;
			}
		}
	}
	
	private Vector3 SuspensionForce()
	{
		springForce = 0.0f;
		
		if (suspensionRelease > 0)
		{
			springForce = compression * fullCompressionSpringForce * Time.deltaTime * suspensionRelease;
		}
		else
		{
			springForce = compression * fullCompressionSpringForce;
		}
		
		normalForce = springForce;
		damperForce = Vector3.Dot(localVelo, groundNormal) * suspensionStiffness;
		return (springForce - damperForce + suspensionForceInput) * up;
	}
	
	private float SlipRatio()
	{			
		wheelRoadVelo = Vector3.Dot(wheelVelo, forwardNormal);
		
		if (wheelRoadVelo == 0)
		{
			return 0;
		}
		
		absRoadVelo = Mathf.Abs(wheelRoadVelo);
		damping = Mathf.Clamp01(absRoadVelo / fullSlipVelo);
		
		wheelTireVelo = angularVelocity * radius;
		slipRatio = ((wheelTireVelo - wheelRoadVelo) / absRoadVelo) * damping;
		return slipRatio;
	}
	
	private float SlipAngle()
	{
		auxLocalVelo = localVelo;
		auxLocalVelo.y = 0f;
		
		if (auxLocalVelo.sqrMagnitude < float.Epsilon)
		{
			return 0f;
		}

		Mathf.Clamp(auxLocalVelo.normalized.x, -1f, 1f);
		auxT = Mathf.Clamp01(localVelo.magnitude / 2f);
		return ((-Mathf.Asin(auxLocalVelo.normalized.x) * auxT) * auxT);
	}
	
	private Vector3 RoadForce()
	{
		slipRes = (int)((100.0f - Mathf.Abs(angularVelocity)) / (slipFactor));
		
		if (slipRes < 1)
		{
			slipRes = 1;
		}
		
		invSlipRes = (1f / (float)slipRes);
		
		auxTotalInertia = inertia + drivetrainInertia;
		auxDriveAngularDelta = driveTorque * Time.deltaTime * invSlipRes / auxTotalInertia;
		totalFrictionTorque = brakeTorque * brake + handbrakeTorque * handbrake + frictionTorque + auxDriveAngularDelta / 2;
		auxFrictionAngularDelta = totalFrictionTorque * Time.deltaTime * invSlipRes / auxTotalInertia;
		
		totalForce = Vector3.zero;
		newAngle = maxSteeringAngle * steering;
		
		for (int i = 0; i < slipRes; i++)
		{
			auxF = i * 1.0f / (float)slipRes;
			localRotation = Quaternion.Euler(0, oldAngle + (newAngle - oldAngle) * auxF, 0);
			inverseLocalRotation = Quaternion.Inverse(localRotation);
			forwardNormal = transform.TransformDirection(localRotation * Vector3.forward);
			right = transform.TransformDirection(localRotation * Vector3.right);
			
			groundCalc = Vector3.Dot(right, groundNormal);
			rightNormal = right - groundNormal * groundCalc;
			forwardNormal = Vector3.Cross(rightNormal, groundNormal);
			
			slipRatio = SlipRatio();
			slipAngle = SlipAngle();
			
			if (brake > 0)
			{
				force = invSlipRes * wheelGrip * 1.5f * CombinedForce(normalForce, slipRatio / 2, slipAngle / 2);
			}
			else
			{
				force = invSlipRes * wheelGrip * CombinedForce(normalForce, slipRatio, slipAngle);
			}
			
			worldForce = transform.TransformDirection(localRotation * force);
			
			angularVelocity -= (force.z * radius * Time.deltaTime) / auxTotalInertia;
			angularVelocity += auxDriveAngularDelta;
			
			if (Mathf.Abs(angularVelocity) > auxFrictionAngularDelta)
			{
				angularVelocity -= auxFrictionAngularDelta * Mathf.Sign(angularVelocity);
			}
			else
			{
				angularVelocity = 0;
			}
			
			//wheelVelo += worldForce * (1 / body.mass) * Time.deltaTime * invSlipRes; // 1.2 Implementation
			totalForce += worldForce;
		}
		
		longitunalSlipVelo = Mathf.Abs(angularVelocity * radius - Vector3.Dot(wheelVelo, forwardNormal));
		
		lateralSlipVelo = Vector3.Dot(wheelVelo, right);
		
		slipVelo = Mathf.Sqrt(longitunalSlipVelo * longitunalSlipVelo + lateralSlipVelo * lateralSlipVelo);
		
		oldAngle = newAngle;
		
		return totalForce;
	}
	
	private void UpdateModels()
	{
		///WHEEL MODEL
		if (WheelModel != null)
		{
			WheelModel.transform.localPosition = Vector3.up * (compression - 1.0f) * suspensionHeight;
			WheelModel.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * rotation, maxSteeringAngle * steering, 0);
			
			if (WheelBlurModel != null)
			{
				WheelBlurModel.transform.localPosition = Vector3.up * (compression - 1.0f) * suspensionHeight;
				WheelBlurModel.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * rotation, maxSteeringAngle * steering, 0);
			}
		}
		
		if (WheelBlurModel != null)
		{
			if (angularVelocity > blurSwitchVelocity)
			{
				WheelBlurModel.SetActive(true);
				WheelModel.SetActive(false);
				
			}
			else
			{
				WheelModel.SetActive(true);
				WheelBlurModel.SetActive(false);
			}
		}
		
		///DISK MODEL
		if (DiskModel != null)
		{
			DiskModel.transform.localPosition = WheelModel.transform.localPosition + DiskModelOffset;
			DiskModel.transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * rotation, maxSteeringAngle * steering, 0);
		}
		
		///CALIPER MODEL
		if (CaliperModel != null)
		{
			CaliperModel.transform.localPosition = WheelModel.transform.localPosition + CaliperModelOffset;
			CaliperModel.transform.localRotation = Quaternion.Euler(0, maxSteeringAngle * steering, 0);
		}
	}
	#endregion
	
	#region Properties
	public RaycastHit HitToSurface
	{
		get { return hit; }
	}
	
	float IEarthFX.SlipRatio
	{
		get { return slipRatio; }
	}
	
	float IEarthFX.SlipVelo
	{
		get { return slipVelo; }
	}
	
	public float AngularVelocity
	{
		get { return angularVelocity; }
	}
	
	public float Grip
	{
		get { return wheelGrip; }
		set { wheelGrip = value; }
	}
	
	public float SideGrip
	{
		get	{ return sideGrip; }
		set	{ sideGrip = value; }
	}
	
	public float DefinedGrip
	{
		get	{ return definedGrip; }
		set	{ definedGrip = value; }
	}
	
	public float DefinedSideGrip
	{
		get { return definedSideGrip; }
		set	{ definedSideGrip = value; }
	}
	
	public Transform WheelTransform
	{
		get { return WheelModel.transform; }
	}
	
	bool IEarthFX.OnGround
	{
		get { return this.onGround; }
	}
	
	public int LastSkid
	{
		get { return m_LastSkid; }
		set { m_LastSkid = value; }
	}
	
	public int LastTrail
	{
		get { return m_lastTrail; }
		set { m_lastTrail = value; }
	}
	
	public bool EarthFXEnabled
	{
		set { UseEarthFX = value; }
	}
	
	public string PreviousSurface
	{
		get	{ return m_previousSurface; }
		set { m_previousSurface = value; }
	}
	
	public bool SurfaceChanged
	{
		get { return m_surfaceChanged; }
		set { m_surfaceChanged = value; }
	}
		
	public GameObject SkidObject
	{
		get { return m_SkidObject; }
		set { m_SkidObject = value; }
	}
	
	public GameObject TrailObject
	{
		get { return m_TrailObject; }
		set { m_TrailObject = value; }
	}
	
	public ParticleEmitter SkidSmoke
	{
		get { return m_SkidSmoke; }
		set { m_SkidSmoke = value; }
	}
	
	public ParticleEmitter TrailSmoke
	{
		get { return m_TrailSmoke; }
		set { m_TrailSmoke = value;	}
	}
	
	public ParticleEmitter Splatter
	{
		get { return m_Splatter; }
		set	{ m_Splatter = value; }
	}
	
	public CarSkidmarks SkidMark
	{
		get	{ return m_SkidMarks; }
		set	{ m_SkidMarks = value; }
	}
	
	public CarSkidmarks TrailMark
	{
		get { return m_TrailMarks; }
		set	{ m_TrailMarks = value;	}
	}
	#endregion
}