using UnityEngine;
using System.Collections;

public class SCR_collectable : MonoBehaviour {
	
	public GameObject collectEffectObj;
	//an object that is generated when the item is collected.
	
	public AudioClip collectSound;
	//the sound effect of the item being collected.

	public float yOffset;
	//the height of the object from the ground.
	
	public float spinSpeed;
	//the speed at which the object spins. Use minus values to make it spin the opposite way, or 0 for no spinning.
	
	public bool collectPoints=false;
	
	public int points;
	//the amount of points the player gains for collecting this item.
	
	public int healthRecovery;
	//the total amount of health the item gives the player.
	
	public int powerBoost;
	//the % increase in the power that the player gains from the item.
		
	public float powerBoostDuration;
	//the time that the power increase lasts for in seconds.
	
	SCR_sound SND_collect;
	Renderer[] mesh;
	Light[] lights;
	
	float flashCounter=0f;
	int flashTotal=4;
	bool flashOn=false;
	
	[HideInInspector]
	public bool isActive=false;
	
	float fadeCounter=0f;
	
	int spawnSlot;
	
	
	public void StartUp (int _spawnSlot) {
		
		spawnSlot=_spawnSlot;
		transform.position+=(Vector3.up*yOffset);
		
		//Mesh Setup
		
		int meshCount=0;
		int lightCount=0;
		
		for(int i=0; i<2; i++){
			foreach (Renderer m in gameObject.GetComponentsInChildren<Renderer>()){
				if(i==1){
					mesh[meshCount]=m;	
				}
				meshCount++;
			}
			
			foreach (Light l in gameObject.GetComponentsInChildren<Light>()){
				if(i==1){
					lights[lightCount]=l;
				}
				lightCount++;
			}
			
			if(i==0){
				mesh=new Renderer[meshCount];
				
				if(lightCount>0){
					lights=new Light[lightCount];
				}
			}
			
			meshCount=0;
			lightCount=0;
		}
		
		if(collectSound){
			SND_collect=SCR_main.CreateSound(transform,collectSound,false,true);	
		}
		
		StartFlash();
	}
	
	
	void Update () {
		if(flashTotal>0){
			UpdateFlash();
		}
		if(spinSpeed!=0f){
			transform.Rotate (Vector3.up*spinSpeed*Time.deltaTime);	
		}
		
		if(fadeCounter>0f){
			fadeCounter=Mathf.MoveTowards(fadeCounter,0f,Time.deltaTime);	
			
			if(fadeCounter==0f){					
				SCR_main.DestroyObj(gameObject);
			}
		}
	}
	
	//FLASH
	
	public void StartFlash(){
		flashTotal=4;
		flashCounter=0f;
		flashOn=true;
		DisplayFlash();
	}
	
	void DisplayFlash(){
		int i=0;
		
		for(i=0; i<mesh.Length; i++){
			mesh[i].enabled=!flashOn;	
		}
		
		if(lights!=null){
			for(i=0; i<lights.Length; i++){
				lights[i].enabled=!flashOn;	
			}
		}
	}
	
	void UpdateFlash(){
		flashCounter+=SCR_main.counterMult;
		
		float flashLimit=0.2f;
		
		if(flashOn==false){
			flashLimit=0.4f;	
		}
		
		if(flashCounter>=flashLimit){
			flashCounter=0f;
			flashOn=!flashOn;
			
			DisplayFlash();
			
			if(flashOn==false){
				flashTotal--;
				
				if(flashTotal==0){
					isActive=true;
				}
			}
		}
	}
	
	public void Collect(){
		SCR_collectableSpawner.collectableTotal--;
		SCR_collectableSpawner.spawnPointOccupied[spawnSlot]=0;
		
		isActive=false;
		fadeCounter=1.5f;
		
		if(collectEffectObj){
			//GameObject collectEffectInst=
			Instantiate(collectEffectObj,transform.position,Quaternion.identity); // as GameObject;
		}
		
		if(SND_collect){
			SND_collect.PlaySound();
			
			if (collectPoints==true){
				SCR_main.score+=points;	
				
				if(SCR_main.pOn){
					SCR_text p=SCR_gui.CreateText("PopUpPoints",Vector3.zero);
					p.UpdateText(points.ToString());
					p.SetWorldPos(transform.position,1f);
					p.SetFadeSpeed(0.55f,0.2f);
				}
			}			
		}
		
		flashOn=true;
		DisplayFlash();
	}
}
