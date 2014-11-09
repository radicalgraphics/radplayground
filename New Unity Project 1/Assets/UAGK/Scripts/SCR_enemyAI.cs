using UnityEngine;
using System.Collections;

public class SCR_enemyAI : MonoBehaviour {

	public GameObject deathObj;
	//the enemy death effect.
	
	public float detectRange;
	//the range within which the enemy will detect the player.
	
	public int points;
	//the amount of points the player gains for defeating this enemy.
	
	public int aggression=1;
	//this effects how frequently the enemy attacks.
	//set this to 1,2,3,4 or 5.
	//1 is least aggressive, 5 is most aggressive.
	
	bool alerted=false;
	
	SCR_character character;
	SCR_character player;
	
	int action=0;
	float actionCounter=0f;
	float actionLimit=0f;
	
	bool playerDefeated=false;
	
	float edgeCD=0f;
	
	float startUpCD=0.5f;
	
	float[] attackFreq=new float[2];
	

	public void StartUp () {
		character=GetComponent<SCR_character>();
		player=GameObject.FindWithTag ("Player").GetComponent<SCR_character>();
		CheckPlayerDistance(0);
		
		aggression=Mathf.Clamp(aggression,1,5);
		
		float aggressionMult=((aggression-1)*0.7f);
		attackFreq[0]=(2.8f-aggressionMult);
		attackFreq[1]=(attackFreq[0]+0.55f);
		
		if(alerted){
			character.SetRotAngle(player.transform.position);
		}	else {
			character.SetPatrolSpeed(true);
			character.SetRandomRotAngle();
			SetPatrolAction(0);
			actionCounter=(actionLimit*0.7f);
		}
		
		character.SetRotTarget();
		transform.rotation=character.rotTarget;
	}
	
	
	void Update () {
		
		if(startUpCD>0f){
			startUpCD=Mathf.MoveTowards(startUpCD,0f,Time.deltaTime);
			if(startUpCD==0f){
				character.invulnerable=false;	
			}
		}
		
		if(alerted==false){
			UpdatePatrol();
			CheckPlayerDistance(0);
		}	else {
			UpdateAction();
		}
	}
	
	void UpdatePatrol(){
		actionCounter+=Time.deltaTime;
		
		if(edgeCD>0f){
			edgeCD=Mathf.MoveTowards(edgeCD,0f,Time.deltaTime);		
		}
		
		if(action==0){
			if(actionCounter>=actionLimit){
				SetPatrolAction(1);	
			}
		}	else {
			if(action==1){
				
				if(edgeCD==0f){
					bool closeToEdge=EdgeCheck();
					
					if(closeToEdge){
						character.SetRotAngle(Vector3.zero);
						character.SetRotTarget();
						character.speed[1]=(character.dir*character.runSpeed);
						edgeCD=0.6f;
					}
				}
				
				if(actionCounter>=actionLimit){
					SetPatrolAction(0);
				}
			}
		}
	}
	
	void UpdateAction(){
		
		character.SetRotAngle(player.transform.position);
		
		actionCounter+=Time.deltaTime;
		
		if(action==0||action==1){
			if(action==0){
				character.speed[1]=(character.dir*character.runSpeed);
				
				if(actionCounter>=actionLimit){
					SetAction(1);	
				}
				
			}	else {
				if(actionCounter>=actionLimit){
					SetAction(0);	
				}
			}
			character.SetRotTarget();
			
			CheckPlayerDistance(1);
			
		}	else {
			
			if(action==2){
				//ready to attack
				character.SetRotTarget();
				
				if(actionCounter>=actionLimit){
					SetAction (3);
				}
				
				CheckPlayerDistance(2);
			}	else {
				
				if(action==10){
					//dying
					
					if(actionCounter>=1.25f){
						if(SCR_main.pOn){
							SCR_text p=SCR_gui.CreateText("PopUpPoints",Vector3.zero);
							p.UpdateText(points.ToString());
							p.SetWorldPos(transform.position,1f);
							p.SetFadeSpeed(0.55f,0.2f);
						}
						
						SetAction(11);
						character.StartFlash(1,1000);
						GetComponent<BoxCollider>().isTrigger=true;
					}
				}	else {
					if(action==11){
						//vanishing
						if(actionCounter>=0.55f){
							SCR_main.DestroyObj(gameObject);
						}
					}
				}
			}
		}
	}
	
	void CheckPlayerDistance(int com){
		bool inRange=false;
		float playerDist=Vector2.Distance(	new Vector2(player.transform.position.x,player.transform.position.z),
											new Vector2(transform.position.x,transform.position.z));
		
		if(com==0){
			if(player.stunned<2){
				if(playerDist<detectRange){
					alerted=true;
					character.SetPatrolSpeed(false);
					SetAction (0);
				}
			}
		}
		
		if(com==1){
			if(character.melee){
				if(playerDist<character.characterMelee.attackRange*0.9f){
					inRange=true;
				}
			}	else {
				if(playerDist<character.characterRanged.attackRange*0.8f){
					inRange=true;
				}
			}
			
			if(inRange){
				SetAction (2);
				actionCounter=(actionLimit*0.7f);

			}
		}
		
		if(com==2){
			if(character.melee){
				if(playerDist<character.characterMelee.attackRange){
					inRange=true;
				}
			}	else {
				if(playerDist<character.characterRanged.attackRange){
					inRange=true;
				}
			}
			
			if(inRange==false){
				SetAction (0);
			}
		}
	}
	
	void SetPatrolAction(int com){
		action=com;
		actionCounter=0f;
		
		if(action==0){
			character.PlayAnim(0);
			character.speed[1]=Vector3.zero;
			actionLimit=Random.Range (1.25f,2.25f);
		}
		
		if(action==1){
			character.PlayAnim(1);
			actionLimit=Random.Range (2.25f,3.25f);
			
			float turnChance=Random.Range(0f,100f);
			
			if(turnChance>=35f){
				character.SetRandomRotAngle();	
			}
			
			character.speed[1]=(character.dir*character.runSpeed);
			
		}
	}
	
	bool EdgeCheck(){
		bool closeToEdge=false;
		
		float edgeLimit=1.35f;
		
		if(	((transform.position.x+character.hitBoxSize)>(SCR_stage.sX[1]-edgeLimit))||
			((transform.position.x-character.hitBoxSize)<(SCR_stage.sX[0]+edgeLimit))||
			((transform.position.z+character.hitBoxSize)>(SCR_stage.sZ[1]-edgeLimit))||
			((transform.position.z-character.hitBoxSize)<(SCR_stage.sZ[0]+edgeLimit))){
				closeToEdge=true;
		}
		
		return closeToEdge;
	}
	
	public void SetAction(int com){
		action=com;
		actionCounter=0f;
		
		if(action==0){
			actionLimit=Random.Range (4f,6f);
			character.PlayAnim(1);
		}	else {
			if(action==1){
				actionLimit=Random.Range (0.5f,0.85f);
				character.PlayAnim(0);
				character.speed[1]=Vector3.zero;
			}	else {
				if(action==2){
					if(playerDefeated==false){
						character.PlayAnim(0);
						character.speed[1]=Vector3.zero;
						if(aggression<5){
							actionLimit=Random.Range (attackFreq[0],attackFreq[1]);
						}	else {
							actionLimit=0.1f;
						}
					}	else {
						SetAction (20);
					}
				}	else {
					if(action==3){
						character.attackPoint=player.transform.position;
						character.AttackStart();
					}	else {
						if(action==20){
							character.PlayAnim(0);
							character.speed[1]=Vector3.zero;
						}
					}
				}
			}
		}
	}
	
	public void PlayerDefeated(){
		playerDefeated=true;
		
		character.StopFlash();
		
		if(character.attacking==false){
			SetAction (20);
		}
	}
	
	public void Damage(){
		if(alerted==false){
			character.SetPatrolSpeed(false);
			alerted=true;
		}
	}
	
	public void Kill(){
	    //GameObject deathInst = 
		Instantiate(deathObj, transform.position, transform.rotation); // as GameObject;
		SCR_main.score+=points;
		SetAction (10);
	}
}
