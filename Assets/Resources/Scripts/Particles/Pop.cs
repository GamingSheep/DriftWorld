using UnityEngine;
using System.Collections;

public class Pop : MonoBehaviour 
{
	#region Public Attributes
    public bool StartAnimation = false;
    public float AnimationSpeed = 1f;
    public bool ToLeft = false;
    public float offset = 0;
    public  float intTimer = 1;
	#endregion
	
	#region Private Attributes
	private ParticleSystemRenderer particleRenderer;
	private Renderer auxRenderer;
	#endregion
	
	#region Main Methods
	private void Start() 
	{ 
		particleRenderer = GetComponent<ParticleSystemRenderer>();
		auxRenderer = GetComponent<Renderer>();
	}
	
    private void Update()
    {
        if (StartAnimation)
        {
            intTimer -= Time.deltaTime;
            
            if (intTimer <= 0)
            {
                intTimer = AnimationSpeed;
                offset += 0.125f;
                particleRenderer.material.mainTextureOffset = new Vector2(offset, auxRenderer.material.mainTextureOffset.y);
            }
        }
    }
    #endregion
}