using UnityEngine;
using System.Collections;

public class SCR_projectileTrail : MonoBehaviour {
	
	public Texture trailTex;
	//texture used for the trail.
	
	public bool UseAdditiveMaterial;
	//use an additive effect for the material.
	
	public float trailWidth;
	//width of the trail.
	
	public Color trailColor;
	//color of the trail.
	
	public float trailAlpha=1f;
	//the trail is at full opacity at 1, half at 0.5 etc.
	
	public float trailTime;
	//the duration of the trail. A short trail time will mean the trail appears shorter.
	//the length of the trail is also influenced by the speed of the projectile.
	
	bool fading=false;
	float fadeCounter=1f;
	TrailRenderer trail;
	

	void Awake () {
		gameObject.AddComponent<TrailRenderer>();
		trail=GetComponent<TrailRenderer>();
		
		if(UseAdditiveMaterial){
			renderer.material=SCR_main.particleMat[1];
		}	else {
			renderer.material=SCR_main.particleMat[0];
		}
		
		renderer.material.mainTexture=trailTex;
		trail.time=trailTime;
		trail.endWidth=trailWidth;
		trail.startWidth=trailWidth;
		
		DisplayColour();
	}
	
	void Update () {
		if(fading){
			if(fadeCounter!=0f){
				fadeCounter=Mathf.MoveTowards(fadeCounter,0f,(Time.deltaTime*3f));
				
				trail.endWidth=(trailWidth+((1f+(fadeCounter*-1f))*trailWidth*5f));
				DisplayColour();
				
				if(fadeCounter==0f){
					SCR_main.DestroyObj(gameObject);
				}
			}
		}
	}
	
	void DisplayColour(){
		renderer.material.SetColor("_TintColor",new Color(trailColor.r,trailColor.g,trailColor.b,(fadeCounter*trailAlpha*0.5f)));
	}
	
	public void Kill(){
		fading=true;
		fadeCounter=1f;
		
	}
}
