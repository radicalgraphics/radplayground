using UnityEngine;
using System.Collections;

public class SCR_characterSelect : MonoBehaviour {

    void OnMouseDown() {

		GameObject m=GameObject.Find ("MAIN");
		SCR_main main=m.GetComponent<SCR_main>();

		//GameObject s=GameObject.Find ("player select control");
        //SCR_playerSelect player=s.GetComponent<SCR_playerSelect>();		
	
		if(gameObject.tag == "Player01"){
			main.playerSelect = true;
		}
		else if(gameObject.tag == "Player02"){
			main.playerSelect = false;
		}	

	    SCR_main.level=1;			
        Application.LoadLevel (Application.loadedLevel +1);
    }
}
