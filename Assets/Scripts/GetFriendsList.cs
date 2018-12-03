﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System.Text;
using ServerManager;
using Navigator;
using static SocketPasser;

public class GetFriendsList : MonoBehaviour {
	public ScrollRect scrollView;
	public GameObject Friend_Item;
	public GameObject ScrollViewContent;
    public GameObject FriendUsername;
    public Button ChallengeButton;
    string userId;

	public Sprite Online_Icon;

	// Use this for initialization
	void Start () {
		string data = File.ReadAllText(Application.persistentDataPath + "/MyInfo.json");
		UserInfo playerinfo = JsonUtility.FromJson<UserInfo>(data);
		string username = playerinfo.getUsername();
        userId = playerinfo.getUserId();
		getFriends(username);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//get the friends list data from the server
	public void getFriends(string username) {
		ConnectionManager CM = new ConnectionManager();
		if (CM.StartClient() == 1) {
			//the server spits back comma separated string of friends
			string response = CM.getFriendsList(username);
			
			//split the response by comma
			string[] friendsList = response.Split(','); 
			
			foreach(var friend in friendsList) {
				Debug.Log(friend);
			}

			//update UI method
			fillFriends(friendsList);

			//scrollView.verticalNormalizedPosition = 1;
		} else {
			Debug.Log("Connection Manager start client failed.");
		}

		//testing
		// string response = "Steve,Jason,Illya,Nick";
		// string[] friendsList = response.Split(',');
		// fillFriends(friendsList);
	}

	//update the UI with the friends list
	public void fillFriends(string[] friendsList) {
		ConnectionManager CM = new ConnectionManager();
		if (CM.StartClient() == 1) {
			string onlineUsersList = CM.GetOnlineUsers();
			Debug.Log(onlineUsersList);
			string[] onlineUsers = onlineUsersList.Split(',');

			foreach (var online in onlineUsers) {
				Debug.Log(online.Length);
			}

			foreach(var friend in friendsList) {
				Debug.Log(friend.Length);
				if (friend.Length > 0 & friend.Length < 46){
					Debug.Log(friend);

					//instantiate a Friend_Item prefab, set the parent to scrollview's content, and change the text to friend var from friendsList
					GameObject friendObject = Instantiate(Friend_Item);
					friendObject.transform.SetParent(ScrollViewContent.transform, false);
					friendObject.transform.Find("Friend_Name").gameObject.GetComponent<Text>().text = friend;
					friendObject.tag = "Friend_Item";

					foreach (var online in onlineUsers) {
						if (online.Equals(friend)) {
							Debug.Log("Online user found!");
							friendObject.transform.Find("Friend_Image").gameObject.GetComponent<Image>().sprite = Online_Icon;
							Button challengeFriendButton = friendObject.transform.Find("ChallengeButton").gameObject.GetComponent<Button>();
						challengeFriendButton.onClick.AddListener(delegate {ChallengeFriend(friend); });
						}
					}
				}
			}
		}
	}
    //Called when challenge button is pressed and sends Challenger's userid, challengee's username and message "Challenge Message" to the backend
    public void ChallengeFriend(string friendUsername)
    {
        string[] param = new string[3];
        //string friendUsername = FriendUsername.GetComponent<Text>().text;
        param[0] = userId;
        param[1] = friendUsername;
        param[2] = "Challenge Message";


        Debug.Log("This is the Username of the challenged player: " + friendUsername);
        ConnectionManager CM = new ConnectionManager();

        if (CM.StartClient() == 1)
        {
            string[] response = CM.ChallengeFriend(param);
            Debug.Log(response[4]);
			SocketPasser.setCM(CM);
        }

		SceneNavigator navi = new SceneNavigator();
        navi.GoToScene("GameScreen_Friend");
        
    }
}
