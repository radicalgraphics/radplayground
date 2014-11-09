using UnityEngine;
using System.Collections;

public class SCR_input : MonoBehaviour {
	
	public AudioClip highlightSound;
	//the sound of an option being highlighted
	static SCR_sound SND_highlight;
	
	public AudioClip selectSound;
	//the sound of an option being selected
	
	public int controlType;
	//0		-	mouse
	//1		-	keyboard and mouse
	
	
	static SCR_sound SND_select;
	
	
	//SCR_main main;		// [DGT] Unused var
	
	public static int cType=0;
	
	public static bool playerControlActive=false;
	static Vector2 mousePos;
	
	public static int rightClickAction=-1;
	
	public void StartUp(){
		
		cType=controlType;
		
		//main=GetComponent<SCR_main>();		// [DGT] unused var code
		
		if(highlightSound){
			SND_highlight=SCR_main.CreateSound(transform,highlightSound,false,false);	
		}
		if(selectSound){
			SND_select=SCR_main.CreateSound(transform,selectSound,false,false);	
		}
	}
	
	void Update(){
		mousePos=new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		
		OptionCycle(0);
		
		if(rightClickAction>=0){
			if(Input.GetMouseButtonDown(1)){
				SCR_main.OptionAction(rightClickAction);
				OptionCycle (3);
				rightClickAction=-1;
			}
		}
		
		if(Input.GetMouseButtonDown(0)){
			OptionCycle(1);
		}
	}
	
	
	public static void OptionCycle(int com){
		
		GameObject[] options = GameObject.FindGameObjectsWithTag("Option");
		
		foreach (GameObject option in options){
			if(option.GetComponent<SCR_option>()){
				SCR_option o=option.GetComponent<SCR_option>();
				
				int action=0;
				
				if(com==0){
					action=o.Highlight(0,mousePos);
					
					if(action==1){
						if(SND_highlight){
							SND_highlight.PlaySound();
						}
					}
				}
				if(com==1){
					action=o.Highlight(1,mousePos);
					
					if(action==2||action==3){
						if(SND_select){
							SND_select.PlaySound();
						}	
						if(action==2){
							rightClickAction=-1;			
						}
					}
				}
				if(com==2){
					o.OptionEvent(0);
				}
				if(com==3){
					o.Kill();
				}
				if(com==4){
					o.OptionEvent(1);
				}
				
			}
		}
	}
	
}
