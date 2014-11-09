using UnityEngine;
using System.Collections;

public class SCR_enemySpawner : MonoBehaviour {

	public bool enemyBossFight;
	
	public GameObject[] enemyObjects;
	//an array of all the different types of enemies that can spawn.
	
	public int[] enemySpawnChance;
	//An array of chances for each enemy type to spawn.
	//This array must be the same size as the enemyObjects array.
	//The numbers used are relative, so it doesn't matter how high or low they are.
	//Example:
	//if slot 0 is set 10, and slot 1 was at 20, the enemy in enemyObjects slot 1 would spawn twice as often as the enemy in slot 0.
	
	public float enemySpawnInterval;
	//the frequency at which the enemies spawn in seconds.
	
	public int enemyLimit;
	//the total number of enemies that can be in the stage at the same time.
	
	public int enemyStockTotal;
	//the total number of enemies that spawn throughout the stage.
	
	public static bool spawnOn;
	
	float spawnCounter=0f;
	float spawnTarget=0f;
    [HideInInspector]
	public int enemyTotal=0;
	[HideInInspector]
	public int enemyStock;
	Vector2[] spawnPoints;
	int[] spawnPositionUsed;
	
	float[] spawnChanceCurrent;
	
	
	void Awake () {
		
		SetupSpawnPoints();
		
		enemyTotal=0;
		enemyStock=enemyStockTotal;
		spawnOn=false;
		spawnChanceCurrent=new float[enemyObjects.Length];
		spawnPositionUsed=new int[spawnPoints.Length];
	}
	
	void SetupSpawnPoints(){
		GameObject[] spawners = GameObject.FindGameObjectsWithTag("SpawnPointEnemy");
		
		int spawnerCount=0;
		
		for(int i=0; i<2; i++){
			foreach (GameObject s in spawners)  {
				if(i==1){
					spawnPoints[spawnerCount]=new Vector2(s.transform.position.x,s.transform.position.z);
					Destroy(s);
				}
				spawnerCount++;
			}
			
			if(i==0){
				spawnPoints=new Vector2[spawnerCount];
				spawnerCount=0;
			}
		}
	}
	

	void Update () {
		if(spawnOn){
			UpdateSpawn();
		}
	}
	
	void UpdateSpawn(){
		
		if(enemyTotal<enemyLimit&&enemyStock>0){
			spawnCounter+=Time.deltaTime;
		}
		
		if(spawnCounter>=spawnTarget){
			spawnCounter=0f;
			int i=0;
			
			//decide which enemy to spawn
			int typeToSpawn=0;
			
			if(enemyObjects.Length>1){
				
				float highestChance=-1f;
				
				for(i=0; i<enemySpawnChance.Length; i++){
					spawnChanceCurrent[i]+=((float)enemySpawnChance[i]*Random.Range (0.85f,1.15f));
					
					if(spawnChanceCurrent[i]>highestChance){
						highestChance=spawnChanceCurrent[i];
						typeToSpawn=i;
					}
				}
				
				spawnChanceCurrent[typeToSpawn]=0f;
			}
			
			//decide which spawn position to use
			
			int spawnSlot=-1;
			
			if(spawnPoints.Length>1){
				bool resetSlots=true;
				
				for(i=0; i<spawnPoints.Length; i++){
					if(spawnPositionUsed[i]==0){
						resetSlots=false;
					}
				}
				
				if(resetSlots){
					spawnPositionUsed=new int[spawnPoints.Length];	
				}
				
				while(spawnSlot==-1){
					i=Random.Range (0,spawnPoints.Length);
					
					if(spawnPositionUsed[i]==0){
						spawnSlot=i;
						spawnPositionUsed[i]=1;
					}
				}
			}	else {
				spawnSlot=0;
			}
			
			
			//spawn enemy
			CreateEnemy (typeToSpawn,new Vector3(spawnPoints[spawnSlot].x,0f,spawnPoints[spawnSlot].y));
			
			SetSpawnInterval();
		}
	}
	
	void SetSpawnInterval(){
		spawnTarget=(enemySpawnInterval*Random.Range (0.85f,1.15f));
	}
	
	void CreateEnemy(int enemySlot,Vector3 enemyPos){
		GameObject enemyInst = Instantiate(enemyObjects[enemySlot],enemyPos,Quaternion.identity) as GameObject;
		enemyInst.GetComponent<SCR_characterHealth>().StartUp(enemyTotal);
		
		enemyTotal++;
		enemyStock--;
	}
}
