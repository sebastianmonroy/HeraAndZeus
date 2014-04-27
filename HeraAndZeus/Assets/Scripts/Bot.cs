using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;
using MicrosoftResearch.Infer.Maths;
using MicrosoftResearch.Infer.Distributions;



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
		//Vector cardProbs = new Vector(1,1,1,1,1);
		double[] cardProbs =   {1.0/43.0, //ZEUS
								1.0/43.0, //ARGUS
								5.0/43.0, //HERO
								1.0/43.0, //POSEIDON
								2.0/43.0, //APOLLO
								3.0/43.0, //GIANT
								4.0/43.0, //CYCLOPS
								5.0/43.0, //CENTAUR
								2.0/43.0, //DIONYSUS
								1.0/43.0, //HADES
								4.0/43.0, //MEDUSA
								1.0/43.0, //PANDORA
								9.0/43.0, //PEGASUS
								1.0/43.0, //PERSEPHONE
								2.0/43.0, //PYTHIA
								1.0/43.0, //SIRENS
								0};   //NONE

		double[] testProbs =   {1.0/43.0, //ZEUS
								2.0/43.0, //ARGUS
								3.0/10.0, //HERO
								4.0/43.0, //POSEIDON
								0.0/43.0, //APOLLO
								0.0/43.0, //GIANT
								0.0/43.0, //CYCLOPS
								0.0/43.0, //CENTAUR
								0.0/43.0, //DIONYSUS
								0.0/43.0, //HADES
								0.0/43.0, //MEDUSA
								0.0/43.0, //PANDORA
								0.0/10.0, //PEGASUS
								0.0/43.0, //PERSEPHONE
								0.0/43.0, //PYTHIA
								0.0/43.0, //SIRENS
								0};   //NONE

		List<Variable> cardVars = new List<Variable>();
		for (int i = 0; i<10; i++){
			//Variable<CardType> card = Variable.EnumDiscrete<CardType>(testProbs);
			//cardVars.Add(card);
		}
		Variable<bool> firstCoin = Variable.Bernoulli(0.5).Named("firstCoin");


		InferenceEngine ie = new InferenceEngine();
		if (!(ie.Algorithm is VariationalMessagePassing))
		{
			Debug.Log(ie.Infer<Bernoulli>(firstCoin));
			//Variable<CardType> myCard = Variable.EnumDiscrete<CardType>(testProbs).Named("myCard");
			//DiscreteEnum<CardType>[] dist = ie.Infer<DiscreteEnum<CardType>[]>(myCard);

			Variable<int> myCard = Variable.Discrete(testProbs);
			Discrete dist = ie.Infer<Discrete>(myCard);
			//DiscreteEnum<CardType>[] dist = ie.Infer<DiscreteEnum<CardType>[]>(myCard);
			Debug.Log("Probability distribution: " + dist[(int)CardType.ZEUS]);
		}
		else
			Debug.Log("This example does not run with Variational Message Passing");

	}
	
	public override void SetupField(){

	}
}
