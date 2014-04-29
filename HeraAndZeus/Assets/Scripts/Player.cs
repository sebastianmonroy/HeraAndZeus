using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;

public enum PlayerType {
	HERA, ZEUS
}

public enum MythPhase {
	NONE, PYTHIA, HADES, SIRENS, DIONYSUS, PEGASUS
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

	public bool showHand;
	private int discardIndex = 0;

	public MythPhase phase = MythPhase.NONE;
	

	//Play(Card, int column);
	//Challenge(int column); (calls GameHandler.Instance to resolve);


	// Use this for initialization
	protected void Start () {
		this.transform.position = new Vector3(0, transform.position.y, transform.position.z) - transform.right * (Card.width * 1.5f + buffer);
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
								+ transform.right * (Card.width + buffer) * 4
								- transform.forward * Card.height/2;
		discardPilePos = this.transform.position 
								+ transform.right * (Card.width + buffer) * 4 
								- transform.forward * Card.height/2
								- transform.forward * (Card.height + buffer);

		allCards = BuildDrawPile();

		Shuffle(drawPile);

		for (int i = 0; i < 9; i++){
			Draw ();
		}

		actionPoints = 3;

		BuildPlayField();

		ArrangeHand();

		actionPointText.transform.position = this.transform.position - transform.right * Card.width * 2 - transform.forward * 0;
	}
	
	// Update is called once per frame
	protected void Update () {
		actionPointText.text = actionPoints.ToString();
	}

	public virtual void SetupField(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		bool raycast = Physics.Raycast(ray, out hit);
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
			if (raycast && hit.transform.tag == "Card") {
				Card chosen = hit.transform.gameObject.GetComponent<Card>();
				if (!chosen.moving && !chosen.flipping && chosen.isFlipped) {
					if (heldCard != null && chosen != heldCard) {
						// put down currently held up card
						heldCard.HoldUp(false);
						// hold up chosen
						chosen.HoldUp(true);
						heldCard = chosen;
						SelectCard(null);
					} else if (heldCard == null) {
						// hold up chosen
						chosen.HoldUp(true);
						heldCard = chosen;
						SelectCard(null);
					}
				}
			}
		} else if (scrollDown) {
			Card chosen = hit.transform.gameObject.GetComponent<Card>();
			if (raycast && chosen == heldCard && heldCard != null) {
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

	public virtual void CheckInput(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		bool raycast = Physics.Raycast(ray, out hit);
		RaycastHit[] hitAll = Physics.RaycastAll(ray);
		
		FieldSpot overSpot = null;
		Card overCard = null;
		foreach (RaycastHit h in hitAll){
			if (h.transform.tag == "Spot"){
				overSpot = h.transform.GetComponent<FieldSpot>();
			}
			else if (h.transform.tag == "Card"){
				overCard = h.transform.GetComponent<Card>();
			}
		}

		bool leftClick = Input.GetMouseButtonDown(0);
		bool rightClick = Input.GetMouseButtonDown(1);
		bool scrollUp = Input.GetAxis("Mouse ScrollWheel") > 0;
		bool scrollDown = Input.GetAxis("Mouse ScrollWheel") < 0;
		
		ArrangeField();

		if (scrollUp) {
			if (raycast && hit.transform.tag == "Card") {
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
			if (raycast && heldCard != null && hit.transform.gameObject.GetComponent<Card>() == heldCard && heldCard.isPickedUp) {
				// put down held card if it is already picked up
				heldCard.HoldUp(false);
				heldCard = null;
			}
		}

		if (phase == MythPhase.PYTHIA){
			if (leftClick && raycast){
				phase = MythPhase.NONE;
				GameHandler.Instance.EndPythiaPhase();
				actionPoints --;
			}
		} else if (phase == MythPhase.PEGASUS) {
			selectedCard.owner.ArrangeField();
			
			if (overSpot != null){
				if (overSpot.card != null && selectedCard != null && !OwnsFieldSpot(overSpot)){//a card is selected and the cursor is over a field spot with a card
					if (FindOnField(selectedCard) == null) {
						if (overSpot.card.type != CardType.ZEUS || (selectedCard.type == CardType.ZEUS && overSpot.row == 0)) {
							selectedCard.owner.MakeGap(overSpot);
						}
					}
				}
			}

			if (leftClick && raycast){
				FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
				if (chosen == selectedCard.owner.playField[chosen.row, chosen.col] && chosen.row == 0){ //does this spot belong to the active player
					selectedCard.owner.actionPoints++;
					selectedCard.owner.Play(selectedCard, chosen);
					SelectCard(null);
					actionPoints--;
					phase = MythPhase.NONE;
				}
			}
		} else if (phase == MythPhase.HADES){
			if (leftClick && raycast) {
				Card chosen = hit.transform.GetComponent<Card>();
				if (selectedCard.type == CardType.HADES) {
					if (chosen == heldCard) {
						Debug.Log("hades choose");
						discardPile.Remove(chosen);
						chosen.Reveal(false);
						chosen.HoldUp(false);
						heldCard = null;
						Discard(selectedCard);
						hand.Add(chosen);
						SelectCard(null);
						actionPoints--;
						discardIndex = 0;
						phase = MythPhase.NONE;
						ArrangeHand();
					} else if (discardPile.Contains(chosen)) {
						
						heldCard.HoldUp(false);

						discardIndex++;
						if (discardIndex >= discardPile.Count) {
							discardIndex = 0;
						}
						//Debug.Log("hades scroll " + discardIndex);

						chosen = discardPile[discardPile.Count-1-discardIndex];
						chosen.HoldUp(true);
						heldCard = chosen;
					} else if (chosen == selectedCard) {
						heldCard.HoldUp(false);
						heldCard = null;
						SelectCard(null);
						discardIndex = 0;
						phase = MythPhase.NONE;
					}
				} else {
					Debug.Log("HADES broke");
					heldCard.HoldUp(false);
					heldCard = null;
					Discard(selectedCard);
					SelectCard(null);
					actionPoints--;
					discardIndex = 0;
					phase = MythPhase.NONE;
				}
				ArrangeHand();
			}
		} else if (phase == MythPhase.SIRENS){
			if (leftClick && raycast) {
				Card chosen = hit.transform.GetComponent<Card>();
				if (selectedCard.type == CardType.SIRENS) {
					if (chosen == heldCard) {
						Debug.Log("sirens choose");						
						chosen.owner.discardPile.Remove(chosen);
						chosen.Reveal(false);
						chosen.HoldUp(false);
						heldCard = null;
						Discard(selectedCard);
						hand.Add(chosen);
						SelectCard(null);
						actionPoints--;
						discardIndex = 0;
						phase = MythPhase.NONE;
						ArrangeHand();
					} else if (chosen.owner != this && chosen.owner.discardPile.Contains(chosen)) {
						
						heldCard.HoldUp(false);

						discardIndex++;
						if (discardIndex >= chosen.owner.discardPile.Count) {
							discardIndex = 0;
						}
						//Debug.Log("hades scroll " + discardIndex);

						chosen = chosen.owner.discardPile[chosen.owner.discardPile.Count-1-discardIndex];
						chosen.HoldUp(true);
						heldCard = chosen;
					} else if (chosen == selectedCard) {
						heldCard.HoldUp(false);
						heldCard = null;
						SelectCard(null);
						discardIndex = 0;
						phase = MythPhase.NONE;
					}
				} else {
					Debug.Log("SIRENS broke");
					heldCard.HoldUp(false);
					heldCard = null;
					Discard(selectedCard);
					SelectCard(null);
					actionPoints--;
					discardIndex = 0;
					phase = MythPhase.NONE;
				}
				ArrangeHand();
			}
		} else if (phase == MythPhase.DIONYSUS){
			if (overSpot != null){
				if (overSpot.card != null && selectedCard != null && OwnsFieldSpot(overSpot)){//a card is selected and the cursor is over a field spot with a card
					if (overSpot.card.type != CardType.ZEUS && (selectedCard.spot.col == overSpot.col || selectedCard.spot.row == overSpot.row)) {
						MakeGap(overSpot);
					}
				}
			}

			if (leftClick && heldCard == null && raycast) {
				if (hit.transform.tag == "Spot") {
					FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
					if (chosen.row == selectedCard.spot.row || chosen.col == selectedCard.spot.col) {
						Play(selectedCard, chosen);
						SelectCard(null);
						phase = MythPhase.NONE;
					}
				}
			}
		} else {
			if (overSpot != null){
				if (overSpot.card !=null && selectedCard != null && OwnsFieldSpot(overSpot)){//a card is selected and the cursor is over a field spot with a card
					if (FindOnField(selectedCard) == null) {
						if (overSpot.card.type != CardType.ZEUS && selectedCard == null || (selectedCard.type != CardType.ZEUS && selectedCard.strength != -1) || (selectedCard.type == CardType.ZEUS && overSpot.row == 0)) {
							MakeGap(overSpot);
						}
					}
				}
			}
			
			if (leftClick && heldCard == null && raycast) {
				if (hit.transform.tag == "Card") {
					Card chosen = hit.transform.GetComponent<Card>();
					if (selectedCard != null && selectedCard == chosen) {
						SelectCard(null);
					} else if (hand.Contains(chosen)){
						SelectCard(chosen);
					} else if (drawPile.Contains(chosen)){
						Draw();
					} else if (chosen != null && selectedCard != null) {
						//Debug.Log(chosen.owner);
						// player has selected card and chosen card
						if (chosen.owner != this) {
							int context = 3;
							// clicked on enemy card
							if (selectedCard.type == CardType.SIRENS && chosen.owner.discardPile.Contains(chosen) != null) {
 								discardIndex = 0;
 								chosen = chosen.owner.discardPile[chosen.owner.discardPile.Count-1-discardIndex];
								heldCard = chosen;
								chosen.HoldUp(true);
								chosen = null;
								phase = MythPhase.SIRENS;
 							} else if (selectedCard.spot != null && chosen.spot != null && chosen.spot.row == 0 && selectedCard.spot.col == (2 -chosen.spot.col)) {
								// clicked on enemy card in same column as selected card
								context = 0;
							} else if (hand.Contains(selectedCard) && chosen.owner.hand.Contains(chosen)) {
								// clicked own card in hand and then enemy card in hand
								context = 1;
							} else if (hand.Contains(selectedCard) && chosen.spot != null && chosen.spot.row == 0) {
								// clicked own card in hand and then enemy card in front row of field
								context = 2;
							}

							Debug.Log("context " + context);

							if (context != 3) {
								int resolution = GameHandler.Instance.Challenge(context, selectedCard, chosen);
								if (context == 1 && resolution == 2 && selectedCard.type == CardType.PEGASUS && chosen.owner.hand.Contains(chosen) != null) {
	 								SelectCard(chosen);
	 								chosen = null;
	 								phase = MythPhase.PEGASUS;
	 							} else if (resolution != 0) {
									actionPoints --;
									SelectCard(null);
								} else {
									SelectCard(null);
								}
							}							
						} else {
							// clicked on own card
							if (selectedCard.type == CardType.DIONYSUS && FindOnField(chosen) != null) {
								// selected DIONYSUS and clicked on any card in own field
								Discard(selectedCard);
								chosen.spot.card = null;
								SelectCard(chosen);
								hand.Add(chosen);
								chosen = null;
								ArrangeHand();
								phase = MythPhase.DIONYSUS;
							} else if (selectedCard.type == CardType.PERSEPHONE && discardPile.Contains(chosen) != null) {
								actionPoints--;
								List<Card> pegs = GetPegsFromDiscard();
								Discard(selectedCard);
								SelectCard(null);
								chosen = null;
								hand.AddRange(pegs);
								ArrangeHand();
							} else if (selectedCard.type == CardType.HADES && discardPile.Contains(chosen) != null) {
								discardIndex = 0;
								chosen = discardPile[discardPile.Count-1-discardIndex];
								heldCard = chosen;
								chosen.HoldUp(true);
								chosen = null;
								phase = MythPhase.HADES;
 							}
						}
					} else if (FindOnField(chosen) != null){
						SelectCard(chosen);
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
			} 

			if (rightClick){
				SelectCard(null);
				//GameHandler.Instance.EndGame(this);
			}
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


	public virtual void BeginTurn(){
		if (FindOnField(CardType.ZEUS) != null) {
			actionPoints = 4;
		} else {
			actionPoints = NumOccupiedColumns();
		}
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
		//Debug.Log("Discarding: " + card.type);
		FieldSpot spot = FindOnField(card);
		if (spot != null){
			//Debug.Log (this.name + " discards from field " + card.type);
			card.spot = null;
			card.Reveal(true);
			discardPile.Add(card);
			card.MoveTo(discardPilePos, false);
			spot.card = null;
			ArrangeField();
			return true;
		} else if (hand.Contains(card)){
			card.spot = null;
			card.Reveal(true);
			discardPile.Add(card);
			card.MoveTo(discardPilePos, false);
			hand.Remove(card);
			ArrangeHand();
			return true;
		}

		return false;
	}
	
	
	public bool Play(Card card, FieldSpot spot){
		if (card.strength > -1 || card.type == CardType.ZEUS) {
			if (spot.card != null) {//there is already a card in the target spot

				if (NextAvailableSpot(spot.col) == null) { //the column is full
					return false; //play unsuccessful
				} else {//move every card in the target row or higher down a row to make room
					for (int row = 2; row >= spot.row; row--){
						if (playField[row,spot.col].card != null){
							playField[row+1,spot.col].card = playField[row,spot.col].card;
							playField[row,spot.col].card = null;
							playField[row+1,spot.col].card.MoveTo(playField[row+1,spot.col].transform.position + Vector3.up * 2, true);
						}
					}
				}
			} else { // this spot is unoccupied
				spot = NextAvailableSpot(spot.col); //ge the next available spot in this column
				//Debug.Log(spot);
				// ZEUS can not be played in empty column
				if (card.type == CardType.ZEUS && spot.row == 0 && spot.card == null) {
					return false;
				}
			}

			// ZEUS can only be played in front row
			if (card.type == CardType.ZEUS && spot.row != 0) {
				return false;
			}

			FieldSpot old_spot = FindOnField(card);
			if (old_spot != null) 
				old_spot.card = null;

			spot.card = card;
			card.spot = spot;
			card.MoveTo(spot.transform.position + Vector3.up * 2, true);
			hand.Remove(card);

			ArrangeHand();
			ArrangeField();
			card.Flip(false);
			if (card.type != CardType.ZEUS) {
				actionPoints --;
			}

			card.updatePredictionVector(CardType.DIONYSUS, -2);
			card.updatePredictionVector(CardType.HADES, -1);
			card.updatePredictionVector(CardType.PERSEPHONE, -1);
			card.updatePredictionVector(CardType.SIRENS, -1);
			if (spot.row != 0) {
				card.updatePredictionVector(CardType.ZEUS, -1);
			}
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

	public static void Shuffle(List<Card> cards){
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
			c.setupPredictionVector(allCardsList);
			//c.predictList = allCardsList;
			c.setupPredictionVector(allCardsList);
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

	protected int NumOccupiedColumns(){
		int num = 0;
		for (int col = 0; col < 3; col ++){
			if (playField[0,col].card !=  null)
				num++;
		}
		
		return num;
	}

	public void ArrangeHand(){
		Vector3 left = transform.position 
				- transform.forward * (Card.height + buffer) * 4
				- transform.forward * Card.height/2
				+ transform.right * Card.width
				+ transform.right * (Card.width + buffer)
				- transform.right * ((hand.Count * Card.width) + ((hand.Count - 1) * buffer))/2;
		for (int i = 0; i < hand.Count; i++){
			Card card = hand[i];
			card.MoveTo(left + this.transform.right * (Card.width + buffer) * i, true);
			card.Flip(showHand);
		}
	}

	//Will eliminate gaps in the play field, push everything to the top of its column
	public void ArrangeField(){
		//Debug.Log("arranged field");
		for (int col = 0; col < 3; col++){
			if (NextAvailableSpot(col) != null){ // if the column is not full
				for (int row = 0; row < 4; row++){
					FieldSpot spot = playField[row,col];
					if (spot.card != null){
						spot.card.MoveTo(spot.transform.position + Vector3.up * 2, false); //make sure everything is moving to its proper spot
						FieldSpot nextSpot = NextAvailableSpot(col);
						if (nextSpot.row < row){
							Card current = playField[row,col].card;
							nextSpot.card = current;
							playField[row, col].card = null;
							current.MoveTo(nextSpot.transform.position + Vector3.up * 2, false);
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

	public FieldSpot FindOnField(CardType ct){
		foreach (FieldSpot spot in playField){
			if (spot.card != null && spot.card.type == ct){
				return spot;
			}
		}
		return null;
	}

	public List<Card> GetPegsFromDiscard() {
		List<Card> pegs = new List<Card>();
		int count = 0;
		foreach (Card c in discardPile) {
			if (c.type == CardType.PEGASUS) {
				if (pegs.Count < 3) {
					pegs.Add(c);
					c.Reveal(false);
				} else {
					break;
				}
			}
		}
		return pegs;
	}

	public bool MakeGap(FieldSpot spot){
		//ArrangeField();
		if (NextAvailableSpot(spot.col) == null) return false;

		for (int row = 2; row >= spot.row; row--){
			if (playField[row,spot.col].card != null){
				playField[row,spot.col].card.MoveTo(playField[row+1,spot.col].transform.position + Vector3.up * 2, true);
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
			card.updatePredictionVector(ct, amount);
		}
	}
}