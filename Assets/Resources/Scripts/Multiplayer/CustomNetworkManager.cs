using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class CustomNetworkManager: NetworkManager 
{
    #region Public Attributes
    private NetworkID _networkID;
    public NodeID _nodeID;
	#endregion
	
	#region Private Attributes
	
	#endregion
	
	#region References
	private MultiplayerManager multiplayerManager;
	#endregion
	
	#region Main Methods
	private void Start()
	{
		multiplayerManager = GetComponent<MultiplayerManager>();
	}
	
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);
        multiplayerManager.UpdateMatchData();
    }
	
	public override void OnServerRemovePlayer (NetworkConnection conn, PlayerController player)
	{
		base.OnServerRemovePlayer (conn, player);
        multiplayerManager.UpdateMatchData();
    }
	
	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
        multiplayerManager.UpdateMatchData();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        multiplayerManager.UpdateMatchData();
    }

    public override void OnMatchCreate(CreateMatchResponse response)
    {
        base.OnMatchCreate(response);
        _networkID = response.networkId;
        _nodeID = response.nodeId;
    }

    public void CustomOnMatchJoined(JoinMatchResponse response)
    {
        OnMatchJoined(response);
        _networkID = response.networkId;
        _nodeID = response.nodeId;
    }

    public void CancelMatch(BasicResponse response)
    {
        if(response.success)
        {
            Debug.Log("CustomNetworkManager: match canceled succesful");
           
        }
        else
        {
            Debug.Log("CustomNetworkManager: match cancel failed (error: " + response.extendedInfo + ")");
        }

        StopHost();
    }

    public void DropConnection()
    {
        DropConnectionRequest dropReq = new DropConnectionRequest();
        dropReq.networkId = _networkID;
        dropReq.nodeId = _nodeID;
        matchMaker.DropConnection(dropReq, OnConnectionDrop);
    }

    public void OnConnectionDrop(BasicResponse response)
    {
        if(response.success)
        {
            Debug.Log("CustomNetworkManager: connectecion droped succesful");
        }
        else
        {
            Debug.Log("CustomNetworkManager: connectecion droped failed (error: " + response.extendedInfo + ")");
        }

        StopClient();
    }
    #endregion

    #region Properties
    public NetworkID networkID
    {
        get { return _networkID; }
        set { _networkID = value; }
    }

    public NodeID nodeID
    {
        get { return _nodeID; }
        set { _nodeID = value; }
    }
    #endregion
}