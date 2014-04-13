using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {

	public Player p1;
	public Player p2;
	
	Player activePlayer;

	Card selectedCard;

	public static GameHandler Instance;

	// Use this for initialization
	void Start () {
		Instance = this;
		activePlayer = p1;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Physics.Raycast(ray, out hit);

			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				if (activePlayer.hand.Contains(chosen)){
					selectedCard = chosen;
				} else if (activePlayer.drawPile.Contains(chosen)){
					activePlayer.Draw();
				} else if (chosen.inField && selectedCard != null) {
					// if player has clicked on a card in their hand and then a card in the field, insert card there
					activePlayer.Insert(chosen, selectedCard);
					selectedCard = null;
				}
			}

			if (hit.transform.tag == "Spot"){
				FieldSpot chosen = hit.transform.GetComponent<FieldSpot>();
				if (activePlayer.playField[chosen.row, chosen.col] == chosen){
					if (selectedCard != null){
						activePlayer.Play(selectedCard, chosen.col);
						selectedCard = null;
					}
				}
			}
		}

		if (Input.GetMouseButtonDown(1)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Physics.Raycast(ray, out hit);
			
			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				foreach (FieldSpot spot in activePlayer.playField){
					if (spot.card == chosen){
						chosen.Flip();
					}
				}
			}
		}

		if (Input.GetMouseButtonDown(2)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Physics.Raycast(ray, out hit);
			
			if (hit.transform.tag == "Card"){
				Card chosen = hit.transform.GetComponent<Card>();
				foreach (FieldSpot spot in activePlayer.playField){
					if (spot.card == chosen){
						activePlayer.Discard(chosen);
					}
				}
			}
		}
	
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
