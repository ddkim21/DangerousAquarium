using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ID{
	//Not the best name for a class, since ID is a private variable for both sharks and fish.
	//Thankfully, one uses capitals (the sharks and fish), whereas this class uses lowercase.
	//It was late at the time. 

	public int id = -1;
	public float [] coordinates = new float[2];

	public ID(int number){
		id = number;
	}

}
