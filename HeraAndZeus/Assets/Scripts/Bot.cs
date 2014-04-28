using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;
using MicrosoftResearch.Infer.Maths;
using MicrosoftResearch.Infer.Distributions;



public class Bot : Player {
	GameState curState;
	// Use this for initialization
	new void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update();
	}

	public override void CheckInput(){
		
	}
	

	public override void SetupField(){

	}


	public override void BeginTurn(){
		curState = GameHandler.Instance.GetState();
		if (FindOnField(CardType.ZEUS) != null) {
			actionPoints = 4;
		} else {
			actionPoints = NumOccupiedColumns();
		}
	}
}
