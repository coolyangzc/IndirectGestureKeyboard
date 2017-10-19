using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PCControl : MonoBehaviour {

	public Server server;
	public Canvas canvas;
	public Image keyboard;
	public Lexicon lexicon;
	public Gesture gesture;
	public Info info;
	public int blockID = 0, phraseID = 0; 
	public InputField userID;
	//public Text userID;

	private bool mouseHidden = false;
	private bool debugOn = false;
	private float distance = 0;

	private float MinDistance = 4;
	private float MaxDistance = 12;
	private float ScrollKeySpeed = -1f;
    private int display_cnt = 0;
	
	// Use this for initialization
	void Start() 
	{
		distance = canvas.transform.localPosition.z;
		info.Log("Debug", debugOn.ToString());
        lexicon.SetDebugDisplay(debugOn);

        //Alternative Start Option
        gesture.ChangeRatio();
		lexicon.CalcKeyLayout();
		lexicon.CalcLexicon();
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			KeyControl();
			MouseControl();
		}
	}

    private void HideDisplay()
    {
        ColorBlock cb = userID.colors;
        Color c = userID.colors.normalColor;
        c.a = 0;
        cb.normalColor = c;
        userID.colors = cb;
        c = userID.transform.Find("Text").GetComponent<Text>().color;
        c.a = 0;
        userID.transform.Find("Text").GetComponent<Text>().color = c;
    }

	void KeyControl()
	{
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.D))
        {
            display_cnt++;
            if (display_cnt % 2 == 0)
                lexicon.SetPhrase("thanks for watching");
            else
                lexicon.SetPhrase("a subject one can really enjoy");
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
				server.Send("TouchScreen Keyboard Width", "+");
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				server.Send("TouchScreen Keyboard Width", "-");
			if (Input.GetKeyDown(KeyCode.D))
			{
				debugOn ^= true;
				info.Log("Debug", debugOn.ToString());
				lexicon.SetDebugDisplay(debugOn);
			}
			if (Input.GetKeyDown(KeyCode.M))
				lexicon.ChangeMode();
			if (Input.GetKeyDown(KeyCode.S))
				lexicon.ChangeShapeFormula();
			if (Input.GetKeyDown(KeyCode.L))
				lexicon.ChangeLocationFormula();
			if (Input.GetKeyDown(KeyCode.N))
				lexicon.ChangePhrase();
			if (Input.GetKeyDown(KeyCode.C))
				lexicon.ChangeCandidatesChoose(true);
			if (Input.GetKeyDown(KeyCode.R))
			{
			    server.Send("Change Ratio", "");
				server.Send("Get Keyboard Size", "");
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				gesture.ChangeRatio();
				lexicon.CalcKeyLayout();
				lexicon.CalcLexicon();
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
                HideDisplay();
                info.Clear();
                if (Lexicon.userStudy == Lexicon.UserStudy.Basic)
                {
                    Lexicon.userStudy = Lexicon.UserStudy.Train;
                    lexicon.ChangePhrase();
                    lexicon.HighLight(-100);
                    info.Log("Phrase", "Warmup");
                    return;
                }
				Lexicon.userStudy = Lexicon.UserStudy.Study1;
				lexicon.ChangePhrase(phraseID);
				SendPhraseMessage();
				lexicon.HighLight(-100);
				info.Log("Phrase", (phraseID+1).ToString() + "/60");
				server.Send("Get Keyboard Size", "");
				
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
                HideDisplay();
				Lexicon.userStudy = Lexicon.UserStudy.Study2;
				lexicon.ChangePhrase();
				SendPhraseMessage();
				info.Clear();
                info.Log("Mode", Lexicon.mode.ToString());
                info.Log("Block", (blockID+1).ToString() + "/8");
				info.Log("Phrase", (phraseID % 6 + 1).ToString() + "/6");
                blockID = (blockID + 1) % 8;
            }
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (Input.GetKey(KeyCode.E))
				lexicon.ChangeEndOffset(0.1f);
			if (Input.GetKey(KeyCode.R))
				lexicon.ChangeRadius(0.1f);
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				server.Send("TouchScreen Keyboard Height", "+");
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				server.Send("TouchScreen Keyboard Size", "+");
				server.Send("Get Keyboard Size", "");
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (Input.GetKey(KeyCode.E))
				lexicon.ChangeEndOffset(-0.1f);
			if (Input.GetKey(KeyCode.R))
				lexicon.ChangeRadius(-0.1f);
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				server.Send("TouchScreen Keyboard Height", "-");
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				server.Send("TouchScreen Keyboard Size", "-");
				server.Send("Get Keyboard Size", "");
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			switch(Lexicon.userStudy)
			{
				case Lexicon.UserStudy.Basic:
					lexicon.ChangePhrase();
					break;
				case Lexicon.UserStudy.Train:
					lexicon.ChangePhrase();
					lexicon.HighLight(-1000);
					break;
				case Lexicon.UserStudy.Study1:
					server.Send("Study1 End Phrase", Lexicon.mode.ToString());
					phraseID++;
					
					if (phraseID % 10 == 0)
					{
						Lexicon.userStudy = Lexicon.UserStudy.Train;
						lexicon.ChangePhrase();
						lexicon.HighLight(-100);
						info.Log("Phrase", "Warmup");
						return;
					}
					lexicon.ChangePhrase(phraseID);
					SendPhraseMessage();
					lexicon.HighLight(-100);
					info.Log("Phrase", (phraseID+1).ToString() + "/60");
					break;
				case Lexicon.UserStudy.Study2:
					server.Send("Study2 End Phrase", lexicon.inputText.text + "\n" + Lexicon.mode.ToString());
					phraseID++;
					if (phraseID % 6 == 0)
					{
						Lexicon.userStudy = Lexicon.UserStudy.Basic;
						lexicon.ChangePhrase();
                        info.Log("Block", "<color=red>Rest</color>");
						info.Log("Phrase", "<color=red>Rest</color>");
						return;
					}
					lexicon.ChangePhrase();
					SendPhraseMessage();
					info.Log("Phrase", (phraseID % 6 + 1).ToString() + "/6");
					break;
			}
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (Lexicon.userStudy == Lexicon.UserStudy.Study1 || Lexicon.userStudy == Lexicon.UserStudy.Study2)
				server.Send("Backspace", "");
			if (Lexicon.userStudy == Lexicon.UserStudy.Study1 || Lexicon.userStudy == Lexicon.UserStudy.Train)
				lexicon.HighLight(-100);
			if (Lexicon.userStudy == Lexicon.UserStudy.Study2 || Lexicon.userStudy == Lexicon.UserStudy.Basic)
				lexicon.Clear();
		}

	}

	void MouseControl() 
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				distance += Input.GetAxis("Mouse ScrollWheel") * ScrollKeySpeed;
				if (distance < MinDistance)
					distance = MinDistance;
				else if (distance > MaxDistance)
					distance = MaxDistance;
			}
			Vector3 pos = canvas.transform.localPosition;
			canvas.transform.localPosition = new Vector3(pos.x, pos.y, distance);
		}
		if (Input.GetKeyDown(KeyCode.Escape)) 
			mouseHidden ^= true;

		if (mouseHidden) 
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		} else 
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void SendPhraseMessage()
	{
		if (Lexicon.userStudy == Lexicon.UserStudy.Study1)
			server.Send("Study1 New Phrase", 
			            userID.text + "_" + phraseID.ToString() + ".txt" + "\n" + 
			            lexicon.phraseText.text);
		else if (Lexicon.userStudy == Lexicon.UserStudy.Study2)
			server.Send("Study2 New Phrase", 
			            userID.text + "_" + phraseID.ToString() + ".txt" + "\n" + 
			            lexicon.phraseText.text);
	}
}
