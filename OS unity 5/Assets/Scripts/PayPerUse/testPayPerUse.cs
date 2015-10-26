using UnityEngine;
using System.Collections;

public class testPayPerUse : MonoBehaviour {
	
	private PayPerUseCtrl m_payperuseCtrl;
	
	public string _ActivationCode="";
	public string _DaysUsed="";
	private ArrayList _days;
	
	void Awake()
    {
		m_payperuseCtrl = new PayPerUseCtrl(PayPerUseMdl.Instance);
		PayPerUseMdl.PayPerUseSendRequest += ActivateSendCode;	
	}
	
	// Use this for initialization
	void Start () {
		PayPerUseMdl.Instance.ResetDays();	
		ReinitDays();
	}
	
	private void ReinitDays()
	{
		m_payperuseCtrl.NotifyAddToday();
		
		
		/*int day = 2-1;
		int month = 10-1;
		int year = 2013-1970;		
		m_payperuseCtrl.NotifyAddThisDate(day, month, year);
		day = 25-1;
		month = 08-1;
		year = 2013-1970;		
		m_payperuseCtrl.NotifyAddThisDate(day, month, year;*/
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI()
	{
		if (GUI.Button(new Rect(10,70,150,30),"Send Activation Code"))
		{
			Debug.Log("PayPerUseMdl send code");
			StartCoroutine(PayPerUseMdl.Instance.SendCode());
		//	ReinitDays();
		}
		if (GUI.Button(new Rect(10,110,150,30),"ReinitDays"))
		{
			Debug.Log("ReinitDays");
			ReinitDays();
			ActivateSendCode(false);
		}
		
		GUI.Label (new Rect (10, 10, 500, 20), _ActivationCode);
		GUI.Label (new Rect (10, 40, 500, 20), _DaysUsed);
	}
	
	private void ActivateSendCode(bool enable)
	{
		_ActivationCode = PayPerUseMdl.Instance.GetCode();
		Debug.Log("Activation Code : "+_ActivationCode);
		_days = PayPerUseMdl.Instance.CreateDaysFromCode(_ActivationCode);
		_DaysUsed = "";
		foreach(int day in _days)
		{
			_DaysUsed+=day+" ";
		}	
	}
}
