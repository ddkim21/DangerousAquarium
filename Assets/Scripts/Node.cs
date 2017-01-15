using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node{
	public float[] point = new float[KDTree.dimension]; 

	public int ID = -1;
	public Node left;
	public Node right;

	public Node(float [] arr, int fishid){
		for(int i = 0; i < KDTree.dimension; i++){
			point[i] = arr[i];
		}
		ID = fishid;
		left = null;
		right = null;
	}

	public void setID(int id){
		ID = id;
	}

}
 


