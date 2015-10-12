using UnityEngine;
using System.Collections;

public class LightsSetup : MonoBehaviour 
{
	
	#region Public Attributes
	[Header("Lights Configuration")]
	public bool useBreakLights;
	public bool useReverseLights;
	public bool useFrontLights;
	
	[Header("Lights References")]
	public Light[] breakLights;
	public Light[] frontLights;
	public Light[] reverseLights;
	
	#endregion
	
	#region Private Attributes

	#endregion
	
	#region References
	private CarSetup setup;
	private GameObject nosFire;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Get object references
		setup = GetComponent<CarSetup>();
		
		if(transform.FindChild("NosFire"))
		{
			nosFire = transform.FindChild ("NosFire").gameObject;
		}
		
		if(setup.NitroEnable)
		{
			if(nosFire != null) nosFire.SetActive (true);
			
		}
		else
		{
			if(nosFire != null) nosFire.SetActive (false);
		}
		
		// Set up lights configuration
		SetupFrontLights();
			
		SetupBreakLights();
			
		SetupReverseLights();
	}
	#endregion
	
	#region Lights Configuration Methods
	private void SetupFrontLights()
	{
		if(useFrontLights)
		{
			if(GameObject.Find ("Environment").GetComponent<EnvironmentManager>().LightsOn)
			{
				for(int i = 0; i < frontLights.Length; i++)
				{
					frontLights[i].enabled = true;
				}
			}
			else
			{
				for(int i = 0; i < frontLights.Length; i++)
				{
					frontLights[i].enabled = false;
				}
			}
		}
	}
	
	private void SetupBreakLights()
	{
		
		for(int i = 0; i < breakLights.Length; i++)
		{
			breakLights[i].enabled = false;
		}
	}
	
	private void SetupReverseLights()
	{
		
		for(int i = 0; i < reverseLights.Length; i++)
		{
			reverseLights[i].enabled = false;
		}
	}
	
	private void DisableAllLights()
	{
		
		for(int i = 0; i < frontLights.Length; i++)
		{
			frontLights[i].enabled = false;
		}
		
		for(int i = 0; i < breakLights.Length; i++)
		{
			breakLights[i].enabled = false;
		}
		
		for(int i = 0; i < reverseLights.Length; i++)
		{
			reverseLights[i].enabled = false;
		}
	}
	#endregion
	
	#region Lights Methods
	public void SetFrontLights(bool value)
	{
		if(useFrontLights)
		{
			for(int i = 0; i < frontLights.Length; i++)
			{
				frontLights[i].enabled = value;
			}
		}
	}
	
	public void SetBreakLights(bool value)
	{		
		if(useBreakLights)
		{
			for(int i = 0; i < breakLights.Length; i++)
			{
				breakLights[i].enabled = value;
			}
		}
	}
	
	public void SetReverseLights(bool value)
	{		
		if(useReverseLights)
		{
			for(int i = 0; i < reverseLights.Length; i++)
			{
				reverseLights[i].enabled = value;
			}
		}
	}
	
	public void ResetEmission()
	{

	}
	#endregion
}