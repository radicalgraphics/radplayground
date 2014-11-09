using UnityEngine;
using System.Collections;

public class SCR_sound : MonoBehaviour {
	
	bool randomPitch;
	bool distanceOn;

	public void StartUp (AudioClip fxClip,bool _randomPitch,bool _distanceOn) {
		audio.clip=fxClip;
		randomPitch=_randomPitch;
		distanceOn=_distanceOn;
	}
	
	public void PlaySound(){
		if(SCR_main.fxOn==1){
			if(randomPitch){
				audio.pitch=(1f+Random.Range(-0.1f,0.1f));
			}
			
			if(distanceOn){
				float distanceMult=SCR_main.GetSoundDistanceMultiplier(transform.position);
				audio.volume=distanceMult;
			}
			
			audio.Play();
		}
	}
}
