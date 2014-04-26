using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerType {
	HERA, ZEUS
}

public class Player : MonoBehaviour {
	int buffer = 3;

	//public PlayerType type;
	public int actionPoints;

	private CardType[] deck;
	private Card[] allCards;
	public CardType[] forceDeck;

	public List<Card> hand;
	public List<Card> drawPile;
	public List<Card> discardPile;

	public FieldSpot[,] playField;

	public FieldSpot fieldSpotPrefab;
	public Card cardPrefab;

	Vector3 drawPilePos;
	Vector3 discardPilePos;

	public Card selectedCard;
	public Card heldCard;

	public bool setupPhase = false;

	public TextMesh actionPointText;

	

	//Play(Card, int column);
	//Challenge(int column); (calls GameHandler.Instance to resolve);


	// Use this for initialization
	protected void Start () {
		if (forceDeck != null && forceDeck.Length > 0) {
			deck = forceDeck;
		} else {
		 	deck = BuildDeck();
		}

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

		allCards = BuildDrawPile();

		Shuffle(drawPile);

		for (int i = 0; i < 9; i++){
			Draw ();
		}

		BuildPlayField();

		ArrangeHand();

		actionPointText.transform.position = this.transform.position - transform.right * 25 - transform.forward * 10;

		//Debug.Log(Play (hand[0], 0));
		//hand[0].Flip();
	
	}
	
	// Update is called once per frame
	void Update () {
		actionPointText.text = actionPoints.ToString();
	}

	public void SetupField(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		Physics.Raycast(ray, out hit);
		RaycastHit[] hitAll = Physics.RaycastAll(ray);

		//Debug.Log(hit.collider.gameObject.name);
		
		bool leftClick = Input.GetMouseButtonDown(0);
		bool rightClick = Input.GetMouseButtonDown(1);
		bool scrollUp = Input.GetAxis("Mouse ScrollWheel") > 0;
		bool scrollDown = Input.GetAxis("Mouse ScrollWheel") < 0;
		
		
		if (leftClick && heldCard == null) {
			if (hit.transform.tag == "Card") {
				Card chosen = hit.transform.GetComponent<Card>();
				if (selectedCard != null && selectedCard == chosen) {
					SelectCard(null);
				} else if (hand.Contains(chosen)){
					//Debug.Log(name + " card chosen");
					SelectCard(chosen);
					//Debug.Log(name + " " + selectedCard);

				} else if (drawPile.Contains(chosen)){
					Draw();
				} 
			} else if (hit.transform.tag == "Spot") {
				FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
				if (playField[chosen.row, chosen.col] == chosen && NextAvailableSpot(chosen.col).row == 0){ //does this spot belong to the active player, and is the selected column empty
					if (selectedCard != null){
						Play(selectedCard, chosen);
						SelectCard(null);
					}
				}
			}
		} else if (scrollUp) {
			if (hit.transform.tag == "Card") {
				Card card = hit.transform.gameObject.GetComponent<Card>();
				if (!card.moving && !card.flipping && !card.picking && card.isFlipped) {
					if (heldCard != null && heldCard.isPickedUp) {
						// put down currently held up card
						heldCard.HoldUp(false);
						// hold up card
						card.HoldUp(true);
						heldCard = card;
						SelectCard(null);

					} else if (heldCard == null) {
						// hold up card
						card.HoldUp(true);
						heldCard = card;
						SelectCard(null);
					}
				}
			}
		} else if (scrollDown) {
			if (hit.transform.gameObject.GetComponent<Card>() == heldCard && heldCard != null && heldCard.isPickedUp) {
				// put down held card if it is already picked up
				heldCard.HoldUp(false);
				heldCard = null;
			}
		}

		//if each column has 1 card
		if (playField[0,0].card != null &&
		    playField[0,1].card != null &&
		    playField[0,2].card != null){
			setupPhase = false;
		}
		//Debug.Log(selectedCard);

	}

	public void CheckInput(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		Physics.Raycast(ray, out hit);
		RaycastHit[] hitAll = Physics.RaycastAll(ray);
		
		FieldSpot overSpot = null;
		foreach (RaycastHit h in hitAll){
			if (h.transform.tag == "Spot"){
				overSpot = h.transform.GetComponent<FieldSpot>();
			}
		}
		
		bool leftClick = Input.GetMouseButtonDown(0);
		bool rightClick = Input.GetMouseButtonDown(1);
		bool scrollUp = Input.GetAxis("Mouse ScrollWheel") > 0;
		bool scrollDown = Input.GetAxis("Mouse ScrollWheel") < 0;
		
		ArrangeField();
		if (overSpot != null){
			if (overSpot.card !=null && selectedCard != null && OwnsFieldSpot(overSpot)){//a card is selected and the cursor is over a field spot with a card
				if (FindOnField(selectedCard) == null)
				MakeGap(overSpot);
//				gapSpot = overSpot;
			}
		}
		//activePlayer.ArrangeField();
		/*
		if (selectedCard != null && hit.transform.tag == "Card"){ // push cards down to preview play
			Card chosen = hit.transform.GetComponent<Card>();
			FieldSpot spot = activePlayer.FindOnField(chosen);
			if (spot != null){
				Debug.Log("Here");
				activePlayer.MakeGap(spot);
				gapSpot = spot;
			}
		}
		*/
		
		
		if (leftClick && heldCard == null) {
			if (hit.transform.tag == "Card") {
				Card chosen = hit.transform.GetComponent<Card>();
				if (selectedCard != null && selectedCard == chosen) {
					SelectCard(null);
				} else if (hand.Contains(chosen)){
					SelectCard(chosen);
				} else if (FindOnField(chosen) != null){
					SelectCard(chosen);
				}else if (drawPile.Contains(chosen)){
					Draw();
				} else if (chosen != null && selectedCard != null) {
					// player has selected card and chosen card
					if (chosen.owner != this) {
						//Debug.Log(selectedCard.spot.col == (2 -chosen.spot.col));
						int context = 3;
						// clicked on enemy card
						if (selectedCard.spot != null && chosen.spot != null && selectedCard.spot.col == (2 -chosen.spot.col)) {
							// clicked on enemy card in same column as selected card
//							Debug.Log("Context: " + 0 + " selectedCard.type: " + (int)selectedCard.type + " chosen.type: " + (int)chosen.type);
							context = 0;
						} else if (hand.Contains(selectedCard) && chosen.owner.hand.Contains(chosen)) {
							// clicked own card in hand and then enemy card in hand
//							Debug.Log("Context: " + 1 + " selectedCard.type: " + (int)selectedCard.type + " chosen.type: " + (int)chosen.type);
							context = 1;
						} else if (hand.Contains(selectedCard) && chosen.spot != null && chosen.spot.row == 0) {
							// clicked own card in hand and then enemy card in front row of field
//							Debug.Log("Context: " + 2 + " selectedCard.type: " + (int)selectedCard.type + " chosen.type: " + (int)chosen.type);
							context = 2;
						}

						switch(ResolveChallenge.resolve(context, selectedCard.type, chosen.type)) {
							case 0:
								Debug.Log("Invalid Challenge");
								break;
							case 1:

								break;
							case 2:

								break;
							default:
								break;
						}
					}
				}
			} else if (hit.transform.tag == "Spot") {
				FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
				if (playField[chosen.row, chosen.col] == chosen){ //does this spot belong to the active player
					if (selectedCard != null){
						Play(selectedCard, chosen);
						SelectCard(null);
					}
				}
			}
		} else if (scrollUp) {
			if (hit.transform.tag == "Card") {
				Card card = hit.transform.gameObject.GetComponent<Card>();
				if (!card.moving && !card.flipping && !card.picking && card.isFlipped) {
					if (heldCard != null && heldCard.isPickedUp) {
						// put down currently held up card
						heldCard.HoldUp(false);
						
						// hold up card
						card.HoldUp(true);
						heldCard = card;
						
						SelectCard(null);
					} else if (heldCard == null) {
						// hold up card
						card.HoldUp(true);
						heldCard = card;
						
						SelectCard(null);
					}
				}
			}
		} else if (scrollDown) {
			if (heldCard != null && hit.transform.gameObject.GetComponent<Card>() == heldCard && heldCard.isPickedUp) {
				// put down held card if it is already picked up
				heldCard.HoldUp(false);
				heldCard = null;
			}
		}

		if (rightClick){
			SelectCard(null);
		}

	}

	void SelectCard(Card c){
		if (selectedCard != null){ //if a card is currently selected
			selectedCard.border.renderer.enabled = false;
			selectedCard = null;
		}
		if (c != null){
			c.border.renderer.enabled = true;
			selectedCard = c;
		}
	}


	public void BeginTurn(){
		if (NumOccupiedColumns() == 0) {
			//GameHandler.Instance.EndGame();
		}
		else 
			actionPoints = NumOccupiedColumns();
	}


	public bool Draw(){
		if (drawPile.Count > 0){
			hand.Add(drawPile[0]);
			drawPile.RemoveAt(0);
			ArrangeHand();
			actionPoints --;
			return true;
		}
		else return false;

	}

	public bool Discard(Card card){
		FieldSpot spot = FindOnField(card);
		if (spot != null){
			discardPile.Add(card);
			card.Flip(false);
			card.MoveTo(discardPilePos);
			card.inField = false;
			spot.card = null;
			ArrangeField();
			return true;
		}

		return false;
	}
	
	
	public bool Play(Card card, FieldSpot spot){
		if (card.strength > -1) {
			if (spot.card != null){//there is already a card in the target spot

				if (NextAvailableSpot(spot.col) == null){ //the column is full or card is not playable in the field

					return false; //play unsuccessful
				}		
				else{//move every card in the target row or higher down a row to make room
					for (int row = 2; row >= spot.row; row--){
						if (playField[row,spot.col].card != null){
							playField[row+1,spot.col].card = playField[row,spot.col].card;
							playField[row,spot.col].card = null;
							playField[row+1,spot.col].card.MoveTo(playField[row+1,spot.col].transform.position + Vector3.up * 2);
						}
					}
				}
			}
			else{ // this spot is unoccupied
				spot = NextAvailableSpot(spot.col); //ge the next available spot in this column
			}

			spot.card = card;
			card.spot = spot;
			card.MoveTo(spot.transform.position + Vector3.up * 2);
			hand.Remove(card);

			ArrangeHand();
			card.Flip();
			actionPoints --;
			return true;
		} else {
			return false;
		}
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

	private Card[] BuildDrawPile() {
		List<CardType> allCardsList = new List<CardType>();
		allCardsList.AddRange(deck);

		foreach (CardType type in deck){
			Card c = GameObject.Instantiate(cardPrefab, drawPilePos, Quaternion.identity) as Card;
			c.SetType(type);
			c.SetFlip(false);
			c.owner = this;
			c.predictList = allCardsList;
			c.setupPredictionVector();
			drawPile.Add(c);
		}

		return drawPile.ToArray();
	}

	private CardType[] BuildDeck() {
		CardType[] d = new CardType[43];
		for (int i = 0; i < 43; i++) {
			if (i < 1)			d[i] = CardType.ZEUS;		// 1
			else if (i < 2)		d[i] = CardType.ARGUS;		// 1
			else if (i < 4)		d[i] = CardType.DIONYSUS;	// 2
			else if (i < 5)		d[i] = CardType.HADES;		// 1
			else if (i < 9)		d[i] = CardType.MEDUSA;		// 4
			else if (i < 10)	d[i] = CardType.PANDORA;	// 1
			else if (i < 19)	d[i] = CardType.PEGASUS;	// 9
			else if (i < 20)	d[i] = CardType.PERSEPHONE;	// 1
			else if (i < 22)	d[i] = CardType.PYTHIA;		// 2
			else if (i < 23)	d[i] = CardType.SIRENS;		// 1
			else if (i < 28)	d[i] = CardType.HERO;		// 5
			else if (i < 29)	d[i] = CardType.POSEIDON;	// 1
			else if (i < 31)	d[i] = CardType.APOLLO;		// 2
			else if (i < 34)	d[i] = CardType.GIANT;		// 3
			else if (i < 38)	d[i] = CardType.CYCLOPS;	// 4
			else 				d[i] = CardType.CENTAUR;	// 5
		}

		return d;
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
			if (playField[0,col].card !=  null)
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
	public void ArrangeField(){
		for (int col = 0; col < 3; col++){
			if (NextAvailableSpot(col) != null){ // if the column is not full
			for (int row = 0; row < 4; row++){
				FieldSpot spot = playField[row,col];
				if (spot.card != null){
					spot.card.MoveTo(spot.transform.position + Vector3.up * 2); //make sure everything is moving to its proper spot
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
	}

	public FieldSpot FindOnField(Card card){
		foreach (FieldSpot spot in playField){
			if (spot.card == card){
				return spot;
			}
		}
		return null;
	}

	public bool MakeGap(FieldSpot spot){
		//ArrangeField();
		if (NextAvailableSpot(spot.col) == null) return false;

		for (int row = 2; row >= spot.row; row--){
			if (playField[row,spot.col].card != null){
				playField[row,spot.col].card.MoveTo(playField[row+1,spot.col].transform.position + Vector3.up * 2);
			}
		}
		return true;
	}

	public bool OwnsFieldSpot (FieldSpot spot){
		foreach (FieldSpot s in playField){
			if (s == spot) return true;
		}
		return false;
	}

	public void updateAllCardPredictions(CardType ct, int amount) {
		foreach (Card card in allCards) {
			card.updatePredictionList(ct, amount);
		}
	}

	/*
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
	*/
}