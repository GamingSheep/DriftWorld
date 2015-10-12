using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Speedometer: MonoBehaviour 
{
	#region Public Attributes
	public float moveScale;
	#endregion
	
	#region Private Attributes
	private Vector3 speedRPM;
	private float initZ;
	#endregion
	
	#region References
	public RectTransform rectTransform;
	public Text speedLabel;
	public Slider nitroSlider;
	private CarEngine carEngine;
	private CarSetup carSetup;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		carEngine = GameObject.FindWithTag ("Player").GetComponent<CarEngine>();
		carSetup = carEngine.GetComponent<CarSetup>();
		speedRPM = Vector3.zero;
		initZ = rectTransform.localRotation.eulerAngles.z;
	}
	
	private void Update () 
	{
		if(carEngine && carSetup)
		{
			// Update speedometer graphic
			speedRPM.z = initZ + (-carEngine.SpeedAsKM * moveScale);
			rectTransform.localRotation = Quaternion.Euler (speedRPM);
			
			// Update speed label
			speedLabel.text = carEngine.SpeedAsKM.ToString ("F0") + " Km/h";
			
			// Update Nitro slider
			nitroSlider.value = carSetup.NitroLeft;
		}
	}
	#endregion
}