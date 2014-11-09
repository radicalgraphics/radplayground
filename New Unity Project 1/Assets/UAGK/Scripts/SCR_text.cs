using UnityEngine;
using System.Collections;

public class SCR_text : MonoBehaviour {
	
	public int fontType=0;
	//This determines the font that the text will use. It corresponds to a slot in the SCR_gui.textFont array.
	
	public float lifeSpan=0f;
	//set this to the number of seconds that the text exists for before fading out.
	//setting it to 0 will mean that the text remains until manually destroyed.
	
	public Vector2 scaleBase;
	//this determines the scale of the text.
	
	public Vector3 textPosition;
	//the viewport position of the text.
	
	public Color textColour;
	//the colour of the text.
	//the alpha value on this isn't used.
	
	[HideInInspector]
	public bool worldLinked=false;
	
	Transform cam;
	Vector3 worldPos;
	float ySpeed;
	
	float[] alpha=new float[4]{0f,1f,0.65f,0.65f};
	//bool fading=false;		// [DGT] unused var
	
	static float scaleMultiplier=1f;
	
	
	public void StartUp(Vector3 forcePosition){
		scaleBase*=scaleMultiplier;
		
		guiText.font=SCR_gui.tFont[fontType];
		guiText.material=SCR_gui.tFontMat[fontType];
		
		if(forcePosition!=Vector3.zero){
			transform.position=forcePosition;	
		}	else {
			transform.position=textPosition;
		}
		
		SetScale ();
		DisplayAlpha();
	}
	
	public void SetWorldPos(Vector3 _worldPos,float _ySpeed){
		cam=GameObject.FindWithTag("MainCamera").transform;
		worldPos=_worldPos;
		ySpeed=_ySpeed;
		worldLinked=true;
		
		UpdatePos();
	}
	
	public void SetFadeSpeed(float fadeIn,float fadeOut){
		alpha[2]=fadeIn;
		alpha[3]=fadeOut;
	}
	
	
	void Update () {
		if(alpha[0]!=alpha[1]){
			int alphaSlot=2;
			if(alpha[1]==0f){
				alphaSlot=3;
			}
			
			alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(alpha[alphaSlot]*SCR_main.counterMult));
			
			DisplayAlpha ();
			
			if(alpha[1]==0f&&alpha[0]==0f){
				SCR_main.DestroyObj(gameObject);	
			}
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
	
	public void UpdatePos(){
		worldPos=new Vector3(worldPos.x,(worldPos.y+(ySpeed*Time.deltaTime)),worldPos.z);
		Vector3 camPos=cam.camera.WorldToViewportPoint(worldPos);
		transform.position=new Vector3(camPos.x,camPos.y,worldPos.z);
	}
	
	public void SetScale(){
		Vector2 scale=new Vector2((scaleBase.x*SCR_main.aspectShrink),(scaleBase.y));
		transform.localScale=new Vector3(scale.x,scale.y,1f);
	}
	
	public void UpdateText(string txt){
		guiText.text=txt;	
	}
	
	void DisplayAlpha(){
		guiText.color=new Color(textColour.r,textColour.g,textColour.b,alpha[0]);	
	}
	
	public void Kill(){
		if(alpha[1]>0f){
			alpha[1]=0f;
		}
	}
}
