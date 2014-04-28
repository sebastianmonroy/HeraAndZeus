using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	List<Card> myHand;
	List<Card> otherHand; //If PYTHIA is used, I have knowledge about some cards in otherHand.

	List<Card> myDrawPile;
	List<Card> otherDrawPile;

	List<Card> myDiscardPile;
	List<Card> otherDiscardPile;

	int myActionPoints;
	int otherActionPoints;

	float ZEUS = 5;
	float ARGUS = 0;
	float HERO = 5;
	float POSEIDON = 7;
	float APOLLO = 6;
	float GIANT = 5;
	float CYCLOPS = 4;
	float CENTAUR = 3;
	float DIONYSUS= 5;
	float HADES = 5;
	float MEDUSA = 5;
	float PANDORA = 5;
	float PEGASUS = 5;
	float PERSEPHONE = 5;
	float PYTHIA = 5;
	float SIRENS = 5;

	public void Build(Player p1, Player p2){
		Debug.Log("Build called");

		foreach (FieldSpot spot in p1.playField){
			myField[spot.row, spot.col] = spot.card;
		}
		foreach (FieldSpot spot in p2.playField){
			otherField[spot.row, spot.col] = spot.card;
		}

		myHand = p1.hand;
		otherHand = p2.hand;

		myDrawPile = p1.drawPile;
		otherDrawPile = p2.drawPile;

		myDiscardPile = p1.discardPile;
		otherDiscardPile = p2.discardPile;
/*
		foreach (Card card in p1.hand){
			myHand.Add(card);
		}
		foreach (Card card in p2.hand) {
			otherHand.Add (card);
		}

		foreach (Card card in p1.drawPile) {
			myDrawPile.Add (card);
		}
		foreach (Card card in p2.drawPile) {
			otherDrawPile.Add (card);
		}

		foreach (Card card in p2.discardPile) {
			myDiscardPile.Add (card);
		}
		foreach (Card card in p2.discardPile) {
			otherDiscardPile.Add (card);
		}
*/
		myActionPoints = p1.actionPoints;
		otherActionPoints = p2.actionPoints;
	}

	public int CardValue(Card card){
		Debug.Log ("Card Type: " + card.type.ToString);
		switch(card.type){
			case CardType.ZEUS:
			    return 5;
				break;
			case CardType.ARGUS:
			    return 0;
			    break;
			case CardType.HERO:
			    return 5;
				break;
			case CardType.POSEIDON:
				return 7;
				break;
			case CardType.APOLLO:
				return 6;
				break;
			case CardType.GIANT:
				return 5;
				break;
			case CardType.CYCLOPS:
				return 4;
				break;
			case CardType.CENTAUR:
				return 3;
				break;
			case CardType.DIONYSUS:
				return 5;
				break;
			case CardType.HADES:
				return 5;
				break;
			case CardType.MEDUSA:
				return 5;
				break;
			case CardType.PANDORA:
				return 5;
				break;
			case CardType.PEGASUS:
				return 5;
				break;
			case CardType.PERSEPHONE:
				return 5;
				break;
			case CardType.PYTHIA:
				return 5;
				break;
			case CardType.SIRENS:
				return 5;
				break;
			default:
				return 5;
				break;
		}
	}

/* 
 * 
 * |||| (My unflipped cards at 1st row * 2 + my flipped cards / 2 + my cards in other rows 
 * |||| + my cards in the hand + other cards revealed in the field + other cards revealed in the hand)
 * |||| * (myActionPoints + myActionPoint - otherActionPoints)
 * |||| + total value of myDrawPile / 2 + total value of otherDiscardPile / 2
 * 
 */

	public float CalculateMine(Card[,] myField, List<Card> myHand){
		Debug.Log("CalculateMine called");
		float result = 0;

		// My unflipped cards 1st row * 2 + my flipped cards / 2 + my cards in other rows 
		int row = myField[0].Length;
		int col = myField.Length;
		for (int i = 0; i <= row; i++)
		{
			for (int j = 0; j <= col; j++)
			{
				if ( i == 0 && myField[i,j] != null && myField[i,j].isFlipped == false ){
					Debug.Log("My unflipped 1st row");
					result += CardValue(myField[i,j]) * 2;
				}
				else if ( myField != null && myField[i,j].isFlipped == true ){
					Debug.Log("My flipped cards");
					result += CardValue(myField[i,j]) / 2;
				}
				else if (myField[i,j] != null){
					Debug.Log("My Cards");
					result += CardValue(myField[i,j]);
				}
			}
		}
		Debug.Log("CalculateMine first part : " + result);

		//my cards in the hand
		for (int i = 0; i<myHand.Count; i++) {
			result += CardValue (myHand [i]);
		}
		Debug.Log ("plus my cards in the hand : " + result);
	}

	public float CalculateOther (Card[,] otherField, List<Card> otherHand){
		Debug.Log("CalculateOther called");
		float result = 0;
		// other cards revealed in the field
		for (int i = 0; i <= bound0; i++)
		{
			for (int j = 0; j <= bound1; j++)
			{
				if ( otherField != null && otherField[i,j].isFlipped == true ){
					result += CardValue(otherField[i,j]);
				}
			}
		}
		Debug.Log("Sum of other card values revealed in the field: " + result);

		// other cards revealed in the hand
		for (int i = 0; i<otherHand.Count; i++) {
			if (otherHand[i].isFlipped == true){
				result += CardValue (otherHand [i]);
			}
		}

		Debug.Log ("Sum of other card values revealed in the field and hand: " + result);
	}

	public float TotalPileValue(List<Card> pile){
		Debug.Log("totalPileValue called");
		float result = 0; 
		for (int i = 0; i<pile.Count; i++) {
			result += CardValue (pile [i]);
		}
		Debug.Log("totalPileValue result: " + result);
		return result;
	}
/*
* |||| (My unflipped cards 1st row * 2 + my flipped cards / 2 + my cards in other rows 
* |||| + my cards in the hand + other cards revealed in the field + other cards revealed in the hand)
* |||| * (myActionPoints + myActionPoint - otherActionPoints)
* |||| + total value of myDrawPile / 2 + total value of otherDiscardPile / 2
*/
	public float Eval(){
		float result = 0;
		result = (CalculateMine(myField, myHand) + CalculateOther(otherField, otherHand)) 
			* ( 2 * myActionPoints - otherActionPoints ) + TotalPileValue(myDrawPile) / 2 
				+ TotalPileValue(otherDiscardPile) / 2;
		Debug.Log("Eval Score: " + result);
		return result;
	}
}
