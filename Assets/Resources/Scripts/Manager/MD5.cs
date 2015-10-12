using UnityEngine;
using System.Collections;

public class MD5: MonoBehaviour 
{
	#region Public Attributes
	
	#endregion
	
	#region Private Attributes
	private static byte[] bytes;
	private static byte[] hashBytes;
	private static string hashString;
	#endregion
	
	#region References
	
	#endregion
	
	#region Main Methods
	public static string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
	#endregion
}