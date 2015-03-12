using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public enum Direction : int { Up, Right, Down, Left };

public class GameController : MonoBehaviour {

	//debug parameters and objects
	public GUIText dir;

	//tweakable parameters
	public int gridSize = 10;
	public float timeSlice =1f;
	public int foodAmount = 1;
	public int obstacleAmount = 8;
	public GameObject snakeObject;	//snakeObject should be a prefab with a SnakeScript applied to it

	//holds level tiles, adjustable in inspector:
	public FloorTile floorTiles;
	public FoodTile foodTiles;
	
	//grid
	[HideInInspector] public Vector3[,] grid;

	//level dynamics, info for inter-class communication
	private SnakeScript snake;
	private int foodSpawned = 0;
	private Direction direction = Direction.Up;
	private Direction nextDir = Direction.Up;
	private bool gameOver = false;

	//GUI
	private float buttonWidth;
	private Rect upButton;
	private Rect rightButton;
	private Rect downButton;
	private Rect leftButton;

	//level setup & housekeeping
	private Camera cam;
	private GameObject floorTilesHolder;
	
	[Serializable]
	public class FloorTile  {
		public Sprite basicFloorTile;
		public Sprite outerWallTile;
		public Sprite obstacleTile;

		public void SpawnFloorTile(Vector3 position){
			GameObject tile = new GameObject();
			tile.transform.position = position;
			tile.name = "floorTile";
			SpriteRenderer sprite = tile.AddComponent<SpriteRenderer> ();
			sprite.sprite = basicFloorTile;
			sprite.sortingLayerName = "backGround";
			GameObject floorTilesHolder = GameObject.FindGameObjectWithTag("FloorTiles");
			if (floorTilesHolder == null) {
				floorTilesHolder = new GameObject();
				floorTilesHolder.tag = "FloorTiles";
				floorTilesHolder.name = "FloorTiles";
				Debug.Log ("problem with cleanup objects");
			}
			//parent to empty "FloorTilesHolder" gameObject to reduce clutter. Creates if nonexistant.
			tile.transform.SetParent (floorTilesHolder.transform);
		}

		public void SpawnOuterWallTile(Vector3 position){
			GameObject tile = new GameObject();
			tile.transform.position = position;
			tile.name = "wallTile";
			SpriteRenderer sprite = tile.AddComponent<SpriteRenderer> ();
			sprite.sprite = outerWallTile;
			tile.AddComponent<BoxCollider2D>();
			sprite.sortingLayerName = "backGround";
			GameObject floorTilesHolder = GameObject.FindGameObjectWithTag("FloorTiles");
			if (floorTilesHolder == null) {
				floorTilesHolder = new GameObject();
				floorTilesHolder.tag = "FloorTiles";
				floorTilesHolder.name = "FloorTiles";
				Debug.Log ("problem with cleanup objects");
			}
			//parent to empty "FloorTilesHolder" gameObject to reduce clutter. Creates if nonexistant.
			tile.transform.SetParent (floorTilesHolder.transform);
		}

		public void SpawnObstacleTile(Vector3 position){
			GameObject tile = new GameObject();
			tile.transform.position = position;
			tile.name = "obstacleTile";
			SpriteRenderer sprite = tile.AddComponent<SpriteRenderer> ();
			sprite.sprite = obstacleTile;
			tile.AddComponent<BoxCollider2D>();
			sprite.sortingLayerName = "foreGround";
			GameObject floorTilesHolder = GameObject.FindGameObjectWithTag("FloorTiles");
			if (floorTilesHolder == null) {
				floorTilesHolder = new GameObject();
				floorTilesHolder.tag = "FloorTiles";
				floorTilesHolder.name = "FloorTiles";
				Debug.Log ("problem with cleanup objects");
			}
			//parent to empty "FloorTilesHolder" gameObject to reduce clutter. Creates if nonexistant.
			tile.transform.SetParent (floorTilesHolder.transform);
		}
	}

	[Serializable]
	public class FoodTile  {
		public Sprite food;
		
		public void SpawnFood(Vector3 position){
			GameObject tile = new GameObject();
			tile.transform.position = position;
			tile.tag = "food";
			tile.name = "food";
			BoxCollider2D box = tile.AddComponent<BoxCollider2D> ();
			tile.AddComponent<DestroyOnEnter>();
			box.isTrigger = true;
			SpriteRenderer sprite = tile.AddComponent<SpriteRenderer> ();
			sprite.sprite = food;
			sprite.sortingLayerName = "foreGround";
		}
	}

	// Use this for initialization
	void Start () {
		grid = new Vector3[gridSize, gridSize]; 
		CreateCleanupObjects ();
		CreateGrid ();
		SetGUI ();
		snakeObject = GameObject.FindGameObjectWithTag ("Player");
		snake = snakeObject.GetComponent<SnakeScript> ();
		snake.spawnSnake ();
		StartCoroutine (nextTurn ());
	}
	
	// Update is called once per frame
	void Update () {
		CheckDirection ();
	}

	void OnGUI(){
		UpdateDirection();
	}
	
	void CreateCleanupObjects (){
		floorTilesHolder = GameObject.FindGameObjectWithTag("FloorTiles");
		if (floorTilesHolder == null) {
			floorTilesHolder = new GameObject();	
			floorTilesHolder.tag = "FloorTiles";
			floorTilesHolder.name = "FloorTiles";
		}
	}

	void UpdateLevel(){
		while (foodSpawned < foodAmount) {
			int x = Random.Range (1, gridSize-1);
			int y = Random.Range (1, gridSize-1);
			if (Physics2D.OverlapCircle(grid[x,y], 0.4f) == null){
				foodTiles.SpawnFood(grid[x,y]);
				foodSpawned++;
			}
		}
	}

	public void foodEaten(){
		foodSpawned--;
	}

	void CreateGrid(){
		Vector3 position;
		for (int x=0; x<gridSize; x++) {
			for (int y=0; y<gridSize; y++) {
				position = new Vector3(x+0.5f,y+0.5f,0f);
				grid[x,y] = position;
				if(x==0 || y ==0 || x==gridSize-1 || y == gridSize-1){
					floorTiles.SpawnOuterWallTile(position);
				}
				else floorTiles.SpawnFloorTile(position);
			}	
		}
		int count = 0;
		while (count < obstacleAmount) {
			int x = Random.Range (1, gridSize-1);
			int y = Random.Range (1, gridSize-1);
			if( x==1 && y>=1 && y < 4) continue;
			if (Physics2D.OverlapCircle(grid[x,y], 0.4f) == null){
				floorTiles.SpawnObstacleTile(grid[x,y]);
				count++;
			}
		}
		SetCamera ();
	}

	void SetCamera(){
		//set the camera's scale and position relative to an initialized grid
		cam = Camera.main;
		cam.orthographic = true;
		cam.orthographicSize = gridSize * ((Screen.height / 2f)/Screen.width );
		Vector3 position;
		float x = (float)gridSize / 2.0f;
		float y = (float)gridSize - cam.orthographicSize;
		position = new Vector3 (x, y, -10f);
		cam.transform.position = position;
	}

	void SetGUI(){
		buttonWidth = Screen.width / 6;
		upButton = new Rect ((float)(Screen.width / 2 - 0.5*buttonWidth),   (float)(3 * Screen.height / 4-buttonWidth),   buttonWidth, buttonWidth);
		rightButton = new Rect ((float)(Screen.width / 2+0.5*buttonWidth),   (float)(3 * Screen.height / 4),   buttonWidth, buttonWidth);
		downButton = new Rect ((float)(Screen.width / 2 - 0.5*buttonWidth),   (float)(3 * Screen.height / 4+buttonWidth),   buttonWidth, buttonWidth);
		leftButton = new Rect ((float)(Screen.width / 2-1.5*buttonWidth),   (float)(3 * Screen.height / 4),   buttonWidth, buttonWidth);
	}

	IEnumerator nextTurn(){
		yield return new WaitForSeconds (timeSlice);
		direction = nextDir;
		if (!gameOver) {
			StartCoroutine (nextTurn());
			snake.moveSnake (direction);
			UpdateLevel();
		}
	}

	void CheckDirection(){
		switch (direction){
		case(Direction.Up):{
			dir.text = "up";
			break;
		}
		case (Direction.Right):
		{
			dir.text = "right";
			break;
		}
		case (Direction.Down):
		{
			dir.text = "down";
			break;
		}
		case (Direction.Left):
		{
			dir.text = "left";
			break;
		}
		}
	}

	void UpdateDirection(){
		if(GUI.Button(upButton, "U")){
			if (direction != Direction.Down) nextDir = Direction.Up;
			
		}	
		if(GUI.Button(rightButton, "R")){
			
			if (direction != Direction.Left) nextDir = Direction.Right;
			
		}
		if(GUI.Button(downButton, "D")){
			if (direction != Direction.Up) nextDir = Direction.Down;
			
		}
		if(GUI.Button(leftButton, "L")){
			if (direction != Direction.Right) nextDir = Direction.Left;		
		}
	}

	public void GameOver(){
		gameOver = true;
	}
}
