using UnityEngine;
using System.Collections;

public class PayPerUseCtrl
{
	
	private PayPerUseMdl m_model;
	
	public PayPerUseCtrl(PayPerUseMdl mdl)
	{
		m_model = mdl;
	}
	
	public void NotifyAddToday()
	{
		m_model.AddToday();
	}	
	public void NotifyAddThisDate(int day,int month,int year)
	{
		m_model.AddThisDate(day,month,year);
	}
}
