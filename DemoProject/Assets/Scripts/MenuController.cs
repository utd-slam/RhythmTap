﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml;
using System;

public class MenuController : MonoBehaviour {
	public static float bpm = 55;
	public static bool impromptu;
	public static int gameNum;
	public static bool debug = false;

	public Text Username_field;
	public static AudioClip audioClip;
	public InputField inputField;
	public Dropdown bpmDP;
	public Dropdown rhythmDP;
	public Sprite quarterNote;
	public Sprite eighthNotes;
	public Sprite emptySlot;
	public Image[] notes;
	public InputField[] words;

	public static string rhythm = "";
	public static string phrase;
	public static int[] displayOrder;
	public static int displayOrderLen;
	private int noteCount = 0;

	// Use this for initialization
	void Start () {
		bpm = 30;
		phrase = "";
		displayOrder = new int[6];
		displayOrderLen = 0;
		rhythm = "";
		noteCount = 0;
	}

	public void StartGame(){
		switch (bpmDP.value) {
		case 0:
			bpm = 60;
			break;
		case 1:
			bpm = 90;
			break;
		default:
			bpm = 120;
			break;
		}

		switch (rhythmDP.value) {
		case 0:
			phrase = "Yummy Food";
			rhythm = "8n 8n 4n 4r 4r";
			break;
		case 1:
			phrase = "Help Me";
			rhythm = "4n 4n 4r 4r";
			break;
		default:
			phrase = "Like Seeing You";
			rhythm = "4n 8n 8n 4n 4r";
			break;
		}
			
		impromptu = false;
		DrumController.numCycles = 0;  

		SceneManager.LoadScene("Instructions");
	}

	/* Called by DBScript NOT IN USE*/
	/*
	public static void StartGame(string r, string p, int b){
		string[] rSplit = r.Replace("8n 8n", "8n8n").Split(' ');
		string[] pSplit = p.Split(' ');

		for (int i = 0; i < rSplit.Length; i++){
			phrase [i] = pSplit [i];
			displayOrder [displayOrderLen++] = i;

			if (rSplit[i].Equals("8n8n")) {
				displayOrder [displayOrderLen++] = i;
			}
		}
		//gameNum = rSplit.Length == 2 ? 1 : 0;
		gameNum = rSplit.Length-1;
		bpm = b;
		rhythm = r + " 4r";
		impromptu = false;
		SceneManager.LoadScene("MainScene");
	}*/

    public void QuitGame(){
		Application.Quit();
	}

	public void BackToWelcome(){
		SceneManager.LoadScene("Welcome Scene");
	}

	public void StartMenu(){

		SceneManager.LoadScene("Menu");
	}
	public void StartMenu1(){
		
		SceneManager.LoadScene("RhythmSelection");
	}
	public void AddRhythmsScene(){
		SceneManager.LoadScene("Menu");
	}
	public void EnableDebug(){
		debug = true;
	}

	public void onValueChanged(){

	}

	public void RestartSession(){
		Debug.Log (bpm);
		Debug.Log (rhythm + phrase);
		impromptu = false;
		DrumController.numCycles = 0;  

		SceneManager.LoadScene("Instructions");
	}
}
