using UnityEngine;
using System.Collections;

public class SCR_healthBar : MonoBehaviour {
	
	float[] scaleBase=new float[2]{170f,22f};
	
	Vector3 colour;
	public Color[] colourArray;
	
	float[] scaleDefault;
	float[] alpha=new float[4]{0f,0f,7f,3.5f};
	
	Camera cam;
	Transform bar;
	float[] barScale=new float[2]{1f,1f};
	
	float appearCounter=0f;
	bool fadeAway=false;
	
	float zPos;
	
	
	public void StartUp(int id){
		bar=transform.Find ("bar");
		cam=GameObject.FindWithTag ("MainCamera").camera;
		
		SetColour(0);
		
		SetScale();
		
		zPos=(id-10000);
	}
	
	
	public void SetScale(){
		scaleDefault=new float[2]{	(scaleBase[0]*SCR_main.resMult),
									(scaleBase[1]*SCR_main.resMult)
		};
		
		DisplayScale(0);
		DisplayScale(1);
	}
	
	void DisplayScale(int com){
		
		if(com==0){
			guiTexture.pixelInset=new Rect(		(scaleDefault[0]*-0.5f),
												(scaleDefault[1]*-0.5f),
												scaleDefault[0],
												scaleDefault[1]
			);
		}	else {
			bar.guiTexture.pixelInset=new Rect(	(scaleDefault[0]*-0.5f),
												(scaleDefault[1]*-0.5f),
												(scaleDefault[0]*barScale[0]),
												scaleDefault[1]
			);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(alpha[0]!=alpha[1]){
			UpdateAlpha();
		}
		
		if(barScale[0]!=barScale[1]){
			UpdateBarScale();	
		}
		
		if(appearCounter>0f){
			appearCounter=Mathf.MoveTowards(appearCounter,0f,Time.deltaTime);
			
			if(appearCounter==0f){
				alpha[1]=0f;
			}
		}
	}
	
	public void UpdatePos(Vector3 newPos){
		Vector3 screenPos = cam.WorldToViewportPoint(newPos);
		transform.position=new Vector3(screenPos.x,screenPos.y,zPos);
		
		bar.transform.localPosition=new Vector3(0f,0f,(zPos+0.5f));
	}
	
	void UpdateBarScale(){
		barScale[0]=Mathf.Lerp(barScale[0],barScale[1],(Time.deltaTime*6.5f));
		DisplayScale(1);
	}
	
	void UpdateAlpha(){
		if(alpha[1]==1f){
			alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(alpha[2]*Time.deltaTime));
		}	else {
			alpha[0]=Mathf.MoveTowards(alpha[0],alpha[1],(alpha[3]*Time.deltaTime));
		}
		
		DisplayColour();
		
		if(fadeAway){
			if(alpha[0]==0f&&alpha[1]==0f){
				SCR_main.DestroyObj(gameObject);
			}
		}
	}
	
	
	public void SetColour(int com){
		colour=new Vector3(		colourArray[com].r,
								colourArray[com].g,
								colourArray[com].b
			);
		
		DisplayColour();
	}
	
	void DisplayColour(){
		guiTexture.color=new Color(0.5f,0.5f,0.5f,(alpha[0]*0.5f));
		bar.guiTexture.color=new Color(colour.x,colour.y,colour.z,(alpha[0]*0.5f));
	}
	
	public void UpdateHealth(float _barScale){
		barScale[1]=_barScale;
	}
	
	public void Appear(){
		alpha[1]=1f;
		appearCounter=1.4f;
	}
	
	public void Kill(){
		fadeAway=true;
	}
}
