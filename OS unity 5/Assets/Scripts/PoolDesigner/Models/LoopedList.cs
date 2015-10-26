using UnityEngine;
using System.Collections.Generic;

// List permettant un acces aux element avec des indices negatif
// ou superieur a Count
public class LoopedList<T> : List<T> 
{
	protected int CorrectIndex (int index)
	{
		int newIndex = index;
		
		if (newIndex < 0)
		{
			while (newIndex < 0)
			{
				newIndex += Count;	
			}
		}
		else if (newIndex >= Count)
		{
			newIndex = newIndex % Count; 
		}
		
		return newIndex;
	}
	
	new public T this [int index]
	{
		get
		{
			return base [CorrectIndex (index)];
		}
		
		set
		{
			base [CorrectIndex (index)] = value;
		}
	}
}
