﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Server : MonoBehaviour 
{
	public Gesture gesture;
	public Lexicon lexicon;
	public Info info;

	private int clientCount = 0;
	private int port = 9973;
	private string IP;


	
	// Use this for initialization
	void Start() 
	{
		IP = GetIP();
		info.Log("IP", IP);
	}
	
	// Update is called once per frame
	void Update() 
	{
		switch (Network.peerType) 
		{
            case NetworkPeerType.Disconnected:
                StartServer();
                break;
            case NetworkPeerType.Server:
				OnServer();
                break;
            case NetworkPeerType.Client:
                break;
            case NetworkPeerType.Connecting:
                break;
        }
		
	}
	
	string GetIP()
	{
		IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
		var addr = host.AddressList;
		localIP = addr[addr.Length-1].ToString();
        return localIP;
	}
	
	void StartServer() 
	{
        NetworkConnectionError error = Network.InitializeServer(5, port, false);
        switch (error) 
		{
            case NetworkConnectionError.NoError:
                break;
            default:
                Debug.Log("Connect Error: " + error);
                break;
        }
    }
	
	void OnServer()
	{
		int length = Network.connections.Length;  
        if (length != clientCount)
		{
			for (int i=0; i<length; i++)  
			{  
				Debug.Log("客户端" + i);  
				Debug.Log("客户端ip" + Network.connections[i].ipAddress);  
				info.Log("Client" + i, Network.connections[i].ipAddress);

			}
		}
		clientCount = length;
	}

	public void Send(string tag, string message)
	{
		Send(tag + ":" + message);
	}
	
	public void Send(string message)
	{
		GetComponent<NetworkView>().RPC("ReceiveMessage", RPCMode.All, message);
	}
	
	[RPC]
	void ReceiveMessage(string message, NetworkMessageInfo netInfo)
	{
		//Debug.Log(message);
		string tag = message.Split(':')[0];
        string msg = message.Split(':')[1];
        switch (tag)
        {
        	case "Touch.Began":
				gesture.Begin(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
                break;
			case "Touch.Moved":
				gesture.Move(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
				break;
			case "Touch.Ended":
				gesture.End(float.Parse(msg.Split(',')[0]), float.Parse(msg.Split(',')[1]));
				break;
			case "Choose Candidate":
				lexicon.Accept(int.Parse(msg));
				break;
			case "Delete":
				//lexicon.Delete();
				break;
            default:
				break;
		}
	}
}
