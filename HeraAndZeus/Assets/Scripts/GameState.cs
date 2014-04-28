using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum MoveType{PLAY, CHALLENGE, DRAW}

public struct Move{
	public MoveType type;
	public FieldSpot targetSpot;
	public Card playCard;
	public Card attacker;
	public Card defender;

}


public class GameState{
	FieldSpot[,] myField;
	FieldSpot[,] otherField;
	Card[] myHand;
	Card[] otherHand;
	Card[] myDrawPile;

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
		}

		//each mythological card can be played out of the hand


		//each column can be used to challenge if valid
		//if (myField[0,0].card)
		return possibleMoves;
	}
}
