using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFish : MonoBehaviour {

	private static GameObject flock = null;
	private bool loneFish = true;

	// Use this for initialization

	void Awake () {
		if (flock == null){
			flock = GameObject.Find ("Flock");	
			Debug.Log ("Game object has been set!");
		}
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
