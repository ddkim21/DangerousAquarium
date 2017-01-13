using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public class LinkedList : MonoBehaviour {

	private int size = 0;

	public int Count{
		get{ 
			return(size); 
		}
	}

	// Head of list
	private Node head;
	// End of list
	private Node tail;

	public LinkedList(){
		size = 0;
		head = null;
		tail = null;
	}

	public void Add(GameObject content){
		Node newNode = new Node (content);
		if (head == null){
			head = newNode;
			tail = newNode;
		}
		else {
			tail.setNext(newNode);
			Node prevTail = tail;
			tail = newNode;
			tail.setPrev (prevTail);
		}
	}

	public void ListNodes(){
		Node tempNode = head;

		while (tempNode != null){
			Debug.Log (tempNode.data);
			tempNode = tempNode.Next;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
