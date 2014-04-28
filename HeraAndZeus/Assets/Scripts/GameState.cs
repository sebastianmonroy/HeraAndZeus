using UnityEngine;
using System.Collections;

public enum MoveType{PLAY, CHALLENGE, DRAW}

public struct Move{
	MoveType type;
	Card playCard;
	Card attacker;
	Card defender;

}


public class GameState{
	Card[,] myField;
	Card[,] otherField;

	public void Build(Player p1, Player p2){
		foreach (FieldSpot spot in p1.playField){
			myField[spot.row, spot.col] = spot.card;
		}
		foreach (FieldSpot spot in p2.playField){
			otherField[spot.row, spot.col] = spot.card;
		}
	}

	public float Eval(){
		return 0;
	}
}
