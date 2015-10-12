using UnityEngine;
using System.Collections;
using System;

public class OnlineManager : MonoBehaviour
{
	#region Public Attributes
	[Header("Online Attributes")]
	public string addScoreURL = "http://www.victorfisac.com/addscore.php?"; //be sure to add a ? to your url
	public string highscoreURL = "http://www.victorfisac.com/display.php";
	
	public string[] leaderboardNames;
	public string[] leaderboardScores;
	
	public string playerName;
	#endregion
	
	#region Private Attributes
	private string secretKey = "velaflip1"; // Edit this value and make sure it's the same as the one stored on the server
	private int counter;
	#endregion
	
	#region References
	[Header("References")]
	public CarDrifting carDrifting;
	#endregion
	
	#region Auxiliar Attributes
	private string hash;
	private string post_url;
	private WWW hs_post;
	private WWW hs_get;
	private string[] auxData;
	#endregion
	
	#region Main Methods
	private void OnEnable()
	{
		// Initialize values
		leaderboardNames = null;
		leaderboardScores = null;
		counter = 0;
		playerName = "";
	}
	#endregion
	
	#region Internal Methods
	public void CallPostScores()
	{
		Debug.Log ("OnlineManager: post scores method called");
		StartCoroutine (PostScores (playerName, carDrifting.TotalScore));
	}
	
	public void CallGetScores()
	{
		Debug.Log ("OnlineManager: get scores method called");
		StartCoroutine (GetScores ());
	}
	#endregion
	
	#region Online Enumerators
	// remember to use StartCoroutine when calling this function!
	IEnumerator PostScores(string name, float score)
	{
		//This connects to a server side php script that will add the name and score to a MySQL DB.
		// Supply it with a string representing the players name and the players score.
		hash = MD5.Md5Sum(name + score + secretKey);
		
		post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;
		
		// Post the URL to the site and create a download object to get the result.
		hs_post = new WWW(post_url);
		
		yield return hs_post; // Wait until the download is done
		
		if (hs_post.error != null)
		{
			print("There was an error posting the high score: " + hs_post.error);
		}
	}
	
	// Get the scores from the MySQL DB to display in a GUIText.
	// remember to use StartCoroutine when calling this function!
	IEnumerator GetScores()
	{
		Debug.Log("Loading Scores");
		hs_get = new WWW(highscoreURL);
		yield return hs_get;
		
		if (hs_get.error != null || hs_get.text == "")
		{
			print("There was an error getting the high score: " + hs_get.error);
		}
		else
		{
			// Get large string data
			auxData = hs_get.text.Split(new string[] { "," }, StringSplitOptions.None);
			
			// Initialize arrays
			leaderboardNames = new string[auxData.Length - 1];
			leaderboardScores = new string[auxData.Length - 1];
			
			// Setup leaderboard data
			for(int i = 0; i < leaderboardScores.Length; i++)
			{
				if((i % 2) == 0)
				{
					leaderboardNames[counter] = auxData[i];
				}
				else
				{
					leaderboardScores[counter] = auxData[i];
					counter++;
				}
			}
			
			Debug.Log ("OnlineManager: leaderboard loaded successful");
		}
	}
	#endregion
}