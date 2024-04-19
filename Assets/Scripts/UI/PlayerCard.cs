using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
	public GameObject face;
	public Sprite stockImage;
	int currStocks = 0;
	int currInsp = 0;
	public int drifterIndex;

	public Image HealthBar;
	//public GameObject BottomObject;

	// public Sprite[] portraits_no_Charge;
	// public Sprite[] portraits_with_Charge;
	// public Sprite[] portraits_one_Charge;
	public Image[] meterPips;
	public Animator[] meterPipAnimators;

	public Image ribbons;

	public GameObject bar;
	public GameObject stock;

	public GameObject inspoHolder;
	public GameObject stockHolder;
	public GameObject statusHolder;

	//Text TopText;
	//Text BottomText;

	//GameObjectShake TopShake;
	//GameObjectShake BottomShake;

	int mycolor; 
	int previousPercent = 0;

	const int MAX_STOCKS = 4;


	void Awake() {

		//TopShake = TopObject.GetComponent<GameObjectShake>();
		//TopText = TopObject.GetComponent<Text>();

		//BottomShake = BottomObject.GetComponent<GameObjectShake>();
		//BottomText = BottomObject.GetComponent<Text>();

		meterPipAnimators = new Animator[meterPips.Length];
		for(int i = 0; i < meterPips.Length; i++)
			meterPipAnimators[i] =  meterPips[i].gameObject.GetComponent<Animator>();
	}

	public void addStock() {
		addStocks(1);
	}
	public void addStocks( int num) {
		if (currStocks + num > MAX_STOCKS) {
			num = MAX_STOCKS - currStocks;
		}

		for(int i = 0; i < MAX_STOCKS; i++) {
			GameObject newStock = Instantiate(stock, new Vector3(0,0), Quaternion.identity);
			newStock.transform.SetParent(stockHolder.transform, false);
			newStock.transform.localScale = new Vector3(1, 1, 1);
			newStock.GetComponent<Image>().sprite = stockImage;
			currStocks++;
		}
	}

	public void addInspiration( int num) {
		for(int i = 0; i < num; i++) {
			GameObject newStock = Instantiate(stock, new Vector3(0,0), Quaternion.identity);
			newStock.transform.SetParent(inspoHolder.transform, false);
			newStock.transform.localScale = new Vector3(1, 1, 1);
			currInsp++;
		}
	}

	public GameObject addStatusBar(PlayerStatusEffect statusEffect,int icon, int duration, PlayerStatus status) {

		GameObject newBar = Instantiate(bar, new Vector3(0,0), Quaternion.identity);
		newBar.transform.SetParent(statusHolder.transform, false);
		newBar.transform.localScale = new Vector3(100, 100, 1);
		newBar.GetComponent<StatusBar>().status = status;
		newBar.GetComponent<StatusBar>().initialize(statusEffect,icon,duration);
		//newBar.GetComponent<StatusBar>().UpdateFrame();
		//currentStatusCount++;
		return newBar;

	}

	public void SetColor(int color) {
		mycolor = color;

		ribbons.color = CharacterMenu.ColorFromEnum[(PlayerColor)color];

	}

	public void SetCharge(int charge, DrifterMeterState meterState) {

		for(int i = 0; i < meterPips.Length; i++){
			float fillPercent = Mathf.Clamp((charge/100f - i),0f,1f);
			if(fillPercent == 1f) meterPipAnimators[i].Play("Full_Crystal_" + meterState);
			else meterPipAnimators[i].Play("Partial_Crystal_" + meterState);

			meterPips[i].fillAmount = fillPercent;
		}

		// chargeLevels.sprite = levels[(charge/100)];

		// float loc = -(charge/500f) * 65f;

		// chargeBar.anchoredPosition  =  new Vector2(-1 *loc,4);
		// chargeMask.anchoredPosition  = new Vector2(loc,-4);

	}

	public void removeStock() {
		removeStocks(1);
	}

	public void removeStocks(int num) {
		if(currStocks <= 0) {
			return;
		}
		currStocks -= num;

		if (stockHolder.transform.childCount < num) {
			num = stockHolder.transform.childCount;
		}
		for (int i = 0; i<num; i++) {
			Destroy(stockHolder.transform.GetChild(0).gameObject);
		}

	}

	public void setImages(Sprite stock) {
		//this.face.GetComponent<Image>().sprite = face;
		this.stockImage = stock;
	}

	internal void setInspiration(int insp){
		if (currInsp > insp) {
			currInsp--;
			Destroy(inspoHolder.transform.GetChild(0).gameObject);
		}
		else if (currInsp < insp) {
			addInspiration(1);
		}
	}


	internal void removeToStock(int stocks) {
		if (currStocks > stocks) {
			removeStock();
		}
	}

	public void setPercent(int sentPercent, int maxPercent) {
		// if(previousPercent  < sentPercent) {
		// 	TopShake.Shake(18,(sentPercent - previousPercent)/2f);
		// 	BottomShake.Shake(18,(sentPercent - previousPercent)/2f);
		// }
		previousPercent = sentPercent;

		// float greenVal = Mathf.Max((120f - (sentPercent/10f))/120f,0);
		// float blueVal = Mathf.Max((50f - (sentPercent/10f))/50f,0);
		// float redVal = Mathf.Max((500f - (sentPercent/10f))/500f,.8f);

		Color color;

		if(sentPercent < 400)
			color = new Color(151/256f,226/256f,256/256f,.6f);
		else if(sentPercent < 800)
			color = new Color(131/256f,226/256f,106/256f,.6f);
		else if(sentPercent < 1200)
			color = new Color(256/256f,226/256f,66/256f,.6f);
		else if(sentPercent < 1600)
			color = new Color(256/256f,156/256f,66/256f,.6f);
		else if(sentPercent < 2400)
			color = new Color(196/256f,46/256f,76/256f,.6f);
		else
			color = new Color(126/256f,26/256f,76/256f,.6f);

		HealthBar.color = color;

		HealthBar.fillAmount = (maxPercent - sentPercent)/(float)maxPercent;

		// this.BottomText.text = sentPercent.ToString("0.0")+"%";

		// this.TopText.color = new Color(redVal,greenVal,blueVal,1);
		// this.TopText.text = sentPercent.ToString("0.0")+"%";
	}

}
