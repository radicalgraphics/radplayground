using UnityEngine;
using System.Collections;

public class SCR_projectile : MonoBehaviour {
	
	public Texture projectileTex;
	//the texture used for the mesh.
	
	public GameObject impactEffectObj;
	//the object that is created when the projectile hits something.
	
	public string meshStr;
	//the path of the projectile's mesh.
	Renderer mesh;
	
	
	
	public float force;
	//how far the projectile pushes it's target.
	
	public float speed;
	//the speed of the projectile.
	
	public AudioClip[] impactSound=new AudioClip[1];
	//the sound of the projectile hitting. Add more sounds to the array for more variety.
	SCR_sound[] SND_impact;
	
	bool isActive=true;
	int damage;
	string targetStr;
	SCR_projectileTrail trail;
	
	float destroyCounter=0f;
	
	
	public void StartUp (bool isPlayer,int _damage) {
		
		damage=_damage;
		
		SND_impact=SCR_main.SetupSoundArray(transform,impactSound,true,true);
		
		if(GetComponent<SCR_projectileTrail>()){
			trail=GetComponent<SCR_projectileTrail>();	
		}
		
		mesh=transform.Find (meshStr).GetComponent<Renderer>();
		mesh.material.mainTexture=projectileTex;
		
		if(isPlayer){
			targetStr="Enemy";
		}	else {
			targetStr="Player";
		}
	}
	
	void Update(){
		if(isActive==false){
			if(destroyCounter!=0f){
				destroyCounter=Mathf.MoveTowards(destroyCounter,0f,Time.deltaTime);
				
				if(destroyCounter==0f){
					SCR_main.DestroyObj(gameObject);	
				}
			}
		}
	}
	
	void FixedUpdate () {
		if(isActive){
			rigidbody.MovePosition(rigidbody.transform.position+(transform.forward*speed*Time.deltaTime));
		}
	}
	
	void OnTriggerEnter(Collider col){
		if(isActive){
			if(col.gameObject.tag==targetStr){
				//hits enemy / player
				bool hitSuccess=col.gameObject.GetComponent<SCR_characterHealth>().Damage(damage);
				
				if(hitSuccess){
					col.gameObject.GetComponent<SCR_character>().speed[0]+=(transform.forward*force);
					
					Kill();
				}
			}
			
			if(col.gameObject.layer==8){
				//hits background
				Kill();
			}
		}
	}
	
	void Kill(){
		if(isActive){
			isActive=false;
			mesh.renderer.enabled=false;
			
			destroyCounter=1.5f;
			
			SCR_main.PlayRandomSound(SND_impact);
			
			if(impactEffectObj){
				//GameObject impactInst=
				Instantiate(impactEffectObj,transform.position,transform.rotation); // as GameObject;	
			}
			
			if(trail){
				trail.Kill();
			}
		}
	}
}
