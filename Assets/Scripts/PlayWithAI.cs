﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerManager;
using Navigator;
using System;
using System.IO;
using UnityEngine.UI;

public class PlayWithAI : MonoBehaviour {
    public Button Rock_Button, Paper_Button, Scissors_Button;
    public Text Match_Number_Text, Help_Text, Human_Number_Text, AI_Number_Text, Player_Name;
    // public GameObject skinList;
    string userId = "";
    string wins = "";
    string losses = "";
    string skintag = "";
    string username = "";
    int matchNumber = 1;
    int sessionResponse = 2;
    int localAiWin = 0;
    int localHumanWin = 0;
    private static int skinInScreenPosition = 276;
    private static int skinOutScreenPosition = 630;
    private bool skinsOnScreen = false;

    public GameObject PlayerSprite;//Current Sprite, Yours or Opponents
    public GameObject OpponentSprite; //Your Opponent's sprite
    public GameObject RockSkin;
    public GameObject PaperSkin;
    public GameObject ScissorsSkin;


    ConnectionManager connectionManager;
    UserInfo userInfo;
    Skin skin;

	// Use this for initialization
	void Start () {
        //initially, store the necessary info (user_id) into local variable to be ready to pass to playWithAI()
        try {
            using (StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/MyInfo.json")){
                String line = streamReader.ReadToEnd();
                streamReader.Close();
                userInfo = JsonUtility.FromJson<UserInfo>(line);
                userId = userInfo.getUserId();
                wins = userInfo.getWins();
                losses = userInfo.getLosses();
                skintag = userInfo.getSkintag();
                username = userInfo.getUsername();
                skin = new Skin(skintag);

                Debug.Log("add listener");
                // Rock_Button = Rock_Button.GetComponent<Button>();
                // Paper_Button = Paper_Button.GetComponent<Button>();
                // Scissors_Button = Scissors_Button.GetComponent<Button>();

                // skinList = GameObject.FindGameObjectWithTag("skinList");

                Player_Name.text = username;

                Skin.setButtonSkin(Rock_Button, Paper_Button, Scissors_Button, skin);
                
                Rock_Button.onClick.AddListener(delegate {TaskWithParameters("1");});
                Paper_Button.onClick.AddListener(delegate {TaskWithParameters("2");});
                Scissors_Button.onClick.AddListener(delegate {TaskWithParameters("3");});
                Debug.Log("done adding listener");

                connectionManager = new ConnectionManager();
                if (connectionManager.StartClient() == 1) //successful start of client
                {
                    string sessionStartResponse = connectionManager.startGameSession();
                    Debug.Log(sessionStartResponse);
                } else //failed start of client
                {
                    Debug.Log("Failed to start ConnectionManager Client");
                }
            }
        } catch(Exception e) {
            Debug.Log(e.Message, gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public void TaskWithParameters(string move) {
        // ShowMove ShowMoveUI = new ShowMove();
        Debug.Log(move);
        connectionManager.sendUserId(userId);
        Debug.Log("Sent User Id");
        connectionManager.sendMove(move);
        Debug.Log("Sent Move");

        
        string stringResponse = connectionManager.getResponse();
        Debug.Log(stringResponse);

        sessionResponse = Convert.ToInt32(stringResponse);

        // Debug.Log(playerWinResponse);
        // Debug.Log(AIWinResponse);
        Debug.Log(sessionResponse);

        if (sessionResponse == 2) {
            matchNumber++;
            Match_Number_Text.text = matchNumber.ToString();
            string playerWinResponse = connectionManager.getOneResponse();
            string AIWinResponse = connectionManager.getOneResponse();

            localHumanWin = Convert.ToInt32(playerWinResponse);
            localAiWin = Convert.ToInt32(AIWinResponse);
            if (playerWinResponse == "1") { 

            this.Run(move, "W");
             }
            else if (playerWinResponse == "-1")
            {
                this.Run(move, "L");
            }
            else
            {
                this.Run(move, "T");
            }
            Human_Number_Text.text = playerWinResponse;
            AI_Number_Text.text = AIWinResponse;
        } else {
            Debug.Log(sessionResponse);
            EndGame(move);
        }
    }

    public void EndGame(string move) {
        if (sessionResponse == 1) {
            Help_Text.text = "You won!";
            int newWin = int.Parse(wins);
            newWin++;
            userInfo.setWins(newWin.ToString());
            wins = newWin.ToString();
            localHumanWin++;
            this.Run(move, "W");
            Human_Number_Text.text = (localHumanWin).ToString();

        } else if (sessionResponse == 0){
            Help_Text.text = "AI won!";
            int newLosses = int.Parse(losses);
            newLosses++;
            userInfo.setLosses(newLosses.ToString());
            losses = newLosses.ToString();
            localAiWin++;
            this.Run(move, "L");
            AI_Number_Text.text = (localAiWin).ToString();
        }
        string json = JsonUtility.ToJson(userInfo);
        File.WriteAllText(Application.persistentDataPath + "/MyInfo.json", json);

        //update to the DB
        connectionManager = new ConnectionManager();
        connectionManager.StartClient();
        string[] param = new string[4];
        param[0] = wins;
        param[1] = losses;
        param[2] = userId;
        string updateWinLossResponse = connectionManager.updateWinLoss(param);
        Debug.Log(updateWinLossResponse);

        //disable buttons
        Rock_Button.onClick.RemoveListener(delegate {TaskWithParameters("1");});
        Paper_Button.onClick.RemoveListener(delegate {TaskWithParameters("2");});
        Scissors_Button.onClick.RemoveListener(delegate {TaskWithParameters("3");});
    }


    //This is show move hard code because life
    public void Run(string Move, string WINLOSS)
    {

        SetSprites(Move, WINLOSS);
    }

    public void SetSprites(string Move, string WinLossSituation)
    {

        if (WinLossSituation == "W")//You Won
        {

            if (Move == "1")//Set your sprite to Rock, opponent to Scissors
            {

                SetPlayerSpriteToObjectSprite(RockSkin);

                SetOpponentSpriteToObjectSprite(ScissorsSkin);
            }
            else if (Move == "2")//You picked Paper, set your sprite to paper and enemies to rock
            {

                SetPlayerSpriteToObjectSprite(PaperSkin);//Set Sprite Paper

                SetOpponentSpriteToObjectSprite(RockSkin);//Set Sprite Rock
            }
            else//You Won with Scissors, set sprites
            {

                SetPlayerSpriteToObjectSprite(ScissorsSkin);//Set Sprite Scissors

                SetOpponentSpriteToObjectSprite(PaperSkin);//Set Sprite Paper
            }
        }
        else if (WinLossSituation == "L")
        {
            if (Move == "1")//Set your sprite to Rock, opponent to Paper
            {

                SetPlayerSpriteToObjectSprite(RockSkin);

                SetOpponentSpriteToObjectSprite(PaperSkin);
            }
            else if (Move == "2")//You picked Paper, set your sprite to paper and enemies to Scissors
            {

                SetPlayerSpriteToObjectSprite(PaperSkin);//Set Sprite Paper

                SetOpponentSpriteToObjectSprite(ScissorsSkin);//Set Sprite Scissors
            }
            else//You Lost with Scissors, set sprites
            {

                SetPlayerSpriteToObjectSprite(ScissorsSkin);//Set Sprite Scissors

                SetOpponentSpriteToObjectSprite(RockSkin);//Set Sprite Rock
            }
        }
        else//Tie, change both sprites to the same thing
        {


            if (Move == "1")//Set your sprite to Rock, opponent to Rock
            {

                SetPlayerSpriteToObjectSprite(RockSkin);

                SetOpponentSpriteToObjectSprite(RockSkin);
            }
            else if (Move == "2")//You picked Paper, set your sprite to paper and enemies to Paper
            {

                SetPlayerSpriteToObjectSprite(PaperSkin);//Set Sprite Paper

                SetOpponentSpriteToObjectSprite(PaperSkin);//Set Sprite Paper
            }
            else//You Tie with Scissors, set sprites
            {

                SetPlayerSpriteToObjectSprite(ScissorsSkin);//Set Sprite Scissors

                SetOpponentSpriteToObjectSprite(ScissorsSkin);//Set Sprite Scissors
            }
        }

    }
    public void SetPlayerSpriteToObjectSprite(GameObject GameObjectImage)//Sets PlayerSprite to gameobject's image
    {
        Image ChangedImage = PlayerSprite.GetComponent<Image>();
        Image TheImage = GameObjectImage.GetComponent<Image>();
        ChangedImage.sprite = TheImage.sprite;
    }
    public void SetOpponentSpriteToObjectSprite(GameObject GameObjectImage)//Sets Opponent's sprite to gameobject's image
    {

        Image ChangedImage = OpponentSprite.GetComponent<Image>();
        Image TheImage = GameObjectImage.GetComponent<Image>();
        ChangedImage.sprite = TheImage.sprite;
    }
}
