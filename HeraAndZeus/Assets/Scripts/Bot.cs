using UnityEngine;
using System.Collections;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;

public class Bot : Player {

	// Use this for initialization
	new void Start () {
		base.Start();
		TestBayesian();
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update();
	}

	public override void CheckInput(){

	}

	void TestBayesian(){
		Debug.Log("Bayesian things happening");
		Variable<bool> firstCoin = Variable.Bernoulli(0.5).Named("firstCoin");
		Variable<bool> secondCoin = Variable.Bernoulli(0.5).Named("secondCoin");
		Variable<bool> bothHeads  = (firstCoin & secondCoin).Named("bothHeads");
		InferenceEngine ie = new InferenceEngine();
		if (!(ie.Algorithm is VariationalMessagePassing))
		{
			Debug.Log("Probability both coins are heads: "+ie.Infer(bothHeads));
			bothHeads.ObservedValue=false;
			Debug.Log("Probability distribution over firstCoin: " + ie.Infer(firstCoin));
		}
		else
			Debug.Log("This example does not run with Variational Message Passing");

	}
	
	public override void SetupField(){

	}
}
