using UnityEngine;
using System.Collections;

public class ItemAttribute
{
	public string Name;
	public object Value;

	public ItemAttribute(string name, object value)
	{
		Name = name;
		Value = value;
	}
}
