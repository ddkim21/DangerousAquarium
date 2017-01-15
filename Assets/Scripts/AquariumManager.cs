using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquariumManager : MonoBehaviour {

	public GameObject fish;
	public GameObject shark;

	public GameObject fishes;
	public GameObject sharks;

	public GameObject fish_counter;
	public GameObject shark_counter;

	public static float BACKGROUND_HALF_WIDTH = 50f;
	public static float BACKGROUND_HALF_HEIGHT = 28.125f;

	public static float SQUARE_LENGTH = 10f;
	public static int NORMAL_SQUARE_COUNT = 4;
	public static float ALTERED_SQUARE_LENGTH = 8.125f;
	public static int VERTICAL_SQUARE_COUNT = 6;
	public static int HORIZONTAL_SQUARE_COUNT = 10;

	public static List<int>[,] fishGrid;
	public static List<int>[,] sharkGrid;

	public static Node fishTree;
	public static Node sharkTree;

	void Start (){

		fishTree = null;
		sharkTree = null;

		fishGrid = new List<int>[HORIZONTAL_SQUARE_COUNT, VERTICAL_SQUARE_COUNT];
		sharkGrid = new List<int>[HORIZONTAL_SQUARE_COUNT,VERTICAL_SQUARE_COUNT];

		for (int x = 0; x < HORIZONTAL_SQUARE_COUNT; x++)
			for (int y = 0; y < VERTICAL_SQUARE_COUNT; y++) {
				fishGrid[x, y] = new List<int>();
				sharkGrid[x,y] = new List<int>();
			}
	}

	void Update() {
		//Print FPS
		print(1.0f / Time.deltaTime);

		//On mouse click, make all the fish around the area clicked disperse
		if (Input.GetMouseButtonDown(0)){
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 position = (Vector2)point;
			DisperseAgents(position);
		}

		// Build fish KD Tree.
		fishTree = null;

		foreach(GameObject fish in GameObject.FindGameObjectsWithTag("Fish")){
			fishTree = KDTree.insert(fishTree, new float[] {fish.transform.position.x,
			fish.transform.position.y}, fish.GetComponent<Fish>().ID);
		}

		// Build shark KD Tree.
		sharkTree = null;

		foreach(GameObject shark in GameObject.FindGameObjectsWithTag("Shark")){
			sharkTree = KDTree.insert(sharkTree, new float[] {shark.transform.position.x,
			shark.transform.position.y}, shark.GetComponent<Shark>().ID);
		}

		//Update text for fish and shark counters in the Aquarium
		fish_counter.GetComponent<UnityEngine.UI.Text>().text = Fish.FISH_COUNT.ToString() + " Fish";
		shark_counter.GetComponent<UnityEngine.UI.Text>().text = Shark.SHARK_COUNT.ToString() + " Sharks";
	}

	public void AddTenFish(){
		float buffer = 5f;
		int count = 10;
		while (count > 0) {
			float randomX = Random.Range (-BACKGROUND_HALF_WIDTH + buffer,
				                BACKGROUND_HALF_WIDTH - buffer);
			float randomY = Random.Range (-BACKGROUND_HALF_HEIGHT + buffer,
				                BACKGROUND_HALF_HEIGHT - buffer);
			Vector3 position = new Vector3 (randomX, randomY, -1);
			GameObject thisFish = Instantiate (fish, position, Quaternion.identity);
			thisFish.tag = "Fish";
			thisFish.transform.parent = fishes.transform;
			count--;
		}
		fish_counter.GetComponent<UnityEngine.UI.Text>().text = (Fish.FISH_COUNT + 10).ToString() + " Fish";
	}

	public void AddTwoSharks() {
		float buffer = 5f;
		int count = 2;
		while (count > 0) {
			float randomX = Random.Range (-BACKGROUND_HALF_WIDTH + buffer,
				BACKGROUND_HALF_WIDTH - buffer);
			float randomY = Random.Range (-BACKGROUND_HALF_HEIGHT + buffer,
				BACKGROUND_HALF_HEIGHT - buffer);
			Vector3 position = new Vector3 (randomX, randomY, -1);
			GameObject thisShark = Instantiate (shark, position, Quaternion.identity);
			thisShark.tag = "Shark";
			thisShark.transform.parent = sharks.transform;
			count--;
		}
		shark_counter.GetComponent<UnityEngine.UI.Text>().text = (Shark.SHARK_COUNT + 2).ToString() + " Sharks";
	}

	public string FishCounter(){
		return (Fish.FISH_COUNT.ToString ());
	}

	public string SharkCounter(){
		return (Shark.SHARK_COUNT.ToString ());
	}

	public static void fishGridUpdate(Fish fish){
		float xposition = fish.transform.position.x;
		float yposition = fish.transform.position.y;

		xposition += BACKGROUND_HALF_WIDTH;
		yposition += BACKGROUND_HALF_HEIGHT;
		int grid_x = (int)(xposition / SQUARE_LENGTH);
		int grid_y = 0;

		if (0 <= yposition && yposition < ALTERED_SQUARE_LENGTH)
			grid_y = 0;
		else if (yposition >= NORMAL_SQUARE_COUNT * SQUARE_LENGTH + ALTERED_SQUARE_LENGTH)
			grid_y = NORMAL_SQUARE_COUNT + 1;
		else {
			grid_y = (int)((yposition - ALTERED_SQUARE_LENGTH) / SQUARE_LENGTH);
			grid_y++;
		}

		if (fish.getX() == -1){
			fishGrid [grid_x, grid_y].Add (fish.ID);
			fish.setX (grid_x);
			fish.setY (grid_y);
		}
		else if (grid_x != fish.getX() || grid_y != fish.getY()){
			fishGrid [fish.getX (), fish.getY ()].Remove (fish.ID);
			fishGrid [grid_x, grid_y].Add (fish.ID);
			fish.setX (grid_x);
			fish.setY (grid_y);
		}
	}

	public static void sharkGridUpdate(Shark shark){
		float xposition = shark.transform.position.x;
		float yposition = shark.transform.position.y;

		xposition += BACKGROUND_HALF_WIDTH;
		yposition += BACKGROUND_HALF_HEIGHT;
		int grid_x = (int)(xposition / SQUARE_LENGTH);
		int grid_y = 0;

		if (0 <= yposition && yposition < ALTERED_SQUARE_LENGTH)
			grid_y = 0;
		else if (yposition >= NORMAL_SQUARE_COUNT * SQUARE_LENGTH + ALTERED_SQUARE_LENGTH)
			grid_y = NORMAL_SQUARE_COUNT + 1;
		else {
			grid_y = (int)((yposition - ALTERED_SQUARE_LENGTH) / SQUARE_LENGTH);
			grid_y++;
		}

		if (shark.getX() == -1){
			sharkGrid [grid_x, grid_y].Add (shark.ID);
			shark.setX (grid_x);
			shark.setY (grid_y);
		}
		else if (grid_x != shark.getX() || grid_y != shark.getY()){
			sharkGrid [shark.getX (), shark.getY ()].Remove (shark.ID);
			sharkGrid [grid_x, grid_y].Add (shark.ID);
			shark.setX (grid_x);
			shark.setY (grid_y);
		}
	}

	public static void DisperseAgents(Vector2 position){
		int disperseTime = 80;
		float xposition = position.x;
		float yposition = position.y;

		xposition += BACKGROUND_HALF_WIDTH;
		yposition += BACKGROUND_HALF_HEIGHT;

		// Find the grid cells that the mouse click position corresponds to.
		int grid_x = (int)(xposition / SQUARE_LENGTH);
		int grid_y = 0;

		if (0 <= yposition && yposition < ALTERED_SQUARE_LENGTH)
			grid_y = 0;
		else if (yposition >= NORMAL_SQUARE_COUNT * SQUARE_LENGTH + ALTERED_SQUARE_LENGTH)
			grid_y = NORMAL_SQUARE_COUNT + 1;
		else {
			grid_y = (int)((yposition - ALTERED_SQUARE_LENGTH) / SQUARE_LENGTH);
			grid_y++;
		}

		int[] xindices = new int[3]{grid_x-1, grid_x, grid_x + 1};
		int[] yindices = new int[3]{grid_y-1, grid_y, grid_y + 1}; 

		foreach (int x in xindices){
			if (x >= 0 && x < AquariumManager.HORIZONTAL_SQUARE_COUNT){
				foreach (int y in yindices){
					if (y >= 0 && y < AquariumManager.VERTICAL_SQUARE_COUNT){
						foreach(int id in AquariumManager.fishGrid[x,y]){
							if (Vector2.Distance((Vector2)(Fish.ALL_FISH[id].transform.position) , position) < 
							Fish.TEMP_RADIUS){
								Fish.ALL_FISH[id].setDisperse(disperseTime, position);
							}
						}
						foreach(int id in AquariumManager.sharkGrid[x,y]){
							if (Vector2.Distance((Vector2)(Shark.ALL_SHARKS[id].transform.position) , position) < 
							Shark.TEMP_RADIUS){
								Shark.ALL_SHARKS[id].setDisperse(disperseTime, position);
							}
						}
					}
				}
			}
		}

	}


	//END OF FILE
}
