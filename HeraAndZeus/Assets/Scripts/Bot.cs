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
		if (Input.GetKeyDown(KeyCode.H)){
			Debug.Log("H");
			Shuffle(hand);
			ArrangeHand();
		}
		base.Update();
		if (delay >= 0)	delay -= Time.deltaTime;
		else delay = 0;
	}

	public override void CheckInput(){
		if (delay == 0){
			ExecuteMove(PickMove());
			Shuffle(hand);
			ArrangeHand();
			delay = 2;
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
		Shuffle(posMoves);
		Move bestMove = posMoves[0];
		float bestVal = 0;
		foreach(Move m in posMoves){
			float moveVal = state.EvaluateMove(m);
			if (moveVal > bestVal){
				bestVal = moveVal;
					bestMove = m;
			}
		}
		return bestMove;

	}

	void ExecuteMove(Move m){
		switch(m.type){
		case MoveType.DRAW: //COMPLETE
			Draw ();
			GameHandler.Log(name + " draws a card ");

			break;
		case MoveType.PLAY: //COMPLETE
			Play (m.playCard, m.targetSpot);
			GameHandler.Log(name + " plays a card");

			break;
		case MoveType.CHALLENGE: //COMPLETE
			int context = 3;
			if (m.attacker.spot != null && m.defender.spot != null && m.attacker.spot.col == (2 -m.defender.spot.col)) {
				context = 0;
			} else if (hand.Contains(m.attacker) && m.defender.owner.hand.Contains(m.defender)) {
				context = 1;
				if (m.attacker.type == CardType.PEGASUS && m.defender.strength < 8 && m.defender.strength > 1) {
					//m.type = MoveType.PEGASUS;
					bool success = false;
					int i = Random.Range((int) 0, (int) 3);
					int count = 0;
					while (count < 3) {
						if (playField[3,i].card == null && (playField[0,i].card != null && playField[0,i].card.type != CardType.ZEUS) && !(m.defender.type == CardType.ZEUS && playField[0,i].card == null)) {
							m.defender.owner.Play(m.defender, m.defender.owner.playField[0,i]);
							m.defender.owner.actionPoints++;
							success = true;
							break;
						}

						if (i > 3)
							i = 0;
						count++;
					}

					if (!success) {
						m.defender.owner.Discard(m.defender);
					}
				}
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
		case MoveType.MYTH:
			switch(m.playCard.type){
			case CardType.HADES: //COMPLETE
				if (discardPile.Contains(m.hadesCard)){
					discardPile.Remove(m.hadesCard);
					hand.Add(m.hadesCard);
					m.hadesCard.Flip(showHand);
					m.hadesCard.Reveal(false);
					Discard(m.playCard);
					ArrangeHand();
					GameHandler.Log(name + " uses HADES to reclaim " + m.hadesCard.name);
					actionPoints --;
				}
				break;
			case CardType.DIONYSUS: //COMPLETE
				Discard(m.playCard);
				m.dionysusCard.spot.card = null;
				m.dionysusCard.spot = null;
				Play (m.dionysusCard, m.targetSpot);
				GameHandler.Log(name + " uses DIONYSUS to move " + m.dionysusCard.name);
				break;
			case CardType.PERSEPHONE: //COMPLETE
				int numPeg = 0;
				int index = 0;
				//Debug.Log("here2");

				while(index < discardPile.Count && numPeg < 3){
					if (discardPile[index].type == CardType.PEGASUS){
						Card peg = discardPile[index];
						hand.Add(peg);
						discardPile.Remove(peg);
						numPeg ++;
						Debug.Log(numPeg);
					}
					else
						index++;
				}
				Discard(m.playCard);
				ArrangeHand();
				GameHandler.Log(name + " plays PERSEPHONE to reclaim " + numPeg + " PEGASUS");
				actionPoints --;
				break;
			case CardType.PYTHIA: //COMPLETE
				//Pythia is always handled with challenges.
				break;
			case CardType.SIRENS: //COMPLETE
				Card taken = new Card();
				bool found = false;
				int i = GameHandler.Instance.inactivePlayer.drawPile.Count-1;
				while(i >= 0 && !found){
					if (GameHandler.Instance.inactivePlayer.drawPile[i].strength > 0){
						taken = GameHandler.Instance.inactivePlayer.drawPile[i];
						found = true;
					}
					i--;
				}
				if (found){
					GameHandler.Instance.inactivePlayer.discardPile.Remove(taken);
					hand.Add(taken);
					GameHandler.Log(name + " plays " + m.playCard.name + " and takes " + taken.name);
					actionPoints --;
				}
				break;
			}
			break;
			//GameHandler.Log(name + " plays " + m.playCard.name);
		}
		GameHandler.Log(name + " Action Points: " + actionPoints);
	}
	
}
