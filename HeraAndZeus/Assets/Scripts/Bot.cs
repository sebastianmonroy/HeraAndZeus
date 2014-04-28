using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;
using MicrosoftResearch.Infer.Maths;
using MicrosoftResearch.Infer.Distributions;



public class Bot : Player {
	GameState curState;
	float delay;
	// Use this for initialization
	new void Start () {
		base.Start();
		delay = 3;
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update();
		if (delay >= 0)	delay -= Time.deltaTime;
		else delay = 0;
	}

	public override void CheckInput(){
		if (delay == 0){
			ExecuteMove(PickMove());
			delay = 1;
		}
	}
	

	public override void SetupField(){
		if (setupPhase && delay < 0) {
			BotSetupField();
		}
	}


	public override void BeginTurn(){
		curState = GameHandler.Instance.GetState();
		if (FindOnField(CardType.ZEUS) != null) {
			actionPoints = 4;
		} else {
			actionPoints = NumOccupiedColumns();
		}
		delay = 1;
	}

	void BotSetupField(){
		List<Card> sortedHand = new List<Card>();
		foreach (Card c in hand){
			if (sortedHand.Count == 0) sortedHand.Add(c);
			else{
				if (c.type == CardType.MEDUSA) sortedHand.Insert(0, c);
				else{
					int index = 0;
					while(index < sortedHand.Count && c.strength < sortedHand[index].strength){
						index++;
					}
					sortedHand.Insert(index, c);
				}
			}

		}

		Play(sortedHand[0], playField[0,0]);
		Play(sortedHand[1], playField[0,1]);
		Play(sortedHand[2], playField[0,2]);
		Debug.Log(sortedHand[0] + " " + sortedHand[1] + " " + sortedHand[2]);

		setupPhase = false;
	}

	Move PickMove(){
		GameState state = GameHandler.Instance.GetState();
		List<Move> posMoves = state.GetMoves();
		return posMoves[Random.Range(0,posMoves.Count)];

	}

	void ExecuteMove(Move m){
		switch(m.type){
		case MoveType.DRAW:
			Draw ();
			break;
		case MoveType.PLAY:
			Play (m.playCard, m.targetSpot);
			break;
		case MoveType.CHALLENGE:
			int context = 3;
			if (m.attacker.spot != null && m.defender.spot != null && m.attacker.spot.col == (2 -m.defender.spot.col)) {
				context = 0;
			} else if (hand.Contains(m.attacker) && m.defender.owner.hand.Contains(m.defender)) {
				context = 1;
			} else if (hand.Contains(m.attacker) && m.defender.spot != null && m.defender.spot.row == 0) {
				context = 2;
			}
			
			int resolution = GameHandler.Instance.Challenge(context, m.attacker, m.defender);
			
			if (resolution != 0) {
				m.defender.Reveal(true);
				m.attacker.Reveal(true);
				actionPoints --;
			}
			break;
		}
	}
	
}
