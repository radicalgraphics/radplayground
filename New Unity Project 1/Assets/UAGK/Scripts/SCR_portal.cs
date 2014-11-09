using UnityEngine;
using System.Collections;

public class SCR_portal : MonoBehaviour {

	[HideInInspector]
	public bool isActive=true;

	public void Collect(){
        GameObject.FindWithTag ("GameController").GetComponent<SCR_stage>().EnteredPortal(); //AllEnemiesDefeated();	// [DGT]
		isActive=false;
	}
}
