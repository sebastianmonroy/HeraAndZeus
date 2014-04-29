using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;



public enum MoveType{PLAY, CHALLENGE, DRAW, MYTH}

public struct Move{
	public MoveType type;
	public FieldSpot targetSpot;
	public Card playCard;
	public Card dionysusCard;
	public Card hadesCard;
	public Card attacker;
	public Card defender;

	public override string ToString()
	{
		if (type == MoveType.DRAW){
			return ("TYPE: " + type);
		}
		else if (type == MoveType.PLAY){
			return ("TYPE: " + type + 
			        "\nplayCard: " + playCard +
			        "\ntargetSpot: " + targetSpot);
		}
		else if (type ==  MoveType.CHALLENGE){
			return ("TYPE: " + type + 
					"\nAttacker " + attacker +
		      	  	"\nDefender " + defender);
		}
		else if (type ==  MoveType.MYTH){
			return ("TYPE: " + type + 
			        "\nplayCard: " + playCard +
			        "\nDionysusCard " + dionysusCard +
			        "\nHadesCard" + hadesCard);

		}
		return ("TYPE: " + type);

	}

}


public class GameState{
	FieldSpot[,] myField;
	FieldSpot[,] otherField;
	Card[] myHand;
	Card[] otherHand;
	Card[] myDrawPile;
	Card[] myDiscardPile;
	Card[] otherDrawPile;

	
	public void Build(Player p1, Player p2){
		myField = new FieldSpot[4,3];
		otherField = new FieldSpot[4,3];

		foreach (FieldSpot spot in p1.playField){
			myField[spot.row, spot.col] = spot;
		}
		foreach (FieldSpot spot in p2.playField){
			otherField[spot.row, spot.col] = spot;
		}

		otherHand = p2.hand.ToArray();
		myHand = p1.hand.ToArray();
		myDrawPile = p1.drawPile.ToArray();
		myDiscardPile = p1.discardPile.ToArray();
		otherDrawPile = p2.drawPile.ToArray();
	}

	public void Build(GameState original){
		myField = (FieldSpot[,])original.myField.Clone();
		otherField = (FieldSpot[,])original.otherField.Clone();
		myHand = (Card[])original.myHand.Clone();
		otherHand = (Card[])original.otherHand.Clone();
		myDrawPile = (Card[])original.myDrawPile.Clone();
		myDiscardPile = (Card[])original.myDiscardPile.Clone();
		otherDrawPile = (Card[])original.otherDrawPile.Clone();
	}

	public float Eval(){
		return 0;
	}

	public float EvaluateMove(Move move){
		float moveVal = 0;
		switch(move.type){
		case MoveType.DRAW:
			float[] drawProbs = myDrawPile[0].getPredictionProbabilities();
			for (int i = 0; i< drawProbs.Length; i++){
				moveVal += drawProbs[i] * Card.subjectiveValues[i];
			}
			break;
		case MoveType.PLAY: 
			//IF THE TARGET OF THE PLAY IS THE FIRST ROW, TREAT IT LIKE A CHALLENGE
			if (move.targetSpot.row == 0 && otherField[0, 2- move.targetSpot.col].card != null){
				Card otherCard = otherField[0, 2- move.targetSpot.col].card;
				//IF THE OPPONENTS FIRST ROW CARD IS KNOWN
				if (otherCard.revealed){
					float challengeVal = 0;

					int context = 0;
					
					int result = GameHandler.challengeTable[context, (int)move.playCard.type, (int)otherCard.type];
					
					switch(result){
					case(0):
						challengeVal +=0;
						break;
					case(1):
						challengeVal += Card.subjectiveValues[(int)otherCard.type];
						break;
					case(2):
						challengeVal -= Card.subjectiveValues[(int)move.playCard.type];
						break;
					case(3):
						challengeVal += Card.subjectiveValues[(int)otherCard.type];
						challengeVal -= Card.subjectiveValues[(int)move.playCard.type];
						break;
					default:
						break;
					}

					moveVal += challengeVal;
				}
				//IF THE OPPONENTS FIRST ROW CARD IS NOT KNOWN
				else{
					float[] probs = otherField[0, 2- move.targetSpot.col].card.getPredictionProbabilities();
					for (int i = 0; i< probs.Length; i++){
						float challengeVal = 0;
						int context = 0;

						int result = GameHandler.challengeTable[context, (int)move.playCard.type, i];
						
						switch(result){
						case(0):
							challengeVal +=0;
							break;
						case(1):
							challengeVal += Card.subjectiveValues[i];
							break;
						case(2):
							challengeVal -= Card.subjectiveValues[(int)move.playCard.type];
							break;
						case(3):
							challengeVal += Card.subjectiveValues[i];
							challengeVal -= Card.subjectiveValues[(int)move.playCard.type];
							break;
						default:
							break;
						}
						moveVal += probs[i] * challengeVal;
					}
				}
			}
			else moveVal = 1;

			break;
		case MoveType.CHALLENGE:
			float[] probs = move.defender.getPredictionProbabilities();
			//need the chance it being a certain card, the subvalue of that card, and the resolution of a challenge
			//for each possible defender, represented by i i
			for (int i = 0; i< probs.Length; i++){
				float challengeVal = 0;
				int context = 3;
				if (move.attacker.spot != null && move.defender.spot != null && move.attacker.spot.col == (2 -move.defender.spot.col)) {
					context = 0;
				} else if (Array.IndexOf(myHand, move.attacker)>-1 &&  Array.IndexOf(otherHand, move.defender)>-1) {
					context = 1;
				} else if (Array.IndexOf(myHand, move.attacker)>-1 && move.defender.spot != null && move.defender.spot.row == 0) {
					context = 2;
				}
				int result = GameHandler.challengeTable[context, (int)move.attacker.type, i];

				switch(result){
				case(0):
					challengeVal +=0;
					break;
				case(1):
					challengeVal += Card.subjectiveValues[i];
					break;
				case(2):
					challengeVal -= Card.subjectiveValues[(int)move.attacker.type];
					break;
				case(3):
					challengeVal += Card.subjectiveValues[i];
					challengeVal -= Card.subjectiveValues[(int)move.attacker.type];
					break;
				default:
					break;
				}
				moveVal += probs[i] * challengeVal;
			}

			break;
		case MoveType.MYTH:
			switch(move.playCard.type){
			case CardType.HADES: 
				moveVal = Card.subjectiveValues[(int)move.hadesCard.type];
				break;
			case CardType.DIONYSUS:
				//CALCULATE LIKE PLAY
				moveVal = 3;
				break;
			case CardType.PERSEPHONE:
				int numPeg = 0;
				foreach(Card c in myDiscardPile){
					if (c.type ==  CardType.PEGASUS){
						numPeg++;
					}
				}
				if (numPeg > 3) numPeg = 3;
				moveVal = numPeg * Card.subjectiveValues[(int)CardType.PEGASUS];
				break;
			case CardType.PYTHIA:
				moveVal = 6;
				break;
			case CardType.SIRENS: 
				moveVal = Card.subjectiveValues[(int) otherDrawPile[otherDrawPile.Length-1].type];
				break;
			}
			break;
		}

		return moveVal;
	}

	public List<Move> GetMoves(){
		List<Move> possibleMoves = new List<Move>();

		//drawing is always an option if the draw pile is not empty
		if (myDrawPile.Length > 0){
			Move draw = new Move();
			draw.type = MoveType.DRAW;
			possibleMoves.Add(draw);
		}

		//each numbered card in the hand can be played on the field
		foreach (Card c in myHand){
			if (c.strength>-1){
				foreach (FieldSpot spot in myField){
					if (myField[3,spot.col].card == null){ //there is space in this column
						Move play = new Move();
						play.type = MoveType.PLAY;
						play.playCard = c;
						play.targetSpot = spot;
						possibleMoves.Add(play);
					}
				}
			}

			// pegasus and pythia can challenge a random card in the opponent's hand;
			if (c.type == CardType.PEGASUS || c.type == CardType.PYTHIA){
				Move handChallenge = new Move();
				handChallenge.type = MoveType.CHALLENGE;
				handChallenge.attacker = c;
				handChallenge.defender = otherHand[UnityEngine.Random.Range(0, otherHand.Length)];
				possibleMoves.Add(handChallenge);
			}

			if (c.type == CardType.ZEUS){
				for (int i = 0; i<3; i++){
					if (myField[3,i].card == null){//if this column has space
						Move zeus = new Move();
						zeus.type = MoveType.PLAY;
						zeus.playCard = c;
						zeus.targetSpot = myField[0,i];
						possibleMoves.Add(zeus);
					}
				}
			}

			//each mythological card can be played out of the hand
			if (c.type == CardType.DIONYSUS){
				foreach (FieldSpot spot1 in myField){
					if (spot1.card != null){
						foreach(FieldSpot spot2 in myField){
							Move dionysus = new Move();
							dionysus.type = MoveType.MYTH;
							dionysus.playCard = c;
							dionysus.dionysusCard = spot1.card;
							dionysus.targetSpot = spot2;
							possibleMoves.Add(dionysus);
						}
					}
				}
			}
			if (c.type == CardType.HADES){
				foreach (Card h in myDiscardPile){
					Move hades = new Move();
					hades.type = MoveType.MYTH;
					hades.playCard = c;
					hades.hadesCard = h;
					possibleMoves.Add(hades);
				}
			}


			if (c.type == CardType.PERSEPHONE){
				Move myth = new Move();
				myth.type = MoveType.MYTH;
				myth.playCard = c;
				possibleMoves.Add(myth);
			}

		}



		//each column can be used to challenge if valid
		if (myField[0,0].card != null && otherField[0,2].card != null && GameHandler.challengeTable[0, (int)myField[0,0].card.type, (int)otherField[0,2].card.type] != 0){
			Move challenge = new Move();
			challenge.type = MoveType.CHALLENGE;
			challenge.attacker = myField[0,0].card;
			challenge.defender = otherField[0,2].card;
			possibleMoves.Add(challenge);
		}
		if (myField[0,1].card != null && otherField[0,1].card != null && GameHandler.challengeTable[0, (int)myField[0,1].card.type, (int)otherField[0,1].card.type] != 0){
			Move challenge = new Move();
			challenge.type = MoveType.CHALLENGE;
			challenge.attacker = myField[0,1].card;
			challenge.defender = otherField[0,1].card;
			possibleMoves.Add(challenge);

		}
		if (myField[0,2].card != null && otherField[0,0].card != null && GameHandler.challengeTable[0, (int)myField[0,2].card.type, (int)otherField[0,0].card.type] != 0){
			Move challenge = new Move();
			challenge.type = MoveType.CHALLENGE;
			challenge.attacker = myField[0,2].card;
			challenge.defender = otherField[0,0].card;
			possibleMoves.Add(challenge);

		}
		return possibleMoves;
	}
}
