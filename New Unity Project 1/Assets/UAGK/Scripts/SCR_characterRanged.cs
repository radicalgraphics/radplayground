using UnityEngine;
using System.Collections;

public class SCR_characterRanged : MonoBehaviour {
	
	public GameObject projectileObj;
	//the object which the character fires.
	
	public GameObject muzzleFlareObj;
	//the muzzle flare effect object.
	
	public Vector3 projectileOffset;
	//offsets the position from which the projectile is fired. If this is set to 0,0,0, the projectile will fire from the pivot point of the mesh, which in most cases is no good!
	
	public float attackRange;
	//the range at which the character can fire their weapon from.
	public int shotDamage;
	//the damage that the character's projectile deals.
	
	public float standing_shotTime;
	public float running_shotTime;
	//the amount of time it takes for the projectile to be fired, from the beginning of the animation
	
	public bool rapidFireOn;
	//determines whether the character uses rapid fire or not. If this is set to false, ignore the next 2 variables.
	
	public float rapidFireInterval;
	//the time inbetween each shot when rapid firing.
	
	public int rapidFireLimit;
	//the total number of shots that are fired when rapid firing.
	
	public AudioClip[] shotSound=new AudioClip[1];
	//the sound of the gun firing. Add more sounds to the array for more variety.
	SCR_sound[] SND_shot;
	
	
	int rapidFireCurrent=0;
	float rapidFireCounter=0f;
	
	SCR_character character;
	
	bool attackBuffer;
	bool attackBufferAllowed;
	bool attackReady;
	
	[HideInInspector]
	public int attackAnimSlot=0;
	
	float shotTimeCounter=-1f;
	
	GameObject dir;
	

	void Awake () {
		character=GetComponent<SCR_character>();
		attackReady=true;
		attackBufferAllowed=true;
		
		dir=new GameObject("rangedDir");
		Transform dirTrans=dir.transform;
		dirTrans.parent=transform;
		dir.transform.localPosition=Vector3.zero;
		
		SND_shot=SCR_main.SetupSoundArray(transform,shotSound,true,true);
	}
	
	
	void Update () {
		
		if(character.attacking){
			if(rapidFireOn==false&&!animation.IsPlaying(character.animArray[attackAnimSlot])||
				rapidFireOn&&rapidFireCurrent==0){
				attackReady=true;
				
				if(attackBuffer==false){
					character.attacking=false;
					
					if(character.isPlayer==false){
						GetComponent<SCR_enemyAI>().SetAction(2);	
					}
				}
			}
		}
		
		if(attackBuffer&&attackReady){
			
			character.attacking=true;
			
			attackReady=false;
			attackBuffer=false;
			attackBufferAllowed=false;
			
			if(rapidFireOn==false){
				if(character.running==false){
					attackAnimSlot=5;
					shotTimeCounter=standing_shotTime;
				}	else {
					attackAnimSlot=7;
					shotTimeCounter=running_shotTime;
				}
			}	else {
				if(character.running==false){
					attackAnimSlot=6;
				}	else {
					attackAnimSlot=8;
				}
				rapidFireCurrent=rapidFireLimit;
				rapidFireCounter=rapidFireInterval;
			}
			
			character.PlayAnim(attackAnimSlot);
			
		}
		
		
		if(rapidFireCurrent>0){
			UpdateRapidFire();
		}
		
		if(shotTimeCounter>=0f){
			shotTimeCounter-=Time.deltaTime;
			
			if(shotTimeCounter<0f){
				shotTimeCounter=-1f;
				attackBufferAllowed=true;
				CreateProjectile();
			}
		}
	}
	
	public void AttackStart(){
		if(attackBufferAllowed){
			attackBuffer=true;
		}
	}
	
	void UpdateRapidFire(){
		rapidFireCounter+=Time.deltaTime;
			
		if(rapidFireCounter>=rapidFireInterval){
			rapidFireCounter=0f;
			CreateProjectile();
			rapidFireCurrent--;
			
			if(rapidFireCurrent<=2){
				attackBufferAllowed=true;	
			}
		}
	}
	
	public void AttackCancel(){
		attackReady=true;
		attackBuffer=false;
		attackBufferAllowed=true;
		rapidFireCurrent=0;
		rapidFireCounter=0f;
		shotTimeCounter=-1f;
	}
	
	public void CreateProjectile(){
		
		dir.transform.rotation=character.rotTarget;
		
		float projectileZ=(projectileOffset.z*transform.localScale.z);
		Vector3 projectilePos=	(transform.position+
								(dir.transform.right*projectileOffset.x*transform.localScale.x)+
								(dir.transform.up*projectileOffset.y*transform.localScale.y)+
								(dir.transform.forward*projectileZ));
		
		
		float middleDist=Vector2.Distance(new Vector2(character.attackPoint.x,character.attackPoint.z),new Vector2(transform.position.x,transform.position.z));
		
		if(middleDist>(projectileZ+0.35f)){
			Vector3 relativePos=(new Vector3(character.attackPoint.x,projectilePos.y,character.attackPoint.z)-projectilePos);
			dir.transform.rotation=Quaternion.LookRotation(relativePos);	
		}

		if(muzzleFlareObj){
			//GameObject muzzleFlareInst=	
			Instantiate(muzzleFlareObj,projectilePos,dir.transform.rotation); // as GameObject;	
		}
		
		
		int damageFinal=shotDamage;
		
		if(character.powerBoost>0){
			float damageExtra=((float)shotDamage*((float)character.powerBoost*0.01f));
			damageFinal+=(int)damageExtra;
		}
		
		GameObject projectileInst = Instantiate(projectileObj,projectilePos,dir.transform.rotation) as GameObject;
		projectileInst.GetComponent<SCR_projectile>().StartUp(character.isPlayer,damageFinal);
		
		SCR_main.PlayRandomSound(SND_shot);
	}
}
