using UnityEngine;
using System.Collections;

public class ParticleKiller: MonoBehaviour 
{
	#region Public Attributes
	public float delayTime;
	#endregion
	
	#region Private Attributes
	
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	private void OnEnable () 
	{
		Invoke ("KillParticle", delayTime);
	}
	#endregion
	
	#region Particle Methods
	private void KillParticle()
	{
		gameObject.SetActive (false);
	}
	#endregion
}