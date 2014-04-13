using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerType {
	HERA, ZEUS
}

public class Player : MonoBehaviour {
	int buffer = 3;

	public PlayerType type;
	public bool humanControlled;
	public int actionPoints;
	private CardType[] deck = new CardType[43];
	public List<Card> hand;
	public List<Card> drawPile;
	public List<Card> discardPile;

	public FieldSpot[,] playField;

	public FieldSpot fieldSpotPrefab;
	public Card cardPrefab;

	Vector3 drawPilePos;
	Vector3 discardPilePos;

	

	//Play(Card, int column);
	//Challenge(int column); (calls GameHandler.Instance to resolve);


	// Use this for initialization
	void Start () {
		hand = new List<Card>();
		drawPile = new List<Card>();
		discardPile = new List<Card>();
		playField = new FieldSpot[4,3];

		drawPilePos    = this.transform.position 
								+ transform.right * (Card.width + buffer) * 5 
								- transform.forward * Card.height/2;
		discardPilePos = this.transform.position 
								+ transform.right * (Card.width + buffer) * 5 
								- transform.forward * Card.height/2
								- transform.forward * (Card.height + buffer);

		BuildDrawPile();
		Shuffle(drawPile);
		for (int i = 0; i < 9; i++){
			Draw ();
		}

		BuildPlayField();

		ArrangeHand();

		//Debug.Log(Play (hand[0], 0));
		//hand[0].Flip();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void BeginTurn(){
		if (NumOccupiedColumns() == 0) 
			GameHandler.Instance.EndGame();
		else 
			actionPoints = NumOccupiedColumns();
	}


	public bool Draw(){
		if (drawPile.Count > 0){
			hand.Add(drawPile[0]);
			drawPile.RemoveAt(0);
			ArrangeHand();
			return true;
		}
		else return false;

	}

	public bool Discard(Card card){
		foreach (FieldSpot spot in playField){
			if (spot.card == card){
				discardPile.Add(card);
				card.Flip(false);
				card.MoveTo(discardPilePos);
				card.inField = false;
				spot.card = null;
				ArrangeField();
				return true;
			}
		}
		return false;
	}
	
	
	public bool Play(Card card, int col){
		FieldSpot spot = NextAvailableSpot(col);
		if (spot == null)
			return false;
		else{
			card.Flip(false);
			spot.card = card;
			card.MoveTo(spot.transform.position + Vector3.up * 2);
			card.spot = spot;
			hand.Remove(card);
		}

		ArrangeHand();
		return true;
		
	}

	public bool Move(Card card, FieldSpot newSpot) {
		if (newSpot.card == null) {
			card.spot.card = null;
			card.MoveTo(newSpot.transform.position + Vector3.up * 2);
			newSpot.card = card;
			card.spot = newSpot;
			return true;
		} else {
			// can't move to new spot, isn't empty
			return false;
		}
	}

	void Shuffle(List<Card> cards){

		int n = cards.Count;
		while (n > 1) {
			int k = (Random.Range(0, n) % n);
			n--;
			Card value = cards[k];
			cards[k] = cards[n];
			cards[n] = value;
		}
	}

	void BuildDrawPile(){
		BuildDeck();
		foreach (CardType type in deck){
			Card c = GameObject.Instantiate(cardPrefab, drawPilePos, Quaternion.identity) as Card;
			c.SetType(type);
			c.SetFlip(false);
			drawPile.Add(c);
//			Debug.Log(c);
		}
	}

	void BuildDeck() {
		if (type == PlayerType.HERA) {
			for (int i = 0; i < 43; i++) {
				if (i < 1)			deck[i] = CardType.HERA;
				else if (i < 2)		deck[i] = CardType.IO;
				else if (i < 4)		deck[i] = CardType.DIONYSUS;
				else if (i < 5)		deck[i] = CardType.HADES;
				else if (i < 9)		deck[i] = CardType.MEDUSA;
				else if (i < 10)	deck[i] = CardType.PANDORA;
				else if (i < 19)	deck[i] = CardType.PEGASUS;
				else if (i < 20)	deck[i] = CardType.PERSEPHONE;
				else if (i < 22)	deck[i] = CardType.PYTHIA;
				else if (i < 23)	deck[i] = CardType.SIRENS;
				else if (i < 28)	deck[i] = CardType.AMAZON;
				else if (i < 29)	deck[i] = CardType.NEMESIS;
				else if (i < 31)	deck[i] = CardType.ARTEMIS;
				else if (i < 34)	deck[i] = CardType.HYDRA;
				else if (i < 38)	deck[i] = CardType.HARPY;
				else 				deck[i] = CardType.FURY;
			}
		} else {
			for (int i = 0; i < 43; i++) {
				if (i < 1)			deck[i] = CardType.ZEUS;
				else if (i < 2)		deck[i] = CardType.ARGUS;
				else if (i < 4)		deck[i] = CardType.DIONYSUS;
				else if (i < 5)		deck[i] = CardType.HADES;
				else if (i < 9)		deck[i] = CardType.MEDUSA;
				else if (i < 10)	deck[i] = CardType.PANDORA;
				else if (i < 19)	deck[i] = CardType.PEGASUS;
				else if (i < 20)	deck[i] = CardType.PERSEPHONE;
				else if (i < 22)	deck[i] = CardType.PYTHIA;
				else if (i < 23)	deck[i] = CardType.SIRENS;
				else if (i < 28)	deck[i] = CardType.HERO;
				else if (i < 29)	deck[i] = CardType.POSEIDON;
				else if (i < 31)	deck[i] = CardType.APOLLO;
				else if (i < 34)	deck[i] = CardType.GIANT;
				else if (i < 38)	deck[i] = CardType.CYCLOPS;
				else 				deck[i] = CardType.CENTAUR;
			}
		}
	}

	void BuildPlayField(){

		for (int row = 0; row < 4; row++){
			for (int col = 0; col < 3; col++){

				Vector3 pos = this.transform.position + 
					this.transform.right   * (Card.width/2  + (Card.width  + buffer) * col) - 
					this.transform.forward * (Card.height/2 + (Card.height + buffer) * row);

				FieldSpot f = GameObject.Instantiate(fieldSpotPrefab, pos, Quaternion.identity) as FieldSpot;
				f.transform.localScale = new Vector3(Card.width, 1, Card.height);
				f.row = row;
				f.col = col;
				//f.transform.parent = this.transform;
				f.name = ("Spot " + row + ", " + col);
				playField[row,col] = f;
			}
		}
	}

	FieldSpot NextAvailableSpot(int col){
		for (int row = 0; row < 4; row ++){
			if (playField[row,col].card ==  null)
				return playField[row,col];
		}
		
		return null;
	}

	int NumOccupiedColumns(){
		int num = 0;
		for (int col = 0; col < 3; col ++){
			if (playField[0,col].card ==  null)
				num++;
		}
		
		return num;
	}

	void ArrangeHand(){
		Vector3 left = transform.position 
				- transform.forward * (Card.height + buffer) * 4
				- transform.forward * Card.height/2
				+ transform.right * Card.width
				+ transform.right * (Card.width + buffer)
				- transform.right * ((hand.Count * Card.width) + ((hand.Count - 1) * buffer))/2;
		for (int i = 0; i < hand.Count; i++){
			Card card = hand[i];
			card.MoveTo(left + this.transform.right * (Card.width + buffer) * i);
			card.Flip(true);
		}
	}

	//Will eliminate gaps in the play field, push everything to the top of its column
	void ArrangeField(){
		for (int col = 0; col < 3; col++){
			for (int row = 0; row < 4; row++){
				if (playField[row,col].card != null){
					FieldSpot nextSpot = NextAvailableSpot(col);
					if (nextSpot.row < row){
						Card current = playField[row,col].card;
						nextSpot.card = current;
						playField[row, col].card = null;
						current.MoveTo(nextSpot.transform.position + Vector3.up * 2);
						current.spot = nextSpot;
					}
				}
			}
		}
	}

	public FieldSpot FindOnField(Card card){
		foreach (FieldSpot spot in playField){
			if (spot.card == card){
				return spot;
			}
		}
		return null;
	}

	public void Insert(Card oldCard, Card newCard) {
		if (oldCard.spot.row < 4) {
			int col = oldCard.spot.col;
			int row = oldCard.spot.row;
			FieldSpot lastSpot = NextAvailableSpot(col);
			if (lastSpot != null) {
				for (int r = lastSpot.row; r > row; r--) {
					Card thisCard = playField[r-1, col].card;
					Move(thisCard, playField[r, col]);
				}
				Play(newCard, col);
			}
		} else {
			Play(newCard, oldCard.spot.col);
		}
	}
}