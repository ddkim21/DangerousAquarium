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
		if (this.tag == "Left Test Fish") {
			Vector2 direction = new Vector2 (10f, 0f);
			this.GetComponent<Rigidbody2D>().velocity = direction;
		}
		if (this.tag == "Right Test Fish") {
			Vector2 direction = new Vector2 (-10f, 0f);
			this.GetComponent<Rigidbody2D>().velocity = direction;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D (Collider2D collider){
		if(loneFish){
			loneFish = false;
			collider.gameObject.GetComponent<TestFish>().loneFish = false;

			Debug.Log ("Fishes have collided");
			Vector2 otherObjectPos = collider.transform.position;
			Vector2 currentObjectPos = transform.position;

			// Set the xpos and ypos of the flock to be the average
			// between the xcoords and ycoords of the two fish

			float xpos = (currentObjectPos.x + otherObjectPos.x) / 2;
			float ypos = (currentObjectPos.y + otherObjectPos.y) / 2;
			Vector3 flockPos = new Vector3 (xpos, ypos, transform.position.z);

			// Calculate average velocity of the two fish that collided
			Vector3 currentObjectVel = this.GetComponent<Rigidbody2D>().velocity;
			Vector3 otherObjectVel = collider.gameObject.GetComponent<Rigidbody2D> ().velocity;
			Vector3 averageVel = (currentObjectVel + otherObjectVel) / 2;

			// Create flock object at the xpos,ypos location calculated above
			// Set the fish as the children of the flock
			GameObject newFlock = Instantiate (flock, flockPos, Quaternion.identity) as GameObject;
			this.transform.parent = newFlock.transform;
			collider.gameObject.transform.parent = newFlock.transform;

			// Set velocity of the flock
			Vector3 testVel = new Vector3(0f, 5f, 0f);
			newFlock.GetComponent<Rigidbody2D>().velocity = testVel;
			print ("new velocity is: " + newFlock.GetComponent<Rigidbody2D> ().velocity.ToString());
		}
	}
}
