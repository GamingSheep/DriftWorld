using UnityEngine;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour
{
    #region Public Attributes
    [Header("Sync Attributes")]
    public float lerpScale;
    #endregion

    #region Private Attributes
    [SyncVar]
    private Quaternion playerRotation;
    [SyncVar]
    private string playerName;
    [SyncVar]
    private float serverTime;
    [SyncVar]
    private int playerScore;

    [SyncVar]
    private float throttle;
    [SyncVar]
    private float brake;
    [SyncVar]
    private float steer;
    [SyncVar]
    private float handBrake;
    [SyncVar]
    private int gear;

    [SyncVar]
    private bool backFire;
    [SyncVar]
    private bool nitroFire;
    #endregion

    #region References
    private Transform playerTransform;
    private NetworkInstanceId playerNetID;
    private MultiplayerManager multiplayerManager;
    private PlayerSync serverSync;
    private GameObject[] players;
    private NetworkAnimator networkAnimator;
    private CarEngine carEngine;

    [Header("Sync References")]
    public GameObject backFireObject;
    public GameObject nitroFireObject;
    #endregion

    #region Main Methods
    private void Start()
    {
        // Get references
        multiplayerManager = GameObject.FindWithTag("MultiplayerManager").GetComponent<MultiplayerManager>();
        playerTransform = transform;
        carEngine = GetComponent<CarEngine>();

        if (!isLocalPlayer)
        {
            GetComponent<CarController>().enabled = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        // Get references
        multiplayerManager = GameObject.FindWithTag("MultiplayerManager").GetComponent<MultiplayerManager>();
        playerTransform = transform;
        carEngine = GetComponent<CarEngine>();

        // Set up player name
        GetNetIdentity();
        SetNetIdentity();

        // Set up MultiplayerManager if player is match server
        if (isServer)
        {
            multiplayerManager.itIsServer = true;
        }

        if (!isLocalPlayer)
        {
            GetComponent<CarController>().enabled = false;
        }
    }

    private void FixedUpdate()
    {
        // Update rigidbody and rotation values
        TransmitRotation();
        LerpRotations();
    }

    private void Update()
    {
        // Check for null references
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Set identity if it is not set yet
        if (playerTransform.name == "" || playerTransform.name.Contains("Clone"))
        {
            SetNetIdentity();
        }

        // Check if player is match server
        if (isServer)
        {
            // Check for null references
            if (multiplayerManager == null)
            {
                multiplayerManager = GameObject.FindWithTag("MultiplayerManager").GetComponent<MultiplayerManager>();
            }

            // Set the match time
            SetServerTime(multiplayerManager.LeftTime);
        }
        else
        {
            // Check for null references
            if (serverSync == null)
            {
                players = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].name.Contains("host"))
                    {
                        serverSync = players[i].GetComponent<PlayerSync>();
                        break;
                    }
                }
            }

            if (multiplayerManager == null)
            {
                multiplayerManager = GameObject.FindWithTag("MultiplayerManager").GetComponent<MultiplayerManager>();
            }

            // If player is not match server, read match left time from server sync
            multiplayerManager.LeftTime = serverSync.serverTime;
        }

        // Update player score
        TransmitScore();
    }
    #endregion

    #region Sync Name Methods
    private void LerpRotations()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if(carEngine == null)
        {
            carEngine = GetComponent<CarEngine>();
        }

        if (!isLocalPlayer)
        {
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, playerRotation, Time.deltaTime * lerpScale);

            carEngine.throttle = throttle;
            carEngine.brake = brake;
            carEngine.handbrake = handBrake;
            carEngine.steer = steer;
            carEngine.Gear = gear;

            backFireObject.SetActive(backFire);
            nitroFireObject.SetActive(nitroFire);
        }
    }

    [Command]
    private void CmdProvideRotationToServer(Quaternion auxRotation)
    {
        playerRotation = auxRotation;
    }

    [Command]
    private void CmdProvideEngineToServer(float auxThrottle, float auxBrake, float auxHandBrake, float auxSteer, int auxGear, bool auxBackFire, bool auxNitroFire)
    {
        throttle = auxThrottle;
        brake = auxBrake;
        handBrake = auxHandBrake;
        steer = auxSteer;
        gear = auxGear;

        backFire = auxBackFire;
        nitroFire = auxNitroFire;
    }

    [Client]
    private void TransmitRotation()
    {
        if (isLocalPlayer)
        {
            CmdProvideRotationToServer(playerTransform.rotation);
            CmdProvideEngineToServer(carEngine.throttle, carEngine.brake, carEngine.handbrake, carEngine.steer, carEngine.Gear, backFireObject.activeSelf, nitroFireObject.activeSelf);
        }
    }

    [Client]
    private void GetNetIdentity()
    {
        playerNetID = GetComponent<NetworkIdentity>().netId;
        if (isServer)
        {
            CmdTellServerMyIdentity("Player " + playerNetID.ToString() + " (host)");
        }
        else
        {
            CmdTellServerMyIdentity("Player " + playerNetID.ToString());
        }
    }

    [Client]
    private void SetNetIdentity()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if (!isLocalPlayer)
        {
            playerTransform.name = playerName;
        }
        else
        {
            if (isServer)
            {
                playerTransform.name = "Player " + GetComponent<NetworkIdentity>().netId.ToString() + " (host)";
            }
            else
            {
                playerTransform.name = "Player " + GetComponent<NetworkIdentity>().netId.ToString();
            }
        }
    }

    [Command]
    private void CmdTellServerMyIdentity(string name)
    {
        playerName = name;
    }
    #endregion

    #region Sync Time Methods
    [Client]
    private void SetServerTime(float time)
    {
        if (isLocalPlayer)
        {
            CmdTellServerMyTime(time);
        }
    }

    [Command]
    private void CmdTellServerMyTime(float time)
    {
        serverTime = time;
    }
    #endregion

    #region Sync Score Methods
    [Client]
    private void TransmitScore()
    {
        if (isLocalPlayer)
        {
            CmdProvideScoreToServer(multiplayerManager.TotalScore);
        }
    }

    [Command]
    private void CmdProvideScoreToServer(int score)
    {
        playerScore = score;
    }
    #endregion

    #region Properties
    public int shareScore
    {
        get { return playerScore; }
    }
    #endregion
}