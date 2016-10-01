using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chest : MonoBehaviour 
{
	public List<GridItemData> Items;
	public int ColSize;
	public int RowSize;

	public void GenerateContent()
	{
		Items = GameManager.Inst.ItemManager.GenerateRandomInventory(null, ColSize, RowSize);
	}


}
