﻿using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {

	Player p1;
	Player p2;

	public Player human1;
	public Player human2;
	public Bot bot;
	
	public Player activePlayer;
	public Player inactivePlayer;

	public GUIText endMessage;
	bool gameOver = false;

	public bool botOpponent;

	public static string GameLog;

	public GUISkin guiSkin;

	public static GameHandler Instance;

	Vector2 scrollPosition = Vector2.zero;
	public bool scrollLock = true;
	public bool debug;
	float logHeight = 0;

	string cardData = "";

	
	/*
	 * 3*16*16 matrix
	 * [context, attacker, defender]
	 * 0 : Challenge not possible, cannot be initiated
	 * 1 : Attacker wins, defender's card is discarded
	 * 2 : Defender wins, attacker's card is discarded
	 * 3 : Both cards are discarded
	 */
	public static int[,,] challengeTable = new int[,,] {{ 
		//Field to Field
		//                            |P|    
		//                            |E|    
		//        |P|       |D|       |R|    
		//        |O|   |C|C|I|   |P|P|S|
		//        |S|A| |Y|E|O| |M|A|E|E|P|S|
		//    |A| |E|P|G|C|N|N|H|E|N|G|P|Y|I|
		//  |Z|R|H|I|O|I|L|T|Y|A|D|D|A|H|T|R|
		//  |E|G|E|D|L|A|O|A|S|D|U|O|S|O|H|E|
		//  |U|U|R|O|L|N|P|U|U|E|S|R|U|N|I|N|
		//  |S|S|O|N|O|T|S|R|S|S|A|A|S|E|A|S|
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //ZEUS
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //ARGUS
			{0,1,3,2,2,2,2,2,0,0,1,3,1,0,1,0}, //HERO
			{0,1,1,3,1,1,1,1,0,0,2,3,1,0,1,0}, //POSEIDON
			{0,1,1,2,3,1,1,1,0,0,2,3,1,0,1,0}, //APOLLO
			{0,1,1,2,2,3,1,1,0,0,2,3,1,0,1,0}, //GIANT
			{0,1,1,2,2,2,3,1,0,0,2,3,1,0,1,0}, //CYCLOPS
			{0,1,1,2,2,2,2,3,0,0,2,3,1,0,1,0}, //CENTAUR
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //DIONYSUS
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //HADES
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //MEDUSA
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //PANDORA
			{0,1,2,2,2,2,2,2,0,0,2,3,3,0,1,0}, //PEGASUS
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //PERSEPHONE
			{0,3,2,1,2,2,2,2,0,0,2,3,2,0,3,0}, //PYTHIA
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}},//SIRENS
		
		   {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},//Hand to Hand
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{3,3,2,2,2,2,2,2,3,3,3,3,3,3,3,3}, //If PEGASUS picks the cards with strength 3-7, opponent places the picked card in a 1st row face up. If there are no field spot to put it, it is discarded.
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{2,2,2,3,2,2,2,2,2,2,2,2,2,2,2,2},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}},
		
		   {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},//Hand to Field
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{3,2,2,2,2,2,2,2,0,0,3,2,3,2,2,2},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}},

		   {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},//Field to Hand
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}}};
	
	// Use this for initialization
	void Awake () {
		Instance = this;
		human1.gameObject.SetActive(true);
		p1 = human1;

		//human2.enabled = false;
		//bot.enabled = false;
		if (botOpponent){
			p2 = bot;
			bot.gameObject.SetActive(true);
			human2.gameObject.SetActive(false);
		}
		else{
			p2 = human2;
			bot.gameObject.SetActive(false);
			human2.gameObject.SetActive(true);
		}
		activePlayer = p2;
		inactivePlayer = p1;
		endMessage.enabled = false;
		GameLog = "Bot Log Initialized";

	}

	void Start(){
		scrollPosition.y = Mathf.Infinity;
		Log("Begin Game");

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			Application.LoadLevel(Application.loadedLevel);
		}
		if (debug){
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit[] hitAll = Physics.RaycastAll(ray);
			
			Card hoverCard = null;
			cardData = "";

			foreach (RaycastHit h in hitAll){
				if (h.transform.tag == "Card"){
					hoverCard = h.transform.GetComponent<Card>();
					cardData = hoverCard.GetData();
				}
			}
		}
		if (gameOver){
			if (Input.GetMouseButtonDown(0)){
				Application.LoadLevel(Application.loadedLevel);
			}
		}
		else{
			if (p1.setupPhase || p2.setupPhase){
				p1.SetupField();
				p2.SetupField();
			}
			else if (activePlayer.actionPoints <= 0){
				SwitchPlayer();
			}
			else activePlayer.CheckInput();

			if (activePlayer.actionPoints > 0){//check to see if the active player has remaining action points and no moves
				if (activePlayer.hand.Count ==0 && activePlayer.drawPile.Count == 0){
					bool canAttack = false;
					foreach (FieldSpot spot in activePlayer.playField){
						if (spot.row == 0 && spot.card != null){
							if (spot.card.type != CardType.MEDUSA && spot.card.type != CardType.PANDORA && spot.card.type != CardType.ZEUS){
								canAttack = true;
							}
						}
					}
					if (!canAttack) EndGame(inactivePlayer);
				}
			}
		}
	}

	public void EndGame(Player winner){
		endMessage.enabled = true;
		endMessage.text = (winner.name + "\nWins!");
		gameOver = true;
	}

	public void SwitchPlayer(){
		if (activePlayer.playField[0,0].card == null && activePlayer.playField[0,1].card == null && activePlayer.playField[0,2].card == null){
			EndGame(inactivePlayer);
		}

		if (activePlayer == p1){
			p2.selectedCard = null;
			activePlayer = p2;
			inactivePlayer = p1;
			p2.BeginTurn();
		}
		else {
			p1.selectedCard = null;
			activePlayer = p1;
			inactivePlayer = p2;
			p1.BeginTurn();
		}

		if (activePlayer.actionPoints < 1){
			EndGame(inactivePlayer);
		}

	}

	public int Challenge(int context, Card attacker, Card defender){
		int result = challengeTable[context, (int)attacker.type, (int)defender.type];
		Debug.Log ("Challenge!" + "\nAttacker: " + attacker + "  Defender: " + defender + "  Resolution: ");
		string challengeLog = ("Challenge! Attacker: " + attacker.name + "  Defender: " + defender.name + "  Resolution: ");

		//SPECIAL CASES
		if (attacker.type == CardType.PYTHIA && context == 1){
			//result = 4;

			Debug.Log("Special Case: Pythia reveals opponent's hand");
			GameHandler.Log("PYTHIA used to reveal opponent's hand");
			activePlayer.actionPoints ++;
			Card target = null;
			activePlayer.phase = MythPhase.PYTHIA;
			foreach (Card c in inactivePlayer.hand){
				if (c.type == CardType.POSEIDON){
					target = c;
					GameHandler.Log("PYTHIA found POSEIDON. POSEIDON is discarded");
				}
				c.Flip(true);
				//c.Reveal(true);

			}
			if (target!=null){
				inactivePlayer.Discard(target);
				activePlayer.Discard(attacker);
			}
		} else if (attacker.type == CardType.PYTHIA && context == 2) {
			Debug.Log("Special Case: Pythia reveals opponent's column");
			GameHandler.Log("PYTHIA used to reveal opponent's column");
			if (inactivePlayer.FindOnField(defender) != null) {
				for (int i = 0; i < 4; i++) {
					FieldSpot spot = inactivePlayer.playField[i, defender.spot.col];
					if (spot.card != null) {
						spot.card.Reveal(true);
					}
				}
				activePlayer.Discard(attacker);
			}
		} else if (defender.type == CardType.PANDORA){
			Debug.Log("Special Case: Pandora is Challenged");
			//if pandora is on the field, discard all from that column
			if (inactivePlayer.FindOnField(defender)){
				GameHandler.Log("PANDORA found. Column is discarded on both sides.");

				int defCol = defender.spot.col;

				while (inactivePlayer.playField[0,defCol].card != null){
					inactivePlayer.Discard(inactivePlayer.playField[0,defCol].card);
				}
				while (activePlayer.playField[0,2-defCol].card != null){
					activePlayer.Discard(activePlayer.playField[0,2-defCol].card);
				}
			}
			
			//if pandora is in hand, discard all in hand
			else if (inactivePlayer.hand.Contains(defender)){
				GameHandler.Log("PANDORA found. Opponent's hand discarded.");
				while(inactivePlayer.hand.Count > 0){
					inactivePlayer.Discard(inactivePlayer.hand[0]);
				}
			}
		} 

		if (defender.type == CardType.ARGUS && result != 0){
			Debug.Log("Special Case: Argus has been attacked");
			EndGame(activePlayer);
		}


		switch(result) {
		case 0:
			Log("Invalid Challenge");
			break;
		case 1:
			challengeLog += (attacker.name + " Wins");
			attacker.Reveal(true);
			inactivePlayer.Discard(defender);
			Log (challengeLog);
			break;
		case 2:
			challengeLog +=(defender.name + " Wins");
			activePlayer.Discard(attacker);
			defender.Reveal(true);
			Log (challengeLog);
			break;
		case 3:
			challengeLog += ("Both Discarded");
			inactivePlayer.Discard(defender);
			activePlayer.Discard(attacker);
			Log (challengeLog);
			break;
		default:
			Debug.Log("Something is very wrong");
			break;
		}

		return result; 
	}

	public static void Log(string message){
		GameLog += "\n" + message;
		//GameLog.Insert(GameLog.Length, "\n" + message);
	}

	public void EndPythiaPhase(){
		foreach (Card c in inactivePlayer.hand){
			c.Flip(inactivePlayer.showHand);
			//Debug.Log(inactivePlayer.showHand);
		}
		Player.Shuffle(inactivePlayer.hand);
		inactivePlayer.ArrangeHand();
		//inactivePlayer
	}

	
	public GameState GetState(){
		GameState state = new GameState();
		state.Build(activePlayer, inactivePlayer);
//		Debug.Log(activePlayer);
		return state;
	}

	void OnGUI(){
		GUI.skin = guiSkin;

		//GUI.Box(new Rect(Screen.width - Screen.width/3,0,Screen.width/3,Screen.height), GameLog);

		//scrollPosition = GUI.BeginScrollView(new Rect(Screen.width - Screen.width/3,0,Screen.width/3 -20,Screen.height-20), scrollPosition, new Rect(0,0,200,200));

		//scrollPosition = 
		GUI.BeginGroup(new Rect(Screen.width - Screen.width/3 -10, 10, Screen.width/3, Screen.height));
		if (GUI.Button(new Rect(Screen.width/3 - 120, 300, 100, 20), "Scroll Lock")){
			scrollLock = !scrollLock;
		}
		if (GUI.Button(new Rect(Screen.width/3 - 230, 300, 100, 20), "Hover Debug")){
			debug = !debug;
		}
		if (GUI.Button(new Rect(Screen.width/3 - 340, 300, 100, 20), "RESET")){
			Application.LoadLevel(Application.loadedLevel);
		}
		if (scrollLock) {
			scrollPosition.y = Mathf.Infinity;
			GUI.BeginScrollView(	new Rect(0,0, Screen.width/3, 300), 
			                    	scrollPosition, 
			                    	new Rect(0, 0, Screen.width/3 - 20, guiSkin.box.CalcHeight(new GUIContent(GameLog), Screen.width/3 - 20) + 10));
		}
		else{
			scrollPosition = 
			GUI.BeginScrollView(	new Rect(0,0, Screen.width/3, 300), 
		                       		scrollPosition, 
		                       		new Rect(0, 0, Screen.width/3 - 20, guiSkin.box.CalcHeight(new GUIContent(GameLog), Screen.width/3 - 20) + 10));
		}
			GUI.Box(new Rect(0, 0, Screen.width/3 - 20, Mathf.Max((guiSkin.box.CalcHeight(new GUIContent(GameLog), Screen.width/3 - 20)+5), 295)), GameLog);
			GUI.EndScrollView();
			
		if (debug){
			GUI.Box(new Rect(0, 330, Screen.width/3 - 20, 300), cardData);
		}

		GUI.EndGroup();
	}
}
