using UnityEngine;
using System.Collections;
using System;

public class CarPhysics : MonoBehaviour 
{
    #region Public Attributes
    [Header("Physics Attributes")]
    public float changeSpeed;

    [Header("Wheels Attributes")]
    public PhysicsAttributes frontWheelGrip;
    public PhysicsAttributes frontWheelSideGrip;
    public PhysicsAttributes rearWheelGrip;
    public PhysicsAttributes rearWheelSideGrip;
    public PhysicsAttributes maxSteeringAngle;
    #endregion

    #region Private Attributes

    #endregion

    #region References
    private CarSetup carSetup;
    private CarEngine carEngine;
    #endregion

    #region Main Methods
    private void Start () 
    {
        carSetup = GetComponent<CarSetup>();
        carEngine = GetComponent<CarEngine>();
	}
	
	private void Update () 
    {
        if(carEngine.SpeedAsKM > changeSpeed)
        {
            carSetup.FrontWheelGrip = Mathf.Lerp(carSetup.FrontWheelGrip, frontWheelGrip.maxValue, Time.deltaTime * frontWheelGrip.lerpSpeed);
            //carSetup.FrontWheelSideGrip = Mathf.Lerp(carSetup.FrontWheelSideGrip, frontWheelSideGrip.maxValue, Time.deltaTime * frontWheelSideGrip.lerpSpeed);
            carSetup.RearWheelGrip = Mathf.Lerp(carSetup.RearWheelGrip, rearWheelGrip.maxValue, Time.deltaTime * rearWheelGrip.lerpSpeed);
            //carSetup.RearWheelSideGrip = Mathf.Lerp(carSetup.RearWheelSideGrip, rearWheelSideGrip.maxValue, Time.deltaTime * rearWheelSideGrip.lerpSpeed);
            carSetup.MaxSteeringAngle = Mathf.Lerp(carSetup.MaxSteeringAngle, maxSteeringAngle.maxValue, Time.deltaTime * maxSteeringAngle.lerpSpeed);
        }
        else
        {
            carSetup.FrontWheelGrip = Mathf.Lerp(carSetup.FrontWheelGrip, frontWheelGrip.minValue, Time.deltaTime * frontWheelGrip.lerpSpeed);
            //carSetup.FrontWheelSideGrip = Mathf.Lerp(carSetup.FrontWheelSideGrip, frontWheelSideGrip.minValue, Time.deltaTime * frontWheelSideGrip.lerpSpeed);
            carSetup.RearWheelGrip = Mathf.Lerp(carSetup.RearWheelGrip, rearWheelGrip.minValue, Time.deltaTime * rearWheelGrip.lerpSpeed);
            //carSetup.RearWheelSideGrip = Mathf.Lerp(carSetup.RearWheelSideGrip, rearWheelSideGrip.minValue, Time.deltaTime * rearWheelSideGrip.lerpSpeed);
            carSetup.MaxSteeringAngle = Mathf.Lerp(carSetup.MaxSteeringAngle, maxSteeringAngle.minValue, Time.deltaTime * maxSteeringAngle.lerpSpeed);
        }
	}
    #endregion

    #region Serializable
    [Serializable]
    public class PhysicsAttributes
    {
        #region Private Attributes
        [SerializeField]
        private float _minValue;
        [SerializeField]
        private float _maxValue;
        [SerializeField]
        private float _lerpSpeed;
        #endregion

        #region Properties
        public float minValue
        {
            get { return _minValue; }
        }

        public float maxValue
        {
            get { return _maxValue; }
        }

        public float lerpSpeed
        {
            get { return _lerpSpeed; }
        }
        #endregion
    }
    #endregion
}