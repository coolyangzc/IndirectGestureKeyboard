using System.Net;
using UnityEngine;
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

public class Server : MonoBehaviour 
{
	public Gesture gesture;
	public Lexicon lexicon;
	public Info info;

	private int port = 9973;
	private string IP;
	
	// Use this for initialization
	void Start() 
	{
		IP = GetIP();
		info.Log("IP", IP);
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MyMsgType.Message, ReceiveMessage);
    }
	
	// Update is called once per frame
	void Update() 
	{
	
	}
	
	string GetIP()
	{
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
        return ips[1].ToString();
    }

    void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log(NetworkServer.connections.Count);
        Debug.Log("客户端ip" + NetworkServer.connections[1].address);
        info.Log("Client", NetworkServer.connections[1].address.Split(':')[3]);
    }

	public void Send(string tag, string message)
	{
        NetworkServer.SendToAll(MyMsgType.Message, new MyMessage(tag, message));
    }
	
	void ReceiveMessage(NetworkMessage netMsg)
	{
        //Debug.Log(message);
        var m = netMsg.ReadMessage<MyMessage>();
        string msg = m.msg;
        switch (m.tag)
        {
        	case "Began":
				gesture.Begin(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
                break;
			case "Moved":
				gesture.Move(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
				break;
			case "Ended":
				gesture.End(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
				break;
			case "Choose Candidate":
                int choose = int.Parse(msg);
				lexicon.Accept(ref choose);
				break;
			case "Keyboard Size Msg":
				lexicon.UpdateSizeMsg(msg);
				break;
			case "Delete":
				//lexicon.Delete();
				break;
            default:
				break;
		}
	}
}
