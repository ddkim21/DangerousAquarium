using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour {
	// Set our weights for the flocking behavior function
	// A la Boids model https://en.wikipedia.org/wiki/Boids
	public static float MAX_SPEED = 10f;
	public static float STEER_CONST = 0.003f;

	public static float COHERE_WEIGHT = 0f;
	public static float ALIGN_WEIGHT = 1f;
	public static float SEPERATE_WEIGHT = 0f;
	// Temp radius, used for finding neighbors
	// will be replaced by K Nearest Neighbors implementation
	public static float TEMP_RADIUS = 15f;

	// Use this for initialization
	void Start () {
		// Start off with a slightly random direction
		setDirection ();
	}
	
	// Update is called once per frame
	void Update () {
		// Create and populate list of neighbors
		// or fish within the radius of the current fish.
		// Will be changed to KNN.
		List<Fish> neighbors = new List<Fish> ();
		FindNeighbors (neighbors);

		//Add modifier to the velocity.
		//Modifier is determined by the Flock function. 
		Vector2 modifier = Flock(neighbors);
		Vector2 prepVelocity = this.GetComponent<Rigidbody2D>().velocity + modifier;
		//Limit speed of fish if necessary
		this.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(prepVelocity,MAX_SPEED);

		// Check if our fish is about to go out of bounds of the camera

		/*
		Temporarily commented out for the sake of testing flocking.
		string outofbound = OutOfBounds ();
		if (outofbound != null) { //If about to go out of bounds
			deter (outofbound); // Deter the direction to keep fish inbounds
		}
		*/

		// Orient sprite in direction of travel
		Vector2 moveDirection = gameObject.GetComponent<Rigidbody2D>().velocity;

		if (moveDirection != Vector2.zero) {
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	void setDirection() {
		// Sets a random direction for the fish. Only used on initialization. 
		Vector2 direction = new Vector2 (Random.Range(1f,4f), Random.Range(1f,4f));
		this.GetComponent<Rigidbody2D>().velocity = direction;
	}
		
	void deter(string side){
		// Prevent the fish from escaping the aquarium.
		// Keep the component of the velocity that does not send the fish out of the aquarium
		// and change the component of the velocity that does send the fish out
		// by choosing a random float between .1 and 2 (absolute value) in the opposite direction
		if (side == "Up"){
			Vector2 deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															(-1) * Random.Range(0.1f,2f));
			this.GetComponent<Rigidbody2D>().velocity = deter;
		}
		else if (side == "Down"){
			Vector2 deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															Random.Range(0.1f,2f));
			this.GetComponent<Rigidbody2D>().velocity = deter;
		}
		else if (side == "Left"){
			Vector2 deter = new Vector2 (Random.Range(0.1f,2f), 
															this.GetComponent<Rigidbody2D>().velocity.y);
			this.GetComponent<Rigidbody2D>().velocity = deter;
		}
		else {
			Vector2 deter = new Vector2 ((-1) * Random.Range(0.1f,2f), 
															this.GetComponent<Rigidbody2D>().velocity.y);
			this.GetComponent<Rigidbody2D>().velocity = deter;
		}
	}

	string OutOfBounds(){
		// Checks if the fish is currently out of bounds by using the dimensions and position of the 
		// background image.
		if (this.transform.position.x + 2.25 > 50){
			return("Right");
		}
		else if (this.transform.position.x - 2.25 < -50){
			return("Left");
		}
		else if (this.transform.position.y + 1.65 > 28.125){
			return("Up");
		}
		else if (this.transform.position.y - 1.65 < -28.125){
			return("Down");
		}
		else {
			return(null);
		}
	}

	void FindNeighbors(List<Fish> neighbors){
		// Currently iterates through the entire list of fish within our scene
		// and finds the ones within a certain radius of the fish
		// and then appends them to the neighbors list if they are within the radius.
		foreach(GameObject fish in GameObject.FindGameObjectsWithTag("Fish")){
			float distance = Vector2.Distance (this.transform.position, fish.transform.position);
			if (distance != 0 && distance < TEMP_RADIUS){
				neighbors.Add (fish.GetComponent<Fish>());
			}
		}
	}

	Vector2 Flock(List<Fish> neighbors){
		// Boids governs the movement of a fish based off of three rules:
		// coherence, seperation, and alignment.
		// The influences of each are added to the current velocity of the fish
		// also taking into account how we want to weight the influence of each.
		Vector2 coherence = Cohere (neighbors) * COHERE_WEIGHT;
		Vector2 alignment = Align (neighbors) * ALIGN_WEIGHT;
		Vector2 seperation = Seperate(neighbors) * SEPERATE_WEIGHT;

		return(seperation + alignment + coherence);
	}

	Vector2 Cohere(List<Fish> neighbors){
		// Cohere means to steer towards the center of your neighbors

		Vector2 center = new Vector2 (0f, 0f);
	
		if (neighbors.Count == 0) {
			return center;
		} else {
			foreach (Fish fish in neighbors) {
				center += (Vector2)fish.transform.position;
			}
			center /= neighbors.Count;
			Vector2 current2Dpos = new Vector2 (this.transform.position.x, this.transform.position.y);
			return(steerTo (center - current2Dpos)); // Pass vector of travel to get to center
		}
	}

	Vector2 Align(List<Fish> neighbors){
		// Align functions via finding the average velocity of all of a fish's neighbors
		// and steering towards the average velocity of its neighbors
		Vector2 average = new Vector2 (0f, 0f);

		if (neighbors.Count == 0){
			return average;
		} else {
			foreach (Fish fish in neighbors) {
				average += fish.GetComponent<Rigidbody2D>().velocity;
			}
			average /= neighbors.Count;
			return(steerTo (average));
		}
	}

	Vector2 Seperate(List<Fish> neighbors){
		// will write later

		return(new Vector2 (0f, 0f));
	}
		
	Vector2 steerTo(Vector2 vel){
		// Argument vel corresponds to the intended direction of travel, 
		// or the direction of travel to "steer" towards.

		// steer functions by utilizing the vector subtraction of
		// the intended direction of travel and the current direction of travel.
		Vector2 steer = new Vector2 (0f, 0f); // declare steer. 

		// Change vel to Unit Vector
		vel.Normalize ();

		// Scale the vel vector magnitude to MAX_SPEED. 
		// This allows the steer modifier to not "slow down" the fish
		vel *= MAX_SPEED;

		// Steering velocity is vel - current velocity
		// note by adding the steering velocity to the original velocity
		// we get steering velocity + original velocity
		// = frac(vel - original velocity) + original velocity
		// which in isolation, added over time, will eventually
		// be in the direction of vel
		steer = vel - this.GetComponent<Rigidbody2D>().velocity;
		// Set magnitude of steer to be no greater than STEER_CONST
		// which allows for a smoother (albeit slower) turns 
		steer = Vector2.ClampMagnitude(steer, STEER_CONST);

		return(steer);
	}
}
