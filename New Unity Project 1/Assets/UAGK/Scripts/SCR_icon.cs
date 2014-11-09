using UnityEngine;
using System.Collections;

public class SCR_icon : MonoBehaviour {
	public Vector3 iconPosition;
	//the viewport position of the icon.
	
	public float[] scaleBase=new float[2];
	//the overall scale of the object.
	
	public float scaleInitial=1f;
	//the scale the objects starts at.
	public float scaleDeath=1f;
	//the scale the objects goes to when fading out.
	
	public Color colour=new Color(0.5f,0.5f,0.5f,0.5f);
	//the colour of the object.
	
	public float alphaMult=1f;
	//the transparency of the object.
	
	public float lifeSpan=0f;
	//set this to the number of seconds that the text exists for before fading out.
	//setting it to 0 will mean that the text remains until manually destroyed.
	
	float[] scaleDefault;
	float[] scale;
	float[] alpha=new float[2]{0f,1f};
	float[] changeSpeed=new float[2]{0.5f,0.5f};
	
	
	public void StartUp (Vector3 forcePosition) {
		scale=new float[2]{scaleInitial,1f};
		
		if(forcePosition!=Vector3.zero){
			transform.position=forcePosition;	
		}	else {
			transform.position=iconPosition;
		}
		
		DisplayColour();
		SetScale();
	}
	
	public void SetScale(){
		scaleDefault=new float[2]{	(scaleBase[0]*SCR_main.resMult),
									(scaleBase[1]*SCR_main.resMult)
		};
		
		DisplayScale();
	}
	
	void DisplayScale(){
		guiTexture.pixelInset=new Rect(		(scaleDefault[0]*scale[0]*-0.5f),
											(scaleDefault[1]*scale[0]*-0.5f),
											(scaleDefault[0]*scale[0]),
											(scaleDefault[1]*scale[0])
		);
		
	}
	
	void Update () {
		if(scale[0]!=scale[1]){
			UpdateScale();
		}
		
		if(alpha[0]!=alpha[1]){
			UpdateAlpha();	
		}
		
		if(lifeSpan>0f){
			if(alpha[1]>0f){
				
				lifeSpan=Mathf.MoveTowards(lifeSpan,0f,Time.deltaTime);
				
				if(lifeSpan==0f){
					Kill ();	
				}
			}
		}
	}
	
	void UpdateScale(){
		scale[0]=Mathf.MoveTowards(scale[0],scale[1],(changeSpeed[0]*SCR_main.counterMult));
		
		DisplayScale();
	}
	
	void UpdateAlpha(){
		alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(changeSpeed[1]*SCR_main.counterMult));
		
		DisplayColour();
		
		if(alpha[0]==0f&&alpha[1]==0f){
			SCR_main.DestroyObj(gameObject);
		}
	}
	
	void DisplayColour(){
		guiTexture.color=new Color(colour.r,colour.g,colour.b,(alpha[0]*0.5f));
	}
	

	////////////////////////////////////////////
	
	
	public void Kill(){
		if(alpha[1]>0f){
			alpha[1]=0f;
			scale[1]=scaleDeath;
		}
	}
}
