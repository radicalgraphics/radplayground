using UnityEngine;
using System.Collections;

public class SCR_reticle : MonoBehaviour {
	
	public Texture reticleTexture;
	//the texture to be used for the reticle.
	
	public bool UseAdditiveMaterial;
	//use an additive effect for the material.
	
	public float reticleScale;
	//the size of the reticle.
	
	public float reticleAlpha;
	//how visible the reticle is. Set to a value between 0 and 1.
	
	public float rotationSpeed;
	//the rotation speed of the reticle. Set to 0 if you don't want it to rotate.
	


	float[] alpha=new float[3]{0f,0f,3f};
	

	void Awake () {
		transform.localScale=new Vector3(reticleScale,1f,reticleScale);
		
		if(UseAdditiveMaterial){
			renderer.material=SCR_main.particleMat[1];
		}	else {
			renderer.material=SCR_main.particleMat[0];
		}
		
		renderer.material.mainTexture=reticleTexture;
		
		SetPos (new Vector2(0f,0f));
		DisplayAlpha();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(alpha[0]!=alpha[1]){
			alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(Time.deltaTime*alpha[2]));
			DisplayAlpha();
		}
		
		if(alpha[0]>0f){
			if(rotationSpeed!=0f){
				transform.Rotate(Vector3.up*rotationSpeed*Time.deltaTime);	
			}
		}
	}
	

	
	public void SetPos(Vector2 newPos){
		transform.position=new Vector3(newPos.x,0.025f,newPos.y);	
	}
	
	

	public void Appear(bool com){
		if(com){
			alpha[1]=1f;	
		}	else {
			alpha[1]=0f;
		}
	}
	
	void DisplayAlpha(){
		renderer.material.SetColor("_TintColor",new Color(0.5f,0.5f,0.5f,(alpha[0]*reticleAlpha*0.5f)));
	}
}
