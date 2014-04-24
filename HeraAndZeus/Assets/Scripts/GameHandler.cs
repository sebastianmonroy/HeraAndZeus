using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {

	public Player p1;
	public Player p2;
	
	Player activePlayer;



	public static GameHandler Instance;
	
	// Use this for initialization
	void Start () {
		Instance = this;
		activePlayer = p2;

		p1.actionPoints = 12;
		p2.actionPoints = 12;
	}
	
	// Update is called once per frame
	void Update () {
		if (p1.setupPhase || p2.setupPhase){
			p1.SetupField();
			p2.SetupField();
		}
		else if (activePlayer.actionPoints <= 0)
			SwitchPlayer();
		else activePlayer.CheckInput();
	}

	public void EndGame(){

	}

	public void SwitchPlayer(){
		if (activePlayer == p1){
			activePlayer = p2;
			p2.BeginTurn();
		}
		else {
			activePlayer = p1;
			p1.BeginTurn();
		}

	}
}
