using UnityEngine;
using System.Collections;

public interface ILibBuilder<T>
{
	//create model of type T
	T GetLibModel ();
}
