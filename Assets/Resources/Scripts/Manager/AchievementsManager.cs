using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchievementsManager: MonoBehaviour 
{
	#region Public Attributes
	[Header("UI Attributes")]
	public string[] achievementsName;
	
	#endregion
	
	#region Private Attributes
	private bool[] hasToDo;
	private bool canShow;
	#endregion
	
	#region References
	[Header("UI References")]
	public GameObject trophyUI;
	public Text achievementText;
	public AudioManager audioManager;
	
	private DataManager dataManager;
	private AudioSource audioSource;
	#endregion
	
	#region Main Methods
	private void Start () 
	{
		dataManager = DataManager.Instance;
		audioSource = GetComponent<AudioSource>();
		
		hasToDo = new bool[achievementsName.Length];
		canShow = true;
	}
	
	private void LateUpdate()
	{
		if(canShow && !audioManager.showing)
		{
			for(int i = 0; i < hasToDo.Length; i++)
			{
				if(hasToDo[i])
				{
					UnlockAchievement(i);
				}
			}
		}
	}
	#endregion
	
	#region Achievements Methods
	public void CallUnlockAchievement(int id)
	{
		hasToDo[id] = true;
	}
	
	public void UnlockAchievement(int id)
	{
		hasToDo[id] = false;
		
		if(!dataManager.achievements[id])
		{
			canShow = false;
			achievementText.text = achievementsName[id];		
			trophyUI.SetActive (true);
			
			audioSource.Play ();
			
			dataManager.achievements[id] = true;
			dataManager.SaveData ();
			Debug.Log ("AchievementsManager: unlocked achievement id: " + id);
		}
	}
	
	public void DisableUI()
	{
		canShow = true;
		achievementText.text = "";
		trophyUI.SetActive (false);
	}
	#endregion
}