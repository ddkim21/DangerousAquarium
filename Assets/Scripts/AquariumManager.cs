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

	public static float SQUARE_LENGTH = 5f;
	public static int NORMAL_SQUARE_COUNT = 10;
	public static float ALTERED_SQUARE_LENGTH = 3.125f;
	public static int VERTICAL_SQUARE_COUNT = 12;
	public static int HORIZONTAL_SQUARE_COUNT = 20;
	/*
	public static List<Fish> [,] fishGrid;
	public static List<Shark> [,] sharkGrid;
	public static Dictionary<string, Dictionary<int, Fish>> fishDict;
	public static Dictionary<string, Dictionary<int, Shark>> sharkDict;*/


	void Start (){

		/*
		fishDict = new Dictionary<string, Dictionary<int, Fish>>();
		sharkDict = new Dictionary<string, Dictionary<int, Shark>> ();*/

		/*fishGrid = new List<Fish>[HORIZONTAL_SQUARE_COUNT,VERTICAL_SQUARE_COUNT];
		sharkGrid = new List<Shark>[HORIZONTAL_SQUARE_COUNT, VERTICAL_SQUARE_COUNT];*/

		/*for (int x = 0; x < HORIZONTAL_SQUARE_COUNT; x++)
			for (int y = 0; y < VERTICAL_SQUARE_COUNT; y++) {
				fishGrid [x, y] = new List<Fish> ();
				sharkGrid [x, y] = new List<Shark> ();
			}*/
	}

	void Update() {
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
	/*
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
			fishGrid [grid_x, grid_y].Add (fish);
			fish.setX (grid_x);
			fish.setY (grid_y);
		}
		else if (grid_x != fish.getX() || grid_y != fish.getY()){
			fishGrid [fish.getX (), fish.getY ()].Remove (fish);
			fishGrid [grid_x, grid_y].Add (fish);
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
			sharkGrid [grid_x, grid_y].Add (shark);
			shark.setX (grid_x);
			shark.setY (grid_y);
		}
		else if (grid_x != shark.getX() || grid_y != shark.getY()){
			sharkGrid [shark.getX (), shark.getY ()].Remove (shark);
			sharkGrid [grid_x, grid_y].Add (shark);
			shark.setX (grid_x);
			shark.setY (grid_y);
		}
	}*/
	/*
	public static void fishDictUpdate(Fish fish){
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

		string coordinates = grid_x.ToString () + grid_y.ToString ();

		if (fish.getX() == -1){
			if (!fishDict.ContainsKey(coordinates)){
				Dictionary<int, Fish> fishes = new Dictionary<int,Fish>();
				fishes.Add (fish.getID(), fish);
				fishDict.Add (coordinates, fishes);
			}
			else {
				fishDict [coordinates].Add (fish.getID(),fish);
			}
			fish.setX (grid_x);
			fish.setY (grid_y);
		}

		else if (grid_x != fish.getX() || grid_y != fish.getY()){
			string oldcoordinates = fish.getX ().ToString () + fish.getY ().ToString ();
			fishDict [oldcoordinates].Remove (fish.getID ());
			if (fishDict[oldcoordinates].Count == 0){
				fishDict.Remove (oldcoordinates);
			}
			if (!fishDict.ContainsKey(coordinates)){
				Dictionary<int, Fish> fishes = new Dictionary<int,Fish>();
				fishes.Add (fish.getID(), fish);
				fishDict.Add (coordinates, fishes);
			}
			else {
				fishDict [coordinates].Add (fish.getID (), fish);
			}
			fish.setX (grid_x);
			fish.setY (grid_y);
		}
	}

	public static void sharkDictUpdate(Shark shark){
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

		string coordinates = grid_x.ToString () + grid_y.ToString ();

		if (shark.getX() == -1){
			if (!sharkDict.ContainsKey(coordinates)){
				Dictionary<int, Shark> sharks = new Dictionary<int,Shark>();
				sharks.Add (shark.getID(), shark);
				sharkDict.Add (coordinates, sharks);
			}
			else {
				sharkDict [coordinates].Add (shark.getID(),shark);
			}
			shark.setX (grid_x);
			shark.setY (grid_y);
		}

		else if (grid_x != shark.getX() || grid_y != shark.getY()){
			string oldcoordinates = shark.getX ().ToString () + shark.getY ().ToString ();
			sharkDict [oldcoordinates].Remove (shark.getID ());
			if (sharkDict[oldcoordinates].Count == 0){
				sharkDict.Remove (oldcoordinates);
			}
			if (!sharkDict.ContainsKey(coordinates)){
				Dictionary<int, Shark> sharks = new Dictionary<int,Shark>();
				sharks.Add (shark.getID(), shark);
				sharkDict.Add (coordinates, sharks);
			}
			else {
				sharkDict [coordinates].Add (shark.getID (), shark);
			}
			shark.setX (grid_x);
			shark.setY (grid_y);
		}
	}*/
}
