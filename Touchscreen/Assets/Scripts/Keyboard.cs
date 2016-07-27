using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class Keyboard : MonoBehaviour 
{
	public Image keyboard;
	public Image candidates;
	public Button button;
	public DebugInfo debugInfo;
	public Client client;

	private int userStudy = 0;

	private string path = "sdcard/";
	private string buffer = "";
	private string phraseInfo;
	private StreamWriter writer;

	private const int CandidateNum = 5;

	private const float eps = 1e-10f;
	private Vector2 preLocal;
	private Button[] btn = new Button[CandidateNum];
	private float keyboardWidth, keyboardHeight;
	private float heightRatio = 1f, widthRatio = 1f, overallRatio = 1f;
	private bool ratioChanged = false;

	// Use this for initialization
	void Start () 
	{
		for (int i = 1; i < CandidateNum; ++i)
		{
			btn[i] = candidates.transform.FindChild("Candidate" + i.ToString()).GetComponent<Button>();
		}
		/*
		btn[1].onClick.AddListener(delegate(){ChooseCandidate(1);});
		btn[2].onClick.AddListener(delegate(){ChooseCandidate(2);});
		btn[3].onClick.AddListener(delegate(){ChooseCandidate(3);});
		btn[4].onClick.AddListener(delegate(){ChooseCandidate(4);});
		Button del = keyboard.transform.FindChild("Del").GetComponent<Button>();
		del.onClick.AddListener(TapDelete);
		*/
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		SetKeyboard();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			Touch touch = Input.GetTouch(Input.touchCount - 1);
			Vector2 local;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				keyboard.transform as RectTransform, touch.position, Camera.main, out local);
			debugInfo.Log("World", touch.position.x.ToString() + "," + touch.position.y.ToString());
			debugInfo.Log("Local", local.x.ToString() + "," + local.y.ToString());
			Vector2 relative = new Vector2(local.x / keyboardWidth, local.y / keyboardHeight);
			debugInfo.Log("Relative", relative.x.ToString() + "," + relative.y.ToString());
			string coor = relative.x.ToString() + "," + relative.y.ToString();
			if (userStudy > 0)
				buffer += touch.phase.ToString() + " " + Time.time.ToString() + " " +
					touch.position.x.ToString() + " " + touch.position.y.ToString() + " " +
					relative.x.ToString() + " " + relative.y.ToString() + "\n";
			switch (touch.phase)
			{
				case TouchPhase.Began:
					client.Send("Began", coor);
					preLocal = local;
					break;
				case TouchPhase.Moved:
					if (Vector2.Distance(local, preLocal) > 0.1f)
					{
						client.Send("Moved", coor);
						preLocal = local;
					}
					break;
				case TouchPhase.Ended:
					client.Send("Ended", coor);
					break;
			}
			//client.Send("Touch", );
		}
	}

	void SetKeyboard()
	{
		keyboard.rectTransform.localScale = new Vector3(widthRatio * overallRatio, heightRatio * overallRatio, 1f);
		debugInfo.Log("Width", (keyboardWidth * widthRatio * overallRatio).ToString());
		debugInfo.Log("Height", (keyboardHeight * heightRatio * overallRatio).ToString());
	}

	void ChooseCandidate(int id)
	{
		client.Send("Choose Candidate", id.ToString());
	}

	void TapDelete()
	{
		client.Send("Delete", "");
	}

	public void ZoomIn(bool isWidth, bool isOverall = false)
	{
		if (isOverall)
		{
			overallRatio += 0.25f;
			if (overallRatio > 1)
				overallRatio = 1;
		}
		else
			if (isWidth)
			{
				widthRatio += 0.1f;
				if (widthRatio > 1)
					widthRatio = 1;
			} else
			{
				heightRatio += 0.1f;
				if (heightRatio > 1)
					heightRatio = 1;
			}
		SetKeyboard();
	}

	public void ZoomOut(bool isWidth, bool isOverall = false)
	{
		if (isOverall)
		{
			overallRatio -= 0.25f;
			if (overallRatio < eps)
				overallRatio = 0.25f;
		}
		else
			if (isWidth)
			{
				widthRatio -= 0.1f;
				if (widthRatio < eps)
					widthRatio = 0.1f;
			} else
			{
				heightRatio -= 0.1f;
				if (heightRatio < eps)
					heightRatio = 0.1f;
			}
		SetKeyboard();
	}

	public void SetCandidates(string[] word)
	{

		if (userStudy == 2)
		{
			string line = "";
			int cnt = 0;
			for (int i = 0; i < word.Length; ++i)
				if (word[i].Length > 0)
				{
					line += " " + word[i];
					cnt = i + 1;
				}
			buffer += "Candidates " + Time.time.ToString() + " " + cnt.ToString() + line + "\n";
		}

		//for (int i = 0; i < word.Length; ++i)
			//btn[i+1].GetComponentInChildren<Text>().text = word[i];
	}

	public void ChangeRatio()
	{
		Vector2 v = keyboard.GetComponent<RectTransform>().anchorMax;
		if (ratioChanged)
			v.y = 0.3f;
		else
			v.y = 0.5f;
		keyboard.GetComponent<RectTransform>().anchorMax = v;
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		ratioChanged ^= true;
		SetKeyboard();
	}

	public void NewDataFile(string str, int study)
	{
		userStudy = study;
		string fileName = str.Split('\n')[0]; 
		phraseInfo = str.Split('\n')[1];
		FileInfo file = new FileInfo(path + "//" + fileName);
		if (!file.Exists)
			writer = file.CreateText();
		else
			writer = file.AppendText();
		debugInfo.Log("FileName", file.FullName);
		buffer = "";
	}

	public void EndDataFile(string msg)
	{
		buffer = phraseInfo + "\n" + 
				 msg + "\n" + 
				 (widthRatio * overallRatio).ToString("0.0") + " " + (heightRatio * overallRatio).ToString("0.0") + "\n" +
				 (keyboardWidth * widthRatio * overallRatio).ToString() + " " + (keyboardHeight * heightRatio * overallRatio).ToString() + "\n" +
				 buffer + 
				 "PhraseEnd " + Time.time.ToString();
		writer.WriteLine(buffer);
		writer.Close();
		writer.Dispose();
	}

	public void Accept(string word)
	{
		buffer += "Accept " + Time.time.ToString() + " " + word + "\n";
	}

	public void SingleKey(string key)
	{
		buffer += "SingleKey " + Time.time.ToString() + " " + key + "\n";
	}

	public void Cancel()
	{
		buffer += "Cancel" + " " + Time.time.ToString() + "\n"; 
	}

	public void Delete()
	{
		buffer += "Delete" + " " + Time.time.ToString() + "\n";
	}

	public void Backspace()
	{
		buffer += "Backspace" + " " + Time.time.ToString() + "\n";
	}

	public void SendSizeMsg()
	{
		if (ratioChanged)
			client.Send("Keyboard Size Msg", overallRatio.ToString("0.00") + " " + "3");
		else
			client.Send("Keyboard Size Msg", overallRatio.ToString("0.00") + " " + "1");
			
	}
}		