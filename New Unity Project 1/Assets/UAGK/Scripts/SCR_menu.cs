using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCR_menu : MonoBehaviour {

	public AudioClip menuMusic;
	//the music that plays during the menu.
	
	public Material skyBoxMaterial;
	//fill this in if you want to use a skybox for the menu.

	int state=10;
	float stateCounter=0f;
	
	SCR_text controlTypeDescription = null;		// [DGT] Var made public
	
	int gotoMenu=0;

	private string controlsIcon = "";	// [DGT] Controls Icon (platform-specific)
	
	private SCR_main	main;		//	[DGT]
 
	
	
	void Awake(){
		GameObject m	= GameObject.Find("MAIN");		// [DGT] Get reference to the main script
		this.main		= m.GetComponent<SCR_main>();

		SCR_gui.CreateScreenCover(0);
		
		if(menuMusic&&SCR_main.hMusic){
			GameObject.Find("MAIN").GetComponent<SCR_music>().PlayMusic(menuMusic);	
		}
		
	

		if(skyBoxMaterial){
			RenderSettings.skybox=skyBoxMaterial;
		}
	}
	
	void Update () {
		if(state>0){
			UpdateState();	
		}
	}
	
	void UpdateState(){
		stateCounter+=Time.deltaTime;
		
		if(state==10){
			if(stateCounter>=0.45f){
				SetState(0);
				OpenMenu(gotoMenu);
			}
		}
	}
	
	void SetState(int com){
		state=com;
		stateCounter=0f;
	}
	
	void OpenMenu(int com){
		Vector2 optionPos;
		
		if(com==0){
			//TITLE
			SCR_input.rightClickAction=-1;
			
			SCR_gui.CreateIcon("Title",Vector3.zero);
			
			optionPos=new Vector2(0.5f,0.6f);
			SCR_gui.CreateOption("StartGame",optionPos);
			optionPos+=SCR_gui.optionSpacing;
			
			if(SCR_main.hsOn){
				SCR_gui.CreateOption("HighScores",optionPos);
				optionPos+=SCR_gui.optionSpacing;
			}
			
			if(SCR_main.opOn){
				SCR_gui.CreateOption("Options",optionPos);
				optionPos+=SCR_gui.optionSpacing;
			}
			
			if(SCR_main.egOn){
				SCR_gui.CreateOption("ExitGame",optionPos);
				optionPos+=SCR_gui.optionSpacing;
			}
			

			// [DGT] Choose Controls Info Icon based on the platform.

			if (this.main.RunningOnMobile())
				this.controlsIcon = "Controls_Mobile";
			else 
				this.controlsIcon = "Controls";
			

			SCR_gui.CreateIcon(this.controlsIcon,Vector3.zero);
			//SCR_gui.CreateIcon("Controls",Vector3.zero);
		}
		
		if(com==1){
			//OPTIONS
			SCR_input.rightClickAction=0;
			
			SCR_gui.CreateIcon("Options",Vector3.zero);
			optionPos=new Vector2(0.5f,0.575f);
			
			SCR_gui.CreateOption("ChangeSoundFX",optionPos);
			optionPos+=SCR_gui.optionSpacing;
			
			if(SCR_main.hMusic){
				SCR_gui.CreateOption("ChangeMusic",optionPos);
				optionPos+=SCR_gui.optionSpacing;
			}
			SCR_gui.CreateOption("WipeData",optionPos);
			optionPos+=SCR_gui.optionSpacing;
			SCR_gui.CreateOption("OptionsReturn",optionPos);
		}
		
		if(com==2){
			//WIPE DATA
			
			SCR_input.rightClickAction=2;
			
			SCR_gui.CreateIcon("WipeData",Vector3.zero);
			optionPos=new Vector2(0.5f,0.5f);
			
			SCR_gui.CreateOption("WipeDataYes",optionPos);
			optionPos+=SCR_gui.optionSpacing;
			SCR_gui.CreateOption("WipeDataNo",optionPos);
		}
		
		
		if(com==3){
			//HIGH SCORES
			
			SCR_input.rightClickAction=0;
			
			SCR_gui.CreateIcon("HighScores",Vector3.zero);
			
			float highScoreY=0.7f;
			float highScoreSpacing=-0.045f;
			float rankX=0.475f;
			float scoreX=0.525f;
			
			for(int i=0; i<SCR_main.highScore.Count; i++){
				SCR_text tRank=SCR_gui.CreateText("HighScoreRank",new Vector3(rankX,highScoreY,0f));
				tRank.UpdateText((i+1).ToString());
				
				SCR_text tScore=SCR_gui.CreateText("HighScoreNumber",new Vector3(scoreX,highScoreY,0f));
				tScore.UpdateText(SCR_main.highScore[i].ToString());
				
				highScoreY+=highScoreSpacing;
			}
			
			SCR_gui.CreateOption("HighScoresReturn",new Vector2(0.5f,0.15f));
		}
	}
	
	public void GotoMenu(int com){
		FinishMenu();
		SetState(10);
		
		gotoMenu=com;
	}
	
	public void FinishMenu(){
		SCR_input.rightClickAction=-1;
		
		SCR_gui.RemoveIcon("Title");
		SCR_gui.RemoveIcon("Options");
		SCR_gui.RemoveIcon("WipeData");
		SCR_gui.RemoveIcon(this.controlsIcon); // "Controls");	// [DGT]
		SCR_gui.RemoveIcon("HighScores");
		
		SCR_gui.RemoveText("HighScoreRank");
		SCR_gui.RemoveText("HighScoreNumber");
		
		if(controlTypeDescription){
			controlTypeDescription.Kill();
		}
	}
}
