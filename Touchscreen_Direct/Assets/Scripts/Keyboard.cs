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
    public GameObject nameWindow;

	public int userStudy = 0;

	private const string path = "sdcard//G-Keyboard//Direct//";
	private string buffer = "";
	private string phraseInfo;
	private StreamWriter writer;

	private const int CandidateNum = 5;

	private const float eps = 1e-10f;
    private const bool PhoneSize_5_1_inch = false;
	private Vector2 preLocal;
	private float keyboardWidth, keyboardHeight;
	private float heightRatio = 1f, widthRatio = 1f, overallRatio = 1f;
	private bool ratioChanged = false;

	// Use this for initialization
	void Start () 
	{
		keyboardWidth = keyboard.rectTransform.rect.width;
		keyboardHeight = keyboard.rectTransform.rect.height;
		SetKeyboard();
        if (PhoneSize_5_1_inch)
        {
            ZoomOut(false, true);
            ZoomOut(false, true);
        }
        else
        {
            ZoomOut(true); ZoomOut(true);
            ZoomOut(false); ZoomOut(false);
            ZoomOut(false, true);
        }
		ChangeRatio();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			Touch touch = Input.GetTouch(0);
			Vector2 local;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				keyboard.transform as RectTransform, touch.position, Camera.main, out local);
            Vector2 relative = new Vector2(local.x / keyboardWidth, local.y / keyboardHeight);
            debugInfo.Log("World", touch.position.x.ToString("f2") + "," + touch.position.y.ToString("f2"));
			debugInfo.Log("Local", local.x.ToString("f2") + "," + local.y.ToString("f2"));
			debugInfo.Log("Relative", relative.x.ToString("f2") + "," + relative.y.ToString("f2"));
			string coor = relative.x.ToString() + "," + relative.y.ToString();

            if (relative.y > 1.2f)
                return;

			if (userStudy > 0)
				buffer += touch.phase.ToString() + " " + Time.time.ToString() + " " +
					touch.position.x.ToString() + " " + touch.position.y.ToString() + " " +
					relative.x.ToString() + " " + relative.y.ToString() + "\n";

            switch (touch.phase)
			{
				case TouchPhase.Began:
					preLocal = local;
					break;
				case TouchPhase.Moved:
					if (Vector2.Distance(local, preLocal) > 0.1f)
					{
						preLocal = local;
					}
					break;
				case TouchPhase.Ended:
                    client.HighLight(+1);
                    break;
			}
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
	}

	void TapDelete()
	{
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

	public void ChangeRatio()
	{
        Vector2 minV = keyboard.GetComponent<RectTransform>().anchorMin;
		Vector2 maxV = keyboard.GetComponent<RectTransform>().anchorMax;
        if (ratioChanged)
        {
            minV.y = 0.21875f;
            maxV.y = minV.y + 0.16875f;
        }
        else
        {
            minV.y = 0.05f;
            maxV.y = 0.05f + 0.16875f * 3;
        }
            
        keyboard.GetComponent<RectTransform>().anchorMin = minV;
		keyboard.GetComponent<RectTransform>().anchorMax = maxV;
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
		FileInfo file = new FileInfo(path + fileName);
		if (!file.Exists)
			writer = file.CreateText();
		else
			writer = file.AppendText();
		debugInfo.Log("FileName", file.FullName);
		buffer = "";
        nameWindow.SetActive(false);
        Color c = keyboard.color;
        c.a = 0;
        keyboard.color = c;
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
		buffer += "SingleKey" + " " + Time.time.ToString() + " " + key + "\n";
	}

	public void Cancel()
	{
		buffer += "Cancel" + " " + Time.time.ToString() + "\n"; 
	}

	public void Delete(string type)
	{
		buffer += "Delete" + " " + Time.time.ToString() + " " + type + "\n";
	}

	public void Backspace()
	{
		buffer += "Backspace" + " " + Time.time.ToString() + "\n";
	}

    public void NextCandidatePanel()
    {
        buffer += "NextCandidatePanel" + " " + Time.time.ToString() + "\n";
    }


    public void SendSizeMsg()
	{
	}
}		