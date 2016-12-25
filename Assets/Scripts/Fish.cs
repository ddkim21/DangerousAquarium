using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour {

	private int roamTime = 0;

	// Use this for initialization
	void Start () {
		this.GetComponent<Rigidbody2D>().freezeRotation = true;
	}
	
	// Update is called once per frame
	void Update () {
	// Check if our fish is about to go out of bounds of the camera
		if (OutOfBounds("Right")){
			Deter("Right"); //If about to go out of bounds, slightly deter the direction
		}							 // To prevent the fish from doing so
		if (OutOfBounds("Left")){
			Deter("Left");
		}
		if (OutOfBounds("Up")){
			Deter("Up");
		}
		if (OutOfBounds("Down")){
			Deter("Down");
		}
		// Otherwise, check if the roamtime of the fish is down, if so, then change direction
		else if (roamTime <= 0){
			SetTime();
			SetDirection();
		}
		else { // Otherwise, count down on the roamtime for the given direction
			roamTime--;
		}
	}

	void SetTime() {
		roamTime = Random.Range(30,360);
	}

	void SetDirection() {
		Vector2 direction = new Vector2 (Random.Range(-10f,10f), Random.Range(-10f,10f));
		this.GetComponent<Rigidbody2D>().velocity = direction;
	}



	void Deter(string side){
		if (side == "Up"){
			Vector2 deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															(-1) * Random.Range(0.1f,2f));
			this.GetComponent<Rigidbody2D>().velocity = deter;
			roamTime += 10;
		}
		else if (side == "Down"){
			Vector2 deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															Random.Range(0.1f,2f));
			this.GetComponent<Rigidbody2D>().velocity = deter;
			roamTime += 10;
		}
		else if (side == "Left"){
			Vector2 deter = new Vector2 (Random.Range(0.1f,2f), 
															this.GetComponent<Rigidbody2D>().velocity.y);
			this.GetComponent<Rigidbody2D>().velocity = deter;
			roamTime += 10;
		}
		else {
			Vector2 deter = new Vector2 ((-1) * Random.Range(0.1f,2f), 
															this.GetComponent<Rigidbody2D>().velocity.y);
			this.GetComponent<Rigidbody2D>().velocity = deter;
			roamTime += 10;
		}
	}

	bool OutOfBounds(string side){
		if (side == "Right"){
			return(this.transform.position.x + 2.25 > 50);
		}
		else if (side == "Left"){
			return(this.transform.position.x - 2.25 < -50);
		}
		else if (side == "Up"){
			return(this.transform.position.y + 1.65 > 28.125);
		}
		else {
			return(this.transform.position.y - 1.65 < -28.125);
		}
	}


}
