using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RankingLoader: MonoBehaviour 
{
	#region Public Attributes
	public Text[] names;
	public Text[] scores;
	public Text[] stands;
	public GameObject youObject;
	#endregion
	
	#region Private Attributes
	private bool isPlayer;
	#endregion
	
	#region References
	public OnlineManager onlineManager;
	public CarDrifting carDrifting;
	#endregion
	
	#region Main Methods
	private void OnEnable () 
	{
		Debug.Log ("RankingLoader: starting to load ranking");
		
		for(int i = 0; i < names.Length; i++)
		{
			names[i].transform.parent.gameObject.SetActive(false);
			scores[i].transform.parent.gameObject.SetActive(false);
			stands[i].transform.parent.gameObject.SetActive(false);
		}
		
		names[0].transform.parent.gameObject.SetActive(false);
		scores[0].transform.parent.gameObject.SetActive(false);
		stands[0].transform.parent.gameObject.SetActive(false);
		
		// Initialize values
		Invoke("SetupLeaderboard", 4.0f);
	}
	#endregion
	
	#region Leaderboard Methods
	private void SetupLeaderboard()
	{
		names[0].transform.parent.gameObject.SetActive(true);
		scores[0].transform.parent.gameObject.SetActive(true);
		stands[0].transform.parent.gameObject.SetActive(true);
		
		for(int i = 0; i < names.Length; i++)
		{
			if(i < onlineManager.leaderboardNames.Length)
			{
				names[i].text = onlineManager.leaderboardNames[i];
				scores[i].text = onlineManager.leaderboardScores[i];
				names[i].transform.parent.gameObject.SetActive(true);
				scores[i].transform.parent.gameObject.SetActive(true);
				stands[i].transform.parent.gameObject.SetActive(true);
			}
			else
			{
				names[i].transform.parent.gameObject.SetActive(false);
				scores[i].transform.parent.gameObject.SetActive(false);
				stands[i].transform.parent.gameObject.SetActive(false);
			}
		}
		
		isPlayer = false;
		
		for(int i = 0; i < names.Length; i++)
		{
			if(names[i].text == "")
			{
				names[i].transform.parent.gameObject.SetActive(false);
				scores[i].transform.parent.gameObject.SetActive(false);
				stands[i].transform.parent.gameObject.SetActive(false);
			}
			
			if(names[i].text == onlineManager.playerName)
			{
				Debug.Log ("RankingLoader: found player");
				isPlayer = true;
				names[i].color = Color.yellow;
				scores[i].color = Color.yellow;
				stands[i].color = Color.yellow;
			}
		}
		
		if(!isPlayer)
		{
			names[names.Length - 1].transform.parent.gameObject.SetActive(true);
			scores[names.Length - 1].transform.parent.gameObject.SetActive(true);
			stands[names.Length - 1].transform.parent.gameObject.SetActive(true);
			names[names.Length - 1].text = onlineManager.playerName;
			scores[names.Length - 1].text = carDrifting.TotalScore.ToString();
			names[names.Length - 1].color = Color.yellow;
			scores[names.Length - 1].color = Color.yellow;
			stands[names.Length - 1].color = Color.yellow;
		}
		
		if(isPlayer)
		{
			youObject.SetActive (false);
		}
	}
	#endregion
}