﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using Assets.Scripts.TextureStorage;

public class DrumController : MonoBehaviour {
	private const float NaN = 0.0f / 0;
	public float bpm = 0f; //default

	public AudioSource Clave;
	public AudioSource WoodBlock;
	public AudioSource[] voices_60bpm;
	AudioSource voice;

    //public Text scoreText;
    public Text countdownText;
	public Text promptText;
	public Text[] phraseSections;
	public Image[] phraseBackgrounds;
	public GameObject microphone;
	public GameObject drum;
	//bool analyzed = false;

	public List<float> keyDownList;
	public List<float> stdList;
	public List<float> tickList;
	public List<float> halfNotes;

	bool hasEnded = false;

	float launchTime = 0.0f;
	float nextMetronomeBeat;
	float dspTime;
	static float lengthOfAudio;
	float error = 0.1f;
	float beat = 0;

	public static int TNBText = 8;
	public static float OnsetScoreText = 0.0f;
	public static int FAText = 0;
	public static int NOHText = 0;
	public static int NOMText = 0;

	private static EndScreenController endScreenController;
	private float endScreenTime;

	int stdListCounter = 0;
	int tickListCounter = 0;

	int numCycles = 0;
	static int MAX_CYCLES = 4;
	float offset = 0f;
	bool audioPlayed = false;
	bool micActive = false;
	AudioClip[] audioClip = new AudioClip[MAX_CYCLES];


	void Start () {
		/* Initialize variables */

		// Scoring variables
		OnsetScoreText = 0.0f;
		FAText = 0;											// Number of false alarms
		NOHText = 0;										// Number of hits
		NOMText = 0;										// Number of misses

		// Time-related variables
		bpm = (float)MenuController.bpm;
		lengthOfAudio = 60.0f;								// In seconds
		beat = 60.0f/bpm;									// Duration of each beat
		nextMetronomeBeat = (float)(launchTime + beat);
		launchTime = Time.timeSinceLevelLoad; 				// Time the game starts

		// Other variables
		countdownText.text = "";
		keyDownList = new List<float> ();
	

		/* Load but hide end screen */
		endScreenController = new EndScreenController ();
		endScreenController.Disable ();
		endScreenTime = 0.0f;


		/* Avoid unnecessary logs when debugging*/
		if(MenuController.debug == false)
			LogManager.Instance.LogSessionStart (bpm, MenuController.gameNum);


        /* Load rhythms if in rhythmic */
		if (!DBScript.arrhythmicMode) {
			RhythmLoader rhythmLoader = new RhythmLoader ();
			rhythmLoader.LoadRhythm (MenuController.rhythm, bpm, lengthOfAudio);
			stdList = rhythmLoader.GetRhythmTimes ();
			tickList = rhythmLoader.GetTickTimes ();
		} else {
			stdList = new List<float> ();
			tickList = new List<float> ();
		}

		/* Populate phrase prompt */
		/* NOT IN USE */
		/*string[] phrase = MenuController.phrase;
		for (int i = 0; i < phrase.Length; i++) {
			if (phrase [i] == null || phrase [i].Equals (""))
				phraseBackgrounds [i].enabled = false;
			else
				phraseSections [i].text = phrase [i];
		}*/

		/* Rearrange drum and microphone sprites */
		if (!DBScript.arrhythmicMode) {
			Vector3 micPos = microphone.transform.position;
			Vector3 drumPos = drum.transform.position;
			microphone.transform.position = new Vector3(-2, micPos.y, 0);
			drum.transform.position = new Vector3(2, drumPos.y, 0);
		}

		/* Shuffle voice prompts for randomization */
		System.Random rnd = new System.Random();
		voices_60bpm = voices_60bpm.OrderBy(x => rnd.Next()).ToArray();  
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Q)) {
			SceneManager.LoadScene ("Menu");
		}

		if (MenuController.impromptu == false) {
			UpdateRegularPlayMode ();
		}
	}

	void UpdateRegularPlayMode(){
		if (numCycles < MAX_CYCLES) {
			if (Time.timeSinceLevelLoad - launchTime - offset > beat * 8) {
				microphone.SetActive (false);
				if(!DBScript.arrhythmicMode) drum.SetActive (false);

				numCycles++;
				offset = numCycles * beat * 8;

				audioPlayed = false;
				return;
			} else if (Time.timeSinceLevelLoad - launchTime - offset > beat * 6) {
				countdownText.text = "";
				microphone.SetActive (true);
				if(!DBScript.arrhythmicMode) drum.SetActive (true);
			} else if (Time.timeSinceLevelLoad - launchTime - offset > beat * 2) {
				UpdateCountDownText ();
			} else {
				if (micActive) {
					Microphone.End (Microphone.devices[0]);
					micActive = false;
				}

				if (!audioPlayed) {
					voice = voices_60bpm [numCycles];
					voice.Play ();
					audioPlayed = true;
				}
			}
		} else {
			EndPlayingSession ();
		}


		if(!DBScript.arrhythmicMode) UpdateDrumPrompt ();

		if (hasEnded){
			if (endScreenTime > 0.0f) {
				float temp = Time.timeSinceLevelLoad;
				float diff = temp - endScreenTime;
				if (diff > 1.0f) {
					LoadAnalysis ();
				}
			}
		}
	}

	//analizing timestamps after finishing the song
	void PerformanceAnalysis(){
		TNBText = 0; //TODO: Temporarily set to zero
		int stdListIndex = 0; //index for stdList
		int listIndex = 0; //index for list

		while (stdListIndex < TNBText && listIndex < keyDownList.Count) {
			float upper = stdList [stdListIndex] + error;
			float lower = stdList [stdListIndex] - error;
			if (keyDownList [listIndex] < upper && keyDownList [listIndex] > lower) {
				if (MenuController.debug == false) {
					LogManager.Instance.Log (stdList [stdListIndex], keyDownList [listIndex], stdListIndex);
				}
					
				NOHText++;
				stdListIndex++;
				listIndex++;
			} else if (keyDownList [listIndex] > upper) {
				if(MenuController.debug == false)
					LogManager.Instance.Log (stdList [stdListIndex], NaN, stdListIndex);
				stdListIndex++;
			} else {
				if(MenuController.debug == false)
					LogManager.Instance.Log (NaN, keyDownList [listIndex], -1);
				listIndex++;
				FAText++;
			}
		}

		//log remaining data
		while (stdListIndex < TNBText) {
			if(MenuController.debug == false)
				LogManager.Instance.Log (stdList [stdListIndex], NaN, stdListIndex);
			stdListIndex++;
		}

		while (listIndex < keyDownList.Count) {
			if(MenuController.debug == false)
				LogManager.Instance.Log (NaN, keyDownList [listIndex], -1);
			listIndex++;
			FAText++;
		}

		NOMText = TNBText - NOHText;
		OnsetScoreText = (float)(NOHText * 100) / TNBText;

		if (MenuController.debug == false) {
			LogManager.Instance.Log ("OnsetScore", OnsetScoreText.ToString ());
			LogManager.Instance.Log ("NumberOfHits", NOHText.ToString ());
			LogManager.Instance.Log ("NumberOfMisses", NOMText.ToString ());
			LogManager.Instance.Log ("NumberOfFalseAlarms", FAText.ToString ());
			LogManager.Instance.Log ("TotalNumberOfBeats", TNBText.ToString ());
		}
	}

	void UpdateDrumPrompt(){
		if (stdListCounter < stdList.Count && Time.timeSinceLevelLoad >= launchTime + stdList [stdListCounter]) {
			WoodBlock.Play ();
			stdListCounter++;
		}
		if (tickListCounter < tickList.Count && Time.timeSinceLevelLoad >= launchTime + tickList [tickListCounter]) {
			Clave.Play ();
			tickListCounter++;
		}
	}

	void UpdateCountDownText(){
		if (Time.timeSinceLevelLoad - launchTime - offset > beat * 5) {
			countdownText.text = "Go!";
			if (!micActive) {
				audioClip[numCycles] = Microphone.Start (Microphone.devices[0], false, 6, 44100);
				micActive = true;
			}
		} else if (Time.timeSinceLevelLoad - launchTime - offset > beat * 4) {
			countdownText.text = "1";
		} else if (Time.timeSinceLevelLoad - launchTime - offset > beat * 3) {
			countdownText.text = "2";
		} else if (Time.timeSinceLevelLoad - launchTime - offset > beat * 2) {
			countdownText.text = "3";
		}
	}

	void UpdateKeyDown(){
		//add timestamp to the list
		keyDownList.Add (Time.timeSinceLevelLoad - launchTime);
	}

	void EndPlayingSession(){
		hasEnded = true;

		endScreenController.Enable ();
		endScreenTime = Time.timeSinceLevelLoad;

		for (int i = 0; i < MAX_CYCLES; i++) {
			string filename = WelcomeController.name + i + "_" +
				DateTime.Now.Month.ToString() + "_" + 
				DateTime.Now.Day.ToString() + "_" + 
				DateTime.Now.Hour.ToString() + "_" + 
				DateTime.Now.Minute.ToString();
			SavWav.Save (filename, audioClip[i]);
		}
		/**if (analyzed == false) {
			PerformanceAnalysis ();
			analyzed = true;
		}*/
		SceneManager.LoadScene ("RhythmSelection");
	}

	void LoadAnalysis(){
		SceneManager.LoadScene ("Analysis");
	}
}