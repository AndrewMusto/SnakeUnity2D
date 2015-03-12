using UnityEngine;
using System.Collections;

public class FoodAI : MonoBehaviour {

	private GameObject snakeHead;
	private BoxCollider2D hitBox;
	private RaycastHit2D ahead;
	private Direction nextDir;

	void Start(){
		hitBox = this.gameObject.GetComponent<BoxCollider2D>();
	}

	public void MoveFood(){
		snakeHead = GameObject.Find ("Head");
		nextDir = ChooseDirection ();
		if (CheckForObstacle (nextDir) == false) {
			this.gameObject.transform.position = MoveThis(nextDir);
		}
	}

	bool CheckForObstacle(Direction dir){
		hitBox.enabled = false;
		switch (dir) {
		case Direction.Up:
			ahead = Physics2D.Raycast(this.transform.position, Vector2.up, 1f);
			hitBox.enabled = true;
			if( ahead.collider == null){
				return false;
			} return true;
		case Direction.Right:
			ahead = Physics2D.Raycast(this.transform.position, Vector2.right, 1f);
			hitBox.enabled = true;
			if( ahead.collider == null){
				return false;
			} return true;
		case Direction.Down:
			ahead = Physics2D.Raycast(this.transform.position, -Vector2.up, 1f);
			hitBox.enabled = true;
			if( ahead.collider == null){
				return false;
			} return true;
		case Direction.Left:
			ahead = Physics2D.Raycast(this.transform.position, -Vector2.right, 1f);
			hitBox.enabled = true;
			if( ahead.collider == null){
				return false;
			} return true;
		}
		Debug.Log ("check direction error");
		return false;
	}

	Direction ChooseDirection(){
		int rand = Random.Range (0, 2);

		switch (rand){
			case 0:
				if (snakeHead.transform.position.y > this.transform.position.y) {
					return Direction.Down;
				}
				else if(snakeHead.transform.position.y < this.transform.position.y){
					return Direction.Up;
				}
				else if(snakeHead.transform.position.x > this.transform.position.x){
					return Direction.Left;
				}
				else if(snakeHead.transform.position.x < this.transform.position.x){
					return Direction.Right;
				}
			break;
			case 1:
				if(snakeHead.transform.position.x > this.transform.position.x){
					return Direction.Left;
				}
				else if(snakeHead.transform.position.x < this.transform.position.x){
					return Direction.Right;
				}
				else if (snakeHead.transform.position.y > this.transform.position.y) {
					return Direction.Down;
				}
				else if(snakeHead.transform.position.y < this.transform.position.y){
					return Direction.Up;
				}
			break;
		}
		Debug.Log("directional choice fell through FoodAI");
		return Direction.Up;
	}

	Vector3 MoveThis(Direction direction){
		switch (direction) {
			case Direction.Up:
				return (this.transform.position+Vector3.up);	
			case Direction.Down:
				return (this.transform.position-Vector3.up);		
			case Direction.Right:
				return (this.transform.position+Vector3.right);
			case Direction.Left:
				return (this.transform.position-Vector3.right);
		}
		Debug.Log ("MoveThis error");
		return (this.transform.position+Vector3.up);
	}
}
	