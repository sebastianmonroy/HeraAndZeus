using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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

	}

	public float Eval(){
		return 0;
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
				handChallenge.defender = otherHand[Random.Range(0, otherHand.Length)];
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

			/*
			if (c.type == CardType.PERSEPHONE || c.type == CardType.PYTHIA){
				Move myth = new Move();
				myth.type = MoveType.MYTH;
				myth.playCard = c;
				possibleMoves.Add(myth);
			}
			*/
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
