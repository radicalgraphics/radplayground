using UnityEngine;
using System.Collections;

public class SCR_gui : MonoBehaviour {
	
	
	public Font[] textFont=new Font[1];
	//an array of the fonts to be used for the games text.
	public Material[] textFontMaterial=new Material[1];
	//an array of the font materials. It must much the size of the textFont array!
	
	public Color optionColour;
	//the standard colour of the options text.
	public Color optionColourHighlighted;
	//the colour of the options text when highlighted.
	
	public static Vector3 oColour;
	public static Vector3 oColourHighlighted;
	
	public static Font[] tFont;
	public static Material[] tFontMat;
	
	public static GameObject screenCoverObj;
	
	public static Vector2 optionSpacing=new Vector2(0f,-0.1f);


	public void StartUp () {
		
		oColour=new Vector3(optionColour.r,optionColour.g,optionColour.b);
		oColourHighlighted=new Vector3(optionColourHighlighted.r,optionColourHighlighted.g,optionColourHighlighted.b);
		
		tFont=textFont;
		tFontMat=textFontMaterial;
		
		screenCoverObj=Resources.Load("Objects/Gui/OBJ_screenCover",typeof(GameObject)) as GameObject;
		
	}
	
	////////////////
	//TEXT
	////////////////
	
	public static SCR_text CreateText(string textName,Vector3 pos){
		SCR_text text;
		
		GameObject textInst=Instantiate(Resources.Load (("Objects/Gui/_Text/Text_"+textName),typeof(GameObject)),Vector3.zero,Quaternion.identity) as GameObject;
		textInst.name=textName;
		text=textInst.GetComponent<SCR_text>();
		text.StartUp(pos);
		
		return text;
	}
	
	public static void RemoveText(string textName){
		GameObject[] text = GameObject.FindGameObjectsWithTag("Text");
		
		foreach (GameObject t in text){
			if(t.name==textName){
				t.GetComponent<SCR_text>().Kill();
			}
		}
	}
	
	////////////////
	//ICON
	////////////////
	
	public static SCR_icon CreateIcon(string iconName,Vector3 pos){
		GameObject iconInst=Instantiate(Resources.Load(("Objects/Gui/_Icon/Icon_"+iconName),typeof(GameObject)),Vector3.zero,Quaternion.identity) as GameObject;
		iconInst.name=iconName;
		SCR_icon icon=iconInst.GetComponent<SCR_icon>();
		icon.StartUp(pos);
		
		return icon;
	}
	
	public static void RemoveIcon(string iconName){
		GameObject[] icons = GameObject.FindGameObjectsWithTag("Icon");
		
		foreach (GameObject i in icons){
			if(i.name==iconName){
				i.GetComponent<SCR_icon>().Kill();
			}
		}
	}
	
	public static SCR_option CreateOption(string optionName,Vector2 pos){
		GameObject optionInst=Instantiate(Resources.Load (("Objects/Gui/_Option/Option_"+optionName),typeof(GameObject)),new Vector3(pos.x,pos.y,10f),Quaternion.identity) as GameObject;
		SCR_option option=optionInst.GetComponent<SCR_option>();
		
		return option;
	}
	
	
	////////////////
	//SCREEN COVER
	////////////////
	
	public static SCR_screenCover CreateScreenCover(int com){
		SCR_screenCover screenCover;
		
		float zDepth=100f;
		bool fadeIn=false;
		float fadeSpeed=0.5f;
		float alphaMult=1f;
		string colour="black";
		
		if(com==1){
			fadeIn=true;
		}
		
		if(com==2){
			//pause screen
			zDepth=-5f;
			fadeSpeed=0.95f;
			fadeIn=true;
			alphaMult=0.55f;
		}
		
		GameObject screenCoverInst=Instantiate(screenCoverObj,new Vector3(0.5f,0.5f,zDepth),Quaternion.identity) as GameObject;
		screenCover=screenCoverInst.GetComponent<SCR_screenCover>();
		screenCover.StartUp(colour,fadeIn,fadeSpeed,alphaMult);
		
		return screenCover;
	}
}
