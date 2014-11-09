using UnityEngine;
using System.Collections;

public class SCR_particle : MonoBehaviour {
	
	public Texture particleTex;
	//the texture to be used for the particle
	
	float lifeCounter=0f;
	
	void Awake () {
		renderer.material.mainTexture=particleTex;
	}
	
	void Update(){
		if(lifeCounter>0f){
			lifeCounter=Mathf.MoveTowards(lifeCounter,0f,Time.deltaTime);
			
			if(lifeCounter==0f){
				SCR_main.DestroyObj(gameObject);	
			}
		}
	}
	
	
	public void Kill(){
		if(lifeCounter==0f){
			ParticleEmitter emitter=GetComponent<ParticleEmitter>();
			emitter.emit=false;
			lifeCounter=(emitter.maxEnergy+0.2f);
		}
	}
	
}
