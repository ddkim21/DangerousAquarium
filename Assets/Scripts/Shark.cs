using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shark : Agent {
	public static float MAX_SPEED = 7f;
	public static float STEER_CONST = 0.06f;

	public static float COHERE_WEIGHT = (3f);
	public static float SEPERATE_WEIGHT = 6f;
	public static float WANDER_WEIGHT = 1f;
	public static float TEMP_RADIUS = 15f;
	public static float SEPERATION_RADIUS = 6f;

	public static float SHARK_WIDTH = 3.0f;
	public static float SHARK_HEIGHT = 2.05f;
	public static float BITE_RADIUS = 5f;
	public static float EAT_RADIUS = 2f;

	// Background width and background height is useful for deterring from walls
	public static float BACKGROUND_HALF_WIDTH = 50f;
	public static float BACKGROUND_HALF_HEIGHT = 28.125f;

	//Lastly, our count of fish in order to keep track of the number of sharks
	public static int SHARK_COUNT = 0;

	public Sprite biteSprite;
	public GameObject blood;

	public static bool BRUTE_FORCE = false;
	public static bool GRID_IMPLEMENTATION = true;

	public static List<Shark> ALL_SHARKS;
	public static bool STARTED = false;


	private SpriteRenderer spriteRenderer; 
	private Sprite normalSprite;

	// Private ints and Vector for setting up wander behavior
	// After having eaten a fish, a shark's appetite is satiated for 
	// a certain period of time (roughly 5 seconds).
	// During this period of 5 seconds, the shark no longer chases
	// after fish, nor eats them. It simply wanders around.
	private int SharkWander = 0;
	private int SharkWanderHelper = 0;
	private Vector2 WanderDirection = new Vector2();

	// Global ID and instance ID, since instantiated objects have same IDs
	private static int _globalID = 0;
	private int _ID = 0;



	void Awake (){
		if (STARTED == false){
			Debug.Log("This is being run");
			ALL_SHARKS = new List<Shark>();
			STARTED = true;
		}
		ALL_SHARKS.Add(this);
		_ID = _globalID;
		_globalID++;
	}

	// Use this for initialization
	void Start () {
		SHARK_COUNT++;
		spriteRenderer = this.GetComponent<SpriteRenderer> ();
		normalSprite = spriteRenderer.sprite;
		setDirection ();
	}

	// Update is called once per frame
	void Update () {
		if(GRID_IMPLEMENTATION){
			AquariumManager.sharkGridUpdate(this);
		}

		// Check if our shark is about to go out of bounds of the camera
		string outofbound = OutOfBounds ();
		if (outofbound != null) { //If about to go out of bounds
			deter (outofbound); // Deter the direction to keep shark inbounds
			orientDirection ();
		}

		// Neighbor sharks are also used for the wander component
		List<Shark> neighbors = new List<Shark> ();

		if(BRUTE_FORCE){
			FindNeighborsBrute (neighbors);
		}

		if(GRID_IMPLEMENTATION){
			FindNeighborsGrid (neighbors);
		}

		// Check if the shark should still be wandering
		if (SharkWander > 0){
			Vector2 wanderSteer = wander ();
			// Note that while wandering the shark still avoids other sharks.
			this.GetComponent<Rigidbody2D> ().velocity += wanderSteer * WANDER_WEIGHT + 
				Seperate (neighbors) * SEPERATE_WEIGHT;
			orientDirection ();
			SharkWander--;
			if (SharkWander == 0){
				Debug.Log ("Wander has ended");
			}
			return;
		}
			
		List<Fish> prey = new List<Fish>();
		if(BRUTE_FORCE){
			FindPreyBrute (prey);
		}
		if(GRID_IMPLEMENTATION){
			FindPreyGrid(prey);
		}

		PreyBiteReady (prey);

		//Add modifier to velocity.
		//Modifier is determined by the hunt function.
		Vector2 modifier = Hunt(prey);
		modifier += Seperate (neighbors) * SEPERATE_WEIGHT;
		Vector2 prepVelocity = this.GetComponent<Rigidbody2D>().velocity + modifier;
		//Limit speed of shark if necessary
		this.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(prepVelocity,MAX_SPEED);
		orientDirection ();

	}

	void setDirection() {
		// Sets a random direction for the shark. Only used on initialization. 
		Vector2 direction = new Vector2 (Random.Range(-3f,3f), Random.Range(-3f,3f));
		this.GetComponent<Rigidbody2D>().velocity = direction;
	}

	void orientDirection() {
		// Orient sprite in direction of travel
		Vector2 moveDirection = gameObject.GetComponent<Rigidbody2D>().velocity;

		if (moveDirection != Vector2.zero) {
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	Vector2 wander(){
		// Wander functions via finding a random point in 
		// the aquarium to steer towards.
		// This point changes every (1/4) of the total wander time.
		// So within a single instance of shark wandering, the shark 
		// will change directions 4 times. 
		// Wander helper determines how long the shark steers in a certain
		// random direction.
		float buffer = 5f;
		if (SharkWanderHelper <= 0){
			float randomX = Random.Range (-BACKGROUND_HALF_WIDTH + buffer,
				                BACKGROUND_HALF_WIDTH - buffer);
			float randomY = Random.Range (-BACKGROUND_HALF_HEIGHT + buffer,
				                BACKGROUND_HALF_HEIGHT - buffer);
			WanderDirection = new Vector2 (randomX, randomY);
			SharkWanderHelper = 200;
		}

		Vector2 desired = WanderDirection - (Vector2)this.transform.position;
		Vector2 steer = steerTo (desired);
		SharkWanderHelper--;
		return(steer);
	}

	string OutOfBounds(){
		// Checks if the fish is currently out of bounds by using the dimensions and position of the 
		// background image.
		if (this.transform.position.x + SHARK_WIDTH > BACKGROUND_HALF_WIDTH){
			return("Right");
		}
		else if (this.transform.position.x - SHARK_WIDTH < -BACKGROUND_HALF_WIDTH){
			return("Left");
		}
		else if (this.transform.position.y + SHARK_HEIGHT > BACKGROUND_HALF_HEIGHT){
			return("Up");
		}
		else if (this.transform.position.y - SHARK_HEIGHT < -BACKGROUND_HALF_HEIGHT){
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

	void FindPreyBrute(List<Fish> prey){
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

	void FindPreyGrid(List<Fish> prey){
		List<int> cell_list =AquariumManager.fishGrid[x_coord,y_coord];

		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1}; 
		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT){
						foreach(int id in AquariumManager.fishGrid[x,y]){
							prey.Add (Fish.ALL_FISH[id]);
						}
					}
				}
			}
		}
	}

	void FindNeighborsBrute(List<Shark> neighbors){
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

	void FindNeighborsGrid(List<Shark> neighbors){
		int count = 0;
		List<int> cell_list =AquariumManager.sharkGrid[x_coord,y_coord];
		foreach(int id in cell_list){
			if (id != _ID){
				neighbors.Add (ALL_SHARKS[id]);
			}
		}

		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1}; 
		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT &&
					(x != x_coord || y != y_coord)){
						foreach(int id in AquariumManager.sharkGrid[x,y]){
							neighbors.Add (ALL_SHARKS[id]);
						}
					}
				}
			}
		}
	}

	void PreyBiteReady(List<Fish> prey){
		float distance = float.MaxValue;
		GameObject poorfish = null;

		foreach (Fish fish in prey){
			float d = Vector2.Distance ((Vector2)this.transform.position, 
				(Vector2)fish.transform.position);
			if (d <= distance){
				distance = d;
				if (d < EAT_RADIUS){
					poorfish = fish.gameObject;
				}
			}
		}

		if (EAT_RADIUS < distance && distance < BITE_RADIUS){
			spriteRenderer.sprite = biteSprite;
		} else if (distance < EAT_RADIUS){
			spriteRenderer.sprite = normalSprite;
			Instantiate (blood, poorfish.transform.position, Quaternion.identity);
			this.GetComponent<AudioSource> ().Play ();
			/*AquariumManager.fishGrid [poorfish.GetComponent<Fish> ().getX (),
				poorfish.GetComponent<Fish> ().getY ()].Remove (poorfish.GetComponent<Fish> ());
			AquariumManager.fishDict [poorfish.GetComponent<Fish> ().getX ().ToString () +
			poorfish.GetComponent<Fish> ().getY ().ToString ()].Remove (poorfish.GetComponent<Fish> ().getID ());*/
			AquariumManager.fishGrid[poorfish.GetComponent<Fish> ().getX (),
				poorfish.GetComponent<Fish> ().getY ()].Remove (poorfish.GetComponent<Fish> ().ID);
			Destroy (poorfish);
			Debug.Log ("A fish has been eaten!");
			Fish.FISH_COUNT--;
			SharkWander = 800;
		} else{
			spriteRenderer.sprite = normalSprite;
		}
	}



	Vector2 Hunt(List<Fish> prey){
		Vector2 coherence = Cohere (prey) * COHERE_WEIGHT;
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

	public int ID{
		get{
		return(_ID);
		}
	}

}
