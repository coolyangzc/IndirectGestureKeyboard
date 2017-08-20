using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MyMsgType
{
    public static short Message = 1024;
};

public class MyMessage : MessageBase
{
    public string tag, msg;
    public MyMessage()
    {

    }
    public MyMessage(string tag, string msg)
    {
        this.tag = tag;
        this.msg = msg;
    }
}

public class Client : MonoBehaviour 
{
	public Canvas connectWindow;
	public Button connectButton;
	public Keyboard keyboard;
	public InputField inputIP;

	public DebugInfo debugInfo;
	private string serverIP = "";
	private int port = 9973;
    private NetworkClient client;

	// Use this for initialization
	void Start() 
	{
		connectWindow.enabled = true;
		connectButton.onClick.AddListener(StartConnect);
	}
	
	// Update is called once per frame
	void Update() 
	{
		//if (Input.GetKeyDown(KeyCode.Escape)) 
			//connectWindow.enabled ^= true;
	}

	void OnGUI()
	{
		serverIP = inputIP.text;
	}

	public void StartConnect()
	{
        client = new NetworkClient();
        client.RegisterHandler(MyMsgType.Message, ReceiveMessage);
        client.Connect(serverIP, port);
	}
	
	public void Send(string tag, string message)
	{
        client.Send(MyMsgType.Message, new MyMessage(tag, message));
    }
	
    void ReceiveMessage(NetworkMessage netMsg)
    {
        //Debug.Log(message);
        var m = netMsg.ReadMessage<MyMessage>();
        string msg = m.msg;
        switch (m.tag)
        { 
            case "TouchScreen Keyboard Width":
				if (msg == "+")
					keyboard.ZoomIn(true);
				else if (msg == "-")
					keyboard.ZoomOut(true);
				break;
			case "TouchScreen Keyboard Height":
				if (msg == "+")
					keyboard.ZoomIn(false);
				else if (msg == "-")
					keyboard.ZoomOut(false);
				break;
			case "TouchScreen Keyboard Size":
				if (msg == "+")
					keyboard.ZoomIn(false, true);
				else if (msg == "-")
					keyboard.ZoomOut(false, true);
				break;
			case "Get Keyboard Size":
				keyboard.SendSizeMsg();
				break;
			case "Candidates":
				keyboard.SetCandidates(msg.Split(','));
				break;
			case "Study1 New Phrase":
				keyboard.NewDataFile(msg, 1);
				break;
			case "Study2 New Phrase":
				keyboard.NewDataFile(msg, 2);
				break;
			case "Study1 End Phrase":
			case "Study2 End Phrase":
				keyboard.EndDataFile(msg);
				break;
			case "Accept":
				keyboard.Accept(msg);
				break;
			case "SingleKey":
				keyboard.SingleKey(msg);
				break;
			case "Cancel":
				keyboard.Cancel();
				break;
			case "Delete":
				keyboard.Delete(msg);
				break;
			case "Backspace":
				keyboard.Backspace();
				break;
			case "Change Ratio":
				keyboard.ChangeRatio();
				break;
            case "NextCandidatePanel":
                keyboard.NextCandidatePanel();
                break;
            default:
				Debug.Log("Unknown tag: " + tag);
				break;
		}
	}
}
