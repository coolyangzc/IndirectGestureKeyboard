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
    public Parameter parameter;
    public TextManager textManager;
	public int blockID = 0, phraseID = 0; 
	public InputField userID;
	//public Text userID;

	private bool mouseHidden = false;
	private bool debugOn = false;
	private float distance = 0;

	private const float MinScrollDistance = 2;
	private const float MaxScrollDistance = 12;
	private const float ScrollKeySpeed = -2f;
	
	// Use this for initialization
	void Start() 
	{
		distance = canvas.transform.localPosition.z;
		info.Log("Debug", debugOn.ToString());
        lexicon.SetDebugDisplay(debugOn);

        //Alternative Start Option
        parameter.ChangeRatio();
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
                parameter.ChangeMode();
			if (Input.GetKeyDown(KeyCode.L))
                parameter.ChangeLocationFormula();
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
                parameter.ChangeRatio();
				lexicon.CalcKeyLayout();
				lexicon.CalcLexicon();
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
                HideDisplay();
                info.Clear();
                if (Parameter.userStudy == Parameter.UserStudy.Basic)
                {
                    Parameter.userStudy = Parameter.UserStudy.Study1_Train;
                    lexicon.ChangePhrase();
                    textManager.HighLight(-100);
                    info.Log("Phrase", "Warmup");
                    return;
                }
                Parameter.userStudy = Parameter.UserStudy.Study1;
				lexicon.ChangePhrase(phraseID);
				SendPhraseMessage();
                textManager.HighLight(-100);
				info.Log("Phrase", (phraseID+1).ToString() + "/40");
				server.Send("Get Keyboard Size", "");
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
                HideDisplay();
                Parameter.userStudy = Parameter.UserStudy.Study2;
				lexicon.ChangePhrase();
				SendPhraseMessage();
				info.Clear();
                info.Log("Mode", Parameter.mode.ToString());
                info.Log("Block", (blockID+1).ToString() + "/8");
				info.Log("Phrase", (phraseID % 6 + 1).ToString() + "/6");
                blockID = (blockID + 1) % 8;
            }
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (Input.GetKey(KeyCode.E))
                parameter.ChangeEndOffset(0.1f);
			if (Input.GetKey(KeyCode.R))
                parameter.ChangeRadius(0.1f);
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
                parameter.ChangeEndOffset(-0.1f);
			if (Input.GetKey(KeyCode.R))
                parameter.ChangeRadius(-0.1f);
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
			switch(Parameter.userStudy)
			{
				case Parameter.UserStudy.Basic:
					lexicon.ChangePhrase();
					break;
				case Parameter.UserStudy.Study1_Train:
					lexicon.ChangePhrase();
					textManager.HighLight(-100);
					break;
				case Parameter.UserStudy.Study1:
                    if (!textManager.InputCorrect() && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                        return;
					server.Send("Study1 End Phrase", Parameter.mode.ToString());
					phraseID++;
					if (phraseID % 10 == 0)
					{
                        Parameter.userStudy = Parameter.UserStudy.Study1_Train;
						lexicon.ChangePhrase();
                        textManager.HighLight(-100);
						info.Log("Phrase", "Warmup");
						return;
					}
					lexicon.ChangePhrase(phraseID);
					SendPhraseMessage();
                    textManager.HighLight(-100);
					info.Log("Phrase", (phraseID+1).ToString() + "/40");
					break;
				case Parameter.UserStudy.Study2:
					server.Send("Study2 End Phrase", textManager.inputText.text + "\n" + Parameter.mode.ToString());
					phraseID++;
					if (phraseID % 6 == 0)
					{
                        Parameter.userStudy = Parameter.UserStudy.Basic;
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
			if (Parameter.userStudy == Parameter.UserStudy.Study1 || Parameter.userStudy == Parameter.UserStudy.Study2)
				server.Send("Backspace", "");
			if (Parameter.userStudy == Parameter.UserStudy.Study1 || Parameter.userStudy == Parameter.UserStudy.Study1_Train)
                textManager.HighLight(-100);
			if (Parameter.userStudy == Parameter.UserStudy.Study2 || Parameter.userStudy == Parameter.UserStudy.Basic)
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
				if (distance < MinScrollDistance)
					distance = MinScrollDistance;
				else if (distance > MaxScrollDistance)
					distance = MaxScrollDistance;
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
		if (Parameter.userStudy == Parameter.UserStudy.Study1)
			server.Send("Study1 New Phrase", 
			            userID.text + "_" + phraseID.ToString() + ".txt" + "\n" +
                        textManager.phraseText.text);
		else if (Parameter.userStudy == Parameter.UserStudy.Study2)
			server.Send("Study2 New Phrase", 
			            userID.text + "_" + phraseID.ToString() + ".txt" + "\n" +
                        textManager.phraseText.text);
	}
}
