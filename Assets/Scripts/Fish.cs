using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fish : Agent {
	// Set our weights for the flocking behavior function
	// A la Boids model https://en.wikipedia.org/wiki/Boids
	public static float MAX_SPEED = 10f;
	public static float STEER_CONST = 0.006f;
	public static float ESCAPE_CONST = 0.006f;

	public static float COHERE_WEIGHT = (4f);
	public static float ALIGN_WEIGHT = (4f);
	public static float SEPERATE_WEIGHT = (6f);
	public static float ESCAPE_WEIGHT = (27.5f);
	public static float WALL_WEIGHT = 12.5f;
	public static float DISPERSE_WEIGHT = 100f;
	public static float SEPERATION_RADIUS = 2f;
	// Temp radius, used for finding neighbors
	// will be replaced by K Nearest Neighbors implementation
	public static float TEMP_RADIUS = 15f;
	public static float PREDATION_RADIUS = 10f;

	// Fish width and fish height, useful for figuring out
	// if a fish is about to go out of bounds.
	public static float FISH_WIDTH = 1.125f;
	public static float FISH_HEIGHT = 0.825f;
	// Background width and background height is useful for deterring from walls
	public static float BACKGROUND_HALF_WIDTH = 50f;
	public static float BACKGROUND_HALF_HEIGHT = 28.125f;

	//Lastly, our count of fish in order to keep track of the number of fish
	public static int FISH_COUNT = 0;
	//Set one of the below booleans to true, and the rest false
	//Which one is set true determines which method is used to calculate neighbors
	//Brute force
	public static bool BRUTE_FORCE = false;
	//Grid implementation
	public static bool GRID_IMPLEMENTATION = false;
	//K nearest neighbors grid implementation
	public static bool K_NEIGHBORS_GRID_IMPLEMENTATION = false;
	//KD tree nearest neighbors implementation. 
	//By far the fastest method.
	public static bool KD_TREE_IMPLEMENTATION = true;
	public static int K_NEAREST_NEIGHBORS = 6;

	public static List<Fish> ALL_FISH;
	public static bool STARTED = false;

	// Private Frame Counter for Corner situation
	// Corner situation is a situation where the shark has a fish cornered.
	// Normally the forces applied would trap the fish in the corner
	// and force the fish to spin in circles.
	// Though the application of steering away from walls dampens the frequency in 
	// which this happens, the issue still exists. I have tried other steering methods,
	// and none seem to maintain the type of swimming behavior I desire.
	private int CORNER_ESCAPE_COUNTER = 0;
	private int DISPERSE_COUNTER = 0;
	private Vector2 DISPERSE_LOCATION = new Vector2();

	// Global ID and instance ID, since instantiated objects have same IDs
	private static int _globalID = 0;
	private int _ID = 0;

	void Awake(){
		if (STARTED == false){
			ALL_FISH = new List<Fish>();
			STARTED = true;
		}
		ALL_FISH.Add(this);
		_ID = _globalID;
		_globalID++;
	}

	// Use this for initialization
	void Start () {
		FISH_COUNT++;
		// Start off with a slightly random direction
		setDirection ();
	}
	
	// Update is called once per frame
	void Update () {
		//fishGrid to be used for OnClick event for dispersing the fish.
		AquariumManager.fishGridUpdate(this);


		// First check if we are still running from a corner
		if (CORNER_ESCAPE_COUNTER > 0){
			CORNER_ESCAPE_COUNTER--;
			return;
		}

		// Before executing any of the below functions, first check if we are 
		// about to swim out of the bounds of the aquarium.
		string outofbound = OutOfBounds ();
		if (outofbound != null) { //If about to go out of bounds
			deter (outofbound); // Deter the direction to keep fish inbounds
			setOrientation();
			DISPERSE_COUNTER = 0; //No longer disperse when hitting a wall.
			return;
		}

		Vector2 modifier = new Vector2(0f,0f);

		// Check if we are still in disperse mode (disperse movement away from mouseclick)
		if (DISPERSE_COUNTER > 0){
			modifier += moveDisperse() * DISPERSE_WEIGHT;
			DISPERSE_COUNTER--; 
		}

		// Create and populate list of neighbors
		// or fish within the radius of the current fish.
		// Will be changed to KNN.
		// List<Fish> neighbors = new List<Fish> ();
		// FindNeighbors (neighbors);
		// Also create and populate a list of local predators.
		List<Fish> neighbors = new List<Fish>();
		List<Shark> predators = new List<Shark> ();

		if(BRUTE_FORCE){
			FindNeighborsBrute(neighbors);
			FindPredatorsBrute (predators);
		}

		if(GRID_IMPLEMENTATION){
			if(K_NEIGHBORS_GRID_IMPLEMENTATION){
				FindNeighborsGridKNN(neighbors);
				FindPredatorsGridKNN(predators);
			}
			else{
				FindNeighborsGrid(neighbors);
				FindPredatorsGrid(predators);
			}
		}

		if(KD_TREE_IMPLEMENTATION){
			FindNeighborsKDTree(neighbors);
			FindPredatorsKDTree(predators);
		}

		//Parallel to above for finding sharks.


		//Before steering based on boids and predators, check if we are in a corner situation
		string corner = Cornered(predators); // returns null or corner name
		if (corner != null){
			RunFromCorner (corner);
			setOrientation ();
			return;
		}

		//Add modifier to the velocity.
		//Modifier is determined by the Flock function and Escape function. 
		modifier += Flock(neighbors) + Escape(predators) * ESCAPE_WEIGHT;
		//Also include potential wall buffers
		modifier += steerAwayFromWalls() * WALL_WEIGHT;
		Vector2 prepVelocity = this.GetComponent<Rigidbody2D>().velocity + modifier;
		//Limit speed of fish if necessary
		this.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(prepVelocity,MAX_SPEED);

		setOrientation ();
		// END UPDATE
	}

	void setDirection() {
		// Sets a random direction for the fish. Only used on initialization. 
		Vector2 direction = new Vector2 (Random.Range(-4f,4f), Random.Range(-4f,4f));
		this.GetComponent<Rigidbody2D>().velocity = direction;
	}

	void setOrientation() {
		// Orient sprite in direction of travel
		Vector2 moveDirection = gameObject.GetComponent<Rigidbody2D>().velocity;

		if (moveDirection != Vector2.zero) {
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	void deter(string side){
		// Prevent the fish from escaping the aquarium.
		// Keep the component of the velocity that does not send the fish out of the aquarium
		// and change the component of the velocity that does send the fish out
		// by choosing a random float between .1 and 2 (absolute value) in the opposite direction
		Vector2 deter = new Vector2 (0f,0f);
		if (side == "Up"){
			deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															(-1) * Random.Range(0.1f,2f));
		}
		else if (side == "Down"){
			deter = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x,
															Random.Range(0.1f,2f));
		}
		else if (side == "Left"){
			deter = new Vector2 (Random.Range(0.1f,2f),
				this.GetComponent<Rigidbody2D>().velocity.y);
		}
		else {
			deter = new Vector2 ((-1) * Random.Range(0.1f,2f),
				this.GetComponent<Rigidbody2D>().velocity.y);
		}
		this.GetComponent<Rigidbody2D>().velocity = deter;
	}

	string OutOfBounds(){
		// Checks if the fish is currently out of bounds by using the dimensions and position of the 
		// background image.
		if (this.transform.position.x + FISH_WIDTH > BACKGROUND_HALF_WIDTH){
			return("Right");
		}
		else if (this.transform.position.x - FISH_WIDTH < -BACKGROUND_HALF_WIDTH){
			return("Left");
		}
		else if (this.transform.position.y + FISH_HEIGHT > BACKGROUND_HALF_HEIGHT){
			return("Up");
		}
		else if (this.transform.position.y - FISH_HEIGHT < -BACKGROUND_HALF_HEIGHT){
			return("Down");
		}
		else {
			return(null);
		}
	}

	string Cornered(List<Shark> predators){
		float xpos = this.transform.position.x;
		float ypos = this.transform.position.y;
		float distanceToNearestShark = float.MaxValue;
		string corner = null;
		float wallBuffer = 2.5f;
		float sharkBuffer = 5f;

		// First check if we are in a corner.
		// This is the potentially cheaper operation (always constant)
		// and will allow us to forgo the shark distance check if false
		if (xpos + FISH_WIDTH > BACKGROUND_HALF_WIDTH - wallBuffer &&
		    ypos + FISH_HEIGHT > BACKGROUND_HALF_HEIGHT - wallBuffer) {
			corner = "Top Right";
		} else if (xpos - FISH_WIDTH < -BACKGROUND_HALF_WIDTH + wallBuffer &&
		           ypos + FISH_HEIGHT > BACKGROUND_HALF_HEIGHT - wallBuffer) {
			corner = "Top Left";
		} else if (xpos - FISH_WIDTH < -BACKGROUND_HALF_WIDTH + wallBuffer &&
		           ypos - FISH_HEIGHT < -BACKGROUND_HALF_HEIGHT + wallBuffer) {
			corner = "Bottom Left";
		} else if (xpos + FISH_WIDTH > BACKGROUND_HALF_WIDTH - wallBuffer &&
		         ypos - FISH_HEIGHT < -BACKGROUND_HALF_HEIGHT + wallBuffer) {
			corner = "Bottom Right";
		}

		if (corner != null){
			foreach(Shark shark in predators){
				float distance = Vector2.Distance ((Vector2)this.transform.position,
					(Vector2)shark.transform.position);
				if (distance < distanceToNearestShark)
					distanceToNearestShark = distance;
			}
		}

		if (distanceToNearestShark < sharkBuffer)
			return corner;
		else
			return null;
	}

	void RunFromCorner(string corner){
		Vector2 runaway = new Vector2 (0f, 0f);
		float cointoss = Random.Range (-1f, 1f);
		if (corner == null){
			return;
		}
		else if (corner == "Top Right"){
			if (cointoss >= 0){
				runaway = new Vector2 (Random.Range (-4f, -3f), Random.Range (-1f, 0f));
			}
			else {
				runaway = new Vector2 (Random.Range (-1f, 0f), Random.Range (-4f, -3f));
			}
		}
		else if (corner == "Top Left"){
			if (cointoss >= 0){
				runaway = new Vector2 (Random.Range (3f, 4f), Random.Range (-1f, 0f));
			}
			else {
				runaway = new Vector2 (Random.Range (0f, 1f), Random.Range (-4f, -3f));
			}		
		}
		else if (corner == "Bottom Left"){
			if (cointoss >= 0){
				runaway = new Vector2 (Random.Range (3f, 4f), Random.Range (0f, 1f));
			}
			else {
				runaway = new Vector2 (Random.Range (0f, 1f), Random.Range (3f, 4f));
			}
		}
		else {
			if (cointoss >= 0){
				runaway = new Vector2 (Random.Range (-4f, -3f), Random.Range (0f, 1f));
			}
			else {
				runaway = new Vector2 (Random.Range (-1f, 0f), Random.Range (3f, 4f));
			}
		}
		CORNER_ESCAPE_COUNTER = 30;
		runaway.Normalize ();
		runaway *= MAX_SPEED * (1f/2f);
		this.GetComponent<Rigidbody2D>().velocity = runaway;
	}

	Vector2 steerAwayFromWalls() {
		// Steers away from walls (since bouncing off of walls at all times gets old)
		// Utilizes a buffer between the wall and fish in order to decide when to start steering
		// away from the wall. The closer the fish is to the wall, the larger the magnitude of 
		// the deterrence direction on the desired vector of travel.

		// The desired vector of travel is calculated via maintaining the direction (x or y) 
		// and magnitude of the non deterrence direction of travel (x if the fish is about 
		// to hit the bottom or top walls), and scaling the deterrence direction linearly based 
		// on the distance to the wall itself and in the opposite direction. 
		float buffer = 5f;
		float constantFactor = 5f;

		Vector2 steer = new Vector2 (0f, 0f);

		if (this.transform.position.x + FISH_WIDTH > BACKGROUND_HALF_WIDTH - buffer){
			// Near the right wall
			float distanceToWall = BACKGROUND_HALF_WIDTH - (this.transform.position.x + FISH_WIDTH);
			float desiredXVel = -MAX_SPEED * (buffer - distanceToWall) / buffer *  constantFactor;
			Vector2 desired = new Vector2 (desiredXVel, this.GetComponent<Rigidbody2D> ().velocity.y);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.x - FISH_WIDTH < -BACKGROUND_HALF_WIDTH + buffer){
			// Near the left wall
			float distanceToWall = (this.transform.position.x - FISH_WIDTH) - BACKGROUND_HALF_WIDTH;
			float desiredXVel = MAX_SPEED * (buffer - distanceToWall) / buffer * constantFactor;
			Vector2 desired = new Vector2 (desiredXVel, this.GetComponent<Rigidbody2D> ().velocity.y);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.y + FISH_HEIGHT > BACKGROUND_HALF_HEIGHT - buffer){
			// Near the top wall
			float distanceToWall = BACKGROUND_HALF_HEIGHT - (this.transform.position.y + FISH_HEIGHT);
			float desiredYvel = -MAX_SPEED * (buffer - distanceToWall) / buffer * constantFactor;
			Vector2 desired = new Vector2 (this.GetComponent<Rigidbody2D> ().velocity.x, desiredYvel);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}
		else if (this.transform.position.y - FISH_HEIGHT < -BACKGROUND_HALF_HEIGHT + buffer){
			// Near the bottom wall
			float distanceToWall = (this.transform.position.y - FISH_HEIGHT) - BACKGROUND_HALF_HEIGHT;
			float desiredYvel = MAX_SPEED * (buffer - distanceToWall) / buffer * constantFactor;
			Vector2 desired = new Vector2 (this.GetComponent<Rigidbody2D> ().velocity.x, desiredYvel);
			steer = desired - this.GetComponent<Rigidbody2D> ().velocity;
			steer = Vector2.ClampMagnitude(steer, STEER_CONST);
		}

		return(steer);
	}

	void FindNeighborsBrute(List<Fish> neighbors){
		foreach(GameObject fish in GameObject.FindGameObjectsWithTag("Fish")){
			float distance = Vector3.Distance (this.transform.position, fish.transform.position);
			if (distance != 0 && distance < TEMP_RADIUS){
				neighbors.Add (fish.GetComponent<Fish>());
			}
		}
	}

	void FindNeighborsGrid(List<Fish> neighbors){
		//Returns all fish in neighboring cells. 
		//A neighboring cell is either the cell the fish is currently in, or a cell that is adjacent to
		//the cell the fish is currently in. 
		//This is done through accessing the 2d list of int lists in Aquarium manager.
		List<int> cell_list =AquariumManager.fishGrid[x_coord,y_coord];
		foreach(int id in cell_list){
			if (id != _ID){
				neighbors.Add (ALL_FISH[id]);
			}
		}

		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1}; 
		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT &&
					(x != x_coord || y != y_coord)){
						foreach(int id in AquariumManager.fishGrid[x,y]){
							neighbors.Add (ALL_FISH[id]);
						}
					}
				}
			}
		}
	}

	void FindNeighborsGridKNN(List<Fish> neighbors){
		//Finds the K-nearest neighbors within the grid through accessing neighboring grid int lists
		//This function iterates through the neighbors and finds the k nearest neighbors through conducting linear search.
		int count = 0;
		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1};

		//Create a list of copies of int lists from Aquarium Manager
		//Necessary to copy these lists, since we want to delete ints from the lists
		//and the other fish need access to them too.
	
		List<List<int>> neighboringCells = new List<List<int>>();

		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT){
						List<int> cell = new List<int>(AquariumManager.fishGrid[x,y]); //copy the list itself, instead of a reference
						neighboringCells.Add(cell);
					}
				}
			}
		}

		//Next is the linear search to find the K-nearest neighbors.
		while (count < K_NEAREST_NEIGHBORS){
			float distance = float.MaxValue;
			int nearestID = -1;
			for (int i = 0; i < neighboringCells.Count; i++){
				for(int j = 0; j < neighboringCells[i].Count; j++){
					float d = Vector3.Distance(this.transform.position, ALL_FISH[neighboringCells[i][j]].transform.position);
					if (d< distance && distance > 0){
						distance = d;
						nearestID = neighboringCells[i][j];
						neighboringCells[i].RemoveAt(j);
					}
				}
			}
			if (nearestID == -1)
				return;
			neighbors.Add(ALL_FISH[nearestID]);
			count++;
		}

	}

	void FindNeighborsKDTree(List<Fish> neighbors){
		//This list is used to save the nearest neighbors found so far
		//to ensure that we get unique KNNs.
		List<int> neighborIDs = new List<int>();
		Node Tree = AquariumManager.fishTree;
		float[] position = new float[] {this.transform.position.x, this.transform.position.y};
		int count = 0;
		while (count < K_NEAREST_NEIGHBORS){
			//Find the nearest neighbor (may return null)
			ID fishid = KDTree.nearestNeighbor(Tree, position, neighborIDs);
			if (fishid != null){
				neighborIDs.Add(fishid.id);
				if(fishid.id != _ID){
				//Ensure that the nearest neighbor found is not itself.
					if(ALL_FISH[fishid.id] != null){
						neighbors.Add(ALL_FISH[fishid.id]);
						count++;
					}
				}
			}
			else {
				return;
			}
		}
	}

	void FindPredatorsBrute(List<Shark> predators){
		// Works exactly the same way as FindNeighbors, except for sharks
		foreach(GameObject shark in GameObject.FindGameObjectsWithTag("Shark")){
			float distance = Vector3.Distance (this.transform.position, shark.transform.position);
			if (distance != 0 && distance < TEMP_RADIUS){
				predators.Add (shark.GetComponent<Shark>());
			}
		}
	}

	void FindPredatorsGrid(List<Shark> predators){
		//See the FindNeighborsGrid comments in the Fish Class.

		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1}; 
		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT){
						foreach(int id in AquariumManager.sharkGrid[x,y]){
							predators.Add (Shark.ALL_SHARKS[id]);
						}
					}
				}
			}
		}
	}

	void FindPredatorsGridKNN(List<Shark> predators){
		//See the FindNeighborsGridKNN comments in the Fish Class.
		int count = 0;
		int[] xindices = new int[3]{x_coord-1, x_coord, x_coord + 1};
		int[] yindices = new int[3]{y_coord-1, y_coord, y_coord + 1};
	
		List<List<int>> neighboringCells = new List<List<int>>();

		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT){
						List<int> cell = new List<int>(AquariumManager.sharkGrid[x,y]); //copy the list itself, instead of a reference
						neighboringCells.Add(cell);
					}
				}
			}
		}

		while (count < K_NEAREST_NEIGHBORS){
			float distance = float.MaxValue;
			int nearestID = -1;
			for (int i = 0; i < neighboringCells.Count; i++){
				for(int j = 0; j < neighboringCells[i].Count; j++){
					float d = Vector3.Distance(this.transform.position, Shark.ALL_SHARKS[neighboringCells[i][j]].transform.position);
					if (d< distance && distance > 0){
						distance = d;
						nearestID = neighboringCells[i][j];
						neighboringCells[i].RemoveAt(j);
					}
				}
			}
			if (nearestID == -1)
				return;
			predators.Add(Shark.ALL_SHARKS[nearestID]);
			count++;
		}

	}

	void FindPredatorsKDTree(List<Shark> predators){
		// See comments of FindNeighborsKDTree method of fish class.
		List<int> neighborIDs = new List<int>();
		Node Tree = AquariumManager.sharkTree;
		float[] position = new float[] {this.transform.position.x, this.transform.position.y};
		int count = 0;
		while (count < K_NEAREST_NEIGHBORS){
			ID sharkid = KDTree.nearestNeighbor(Tree, position, neighborIDs);
			if (sharkid != null){
				neighborIDs.Add(sharkid.id);
				predators.Add(Shark.ALL_SHARKS[sharkid.id]);
				count++;
			}
			else {
				return;
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
		// Seperate functions by finding the fish within its SEPERATION_RADIUS
		// If a fish is within its seperation radius, we tell the fish to steer
		// in the direction that is directly away from the fish within its seperation radius

		// Before telling the fish to steer away however, we add up all the "veer off" vectors
		// inversely by their distance from the fish. In other words, the closer a fish is
		// the more influence they will have on the primary fish's movement.

		// We then find the average, and tell the fish to steer in that direction.

		Vector2 avoid = new Vector2 (0f, 0f);
		int count = 0;

		foreach (Fish fish in neighbors) {
			float distance = Vector3.Distance (this.transform.position, fish.transform.position);
			if (distance > 0 && distance < SEPERATION_RADIUS){
				Vector2 veerOff = (Vector2)this.transform.position - (Vector2)fish.transform.position;
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

	Vector2 Escape(List<Shark> predators){
		// Functions nearly exactly similarly to Seperate
		// Escape functions by finding the shark within its PREDATION_RADIUS
		// If a shark is within its predation radius, we tell the fish to steer
		// in the direction that is directly away from the shark

		// Before telling the fish to steer away however, we add up all the "veer off" vectors
		// scaled inversely by their distance from the sharks. In other words, the closer a shark is
		// the more influence they will have on the fish's movement.

		// We then find the average, and tell the fish to steer in that direction.

		Vector2 avoid = new Vector2 (0f, 0f);
		int count = 0;

		foreach (Shark shark in predators) {
			float distance = Vector3.Distance (this.transform.position, shark.transform.position);
			if (distance < PREDATION_RADIUS){
				Vector2 veerOff = (Vector2)this.transform.position - (Vector2)shark.transform.position;
				veerOff.Normalize ();
				veerOff *= (1/distance);
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

	public int ID{
		get{
		return(_ID);
		}
	}

	public void setDisperse(int time, Vector2 location){
		DISPERSE_COUNTER = time;
		DISPERSE_LOCATION = location;
	}

	public Vector2 moveDisperse(){
		Vector2 currentPos = (Vector2)this.transform.position;
		float distance = Vector2.Distance(currentPos, DISPERSE_LOCATION);
		Vector2 desired = currentPos - DISPERSE_LOCATION;
		desired.Normalize();
		desired /= distance;
		return(steerTo(desired));
	}
}