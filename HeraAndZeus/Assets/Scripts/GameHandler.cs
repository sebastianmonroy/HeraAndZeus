﻿using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {

	public Player p1;
	public Player p2;
	
	Player activePlayer;
	Player inactivePlayer;

	public static GameHandler Instance;

	/*
	 * 3*16*16 matrix
	 * [context, attacker, defender]
	 * 0 : Challenge not possible, cannot be initiated
	 * 1 : Attacker wins, defender's card is discarded
	 * 2 : Defender wins, attacker's card is discarded
	 * 3 : Both cards are discarded
	 */
	static int[,,] challengeTable = new int[,,] {{ 
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
			{0,3,2,2,2,2,2,2,0,0,2,3,2,0,3,0}, //PYTHIA
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
			{0,3,2,2,2,2,2,2,3,3,3,3,3,3,3,3}, //If PEGASUS picks the cards with strength 3-7, opponent places the picked card in a 1st row face up. If there are no field spot to put it, it is discarded.
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0},
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
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}}};
	
	// Use this for initialization
	void Start () {
		Instance = this;
		activePlayer = p2;

		p1.actionPoints = 12;
		p2.actionPoints = 12;
	}
	
	// Update is called once per frame
	void Update () {
		if (p1.setupPhase || p2.setupPhase){
			p1.SetupField();
			p2.SetupField();
		}
		else if (activePlayer.actionPoints <= 0)
			SwitchPlayer();
		else activePlayer.CheckInput();
	}

	public void EndGame(){

	}

	public void SwitchPlayer(){
		if (activePlayer == p1){
			activePlayer = p2;
			inactivePlayer = p1;
			p2.BeginTurn();
		}
		else {
			activePlayer = p1;
			inactivePlayer = p2;
			p1.BeginTurn();
		}

	}

	public int Challenge(int context, Card attacker, Card defender){
		int result = challengeTable[context, (int)attacker.type, (int)defender.type];
		Debug.Log ("Challenge!" + "\nAttacker: " + attacker + "  Defender: " + defender + "  Resolution: ");
		switch(result) {
		case 0:
			Debug.Log("Invalid Challenge");
			break;
		case 1:
			Debug.Log(attacker + " Wins");
			inactivePlayer.Discard(defender);
			break;
		case 2:
			Debug.Log(defender + " Wins");
			activePlayer.Discard(attacker);
			break;
		case 3:
			Debug.Log("Both Discarded");
			inactivePlayer.Discard(attacker);
			activePlayer.Discard(defender);
			break;
		
		default:
			Debug.Log("Something is very wrong");
			break;
		}

		//SPECIAL CASES
		if (defender.type == CardType.PANDORA){
			foreach (FieldSpot spot in inactivePlayer.playField){
				if (spot.col = defender)
			}
		}

		return result;
	}
}
