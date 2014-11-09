using UnityEngine;
using System.Collections;

public class SCR_characterMelee : MonoBehaviour {
	
	public float attackRange;
	//the range of the melee weapon.
	
	public int attackTotal=1;
	//the total number of attacks (minimum 1, maximum 2).
	
	public int standingAttack_1_damage;
	public int standingAttack_2_damage;
	public int runningAttack_1_damage;
	public int runningAttack_2_damage;
	//the damage that each attack deals.
	
	public float standingAttack_1_hitTime;
	public float standingAttack_2_hitTime;
	public float runningAttack_1_hitTime;
	public float runningAttack_2_hitTime;
	//the amount of time it takes for the attack to land, from the beginning of the animation.
	
	public GameObject attackHitObj;
	//the effect that appears when the character successfully lands a hit.
	
	public Vector3 attackHitOffset;
	//the offset of the attack hit effect from the character.
	
	public AudioClip[] attackSound=new AudioClip[1];
	//the sound of the slash/swipe. Add more sounds to the array for more variety.
	public AudioClip[] attackHitSound=new AudioClip[1];
	//the sound of the attack hitting. Add more sounds to the array for more variety.
	
	SCR_sound[] SND_attack;
	SCR_sound[] SND_attackHit;
	
	
	public float force;
	//how much the attack pushes back the target.
	
	
	bool attackBuffer;
	bool attackBufferAllowed;
	bool attackReady;
	
	SCR_character character;
	
	int attackCurrent=0;
	[HideInInspector]
	public int attackAnimSlot=0;
	float hitTimeCounter=-1f;
	float attackSoundCounter=-1f;
	
	int damage;
	
	Transform target;
	string targetStr;
	[HideInInspector]
	public bool lockActive;
	
	
	public void StartUp(bool isPlayer){
		character=GetComponent<SCR_character>();
		attackReady=true;
		attackBufferAllowed=true;
		
		attackRange*=transform.localScale.x;
		
		if(isPlayer){
			targetStr="Enemy";
		}	else {
			targetStr="Player";
		}
		
		SND_attack=SCR_main.SetupSoundArray(transform,attackSound,true,true);
		SND_attackHit=SCR_main.SetupSoundArray(transform,attackHitSound,true,true);
	}
	
	void Update () {
		
		if(character.attacking){
			if(!animation.IsPlaying(character.animArray[attackAnimSlot])){
				lockActive=false;
				target=null;
				attackReady=true;
				attackBufferAllowed=true;
				
				if(attackBuffer==false){
					character.attacking=false;
					attackCurrent=0;
					
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
			
			if(attackCurrent==0){	
				if(character.running==false){
					attackAnimSlot=5;
					hitTimeCounter=standingAttack_1_hitTime;
					damage=standingAttack_1_damage;
				}	else {
					attackAnimSlot=7;
					hitTimeCounter=runningAttack_1_hitTime;
					damage=runningAttack_1_damage;
				}
			}
			
			if(attackCurrent==1){
				if(character.running==false){
					attackAnimSlot=6;
					hitTimeCounter=standingAttack_2_hitTime;
					damage=standingAttack_2_damage;
				}	else {
					attackAnimSlot=8;
					hitTimeCounter=runningAttack_2_hitTime;
					damage=runningAttack_2_damage;
				}
			}
		
			if(character.powerBoost>0){
				float damageExtra=((float)damage*((float)character.powerBoost*0.01f));
				damage+=(int)damageExtra;
			}
			
			attackSoundCounter=(hitTimeCounter-0.15f);
			if(attackSoundCounter<0f){
				attackSoundCounter=0f;	
			}
			
			character.PlayAnim(attackAnimSlot);
			
			attackCurrent++;
			
			if(attackCurrent==attackTotal){
				attackCurrent=0;	
			}
			
			if(character.isPlayer){
				TargetLock();	
			}
		}
		
		if(hitTimeCounter>=0f){
			
			if(attackSoundCounter>=0f){
				attackSoundCounter-=Time.deltaTime;
				
				if(attackSoundCounter<0f){
					attackSoundCounter=-1f;
					SCR_main.PlayRandomSound(SND_attack);
				}
			}
			
			hitTimeCounter-=Time.deltaTime;
			
			if(hitTimeCounter<0f){
				hitTimeCounter=-1f;
				attackBufferAllowed=true;
				AttackHit ();
			}
		}
		
		if(lockActive){
			Vector3 destination=new Vector3(target.transform.position.x,0f,target.transform.position.z);
			character.dir=(destination-new Vector3(transform.position.x,0f,transform.position.z));
			float distTotal=character.dir.magnitude;
			character.dir/=distTotal;
			
			float rotAngle=SCR_main.GetAngle(		new Vector2(transform.position.x,transform.position.z),
												new Vector2(destination.x,destination.z));
			rotAngle=(-rotAngle+90f);
			character.rotTarget=Quaternion.Euler(new Vector3(0f,rotAngle,0f));
		}
	}
	
	public void AttackStart(){
		if(attackBufferAllowed){
			attackBuffer=true;
		}
	}
	
	public void AttackCancel(){
		attackCurrent=0;
		attackReady=true;
		attackBuffer=false;
		attackBufferAllowed=true;
		hitTimeCounter=-1f;
		attackSoundCounter=-1f;
		lockActive=false;
		target=null;
	}
	
	void TargetLock(){
		target=null;
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetStr);
		
		float shortestDistance=Mathf.Infinity;
		
		if(targets.Length>0) {
			foreach (GameObject t in targets)  {
				SCR_character c=t.GetComponent<SCR_character>();
				
				if(c.stunned<2&&c.invulnerable==false){
				
					float dist = Vector2.Distance(	new Vector2(transform.position.x,transform.position.z),
													new Vector2(t.transform.position.x,t.transform.position.z));
					
					if (dist<=(attackRange+(t.GetComponent<BoxCollider>().size.x*t.transform.localScale.x))&&dist<shortestDistance) { 
						target=t.transform;
						shortestDistance=dist;
					}
				}
			}
		}
		
		if(target){
			lockActive=true;
		}
	}
	
	public void AttackHit(){
		if(lockActive==false){
			TargetLock();	
		}
		
		if(lockActive){
			float dist = Vector2.Distance(	new Vector2(transform.position.x,transform.position.z),
											new Vector2(target.transform.position.x,target.transform.position.z));
			
			if(dist<=(attackRange+(target.GetComponent<BoxCollider>().size.x*target.transform.localScale.x))){
				bool hitSuccess=target.GetComponent<SCR_characterHealth>().Damage(damage);
				
				if(hitSuccess){
					SCR_character targetCha=target.GetComponent<SCR_character>();
					
					Vector3 hitDir=(new Vector3(target.transform.position.x,0f,target.transform.position.z)-
									new Vector3(transform.position.x,0f,transform.position.z));
					float distTotal=hitDir.magnitude;
					hitDir/=distTotal;
					
					targetCha.speed[0]+=(hitDir*force);
					
					
					if(attackHitObj){
						
						Vector3 relativePos=(target.transform.position-transform.position);
						Quaternion attackRotation=Quaternion.LookRotation(relativePos);
						
						GameObject attackHitInst=Instantiate(attackHitObj,transform.position,attackRotation) as GameObject;
						
						Vector3 attackHitPos=	(transform.position+
												(attackHitInst.transform.forward*attackHitOffset.z)+
												(attackHitInst.transform.up*attackHitOffset.y)+
												(attackHitInst.transform.right*attackHitOffset.x));
						
						attackHitInst.transform.position=attackHitPos;
					}
					
					SCR_main.PlayRandomSound(SND_attackHit);
				}
			}
		}
	}
}
