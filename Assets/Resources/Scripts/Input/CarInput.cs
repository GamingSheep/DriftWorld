#pragma warning disable 0414
#pragma warning disable 0219
using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class CarInput : ScriptableObject 
{
	#region Static Attributes
	private static CarInput instance;
	#endregion
	
	#region Private Attributes
	private float axisSensitivity = 0.1f;
	private bool playerIndexSet = false;
	PlayerIndex playerIndex;
	GamePadState state;
	GamePadState prevState;
	#endregion
	
	#region References

	#endregion
	
	#region Auxiliar Attributes
	private bool result;
	private float floatResult;
	#endregion
	
	#region Main Methods
	public CarInput () 
	{
		playerIndexSet = false;
	}
	
	static CarInput()
	{
		instance = ScriptableObject.CreateInstance<CarInput>();
	}
	
	public bool CheckForGamepad()
	{
		result = false;
		
		for (int i = 0; i < 4; ++i)
		{
			PlayerIndex testPlayerIndex = (PlayerIndex)i;
			GamePadState testState = GamePad.GetState(testPlayerIndex);
			if (testState.IsConnected)
			{
				Debug.Log("CarInput: found gamepad id: " + testPlayerIndex);
				playerIndex = testPlayerIndex;
				playerIndexSet = true;
				result = true;
			}
		}
		
		return result;
	}
	#endregion
	
	#region Input Methods
	public bool Up()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetAxis("Vertical360") > axisSensitivity * 5)
			{
				result = true;
			}
		}
		else
		{
			if(Input.GetAxis("Vertical") > axisSensitivity * 5)
			{
				result = true;
			}
		}
		
		return result;
	}
	
	public bool Down()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetAxis("Vertical360") < -axisSensitivity * 5)
			{
				result = true;
			}
		}
		else
		{
			if(Input.GetAxis("Vertical") < -axisSensitivity * 5)
			{
				result = true;
			}
		}
		
		return result;
	}
	
	public bool Left()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetAxis("Horizontal360") < -axisSensitivity * 5)
			{
				result = true;
			}
		}
		else
		{
			if(Input.GetAxis("Horizontal") < -axisSensitivity * 5)
			{
				result = true;
			}
		}
		
		return result;
	}
	
	public bool Right()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetAxis("Horizontal360") > axisSensitivity * 5)
			{
				result = true;
			}
		}
		else
		{
			if(Input.GetAxis("Horizontal") > axisSensitivity * 5)
			{
				result = true;
			}
		}
		
		return result;
	}
	
	public float HorizontalIntensity()
	{
		floatResult = 0.0f;
		
		if(DataManager.Instance.isGamepad)
		{
			floatResult = Input.GetAxis("Horizontal360");
		}
		else
		{
			floatResult = Input.GetAxis("Horizontal");
		}
		
		return floatResult;
	}
	
	public bool ThrottleForward()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButton ("Forward360");
			
			if(!result)
			{
				if(Input.GetAxis ("Forward360") > axisSensitivity)
				{
					result = true;
				}
			}
		}
		else
		{
			result = Input.GetButton ("Forward");
			
			if(!result)
			{
				if(Input.GetAxis ("Forward") > axisSensitivity)
				{
					result = true;
				}
			}
		}		
		
		return result;
	}
	
	public float ForwardIntensity()
	{
		floatResult = 0.0f;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetButton("Forward360"))
			{
				floatResult = 1.0f;
			}
			
			if(floatResult == 0.0f)
			{
				floatResult = Input.GetAxis ("Forward360");
			}
		}
		else
		{
			if(Input.GetButton("Forward"))
			{
				floatResult = 1.0f;
			}
			
			if(floatResult == 0.0f)
			{
				floatResult = Input.GetAxis ("Forward");
			}
		}
		
		return floatResult;
	}
	
	public bool ThrottleBackward()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButton ("Backward360");
			
			if(!result)
			{
				if(Input.GetAxis ("Backward360") > axisSensitivity)
				{
					result = true;
				}
			}
		}
		else
		{
			result = Input.GetButton ("Backward");
			
			if(!result)
			{
				if(Input.GetAxis ("Backward") > axisSensitivity)
				{
					result = true;
				}
			}
		}
		
		return result;
	}
	
	public float BackwardIntensity()
	{
		floatResult = 0.0f;
		
		if(DataManager.Instance.isGamepad)
		{
			if(Input.GetButton("Backward360"))
			{
				floatResult = 1.0f;
			}
			
			if(floatResult == 0.0f)
			{
				floatResult = Input.GetAxis ("Backward360");
			}
		}
		else
		{
			if(Input.GetButton("Backward"))
			{
				floatResult = 1.0f;
			}
			
			if(floatResult == 0.0f)
			{
				floatResult = Input.GetAxis ("Backward");
			}
		}
		
		return floatResult;
	}
	
	public bool HandBrake()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButton ("HandBrake360");
		}
		else
		{
			result = Input.GetButton ("HandBrake");
		}
		
		return result;
	}
	
	public bool Nitrous()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButton ("Nitrous360");
		}
		else
		{
			result = Input.GetButton ("Nitrous");
		}
		
		return result;
	}
	
	public bool GearUp()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("GearUp360");
		}
		else
		{
			result = Input.GetButtonDown ("GearUp");
		}
		
		return result;
	}
	
	public bool GearDown()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("GearDown360");
		}
		else
		{
			result = Input.GetButtonDown ("GearDown");
		}
		
		return result;
	}
	
	public bool Camera()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Camera360");
		}
		else
		{
			result = Input.GetButtonDown ("Camera");
		}
		
		return result;
	}
	
	public float Look()
	{
		floatResult = 0.0f;
		
		if(DataManager.Instance.isGamepad)
		{
			floatResult = Input.GetAxis ("Look360");
		}
		else
		{
			floatResult = Input.GetAxis ("Look");
		}
		
		return floatResult;
	}
	
	public bool Respawn()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Respawn360");
		}
		else
		{
			result = Input.GetButtonDown ("Respawn");
		}
		
		return result;
	}
	
	public bool Pause()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Pause360");
		}
		else
		{
			result = Input.GetButtonDown ("Pause");
		}
		
		return result;
	}
	
	public bool Cancel()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Cancel360");
		}
		else
		{
			result = Input.GetButtonDown ("Cancel");
		}
		
		return result;
	}
	
	public bool Submit()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Submit360");
		}
		else
		{
			result = Input.GetButtonDown ("Submit");
		}
		
		return result;
	}
	
	public bool Music()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButtonDown ("Music360");
		}
		else
		{
			result = Input.GetButtonDown ("Music");
		}
		
		return result;
	}
	
	public bool LookBack()
	{
		result = false;
		
		if(DataManager.Instance.isGamepad)
		{
			result = Input.GetButton ("LookBack360");
		}
		else
		{
			result = Input.GetButton ("LookBack");
		}
		
		return result;
	}
	
	public void Rumble(float value)
	{
		if(DataManager.Instance.isGamepad)
		{
			if (!prevState.IsConnected)
			{
				for (int i = 0; i < 4; ++i)
				{
					PlayerIndex testPlayerIndex = (PlayerIndex)i;
					GamePadState testState = GamePad.GetState(testPlayerIndex);
					if (testState.IsConnected)
					{
						playerIndex = testPlayerIndex;
					}
				}
			}
			
			prevState = state;
			state = GamePad.GetState(playerIndex);
			
			// Set vibration according to triggers
			GamePad.SetVibration(playerIndex, value, value);
		}
	}
	#endregion
	
	#region Properties
	public static CarInput Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new CarInput();
			}
			
			return instance; 
		}
	}
	
	public bool PlayerIndexSet
	{
		get { return playerIndexSet; }
	}
	#endregion
}