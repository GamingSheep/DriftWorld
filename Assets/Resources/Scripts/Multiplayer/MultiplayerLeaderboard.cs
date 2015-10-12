using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MultiplayerLeaderboard : MonoBehaviour 
{
    #region Public Attributes
    [Header("End Logic Attributes")]
    public float nextTempInit;
    #endregion

    #region Private 
    private GameObject[] players;
    private PlayerSync[] playersData;
    private int[] playersScore;
    private string[] playersName;

    // Next match temp
    private float nextTemp;
    #endregion

    #region References
    private MultiplayerManager multiplayerManager;

    [Header("Leaderboard References")]
    public Text nextLabel;
    public Text[] scoreLabel;
    public Text[] nameLabel;
    #endregion

    #region Main Methods
    private void OnEnable () 
    {
        nextTemp = nextTempInit;
        multiplayerManager = transform.root.GetComponent<MultiplayerManager>();

        for (int i = 0; i < scoreLabel.Length; i++)
        {
            scoreLabel[i].transform.parent.gameObject.SetActive(false);
            nameLabel[i].transform.parent.gameObject.SetActive(false);
        }

        // Get players data
        players = GameObject.FindGameObjectsWithTag("Player");

        playersData = new PlayerSync[players.Length];
        for(int i = 0; i < playersData.Length; i++)
        {
            playersData[i] = players[i].GetComponent<PlayerSync>();
        }

        playersScore = new int[playersData.Length];
        playersName = new string[playersData.Length];
        for(int i = 0; i < playersScore.Length; i++)
        {
            playersScore[i] = playersData[i].shareScore;
            playersName[i] = players[i].name;
        }

        // Sortplayers data
        for (int i = 0; i < playersScore.Length; i++)
        {
            for (int k = i + 1; k < playersScore.Length; k++)
            {
                if(playersScore[k] > playersScore[i])
                {
                    MovePlayerPosition(k, i);
                }
            }
        }

        // Set up leaderboard
        for (int i = 0; i < scoreLabel.Length; i++)
        {
            scoreLabel[i].transform.parent.gameObject.SetActive(false);
            nameLabel[i].transform.parent.gameObject.SetActive(false);
        }

        for (int i = 0; i < nameLabel.Length; i++)
        {
            if(playersScore[i] == multiplayerManager.TotalScore)
            {
                scoreLabel[i].color = Color.yellow;
                nameLabel[i].color = Color.yellow;
            }
            else
            {
                scoreLabel[i].color = Color.white;
                nameLabel[i].color = Color.white;
            }

            if (playersName[i].Length > 8)
            {
                playersName[i].Remove(8);
            }

            scoreLabel[i].text = playersScore[i].ToString();
            nameLabel[i].text = playersName[i];
            scoreLabel[i].transform.parent.gameObject.SetActive(true);
            nameLabel[i].transform.parent.gameObject.SetActive(true);
        }
	}
	
	private void Update () 
    {
	    if(nextTemp > 0.0f)
        {
            nextTemp -= Time.deltaTime;
            nextLabel.text = "Next match starts in " + Mathf.RoundToInt(nextTemp) + " seconds...";
        }
        else
        {
            multiplayerManager.ResetMatch();
        }
	}
    #endregion

    #region Leaderboard Methods
    private void MovePlayerPosition(int from, int to)
    {
        string auxName = "";
        int auxScore = 0;

        auxName = playersName[from];
        auxScore = playersScore[from];

        playersName[from] = playersName[to];
        playersScore[from] = playersScore[to];

        playersName[to] = auxName;
        playersScore[to] = auxScore;
    }
    #endregion
}