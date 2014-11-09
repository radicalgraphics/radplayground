using UnityEngine;
using System.Collections;

public class SCR_option : MonoBehaviour {
	
	public int fontType=0;
	//This determines the font that the text will use. It corresponds to a slot in the SCR_gui.textFont array.
	
	public int actionID=0;
	//the action executed in SCR_main.OptionAction when the option is selected.
	
	public bool switchSoundFX;
	//if true, this option will switch the sound FX on and off.
	
	public bool switchMusic;
	//if true, this option will switch the music on and off.
	
	public static float scaleMultiplier=1f;
	
	
	float scaleBase=0.25f;
	
	float scaleInitial=0.5f;
	float scaleDeath=0.5f;
	float scaleHighlight=1.15f;
	
	Vector3 colour;
	Vector3[] colourArray;
	
	float[] scale;
	float[] scaleDefault;
	float[] alpha=new float[2]{0f,1f};
	float[] changeSpeed=new float[2]{5f,0.65f};
	
	float[] hitBoxMult=new float[2]{0.95f,0.3f};
	Rect hitBox;
	
	bool isActive=false;
	bool highlighted=false;
	bool destroyAll=true;
	
	//FLASH
	
	float flashCounter=0f;
	int flashTotal=0;
	bool flashOn=false;
	
	
	void Awake(){
		
		
		colourArray=new Vector3[2]{	SCR_gui.oColour,
									SCR_gui.oColourHighlighted
		};
		
		scaleBase*=scaleMultiplier;
		
		guiText.font=SCR_gui.tFont[fontType];
		guiText.material=SCR_gui.tFontMat[fontType];
		
		scale=new float[2]{scaleInitial,1f};
		
		SetColour(0);
		
		SetScale();
		
		if(switchSoundFX||switchMusic){
			destroyAll=false;
			SetSwitchText();
		}
	}
	
	
	public void SetScale(){
		scaleDefault=new float[2]{	(scaleBase*SCR_main.aspectShrink),
									scaleBase
		};
		
		
		
		float[] hitBoxScale=new float[2]{	(scaleDefault[0]*hitBoxMult[0]*(float)Screen.width),
											(scaleDefault[1]*hitBoxMult[1]*(float)Screen.height)
		};
		
		float[] pos=new float[2]{	(transform.position.x*(float)Screen.width),
									(transform.position.y*(float)Screen.height)
		};
		
		hitBox=new Rect(	(pos[0]-(hitBoxScale[0]*0.5f)),
							(pos[1]-(hitBoxScale[1]*0.5f)),
							hitBoxScale[0],
							hitBoxScale[1]
		);
		
		DisplayScale();
	}
	
	void DisplayScale(){
		transform.localScale=new Vector3((scaleDefault[0]*scale[0]),(scaleDefault[1]*scale[0]),1f);
	}
	
	
	void Update () {
		
		UpdateScale();
		
		if(alpha[0]!=alpha[1]){
			UpdateAlpha();	
		}
		
		if(flashTotal>0){
			UpdateFlash();	
		}
	}
	
	
	void UpdateScale(){
		
		scale[0]=Mathf.Lerp(scale[0],scale[1],(changeSpeed[0]*SCR_main.counterMult));
		
		DisplayScale();
	}
	
	void UpdateAlpha(){
		alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(changeSpeed[1]*SCR_main.counterMult));
		
		DisplayColour();
		
		if(alpha[0]==alpha[1]){
			if(alpha[1]==1f){
				isActive=true;
			}
			if(alpha[1]==0f){
				SCR_main.DestroyObj(gameObject);
			}
		}
	}
	
	
	void SetColour(int com){
		colour=colourArray[com];
		DisplayColour();
	}
	
	void DisplayColour(){
		guiText.color=new Color(colour.x,colour.y,colour.z,alpha[0]);
	}
	
	////////////////////
	//OPTION ACTIONS
	////////////////////
	
	public int Highlight(int com,Vector2 mPoint){
		int action=0;
	
		if(isActive){
			bool mouseOver=false;
		
			if(mPoint.x>hitBox.xMin
			&&mPoint.x<hitBox.xMax
			&&mPoint.y>hitBox.yMin
			&&mPoint.y<hitBox.yMax){
				mouseOver=true;
			}
			
			if(com==0){
				/*
				if(actionID==1){
					print (	"HitBox: "+hitBox.xMin+", "+hitBox.yMin+", "+hitBox.xMax+", "+hitBox.yMax+
							"Mouse: "+mPoint.x+", "+mPoint.y   );
				}
				*/
				
				if(mouseOver){
					if(highlighted==false){
						HighlightEvent(true);
						action=1;
					}
				}	else {
					if(highlighted){
						HighlightEvent(false);
					}
				}
			}
			
			if(com==1){
				if(mouseOver){
					SCR_input.OptionCycle(2);
					Selected();
					
					if(destroyAll){
						action=2;
					}	else {
						action=3;
					}
				}
			}
		}
		
		return action;
	}
	
	public void HighlightEvent(bool com){
		if(com){
			highlighted=true;
			SetColour(1);
			scale[1]=scaleHighlight;
		}	else {
			highlighted=false;
			SetColour(0);
			scale[1]=1f;
		}
	}
	
	void Selected(){
		StartFlash ();
		HighlightEvent(true);
		SetColour(1);
	}
	
	public void OptionEvent(int com){
		if(com==0){
			HighlightEvent (false);
			isActive=false;
		}
		
		if(com==1){
			isActive=true;	
		}
	}
	
	
	//FLASH
	
	public void StartFlash(){
		flashTotal=3;
		flashCounter=0f;
		flashOn=true;
		DisplayFlash();
	}
	
	void DisplayFlash(){
		if(flashOn){
			guiText.enabled=false;
		}	else {
			guiText.enabled=true;
		}
	}
	
	void UpdateFlash(){
		flashCounter+=SCR_main.counterMult;
		
		if(flashCounter>=0.2f){
			flashCounter=0f;
			flashOn=!flashOn;
			
			DisplayFlash();
			
			if(flashOn==false){
				flashTotal--;
				
				if(flashTotal==0){
					
					if(destroyAll){
						SCR_main.OptionAction(actionID);
						SCR_input.OptionCycle(2);
						SCR_input.OptionCycle(3);
					}	else {
						SCR_input.OptionCycle(4);
						
						SCR_main main =GameObject.Find ("MAIN").GetComponent<SCR_main>();
						
						if(switchSoundFX){
							main.AlterSetting(0);
						}	else {
							if(switchMusic){
								main.AlterSetting(1);
							}
						}
						
						
						SetSwitchText();
						
					}
				}
			}
		}
	}
	
	
	void SetSwitchText(){
		if(switchSoundFX){
			if(SCR_main.fxOn==1){
				guiText.text="Sound FX On";
			}	else {
				guiText.text="Sound FX Off";
			}
		}	else {
			if(switchMusic){
				if(SCR_main.musOn==1){
					guiText.text="Music On";
				}	else {
					guiText.text="Music Off";
				}
			}
		}
	}
	
	
	////////////////////////////////////////////
	
	
	public void Kill(){
		if(alpha[1]>0f){
			flashTotal=0;
			isActive=false;
			alpha[1]=0f;
			scale[1]=scaleDeath;
			changeSpeed[0]=0.5f;
		}
	}
}
