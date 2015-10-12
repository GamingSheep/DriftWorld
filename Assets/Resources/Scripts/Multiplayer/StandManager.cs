using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StandManager : MonoBehaviour 
{
    #region Public Attributes

    #endregion

    #region Private Attributes
    private GameObject[] players;
    private PlayerSync[] playersData;
    private int[] playersScore;
    private string[] playersName;
    private float auxPosition;
    private bool canWork;
    #endregion

    #region References
    private MultiplayerManager multiplayerManager;

    [Header("Stand References")]
    public Text[] stand;
    #endregion

    #region Main Methods
    private void Start()
    {
        multiplayerManager = transform.root.GetComponent<MultiplayerManager>();
        auxPosition = 0;
        canWork = true;
        players = GameObject.FindGameObjectsWithTag("Player");
        playersData = new PlayerSync[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            playersData[i] = players[i].GetComponent<PlayerSync>();
        }
    }

    private void Update()
    {
        if (canWork)
        {
            GetScores();
            UpdateUI();
        }
    }
    #endregion

    #region Score Methods
    private void GetScores()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        playersData = new PlayerSync[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            playersData[i] = players[i].GetComponent<PlayerSync>();
        }

        playersScore = new int[playersData.Length];
        for (int i = 0; i < playersScore.Length; i++)
        {
            if (playersData[i] != null)
            {
                playersScore[i] = playersData[i].shareScore;
            }
        }

        playersName = new string[playersData.Length];
        for (int i = 0; i < playersName.Length; i++)
        {
            if(playersData[i] != null)
            {
                playersName[i] = playersData[i].gameObject.name;
            }
        }
    }
    #endregion

    #region UI Methods
    private void UpdateUI()
    {
        // Sort stands
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

        // Update stand text label
        for (int i = 0; i < stand.Length; i++)
        {
            if(i < players.Length)
            {
                auxPosition = i + 1;
                stand[i].text = auxPosition + "º " + playersName[i] + " - " + playersScore[i];
                stand[i].transform.parent.gameObject.SetActive(true);

                if (playersScore[i] == multiplayerManager.TotalScore)
                {
                    stand[i].color = Color.yellow;
                }
                else
                {
                    stand[i].color = Color.white;
                }
            }
            else
            {
                stand[i].color = Color.white;
                stand[i].transform.parent.gameObject.SetActive(false);
            }
        }

        // Change own score color;
        
    }

    private void MovePlayerPosition(int from, int to)
    {
        int tempScore = 0;
        string tempName = "";

        tempScore = playersScore[from];
        tempName = playersName[from];

        playersScore[from] = playersScore[to];
        playersName[from] = playersName[to];

        playersScore[to] = tempScore;
        playersName[to] = tempName;
    }
    #endregion
}