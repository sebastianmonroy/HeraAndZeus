using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum CardType {
	//HERA, IO, AMAZON, NEMESIS, ARTEMIS, HYDRA, HARPY, FURY,		// HERA cards
	ZEUS, ARGUS, HERO, POSEIDON, APOLLO, GIANT, CYCLOPS, CENTAUR,	// ZEUS cards
	DIONYSUS, HADES, MEDUSA, PANDORA, PEGASUS, PERSEPHONE, PYTHIA, SIRENS,	// common cards
	NONE
}

public class Card : MonoBehaviour {
	public static float width = 5;
	public static float height = 7;

	public CardType type;
	public string title;
	public int strength;
	public bool special;

	public bool moving;
	public bool inField = false;
	public bool flipping;
	public bool isFlipped = false;
	public bool picking;
	public bool isPickedUp = false;
	public Vector3 originalPosition;
	public Vector3 originalScale;
	public Vector3 desiredScale;

	public bool typeSet = false;
	Vector3 goalRotation;
	Vector3 destination;
	public bool showText = false;

	public bool revealed = false;

	// prediction array is as long as number of card types	
	public int[] predictVector = new int[Enum.GetNames(typeof(CardType)).Length-1];
	public List<CardType> predictList = new List<CardType>();
	public FieldSpot spot;

	public TextMesh titleText;
	public TextMesh strengthText;
	public TextMesh specialText;

	public GameObject border;

	public Player owner;

	// Use this for initialization
	void Start () {
		// initialize and fill predictVector
		 

		border.renderer.enabled = false;
		//type = CardType.NONE;
		name = type.ToString();
		this.transform.eulerAngles = new Vector3(0, 0, 0);
		originalScale = this.transform.localScale;
		
		// updatePrediction(CardType.ZEUS, 1);
		// updatePrediction(CardType.ARGUS, 1);
		// updatePrediction(CardType.DIONYSUS, 2);
		// updatePrediction(CardType.HADES, 1);
		// updatePrediction(CardType.MEDUSA, 4);
		// updatePrediction(CardType.PANDORA, 1);
		// updatePrediction(CardType.PEGASUS, 9);
		// updatePrediction(CardType.PERSEPHONE, 1);
		// updatePrediction(CardType.PYTHIA, 2);
		// updatePrediction(CardType.SIRENS, 1);
		// updatePrediction(CardType.HERO, 5);
		// updatePrediction(CardType.POSEIDON, 1);
		// updatePrediction(CardType.APOLLO, 2);
		// updatePrediction(CardType.GIANT, 3);
		// updatePrediction(CardType.CYCLOPS, 4);
		// updatePrediction(CardType.CENTAUR, 5);
	}
	
	// Update is called once per frame
	void Update () {
		if (moving) {
			this.transform.position = Vector3.Lerp(this.transform.position, destination, 0.1f);
			if (Vector3.Distance(this.transform.position, destination) <= 0.1f) {
				this.transform.position = destination;
				moving = false;
				inField = true;
			}
		} else if (picking) {
			this.transform.position = Vector3.Lerp(this.transform.position, destination, 0.1f);
			this.transform.localScale = Vector3.Lerp(this.transform.localScale,desiredScale, 0.1f);
			if (Vector3.Distance(this.transform.position, destination) <= 0.1f 
					&& Vector3.Distance(this.transform.localScale, desiredScale) <= 0.1f) {
				this.transform.position = destination;
				picking = false;
				isPickedUp = !isPickedUp;
			}

		}

		if (flipping) {
			if (isFlipped) {
				this.transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles, new Vector3(0, 0, 0), 0.1f);
				if ((this.transform.eulerAngles - new Vector3(0, 0, 0)).magnitude <= 0.01) {
					SetFlip(false);
				}
			} else {
				this.transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles, new Vector3(0, 0, 180), 0.1f);
				if ((this.transform.eulerAngles - new Vector3(0, 0, 180)).magnitude <= 0.01) {
					SetFlip(true);
				}
			}
		}

		if (type != CardType.NONE && !typeSet) {
 			SetType(type);
 			typeSet = true;
		}

		DetermineTextVisibility();
	}

	public void setupPredictionVector() {
		updatePredictionVector(CardType.ZEUS, 0);
	}

	public void updatePredictionVector(CardType ct, int amount) {
		if (predictVector == null || predictVector.Length == 0) {
			predictVector = new int[Enum.GetNames(typeof(CardType)).Length-1];
			foreach (CardType ctype in predictList) {
				predictVector[(int) ctype]++;
			}
		}
		//Debug.Log(predictVector.Length);
		predictVector[(int) ct] += amount;

		if (predictVector[(int) ct] < 0) {
			predictVector[(int) ct] = 0;
		}
	}

	public void updatePredictionList(Card card, int amount) {
		updatePredictionList(card.type, amount);
	}

	public void updatePredictionList(CardType ct, int amount) {
		if (amount > 0) {
			for (int i = 0; i < amount; i++) {
				predictList.Add(ct);
			}
		} else if (amount < 0) {
			for (int i = 0; i > amount; i--) {
				predictList.Remove(ct);
			}
		}
		updatePredictionVector(ct, amount);
	}

	public float[] getPredictionProbabilities() {
		float[] probabilities = new float[predictVector.Length];
		int sum = 0;
		foreach (int i in predictVector) {
			sum += i;
		}

		for (int i = 0; i < predictVector.Length; i++) {
			probabilities[i] = (float) (predictVector[i]) / sum;
		}
		return probabilities;
	}

	private void DetermineTextVisibility() {
		bool showText = (this.transform.eulerAngles.z > 90 && this.transform.eulerAngles.z < 270);

		titleText.active = showText;
		strengthText.active = showText;
		specialText.active = showText;
	}

	public void MoveTo(Vector3 dest){
		moving = true;
		destination = dest;
	}

	public void Reveal(bool revBool) {
		if (revBool && !revealed) {
			Flip(true);
			revealed = true;
			this.owner.updateAllCardPredictions(type, -1);
		} else if (!revBool && revealed) {
			Flip(false);
			revealed = false;
			this.owner.updateAllCardPredictions(type, 1);
		}
	}

	public void HoldUp(bool pickBool) {
		if (pickBool != isPickedUp && isFlipped) {
			picking = true;
			if (isPickedUp) {
				destination = originalPosition;
				desiredScale = originalScale;
			} else {
				originalPosition = this.transform.position;
				destination = Camera.main.transform.position - Vector3.up * 1;
				desiredScale = 5 * originalScale;
			}
		}
	}

	public void Flip() {
		flipping = true;
	}

	public void Flip(bool flipBool) {
		// only flips the card if it isn't in desired
		if (flipping){
			SetFlip(isFlipped);
		}
		if (isFlipped != flipBool) {
				flipping = true;
		}
	}

	public void SetFlip(bool flipBool) {
		// immediately flips the card to the desired state
		flipping = false;
		isFlipped = flipBool;
		if (isFlipped) {
			this.transform.eulerAngles = new Vector3(0, 0, 180);
		} else {
			this.transform.eulerAngles = new Vector3(0, 0, 0);
		}
	}

	public void SetType(CardType cardType) {
		this.type = cardType;
		this.name = type.ToString();
		this.title = type.ToString();
		switch(type) {
			/*
			case CardType.HERA:
				this.strength = -1;
				this.special = true;
				break;
			case CardType.IO:
				this.strength = 0;
				this.special = true;
				break;
			case CardType.AMAZON:
				this.strength = 2;
				this.special = true;
				break;
			case CardType.NEMESIS:
				this.strength = 7;
				this.special = false;
				break;
			case CardType.ARTEMIS:
				this.strength = 6;
				this.special = false;
				break;
			case CardType.HYDRA:
				this.strength = 5;
				this.special = false;
				break;
			case CardType.HARPY:
				this.strength = 4;
				this.special = false;
				break;
			case CardType.FURY:
				this.strength = 3;
				this.special = false;
				break;
				*/
			case CardType.ZEUS:
				this.strength = 8;
				this.special = true;
				break;
			case CardType.ARGUS:
				this.strength = 0;
				this.special = true;
				break;
			case CardType.HERO:
				this.strength = 2;
				this.special = true;
				break;
			case CardType.POSEIDON:
				this.strength = 7;
				this.special = false;
				break;
			case CardType.APOLLO:
				this.strength = 6;
				this.special = false;
				break;
			case CardType.GIANT:
				this.strength = 5;
				this.special = false;
				break;
			case CardType.CYCLOPS:
				this.strength = 4;
				this.special = false;
				break;
			case CardType.CENTAUR:
				this.strength = 3;
				this.special = false;
				break;
			case CardType.DIONYSUS:
				this.strength = -1;
				this.special = true;
				break;
			case CardType.HADES:
				this.strength = -1;
				this.special = true;
				break;
			case CardType.MEDUSA:
				this.strength = 0;
				this.special = true;
				break;
			case CardType.PANDORA:
				this.strength = 0;
				this.special = true;
				break;
			case CardType.PEGASUS:
				this.strength = 1;
				this.special = true;
				break;
			case CardType.PERSEPHONE:
				this.strength = -1;
				this.special = true;
				break;
			case CardType.PYTHIA:
				this.strength = 0;
				this.special = true;
				break;
			case CardType.SIRENS:
				this.strength = -1;
				this.special = true;
				break;
			default:
				this.title = "ERROR";
				this.strength = -1;
				this.special = false;
				break;
		}

		titleText.text = this.title;
		strengthText.text = (this.strength == -1) ? " " : this.strength.ToString();
		specialText.text = (this.special) ? "X" : " ";
	}
}
