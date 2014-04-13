using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {

	public Player p1;
	public Player p2;
	
	Player activePlayer;

	public Card selectedCard;

	public static GameHandler Instance;

	FieldSpot gapSpot;

	// Use this for initialization
	void Start () {
		Instance = this;
		activePlayer = p1;
	}
	
	// Update is called once per frame
	void Update () {
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

		activePlayer.ArrangeField();
		if (overSpot != null){
			if (overSpot.card !=null && selectedCard != null && activePlayer.OwnsFieldSpot(overSpot)){//a card is selected and the cursor is over a field spot with a card
				activePlayer.MakeGap(overSpot);
				gapSpot = overSpot;
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


		if (leftClick){

			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				if (activePlayer.hand.Contains(chosen)){
					selectedCard = chosen;
				} else if (activePlayer.drawPile.Contains(chosen)){
					activePlayer.Draw();
				} else if ((activePlayer.FindOnField(chosen)) && selectedCard != null) {
					// if player has clicked on a card in their hand and then a card in the field, insert card there
					//activePlayer.Insert(chosen, activePlayer.);
					activePlayer.Play(selectedCard, activePlayer.FindOnField(chosen));
					selectedCard = null;
				}
			}


			if (hit.transform.tag == "Spot"){
				FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
				if (activePlayer.playField[chosen.row, chosen.col] == chosen){ //does this spot belong to the active player
					if (selectedCard != null){
						activePlayer.Play(selectedCard, chosen);
						selectedCard = null;
					}
				}
			}
			
		}


		/*
		if (Input.GetMouseButtonDown(1)){
			
			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				foreach (FieldSpot spot in activePlayer.playField){
					if (spot.card == chosen){
						chosen.Flip();
					}
				}
			}
		}

		if (Input.GetMouseButtonDown(2)){ //Middle mouse click, useful for debug
			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				foreach (FieldSpot spot in activePlayer.playField){
					if (spot.card == chosen){
						activePlayer.Discard(chosen);
					}
				}
			}
		}
		*/

		//activePlayer.ArrangeField();
	
	}

	public void EndGame(){

	}

	public void SwitchPlayer(){
		if (activePlayer == p1)
			activePlayer = p2;
		else 
			activePlayer = p1;

	}
}
