using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class AssetsUpdate: MonoBehaviour 
{
	#region Public Attributes
	
	#endregion
	
	#region Private Attributes
	
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	private void Start () 
	{

	}
	
	private void Update () 
	{
		Debug.Log ("hello");
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
	}
	#endregion
}