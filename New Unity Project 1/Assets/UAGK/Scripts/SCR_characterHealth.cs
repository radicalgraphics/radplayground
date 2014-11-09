using UnityEngine;
using System.Collections;

public class SCR_characterHealth : MonoBehaviour {
	SCR_healthBar healthBar;
	
	public float healthBarOffset;
	//the distance from the ground that the health bar sits.
	
	public int healthMax;
	//the character's maximum health.
	
	public AudioClip[] deathSound=new AudioClip[1];
	//the sound that the character makes when they die. Add more sounds to the array for more variety.
	SCR_sound[] SND_death;
	
	
	int health;
	SCR_character character;
	
	public void StartUp(int id){
		health=healthMax;
		character=GetComponent<SCR_character>();
		
		bool deathSoundRandomPitch=true;
		if(character.isPlayer){
			deathSoundRandomPitch=false;
		}
		//death sound
		SND_death=SCR_main.SetupSoundArray(transform,deathSound,deathSoundRandomPitch,true);
		
		GameObject healthBarInst=Instantiate(SCR_main.healthBarObj,Vector3.zero,Quaternion.identity) as GameObject;
		healthBar=healthBarInst.GetComponent<SCR_healthBar>();
		healthBar.StartUp(id);
		
		UpdateHealthBar();
		
	}
	
	void Update(){
		if(healthBar){
			UpdateHealthBar();
		}
	}
	
	void UpdateHealthBar(){
		healthBar.UpdatePos(transform.position+(Vector3.up*healthBarOffset*transform.localScale.y));	
	}
	
	public bool Damage(int damage){
		bool hitSuccess=false;
		bool canDamage=true;
		
		if(health==0||character.rolling==1||character.invulnerable||character.stunned==2){
			canDamage=false;
		}
		
		if(character.isPlayer){
			if(character.stunned==1){
				canDamage=false;
				hitSuccess=true;
			}
		}
		
		if(canDamage){
			health-=damage;
			
			if(health<0){
				health=0;
			}
			
			if(character.isPlayer==false){
				character.ai.Damage();
			}
			
			AlterHealthBar();
			
			character.AttackCancel();
			
			
			if(health>0){
				character.Hurt();
			}	else {
				healthBar.Kill();
				character.Kill();
				SCR_main.PlayRandomSound(SND_death);
			}
			
			hitSuccess=true;
		}
		
		return hitSuccess;
	}
	
	public void AddHealth(int extraHealth){
		health+=extraHealth;
		
		if(health>healthMax){
			health=healthMax;	
		}
		
		AlterHealthBar();
	}
	
	void AlterHealthBar(){
		healthBar.Appear();
			
		float hpDecimal=((float)health/(float)healthMax);
		
		if(hpDecimal>=0.66f){
			healthBar.SetColour(0);
		}
		if(hpDecimal>=0.33f&&hpDecimal<0.66f){
			healthBar.SetColour(1);
		}
		if(hpDecimal<0.33f){
			healthBar.SetColour(2);
		}
		
		healthBar.UpdateHealth(hpDecimal);
	}
}
