using UnityEngine;
using System.Collections;

public class SCR_gameComplete : MonoBehaviour {
	
	public AudioClip gameCompleteMusic;
	//the music that plays on this screen.
	
	public Material skyBoxMaterial;
	//fill this in if you want to use a skybox for this screen.
	
	int state=10;
	float stateCounter=0f;
	
	void Awake () {
		SCR_gui.CreateScreenCover(0);
		
		if(skyBoxMaterial){
			RenderSettings.skybox=skyBoxMaterial;
		}
		
		GameObject m=GameObject.Find ("MAIN");
		SCR_main main=m.GetComponent<SCR_main>();

		GameObject spawnPointPlayer=GameObject.Find ("SpawnPointPlayer");
		Vector3 playerPos=new Vector3(spawnPointPlayer.transform.position.x,0f,spawnPointPlayer.transform.position.z);

		if(main.playerSelect==true){
			//GameObject playerInst=
			Instantiate(main.playerObj1,playerPos,Quaternion.identity); // as GameObject;
			Destroy(spawnPointPlayer);
		}
		else if(main.playerSelect==false){		
			//GameObject playerInst=
			Instantiate(main.playerObj2,playerPos,Quaternion.identity); // as GameObject;
			Destroy(spawnPointPlayer);		
		}
		
		if(gameCompleteMusic&&SCR_main.hMusic){
			m.GetComponent<SCR_music>().PlayMusic(gameCompleteMusic);	
		}
	}
	
	void Update () {
		if(state>0){
			UpdateState();	
		}
	}
	
	void UpdateState(){
		stateCounter+=Time.deltaTime;
		
		if(state==10){
			if(stateCounter>=0.4f){
				SCR_gui.CreateIcon("GameComplete",Vector3.zero);
				StateNext();	
			}
		}
		
		if(state==11){
			if(stateCounter>=0.2f){
				SCR_gui.CreateText("GameComplete",Vector3.zero);
				StateNext ();
			}
		}
		
		if(state==12){
			if(stateCounter>=0.4f){
				SCR_gui.CreateText("GameCompleteScoreText",Vector3.zero);
				SCR_text score = SCR_gui.CreateText("GameCompleteScoreNumber",Vector3.zero);
				score.UpdateText(SCR_main.score.ToString());
				StateNext();
			}
		}
		
		if(state==13){
			if(stateCounter>=0.4f){
				SCR_gui.CreateOption("GameCompleteReturn",new Vector2(0.5f,0.15f));
				state=0;
			}
		}
	}
	
	void StateNext(){
		state++;
		stateCounter=0f;
	}
}