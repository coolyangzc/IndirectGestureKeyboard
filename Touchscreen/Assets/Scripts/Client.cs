using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Client : MonoBehaviour {

	public Text info;
	public Canvas connectWindow;
	public Button connectButton;
	public InputField inputIP;
	private bool showConnect = false;
	private string IP = "";
	private int port = 9973;

	// Use this for initialization
	void Start () 
	{
		connectWindow.enabled = false;
		connectButton.onClick.AddListener(StartConnect);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			info.text = "!";
			showConnect ^= true;
			connectWindow.enabled ^= true;
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			Debug.Log("Send");
			sendMessage("Connected");
		}
			
	}

	void OnGUI()
	{
		IP = inputIP.text;
	}

	public void StartConnect()
	{
		info.text = "start connecting...";
		NetworkConnectionError error = Network.Connect(IP, port);
		switch (error) 
		{
			case NetworkConnectionError.NoError:
				break;
			default:
				Debug.Log("client:" + error);
				info.text = "client:" + error;
				break;
		}
		
	}
	
	void sendMessage(string message) 
	{
        GetComponent<NetworkView>().RPC("ReceiveMessage", RPCMode.All, message);
    }
	
	[RPC]
	void ReceiveMessage(string message, NetworkMessageInfo info)
	{
	}
}
