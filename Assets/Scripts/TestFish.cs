using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFish : MonoBehaviour {

	private static GameObject flock = null;
	private bool loneFish = true;

	// Use this for initialization
	public static float BACKGROUND_HALF_WIDTH = 50f;
	public static float BACKGROUND_HALF_HEIGHT = 28.125f;
	public static float FISH_WIDTH = 1.125f;
	public static float FISH_HEIGHT = 0.825f;
	public static float MAX_SPEED = 10f;
	public static float STEER_CONST = 0.006f;

	void Awake () {
		if (flock == null){
			flock = GameObject.Find ("Flock");	
			Debug.Log ("Game object has been set!");
		}
	}

	void Start () {
		setDirection ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 modifier = steerAwayFromWalls ();
		Vector2 prepVelocity = this.GetComponent<Rigidbody2D>().velocity + modifier;
		this.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(prepVelocity,MAX_SPEED);
	}
		
	void setDirection() {
		// Sets a random direction for the fish. Only used on initialization. 
		Vector2 direction = new Vector2 (Random.Range(-4f,4f), Random.Range(-4f,4f));
		this.GetComponent<Rigidbody2D>().velocity = direction;

	}

	Vector2 steerAwayFromWalls() {
		float buffer = 20f;
		float damp = 100f;
		float constantFactor = 500f;

		Vector2 steer = new Vector2 (0f, 0f);

		if (this.transform.position.x + FISH_WIDTH > BACKGROUND_HALF_WIDTH - buffer){
			float distanceToWall = BACKGROUND_HALF_WIDTH - (this.transform.position.x + FISH_WIDTH);
			float desiredXVel = -MAX_SPEED * /*(buffer - distanceToWall) / buffer * */ constantFactor;
			Vector2 desired = new Vector2 (desiredXVel, this.GetComponent<Rigidbody2D> ().velocity.y);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.x - FISH_WIDTH < -BACKGROUND_HALF_WIDTH + buffer){
			float distanceToWall = (this.transform.position.x - FISH_WIDTH) - BACKGROUND_HALF_WIDTH;
			float desiredXVel = MAX_SPEED * /*(buffer - distanceToWall) / buffer * */constantFactor;
			Vector2 desired = new Vector2 (desiredXVel, this.GetComponent<Rigidbody2D> ().velocity.y);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.y + FISH_HEIGHT > BACKGROUND_HALF_HEIGHT - buffer){
			float distanceToWall = BACKGROUND_HALF_HEIGHT - (this.transform.position.y + FISH_HEIGHT);
			float desiredYvel = -MAX_SPEED * /*(buffer - distanceToWall) / buffer * */constantFactor;
			Vector2 desired = new Vector2 (this.GetComponent<Rigidbody2D> ().velocity.x, desiredYvel);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.y - FISH_HEIGHT < -BACKGROUND_HALF_HEIGHT + buffer){
			float distanceToWall = (this.transform.position.y - FISH_HEIGHT) - BACKGROUND_HALF_HEIGHT;
			float desiredYvel = MAX_SPEED * /*(buffer - distanceToWall) / buffer * */constantFactor;
			Vector2 desired = new Vector2 (this.GetComponent<Rigidbody2D> ().velocity.x, desiredYvel);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}

		return(steer);
	}


}
