using UnityEngine;
using System.Collections;
using System;


public class SnakeScript : MonoBehaviour {

	public SnakePart standardSnake;

	[Serializable]
	public class SnakePart{
		public Sprite head;
		public Sprite body;
		public Sprite bodyBent;
		public Sprite tail;

		private Vector3 position;
		[HideInInspector]public GameObject part;
		[HideInInspector]public SpriteRenderer spriteRender;
		[HideInInspector]public Direction direction;
		[HideInInspector]public SnakePart forward;
		[HideInInspector]public SnakePart behind;

		[HideInInspector]public int x;
		[HideInInspector]public int y;

		//instantiating a new snakepart will create a new head the the position/direction passed
		public SnakePart(Vector3 pos, Direction dir, SnakePart fw, SnakePart bh, int x, int y){
			direction = dir;
			part = new GameObject();
			Rigidbody2D partBody;
			partBody = part.AddComponent<Rigidbody2D>();
			partBody.gravityScale = 0;
			BoxCollider2D hitBox = part.AddComponent<BoxCollider2D>();
			hitBox.isTrigger = true;
			hitBox.size *= 0.8f; //resizing the snake colliders to prevent collisions with adjacent squares etc
			part.transform.position = pos;
			part.tag = "Player";
			part.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
			spriteRender = part.AddComponent<SpriteRenderer>();
			spriteRender.sprite = head;
			spriteRender.sortingLayerName =  "snake";
            rotatePart(dir);
			forward = fw;
			behind = bh;
			this.x = x;
			this.y = y;
		}

		public void rotatePart(Direction dir){
            Vector3 rot = new Vector3();
			switch(dir)
            {
			case Direction.Right:
				rot.z = 270f;
				break;
			case Direction.Down:
				rot.z = 180f;
				break;
			case Direction.Left:
				rot.z = 90f;
				break;
			}
            part.transform.rotation = Quaternion.Euler(rot);
		}
	}
	
	SnakePart h, b, t, newSnake;

	private GameController gc;
	private GameObject food;
	

	//instantiate snake in lower left hand corner
	public void spawnSnake () {

		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		if (gc == null) {
			Debug.Log ("no game controller found");		
		}
		
		Vector3 position = gc.grid [1, 3];

        newSnake = new SnakePart(position, Direction.Up, null, null, 1, 3);
		h = newSnake;
		h.spriteRender.sprite = standardSnake.head;
		
		position = gc.grid [1, 2];

        newSnake = new SnakePart(position, Direction.Up, h, null, 1, 2);
		b = newSnake;
		b.spriteRender.sprite = standardSnake.body;
		
		position = gc.grid [1, 1];

        newSnake = new SnakePart(position, Direction.Up, b, null, 1, 1);
		t = newSnake;
		t.spriteRender.sprite = standardSnake.tail;
		b.behind = t;
	}

	public void moveSnake(Direction dir){
		bool hitObstacle = false;
		bool hitFood = false;
		Vector3 nextPos = new Vector3();
		Vector3 bendRotation;
        int nextX = 0;
        int nextY = 0;
		BoxCollider2D snakeHeadBox = h.part.GetComponent<BoxCollider2D> ();
		RaycastHit2D ahead;
		Vector2 headPosition = h.part.transform.position; 
		snakeHeadBox.enabled = false;
		switch ((Direction)dir) {
			case Direction.Up:
				ahead = Physics2D.Raycast(headPosition, Vector2.up, 1f);
				if (ahead.collider !=null){
					if(ahead.transform.gameObject.tag == "food"){
						hitFood = true;
					}
					else hitObstacle = true;
				}
				nextX = h.x;
	            nextY = h.y + 1;
				nextPos = gc.grid[nextX, nextY];
				break;
			case Direction.Right:
				ahead = Physics2D.Raycast(headPosition, Vector2.right, 1f);
				if (ahead.collider !=null){
					if(ahead.transform.gameObject.tag == "food"){
						hitFood = true;
					}
					else hitObstacle = true;
				}
	            nextX = h.x + 1;
				nextY= h.y;
				nextPos = gc.grid[nextX, nextY];
				break;
			case Direction.Down:
				ahead = Physics2D.Raycast(headPosition, -Vector2.up, 1f);
				if (ahead.collider!=null){
					if(ahead.transform.gameObject.tag == "food"){
						hitFood = true;
					}
				else hitObstacle = true;
				}
				nextX = h.x;
	            nextY = h.y - 1;
				nextPos = gc.grid[nextX, nextY];
				break;
			case Direction.Left:
				ahead = Physics2D.Raycast(headPosition, -Vector2.right, 1f);
				if (ahead.collider !=null){
					if(ahead.transform.gameObject.tag == "food"){
						hitFood = true;
					}
					else hitObstacle = true;
				}
		            nextX = h.x - 1;
				nextY= h.y;
				nextPos = gc.grid[nextX, nextY];
				break;
		}
		snakeHeadBox.enabled = true;
		newSnake = new SnakePart(nextPos, dir, b, null, nextX, nextY);
		newSnake.spriteRender.sprite = standardSnake.head;

		if (h.direction == dir) {
			h.spriteRender.sprite = standardSnake.body;
		}
		else if(h.direction == (int)Direction.Up){
			h.spriteRender.sprite = standardSnake.bodyBent;
			if(dir == Direction.Right){
				bendRotation = new Vector3(0f, 0f, 90f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
		}
		else if(h.direction == Direction.Right){
			h.spriteRender.sprite = standardSnake.bodyBent;
			if(dir == (int)Direction.Up){
				bendRotation = new Vector3(0f, 0f, 270f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
			else if(dir == Direction.Down){
				bendRotation = new Vector3(0f, 0f, 0f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
		}
		else if(h.direction == Direction.Down){
			h.spriteRender.sprite = standardSnake.bodyBent;
			if(dir == Direction.Right){
				bendRotation = new Vector3(0f, 0f, 180f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
			else if(dir == Direction.Left){
				bendRotation = new Vector3(0f, 0f, 270f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
		}
		else if(h.direction == Direction.Left){
			h.spriteRender.sprite = standardSnake.bodyBent;
			if(dir == Direction.Up){
				bendRotation = new Vector3(0f, 0f, 180f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
			else if(dir == Direction.Down){
				bendRotation = new Vector3(0f, 0f, 90f);
				h.part.transform.rotation = Quaternion.Euler(bendRotation);
			}
		}

		h.forward = newSnake;
		h = newSnake;

		if(hitObstacle){ 
			gc.GameOver();
			Debug.Log ("GameOver");
		}
		if(!hitFood){		//move & update tail. If food was hit don't delete tail on update
			b = t.forward;
			Destroy (t.part);
			b.spriteRender.sprite = standardSnake.tail;
			b.rotatePart (b.forward.direction);
			t = b;
		}if (hitFood) {
			gc.foodEaten();
		}
	}
}
