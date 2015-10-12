#pragma warning disable 0414
#pragma warning disable 0219
using System;
using UnityEngine;
using UnityEngine.Networking;

public class CarCamera : MonoBehaviour
{	
	#region Public Attributes
	public Transform m_Target;            // The target object to follow
	public bool m_AutoTargetPlayer = true;  // Whether the rig should automatically target the player.
    public float m_MoveSpeed = 3; // How fast the rig will move to keep up with target's position
	public float m_TurnSpeed = 1; // How fast the rig will turn to keep up with target's rotation
	public float m_RollSpeed = 0.2f;// How fast the rig will roll (around Z axis) to match target's roll.
	public bool m_FollowVelocity = false;// Whether the rig will rotate in the direction of the target's velocity.
	public bool m_FollowTilt = true; // Whether the rig will tilt (around X axis) with the target.
	public float m_SpinTurnLimit = 90;// The threshold beyond which the camera stops following the target's rotation. (used in situations where a car spins out, for example)
	public float m_TargetVelocityLowerLimit = 4f;// the minimum velocity above which the camera turns towards the object's velocity. Below this we use the object's forward direction.
	public float m_SmoothTurnTime = 0.2f; // the smoothing for the camera's rotation
	public float shake;
	public float shakeAmount;
	public float decreaseFactor;
	public float shakeScale;
    public float shakeScaleInit;
	#endregion
	
	#region Private Attributes
	private Transform m_Cam; // the transform of the camera
	private Transform m_Pivot; // the point at which the camera pivots around
	private Vector3 m_LastTargetPosition;
    private float m_LastFlatAngle; // The relative angle of the target and the rig from the previous frame.
    private float m_CurrentTurnAmount; // How much to turn the camera
    private float m_TurnSpeedVelocityChange; // The change in the turn speed velocity
    private Vector3 m_RollUp = Vector3.up;// The roll of the camera around the z axis ( generally this will always just be up )
    
	private Vector3 targetForward;
	private Vector3 targetUp;
	private float currentFlatAngle;
	private float targetSpinSpeed;
	private float desiredTurnAmount;
	private float turnReactSpeed;
	private Quaternion rollRotation;
	#endregion
	
	#region References
	private Rigidbody targetRigidbody;
	private CarEngine carEngine;
	private CarDrifting carDrifting;
	private CarSetup carSetup;
	private GameObject[] players;
    private CC_RadialBlur radialBlur;
	#endregion

	#region Main Methods
	private void Start() 
	{
		if(GameObject.FindWithTag ("MultiplayerManager"))
		{
			players = GameObject.FindGameObjectsWithTag ("Player");
			for(int i = 0; i < players.Length; i++)
			{
				if(players[i].GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					m_Target = players[i].transform.FindChild ("CameraTarget");
				}
			}
		}
		else
		{
			m_Target = GameObject.Find ("CameraTarget").transform;
		}
		
		targetRigidbody = m_Target.root.GetComponent<Rigidbody>();
		carEngine = m_Target.root.GetComponent<CarEngine>();
		carSetup = carEngine.GetComponent<CarSetup>();
        radialBlur = GetComponent<CC_RadialBlur>();
		
		if(GameObject.Find ("DriftManager"))
		{
			carDrifting = GameObject.Find ("DriftManager").GetComponent<CarDrifting>();
		}
	}
	
    private void Update()
    {
        // if no target, or no time passed then we quit early, as there is nothing to do
        if (!(Time.deltaTime > 0) || m_Target == null)
        {
            return;
        }

        // initialise some vars, we'll be modifying these in a moment
        targetForward = m_Target.forward;
        targetUp = m_Target.up;

        if (m_FollowVelocity && Application.isPlaying)
        {
            // in follow velocity mode, the camera's rotation is aligned towards the object's velocity direction
            // but only if the object is traveling faster than a given threshold.

            if (targetRigidbody.velocity.magnitude > m_TargetVelocityLowerLimit)
            {
                // velocity is high enough, so we'll use the target's velocty
                targetForward = targetRigidbody.velocity.normalized;
                targetUp = Vector3.up;
            }
            else
            {
                targetUp = Vector3.up;
            }
            m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, 1, ref m_TurnSpeedVelocityChange, m_SmoothTurnTime);
        }
        else
        {
            // we're in 'follow rotation' mode, where the camera rig's rotation follows the object's rotation.

            // This section allows the camera to stop following the target's rotation when the target is spinning too fast.
            // eg when a car has been knocked into a spin. The camera will resume following the rotation
            // of the target when the target's angular velocity slows below the threshold.
            currentFlatAngle = Mathf.Atan2(targetForward.x, targetForward.z)*Mathf.Rad2Deg;
            if (m_SpinTurnLimit > 0)
            {
				targetSpinSpeed = Mathf.Abs(Mathf.DeltaAngle(m_LastFlatAngle, currentFlatAngle))/Time.deltaTime;
                desiredTurnAmount = Mathf.InverseLerp(m_SpinTurnLimit, m_SpinTurnLimit*0.75f, targetSpinSpeed);
                turnReactSpeed = (m_CurrentTurnAmount > desiredTurnAmount ? .1f : 1f);
                
                if (Application.isPlaying)
                {
                    m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, desiredTurnAmount,
                                                         ref m_TurnSpeedVelocityChange, turnReactSpeed);
                }
                else
                {
                    // for editor mode, smoothdamp won't work because it uses deltaTime internally
                    m_CurrentTurnAmount = desiredTurnAmount;
                }
            }
            else
            {
                m_CurrentTurnAmount = 1;
            }
            m_LastFlatAngle = currentFlatAngle;
        }

        // camera position moves towards target position:
		transform.position = Vector3.Lerp(transform.position, m_Target.position, Time.deltaTime*m_MoveSpeed);

        // camera's rotation is split into two parts, which can have independend speed settings:
        // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
        if (!m_FollowTilt)
        {
            targetForward.y = 0;
            if (targetForward.sqrMagnitude < float.Epsilon)
            {
                targetForward = transform.forward;
            }
        }
        
        rollRotation = Quaternion.LookRotation(targetForward, m_RollUp);

        // and aligning with the target object's up direction (i.e. its 'roll')
        m_RollUp = m_RollSpeed > 0 ? Vector3.Slerp(m_RollUp, targetUp, m_RollSpeed*Time.deltaTime) : Vector3.up;
		transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, m_TurnSpeed*m_CurrentTurnAmount*Time.deltaTime);
		transform.LookAt (m_Target.root.transform.position);
		
		// Shake logic
		if(carEngine.SpeedAsKM > 80 || (carEngine.UseNitro && carSetup.NitroLeft > 0.0f))
		{
			if(carDrifting != null)
			{
				if(!carDrifting.ended)
				{
					if(carEngine.UseNitro && carSetup.NitroLeft > 0.0f)
					{
						if(shakeScale != 0.2f)
						{
							shakeScale = 0.2f;
						}
					}
					else
					{
						if(shakeScale != shakeScaleInit)
						{
							shakeScale = shakeScaleInit;
						}	
					}
					
					shake = carEngine.SpeedAsKM * shakeScale;
				}
				else
				{
					if(shake != 0)
					{
						shakeScale = shakeScaleInit;
						shake = 0;
					}
				}
			}
			else
			{
				if(carEngine.UseNitro && carSetup.NitroLeft > 0.0f)
				{
					if(shakeScale != 0.2f)
					{
						shakeScale = 0.2f;
					}
				}
				else
				{
					if(shakeScale != shakeScaleInit)
					{
						shakeScale = shakeScaleInit;
					}	
				}
				
				shake = carEngine.SpeedAsKM * shakeScale;
			}
		}
		else
		{
			shake = 0.0f;
        }
		
		if (shake > 0.0f) 
		{
			transform.localPosition += UnityEngine.Random.insideUnitSphere * shakeAmount; 
			
			shake -= Time.deltaTime * decreaseFactor;
		} 
		else 
		{
			shake = 0.0f;
		}

        if (carEngine.UseNitro && carSetup.NitroLeft > 0.0f)
        {
            if (radialBlur.amount < 0.05f)
            {
                radialBlur.amount += Time.deltaTime * 0.5f;
            }
        }
        else
        {
            if (radialBlur.amount > 0.0f)
            {
                radialBlur.amount -= Time.deltaTime * 0.5f;
            }
            else
            {
                radialBlur.amount = 0.0f;
            }
        }
    }
    #endregion
}