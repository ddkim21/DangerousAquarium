using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Agent : MonoBehaviour {
	protected int x_coord = -1;
	protected int y_coord = -1;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setX(int x){
		x_coord = x;
	}

	public void setY(int y){
		y_coord = y;
	}

	public int getX(){
		return (x_coord);
	}

	public int getY(){
		return (y_coord);
	}

}
