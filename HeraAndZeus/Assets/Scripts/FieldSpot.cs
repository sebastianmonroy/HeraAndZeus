using UnityEngine;
using System.Collections;

public class FieldSpot : MonoBehaviour {
	public Card card;
	public bool occupied;

	public int row;
	public int col;

	void Update() {
		if (card != null) {
			occupied = true;
			if (card.spot != this)
				card.spot = this;
		} else {
			occupied = false;
		}
	}
}
