using UnityEngine;
using System.Collections;

public class SCR_collectableSpawner : MonoBehaviour {
	
	public bool collectablesOn;
	//Determines whether the stage has collectables or not.
	
	public GameObject[] collectableObjects;
	//An array of all of the collectable objects.
	
	public int[] collectableSpawnChance;
	//An array of chances for each collectable type to spawn.
	//This array must be the same size as the collectableObjects array.
	//The numbers used are relative, so it doesn't matter how high or low they are.
	//Example:
	//if slot 0 is set 10, and slot 1 was at 20, the collectable in collectableObjects slot 1 would spawn twice as often as the collectable in slot 0.
	
	public float collectableInterval;
	//the frequency at which the collectables spawn in seconds.
	
	public int collectableLimit;
	//the total number of collectables that can be in the stage at the same time.
	
	public static int collectableTotal=0;
	float[] spawnChanceCurrent;
	float spawnCounter=0f;
	float spawnTarget=0f;
	//float rimOffset=1.45f;		// [DGT] unused variable causing warnings
	Vector2[] spawnPoints;
	public static int[] spawnPointOccupied;
	
	

	void Awake () {
		
		collectableTotal=0;
		if(collectablesOn){
			spawnChanceCurrent=new float[collectableObjects.Length];
			
			SetupSpawnPoints();
			spawnPointOccupied=new int[spawnPoints.Length];
			
			SetSpawnInterval();
		}
		
	}
	
	void SetupSpawnPoints(){
		GameObject[] spawners = GameObject.FindGameObjectsWithTag("SpawnPointCollectable");
		
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
		if(collectablesOn&&SCR_enemySpawner.spawnOn){
			UpdateSpawn();
		}
	}
	
	void UpdateSpawn(){
		
		if(collectableTotal<collectableLimit){
			spawnCounter+=Time.deltaTime;
		}
		
		if(spawnCounter>=spawnTarget){
			spawnCounter=0f;
			int i=0;
			
			//decide which collectable to spawn
			int typeToSpawn=0;
			
			if(collectableObjects.Length>1){
				
				float highestChance=-1f;
				
				for(i=0; i<collectableSpawnChance.Length; i++){
					spawnChanceCurrent[i]+=((float)collectableSpawnChance[i]*Random.Range (0.85f,1.15f));
					
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
				while(spawnSlot==-1){
					i=Random.Range (0,spawnPoints.Length);
					
					if(spawnPointOccupied[i]==0){
						spawnSlot=i;
						spawnPointOccupied[i]=1;
					}
				}
			}	else {
				spawnSlot=0;
			}
			
			CreateCollectable(typeToSpawn,spawnSlot);
			
			SetSpawnInterval();
		}
	}
	
	void SetSpawnInterval(){
		spawnTarget=(collectableInterval*Random.Range (0.85f,1.15f));
	}
	
	void CreateCollectable(int com,int spawnSlot){
		
		Vector3 pos=new Vector3(spawnPoints[spawnSlot].x,0f,spawnPoints[spawnSlot].y);
		GameObject collectableInst=Instantiate(collectableObjects[com],pos,Quaternion.identity) as GameObject;
		collectableInst.GetComponent<SCR_collectable>().StartUp(spawnSlot);
		collectableTotal++;
	}
}
