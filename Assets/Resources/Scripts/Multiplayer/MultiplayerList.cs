using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MultiplayerList: MonoBehaviour 
{
	#region Public Attributes
	[Header("List Attributes")]
	public float cursorOffset;
	#endregion
	
	#region Private Attributes
	private int usedSlots;
	private bool pressed;
	private bool moving;
	public int selected;
	#endregion
	
	#region References
	private MultiplayerManager multiplayerManager;
	private CarInput carInput;
	
	[Header("References")]
	public Transform cursor;
	public GameObject errorMessage;
	public Text[] nameLabel;
	public Text[] sizeLabel;
	#endregion
	
	#region Main Methods
	private void OnEnable () 
	{
		usedSlots = 0;
		selected = 0;
		pressed = false;
		moving = false;
		multiplayerManager = transform.root.GetComponent<MultiplayerManager>();
		carInput = CarInput.Instance;
		
		for(int i = 0; i < nameLabel.Length; i++)
		{
			nameLabel[i].transform.parent.gameObject.SetActive (false);
		}
		
		for(int i = 0; i < sizeLabel.Length; i++)
		{
			sizeLabel[i].transform.parent.gameObject.SetActive (false);
		}
		
		cursor.gameObject.SetActive (false);
		errorMessage.SetActive (false);
		
		UpdateList();
	}
	
	private void OnDisable()
	{
		usedSlots = 0;
		selected = 0;
	}
	
	private void Update () 
	{
		// Update list
		if(!pressed)
		{
			if(carInput.Nitrous ())
			{
				pressed = true;		
				for(int i = 0; i < nameLabel.Length; i++)
				{
					nameLabel[i].transform.parent.gameObject.SetActive (false);
				}
				
				for(int i = 0; i < sizeLabel.Length; i++)
				{
					sizeLabel[i].transform.parent.gameObject.SetActive (false);
				}
				
				multiplayerManager.ActiveMainMenu();
				multiplayerManager.ActiveMultiplayerMenu();
			}
		}
		else
		{
			if(!carInput.Nitrous())
			{
				pressed = false;
			}
		}
		
		if(!moving)
		{	
			if(carInput.Up ())
			{
				if(selected > 0)
				{
					moving = true;
					selected--;
					UpdateCursor();
				}
			}
			else if(carInput.Down ())
			{
				if(selected < usedSlots - 1)
				{
					moving = true;
					selected++;
					UpdateCursor();
				}
			}
		}
		else
		{
			if(!carInput.Up () && !carInput.Down ())
			{
				moving = false;
			}
		}
			
		if(carInput.Submit ())
		{
			if(multiplayerManager.Network.matches != null && usedSlots > 0)
			{
                if (multiplayerManager.Network.matches[selected].currentSize < multiplayerManager.Network.matches[selected].maxSize)
                {
                    multiplayerManager.JoinMatch();

                    if (multiplayerManager.Network.matches[selected] != null)
                    {
                        multiplayerManager.Network.matchName = multiplayerManager.Network.matches[selected].name;
                        multiplayerManager.Network.matchSize = (uint)multiplayerManager.Network.matches[selected].currentSize;
                        multiplayerManager.Network.matchMaker.JoinMatch(multiplayerManager.Network.matches[selected].networkId, "", multiplayerManager.Network.OnMatchJoined);
                    }
                }
			}
		}	
	}
	#endregion
	
	#region List Methods
	private void UpdateCursor()
	{
		cursor.localPosition = new Vector3(cursor.localPosition.x, -selected * cursorOffset, cursor.localPosition.z);
	}
	
	private void UpdateList()
	{
		for(int i = 0; i < nameLabel.Length; i++)
		{
			nameLabel[i].transform.parent.gameObject.SetActive (false);
		}
		
		for(int i = 0; i < sizeLabel.Length; i++)
		{
			sizeLabel[i].transform.parent.gameObject.SetActive (false);
		}

        usedSlots = 0;
		
		cursor.gameObject.SetActive (false);
		
		foreach (var match in multiplayerManager.Network.matches)
		{
			if(usedSlots < nameLabel.Length && usedSlots < sizeLabel.Length)
			{
				nameLabel[usedSlots].text = match.name;
				sizeLabel[usedSlots].text = match.currentSize + " / " + match.maxSize;
				nameLabel[usedSlots].transform.parent.gameObject.SetActive (true);
				sizeLabel[usedSlots].transform.parent.gameObject.SetActive (true);
				usedSlots++;
			}
			else
			{
				Debug.Log ("MultiplayerList: match list is longer than UI slots");
			}
		}
		
		if(usedSlots <= 0)
		{
			cursor.gameObject.SetActive (false);
			errorMessage.SetActive (true);
		}
		else
		{
			cursor.gameObject.SetActive (true);
		}
	}
	#endregion
}