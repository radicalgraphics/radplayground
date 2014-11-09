using UnityEngine;
using System.Collections;

public class SCR_camStage : MonoBehaviour {
	
	Transform target;
	
	public float playerFollowSpeed;
	
	public Vector3 offsetDefault;
	public Vector3 rotDefault;
	
	public float edgeOffsetSide;
	public float edgeOffsetUpper;
	public float edgeOffsetLower;
	
	float[] edgeLimit;
	
	Vector3 pos;
	float[] posTarget=new float[3];
	
	
	Quaternion[] rot=new Quaternion[2];
	
	float[] fov=new float[2]{45f,45f};
	
	float[] moveSpeed=new float[2]{0f,3f};
	
	public void InitiateStage(int com){
		
		if(com==0){
			pos=(target.transform.position+(Vector3.up*0.6f)+(Vector3.forward*-1f));
			
			UpdateFollow();
			
			rot[0]=Quaternion.identity;
			rot[1]=Quaternion.Euler(rotDefault.x,rotDefault.y,rotDefault.z);
			
			
			UpdateEdgeLimit();
			DisplayCamera();
		}
		
		if(com==1){
			moveSpeed[1]=playerFollowSpeed;	
		}
	}
	
	public void UpdateEdgeLimit(){
		edgeLimit=new float[4]{		(SCR_stage.sX[0]+(edgeOffsetSide*SCR_main.screenMult)),
									(SCR_stage.sX[1]-(edgeOffsetSide*SCR_main.screenMult)),
									(SCR_stage.sZ[1]+offsetDefault.z-edgeOffsetUpper),
									(SCR_stage.sZ[0]+offsetDefault.z+edgeOffsetLower)
		};
		
		ConstrainEdge();
	}
	
	void Update () {
		
		moveSpeed[0]=Mathf.MoveTowards(moveSpeed[0],moveSpeed[1],(Time.deltaTime*moveSpeed[1]*0.5f));
		
		if(target){
			UpdateFollow();	
		}
		
		ConstrainEdge();
	}
	
	void FixedUpdate(){
		UpdateMovement();
		DisplayCamera();
	}
	
	void UpdateFollow(){
		posTarget[0]=(target.transform.position.x+offsetDefault.x);
		posTarget[1]=(target.transform.position.y+offsetDefault.y);
		posTarget[2]=(target.transform.position.z+offsetDefault.z);
		
	}
	
	void ConstrainEdge(){
		//constrain camera to boundaries
		if(posTarget[0]<edgeLimit[0]){
			posTarget[0]=edgeLimit[0];
		}
		if(posTarget[0]>edgeLimit[1]){
			posTarget[0]=edgeLimit[1];
		}
		if(posTarget[2]>edgeLimit[2]){
			posTarget[2]=edgeLimit[2];	
		}
		if(posTarget[2]<edgeLimit[3]){
			posTarget[2]=edgeLimit[3];
		}	
	}
	
	void UpdateMovement(){
		
		Vector3 posTargetV3=new Vector3(posTarget[0],posTarget[1],posTarget[2]);
		
		pos=Vector3.Lerp (pos,posTargetV3,(Time.deltaTime*moveSpeed[0]));
		
		rot[0]=Quaternion.Slerp (rot[0],rot[1],(Time.deltaTime*moveSpeed[0]));
		fov[0]=Mathf.Lerp (fov[0],fov[1],(Time.deltaTime*moveSpeed[0]));
		
		
		GameObject[] text = GameObject.FindGameObjectsWithTag("Text");
		
		foreach (GameObject t in text){
			SCR_text ts=t.GetComponent<SCR_text>();
			
			if(ts.worldLinked){
				ts.UpdatePos();	
			}
		}
	}
	
	void DisplayCamera(){
		transform.position=pos;
		transform.rotation=rot[0];
		camera.fieldOfView=fov[0];
		
		SetSoundCentre();
	}
	
	void SetSoundCentre(){
		Plane plane=new Plane(Vector3.up,Vector3.zero);
		float hitDist=0f;
		Ray ray;
		
		ray=Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
		
		if (plane.Raycast(ray,out hitDist)) {
			Vector3 soundCentre=ray.GetPoint(hitDist);
			SCR_main.soundCentre=new Vector2(soundCentre.x,soundCentre.z);
		}
	}
	
	public void SetTarget (Transform _target) {
		target=_target;
	}
}
