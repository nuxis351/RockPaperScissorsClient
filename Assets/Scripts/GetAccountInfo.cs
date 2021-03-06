﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System.Text;
using ServerManager;
using UnityShortCuts;
public class GetAccountInfo : MonoBehaviour {

	// Use this for initialization
	string user_id;
	ShortCuts usc;
	string data;
	UserInfo playerinfo;
	void Start() 
	{
		usc = new ShortCuts(); 
		data = File.ReadAllText(Application.dataPath + "/MyInfo.json");
		Debug.Log(data);
		playerinfo = JsonUtility.FromJson<UserInfo>(data);
		Debug.Log(playerinfo.getFirstName());
		string param = playerinfo.getUsername();
		user_id = playerinfo.getUserId();
		usc.updateInputValue("profileUserName", param);
		string[] tagNames = {"profileScore", "profileCurrency", "profileWins", "profileLosses"};
		string[] userParams = {playerinfo.getScore(), playerinfo.getCurrency(), playerinfo.getWins(), playerinfo.getLosses()};
		usc.updateTextValue(tagNames, userParams);
	}
	public void UpdateUserInfo () 
	{
		ConnectionManager CM = new ConnectionManager();
		int connectionResult = CM.StartClient();
		Debug.Log(connectionResult);
		string new_username = usc.InputValue("profileUserName");
		string[] param = {user_id, new_username};
		string updateResult = (CM.UpdateAccountInfo(param)).Trim();

		try
		{
			if(Convert.ToInt32(updateResult) == 1)
			{
				playerinfo.setUsername(new_username);
				Debug.Log(playerinfo.getUsername());
				string json = JsonUtility.ToJson(playerinfo);
        		StreamWriter sw = File.CreateText(Application.dataPath + "/MyInfo.json");
        		sw.Close();
        		File.WriteAllText(Application.dataPath + "/MyInfo.json", json);
			}
			else
			{
				Debug.Log(updateResult);
			}
		} 
		catch (OverflowException error) 
		{
			Debug.Log(updateResult);
		} 
		
	}

	// Update is called once per frame
	void Update () {
		
	}
}
