using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class KDTree{
	// Implement KD tree, a binary tree where each subtree is sorted
	// based on the x or y coordinate, depending on the level of the node.
	// Each node (not just leaves) contains a x,y coordinate pair.

	// Much of the construction, insertion, deletion methods were created through
	// the help of online resources. The exception is the nearest neighbor search.

	// The nearest neighbor search given a node, was constructed through
	// utilizing my knowledge of how to efficiently search through the trees, and was customly
	// created to accomodate use in my Boids implementation.

	// The nearest neighbor search function is arguably the most interesting, and I have commented
	// it to help with understanding the implementation.

	public const int dimension = 2;

	public static Node insertHelper(Node root, float[] point, int depth, int fishid){
		if (root == null)
			return(new Node(point, fishid));

		int coord = depth % dimension;

		if (point[coord] < root.point[coord])
			root.left = insertHelper(root.left, point, depth+1, fishid);
		else
			root.right = insertHelper(root.right, point, depth+1, fishid);
		
		return root;
	}

	public static Node insert(Node root, float[] point, int fishid){
		return insertHelper(root, point, 0, fishid);
	}

	//Find min of 3 nodes, given dimension
	public static Node minNode(Node x, Node y, Node z, int d){
		Node min = x;
		if (y != null && y.point[d] < min.point[d])
			min = y;
		if (z != null && z.point[d] < min.point[d])
			min = z;
		return(min);
	}

	public static Node findMinNode(Node root, int d, int depth){

		if (root == null)
			return null;
		

		int coord = depth % dimension;

		if (coord == d){
			if (root.left == null)
				return root;
			return findMinNode(root.left, d, depth+1);
		}

		return minNode(root, findMinNode(root.left, d, depth+1), findMinNode(root.right, d, depth+1), d);
	}

	public static Node findMin(Node root, int d){
		return findMinNode(root, d, 0);
	}

	public static bool arePointsSame(float [] point1, float [] point2){
		for (int i = 0; i < dimension; i++){
			if (point1[i] != point2[i])
				return false;
		}
		return true;
	}


	public static void copyPoint(float [] point1, float [] point2){
		for (int i = 0; i<dimension; i++)
			point1[i] = point2[i];
	}

	public static Node deleteNodeHelper(Node root, float [] point, int depth){
		if (root == null)
			return null;

		int coordinate = depth % dimension;

		if (arePointsSame(root.point, point)){
			if (root.right != null){
				Node min = findMin(root.right, coordinate);
				copyPoint(root.point, min.point);
				root.right = deleteNodeHelper(root.right, min.point, depth+1);
			}
			else if (root.left != null){
				Node min = findMin(root.left, coordinate);
				copyPoint(root.point, min.point);
				root.right = deleteNodeHelper(root.left, min.point, depth+1);
			}
			else {
				return null;
			}
			return root;
		}

		if (point[coordinate] < root.point[coordinate])
			root.left = deleteNodeHelper(root.left, point, depth+1);
		else
			root.right = deleteNodeHelper(root.right, point, depth+1);
		return root;
	}

	public static Node deleteNode(Node root, float [] point){
		return deleteNodeHelper(root, point, 0);
	}

	public static void nearestNeighborHelper(Node root, float[] point, Distance d, ID fish, int depth, List<int> neighbors){
		// Main function to be concerned about, used to find nearest neighbors for the fish and sharks
		// Works via searching the side of the grid (split by a different axis each level in the tree by the coordinates
		// found at the node) whos x or y coordinate (depending on the depth) is closer to the destination point.
		// If distanceToCurrent (which keeps track of the smallest distance so far) is less than
		// the x or y distance between the destination point and current node, then we do not have to search
		// the right side of the tree, since any node on the right side of the tree MUST have a greater 
		// distance than the node that corresponds to distanceToCurrent.

		// The above reason is the main benefit of KD trees, as it is frequently the case that we do not have
		// to search a side of the tree. Thus, we on average search around log(n) nodes. 

		if (root == null){
			return;
		}

		float distanceToCurrent = calcDistance(root, point);
		// Want to find neighbors that are within the TEMP_RADIUS of the destination point.
		// Since I am not cloning the trees, we cannot delete nodes (as they are used by the other fish).
		// Thus, we also pass the current already found neighbors of a fish, and ensure that we have not
		// saved the neighbor in our list already. This ensures unique K nearest neighbors.
		if (distanceToCurrent < d.distance && distanceToCurrent > 0 && distanceToCurrent < Fish.TEMP_RADIUS &&
			!neighbors.Contains(root.ID)){
			d.distance = distanceToCurrent;
			fish.id = root.ID;
		}

		int axis = depth % dimension;

		if(point[axis] < root.point[axis]){ 
			// Search the side whose x or y coord is closer
			nearestNeighborHelper(root.left, point, d, fish, depth + 1, neighbors);
			// If the current saved distance is greater than the difference
			// between the x or y coordinates of the node and destination point,
			// then we must search the other side as well as it may contain a closer neighbor.
			if(root.point[axis] - point[axis] < d.distance)
				nearestNeighborHelper(root.right, point, d, fish, depth + 1, neighbors);
		}
		else if (point[axis] > root.point[axis]){
			//See comments for first case.
			nearestNeighborHelper(root.right, point, d, fish, depth + 1, neighbors);
			if(point[axis] - root.point[axis] < d.distance)
				nearestNeighborHelper(root.left, point, d, fish, depth+1, neighbors);
		}
		else{
			//There are cases where we land upon the exact point, but we 
			//do not want to use this point as a neighbor of a fish does not include itself.
			//In this case, there may be a nearest neighbor on each side, so we search both.
			nearestNeighborHelper(root.right, point, d, fish, depth + 1, neighbors);
			nearestNeighborHelper(root.left, point, d, fish, depth+1, neighbors);
		}

	} 

	public static ID nearestNeighbor(Node root, float[] point, List<int> neighbors){
		Distance d = new Distance(float.MaxValue);
		ID fish = new ID(-1);
		nearestNeighborHelper(root, point, d, fish, 0, neighbors);
		if (d.distance == float.MaxValue && fish.id == -1){
			return(null);
		}
		return(fish);
	}

	public static float calcDistance(Node root, float[] point){
		float x_diff = root.point[0] - point[0];
		float y_diff = root.point[1] - point[1];
		return( Mathf.Sqrt(Mathf.Pow(x_diff,2f) + Mathf.Pow(y_diff,2f)) );
	}
		


}
