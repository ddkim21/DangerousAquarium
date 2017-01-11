using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour {
	public static float MAX_SPEED = 7f;
	public static float STEER_CONST = 0.06f;

	public static float COHERE_WEIGHT = (3f);
	public static float SEPERATE_WEIGHT = 6f;
	public static float TEMP_RADIUS = 15f;
	public static float SEPERATION_RADIUS = 6f;

	public static float SHARK_WIDTH = 3.0f;
	public static float SHARK_HEIGHT = 2.05f;


	// Use this for initialization
	void Start () {
		setDirection ();
	}

	// Update is called once per frame
	void Update () {
		List<Fish> prey = new List<Fish> ();
		FindPrey (prey);

		List<Shark> neighbors = new List<Shark> ();
		FindNeighbors (neighbors);

		//Add modifier to velocity.
		//Modifier is determined by the hunt function.
		Vector2 modifier = Hunt(prey, neighbors);
		Vector2 prepVelocity = this.GetComponent<Rigidbody2D>().velocity + modifier;
		//Limit speed of shark if necessary
		this.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(prepVelocity,MAX_SPEED);

		// Check if our shark is about to go out of bounds of the camera
		string outofbound = OutOfBounds ();
		if (outofbound != null) { //If about to go out of bounds
			deter (outofbound); // Deter the direction to keep shark inbounds
		}

		// Orient sprite in direction of travel
		Vector2 moveDirection = gameObject.GetComponent<Rigidbody2D>().velocity;

		if (moveDirection != Vector2.zero) {
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	void setDirection() {
		// Sets a random direction for the shark. Only used on initialization. 
		Vector2 direction = new Vector2 (Random.Range(-3f,3f), Random.Range(-3f,3f));
		this.GetComponent<Rigidbody2D>().velocity = direction;
	}

	string OutOfBounds(){
		// Checks if the fish is currently out of bounds by using the dimensions and position of the 
		// background image.
		if (this.transform.position.x + SHARK_WIDTH > 50){
			return("Right");
		}
		else if (this.transform.position.x - SHARK_WIDTH < -50){
			return("Left");
		}
		else if (this.transform.position.y + SHARK_HEIGHT > 28.125){
			return("Up");
		}
		else if (this.transform.position.y - SHARK_HEIGHT < -28.125){
			return("Down");
		}
		else {
			return(null);
		}
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

	void FindPrey(List<Fish> prey){
		// Identical to FindNeighbors from Fish.cs
		// Currently iterates through the entire list of fish within our scene
		// and finds the ones within a certain radius of the fish
		// and then appends them to the neighbors list if they are within the radius.
		foreach(GameObject fish in GameObject.FindGameObjectsWithTag("Fish")){
			float distance = Vector3.Distance (this.transform.position, fish.transform.position);
			if (distance != 0 && distance < TEMP_RADIUS){
				prey.Add (fish.GetComponent<Fish>());
			}
		}
	}

	void FindNeighbors(List<Shark> neighbors){
		// Currently iterates through the entire list of shark within our scene
		// and finds the ones within a certain radius of the shark
		// and then appends them to the neighbors list if they are within the radius.
		foreach(GameObject shark in GameObject.FindGameObjectsWithTag("Shark")){
			float distance = Vector3.Distance (this.transform.position, shark.transform.position);
			if (distance != 0 && distance < TEMP_RADIUS){
				neighbors.Add (shark.GetComponent<Shark>());
			}
		}
	}

	Vector2 Hunt(List<Fish> prey, List<Shark> neighbors){
		Vector2 coherence = Cohere (prey) * COHERE_WEIGHT + 
			Seperate(neighbors) * SEPERATE_WEIGHT;
		return (coherence);
	}

	Vector2 Cohere(List<Fish> prey){
		// Cohere means to steer towards the center of your local prey
		// Identical to the fish cohere function with a few adjustments

		Vector2 center = new Vector2 (0f, 0f);

		if (prey.Count == 0) {
			return center;
		} else {
			foreach (Fish fish in prey) {
				center += (Vector2)fish.transform.position;
			}
			center /= prey.Count;
			Vector2 current2Dpos = new Vector2 (this.transform.position.x, this.transform.position.y);
			return(steerTo (center - current2Dpos)); // Pass vector of travel to get to center
		}
	}

	Vector2 Seperate(List<Shark> neighbors){
		// Identical to fish seperate, except seperating with other sharks

		Vector2 avoid = new Vector2 (0f, 0f);
		int count = 0;

		foreach (Shark shark in neighbors) {
			float distance = Vector3.Distance (this.transform.position, shark.transform.position);
			if (distance < SEPERATION_RADIUS){
				Vector2 veerOff = (Vector2)this.transform.position - (Vector2)shark.transform.position;
				veerOff.Normalize ();
				veerOff /= distance;
				avoid += veerOff;
				count++;
			} 
		}

		if (count > 0 && avoid.magnitude > 0){
			avoid /= count;
			return (steerTo (avoid));
		} 
		else {
			return avoid;
		}
	}

	Vector2 steerTo(Vector2 vel){
		// Identical to fish steerTo
		// Argument vel corresponds to the intended direction of travel, 
		// or the direction of travel to "steer" towards.

		// steer functions by utilizing the vector subtraction of
		// the intended direction of travel and the current direction of travel.
		Vector2 steer = new Vector2 (0f, 0f); // declare steer. 

		// Change vel to Unit Vector
		vel.Normalize ();

		// Scale the vel vector magnitude to MAX_SPEED. 
		// This allows the steer modifier to not "slow down" the shark
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
