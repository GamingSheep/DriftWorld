using UnityEngine;
using System.Collections;

public class ContainerSetup: MonoBehaviour 
{
	#region Public Attributes
	public Material[] containerMaterials;
	#endregion
	
	#region Private Attributes
	private MeshRenderer[] lowRenderers;
	private MeshRenderer[] highRenderers;
	private int lowRendererCounter;
	private int highRendererCounter;
	private int randomValue;
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		// Enable temporally high objects folder to work with arrays
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).transform.GetChild (1).gameObject.SetActive (true);
		}
		
		// Initialize values
		lowRendererCounter = 0;
		highRendererCounter = 0;
		
		// Set up containers materials
		lowRenderers = GetComponentsInChildren<MeshRenderer>();
		highRenderers = new MeshRenderer[lowRenderers.Length];
		
		MeshRenderer[] auxRenderer = lowRenderers;
		
		for(int i = 0; i < auxRenderer.Length; i++)
		{
			if(auxRenderer[i].gameObject.name.Contains ("container1") && auxRenderer[i].transform.parent.transform.parent.gameObject.name.Contains ("LowObjects"))
			{
				lowRenderers[lowRendererCounter] = auxRenderer[i];
				lowRendererCounter++;
			}
			else
			{
				if(auxRenderer[i].gameObject.name.Contains ("container1") && auxRenderer[i].transform.parent.transform.parent.gameObject.name.Contains ("HighObjects"))
				{
					highRenderers[highRendererCounter] = auxRenderer[i];
					highRendererCounter++;
				}
			}
		}
		
		// Fix array length
		auxRenderer = lowRenderers;
		lowRenderers = new MeshRenderer[lowRendererCounter];
		for(int i = 0; i < lowRenderers.Length; i++)
		{
			lowRenderers[i] = auxRenderer[i];
		}
		
		// Fix array length
		auxRenderer = highRenderers;
		highRenderers = new MeshRenderer[highRendererCounter];
		for(int i = 0; i < highRenderers.Length; i++)
		{
			highRenderers[i] = auxRenderer[i];
		}
		
		SetUpContainers();
	}
	#endregion
	
	#region Container Methods
	private void SetUpContainers()
	{
		for(int i = 0; i < lowRenderers.Length; i++)
		{
			randomValue = Random.Range ((int)0, (int)containerMaterials.Length);
			lowRenderers[i].material = containerMaterials[randomValue];
			
			if(i < highRenderers.Length)
			{
				highRenderers[i].material = containerMaterials[randomValue];
			}
		}
	}
	#endregion
}