using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{
	public GameObject Content;
	public Node Previous;
	public Node Next;

	public Node(GameObject content){
		Content = content;
		Previous = null;
		Next = null;
	}

	public void setPrev(Node node){
		Previous = node;
	}

	public void setNext(Node node){
		Next = node;
	}

	public GameObject data{
		get{
			return(Content);
		}
	}

	public void removeSelf(){
		if (Previous != null){
			Previous.setNext(Next);
		}
		if (Next != null){
			Next.setPrev(Previous);
		}
		Previous = null;
		Next = null;
	}
}
