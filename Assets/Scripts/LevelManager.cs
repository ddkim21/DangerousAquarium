using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	public void LoadLevel(string name) {
		Debug.Log ("New Level load: " + name);
		if(name == "Aquarium"){
			Fish.FISH_COUNT = 0;
			Shark.SHARK_COUNT = 0;
		}
		SceneManager.LoadScene(name);
	}

	public void QuitRequest() {
		Debug.Log("Quit requested");
		Application.Quit();
	}

	public void LoadNextLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
