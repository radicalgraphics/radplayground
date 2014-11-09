using UnityEngine;
using System.Collections;

public class SCR_screenCover : MonoBehaviour {
	
	[HideInInspector]
	public bool fadeFinished;
	
	float[] alpha=new float[4];
	Vector3 colour;

	public void StartUp (string _colour,bool fadeIn,float fadeSpeed,float alphaMult) {
		
		if(_colour=="black"){
			colour=Vector3.zero;
		}	else {
			if(_colour=="white"){
				colour=new Vector3(0.5f,0.5f,0.5f);
			}
		}
		
		if(fadeIn){
			alpha[0]=0f;
			alpha[1]=1f;
		}	else {
			alpha[0]=1f;
			alpha[1]=0f;
		}
		
		alpha[2]=fadeSpeed;
		alpha[3]=alphaMult;
		
		UpdateScale();
		DisplayAlpha();
	}
	
	
	void Update () {
		UpdateScale();
		
		if(alpha[0]!=alpha[1]){
			alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(alpha[2]*SCR_main.counterMult));
			
			DisplayAlpha();
			
			if(alpha[0]==alpha[1]){
				if(alpha[1]==1f){
					fadeFinished=true;
				}	else {
					SCR_main.DestroyObj(gameObject);
				}
			}
		}
	}
	
	void UpdateScale(){
		float[] screenSize=new float[2]{	((float)(Screen.width)+10f),
											((float)(Screen.height)+10f)
		};
		
		guiTexture.pixelInset=new Rect(	(-screenSize[0]*0.5f),
										(-screenSize[1]*0.5f),
										screenSize[0],
										screenSize[1]);
	}
	
	void DisplayAlpha(){
		guiTexture.color=new Color(colour.x,colour.y,colour.z,(alpha[0]*alpha[3]*0.5f));
	}
	
	public void FadeOut(){
		alpha[1]=0f;	
	}
}
