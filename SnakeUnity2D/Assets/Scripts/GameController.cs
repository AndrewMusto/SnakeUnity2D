﻿using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject prefab;

	// Use this for initialization
	void Start () {
		GameObject pre = Instantiate (prefab) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
