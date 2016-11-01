using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager
{




	public void Initialize()
	{
		

		/*
		backpack.AddGridItem(item1, 0, 0, GridItemOrient.Landscape);
		backpack.AddGridItem(item2, 2, 3, GridItemOrient.Portrait);
		backpack.AddGridItem(item3, 7, 5, GridItemOrient.Landscape);
		backpack.AddGridItem(item4, 4, 3, GridItemOrient.Landscape);
		backpack.AddGridItem(item5, 0, 3, GridItemOrient.Landscape);
		backpack.AddGridItem(item5, 1, 3, GridItemOrient.Landscape);
		*/

		GameObject [] objects = GameObject.FindGameObjectsWithTag("Chest");

		foreach(GameObject o in objects)
		{
			o.GetComponent<Chest>().GenerateContent();
		}

		objects = GameObject.FindGameObjectsWithTag("PickupItem");

		foreach(GameObject o in objects)
		{
			PickupItem pickup = o.GetComponent<PickupItem>();
			pickup.Item = LoadItem(pickup.ItemID);
		}
	}


	public List<GridItemData> GenerateRandomInventory(List<ItemType> Types, int ColSize, int RowSize)
	{
		List<GridItemData> items = new List<GridItemData>();


		Item item3 = LoadItem("flakjacket");

		Item item7 = LoadItem("pasgthelmet");


		Item item8 = LoadItem("ammo762_39");


		items.Add(new GridItemData(item3, 0, 0, GridItemOrient.Portrait, 1));
		items.Add(new GridItemData(item7, 0, 3, GridItemOrient.Landscape, 1));
		items.Add(new GridItemData(item8, 4, 0, GridItemOrient.Portrait, 50));


		return items;
	}


	public void LoadPartyInventory()
	{





		CharacterInventory inventory1 = GameManager.Inst.PlayerControl.Party.Members[0].Inventory;
		//CharacterInventory inventory2 = GameManager.Inst.PlayerControl.Party.Members[1].Inventory;



		//inventory1.Backpack.Add(new GridItemData(LoadItem("44magnum"), 2, 3, GridItemOrient.Portrait, 1));
		//inventory1.Backpack.Add(new GridItemData(LoadItem("flakjacket"), 5, 3, GridItemOrient.Landscape, 1));
		inventory1.Backpack.Add(new GridItemData(LoadItem("pipegrenade"), 0, 7, GridItemOrient.Landscape, 1));
		//inventory1.Backpack.Add(new GridItemData(LoadItem("kevlarhelmet"), 0, 6, GridItemOrient.Landscape, 1));
		inventory1.Backpack.Add(new GridItemData(LoadItem("ammo762_39"), 0, 9, GridItemOrient.Landscape, 80));
		inventory1.Backpack.Add(new GridItemData(LoadItem("ammo762_54r"), 2, 9, GridItemOrient.Landscape, 40));
		inventory1.Backpack.Add(new GridItemData(LoadItem("ammo12shot"), 4, 9, GridItemOrient.Landscape, 20));
		inventory1.Backpack.Add(new GridItemData(LoadItem("huntingshotgun"), 0, 0, GridItemOrient.Landscape, 1));
		inventory1.Backpack.Add(new GridItemData(LoadItem("svd"), 0, 3, GridItemOrient.Landscape, 1));
		inventory1.Backpack.Add(new GridItemData(LoadItem("ak47"), 1, 6, GridItemOrient.Landscape,1));

		inventory1.RifleSlot = LoadItem("machete");
		inventory1.ThrowSlot = LoadItem("pipegrenade");
		//inventory1.ArmorSlot = LoadItem("flakjacket");
		inventory1.SideArmSlot = LoadItem("44magnum");
		inventory1.HeadSlot = LoadItem("kevlarhelmet");

		/*
		inventory2.Backpack.Add(new GridItemData(LoadItem("flakjacket"), 7, 5, GridItemOrient.Landscape, 1));
		inventory2.Backpack.Add(new GridItemData(LoadItem("pasgthelmet"), 4, 3, GridItemOrient.Landscape, 1));
		inventory2.Backpack.Add(new GridItemData(LoadItem("lightarmor"), 0, 0, GridItemOrient.Landscape, 1));
		inventory2.RifleSlot = LoadItem("ak47");
		inventory2.SideArmSlot = LoadItem("44magnum");
		*/


	}

	public void LoadNPCInventory(CharacterInventory inventory)
	{
		Item item1 = LoadItem("huntingshotgun");
		Item item3 = LoadItem("lightarmor");

		Item item2 = LoadItem("44magnum");

		Item item8 = LoadItem("ammo12shot");


		inventory.Backpack.Add(new GridItemData(item8, 5, 5, GridItemOrient.Landscape, 30));
		inventory.SideArmSlot = item2;
		inventory.RifleSlot = item1;
		inventory.ArmorSlot = item3;
	}



	public string GetItemNameFromID(string itemID)
	{
		if(itemID == "ammo762_39")
		{
			return "7.62x39mm SD";
		}

		if(itemID == "ammo44")
		{
			return ".44 Magnum";
		}

		return "";
	}

	public Item LoadItem(string itemID)
	{
		Item item1 = new Item();
		item1.Name = "AK 47";
		item1.Description = "Standard service rifle of the rebel army, high damage and low accuracy.";
		item1.PrefabName = "AK47";
		item1.SpriteName = "ak47";
		item1.Weight = 6;
		item1.ID = "ak47";
		item1.Type = ItemType.PrimaryWeapon;
		item1.GridCols = 8;
		item1.GridRows = 3;
		item1.MaxStackSize = 1;
		item1.Attributes.Add(new ItemAttribute("_Muzzle Velocity", 110f));
		item1.Attributes.Add(new ItemAttribute("Impact", 10f));
		item1.Attributes.Add(new ItemAttribute("Accuracy", 0.7f));
		item1.Attributes.Add(new ItemAttribute("Range", 20f));
		item1.Attributes.Add(new ItemAttribute("Magazine Size", 30));
		item1.Attributes.Add(new ItemAttribute("Recoil", 0.5f));
		item1.Attributes.Add(new ItemAttribute("Rate of Fire", 10f));
		item1.Attributes.Add(new ItemAttribute("Handling", 0.6f));
		item1.Attributes.Add(new ItemAttribute("_Encumbrance", 0.13f));
		item1.Attributes.Add(new ItemAttribute("_Caliber", "7.62x39mm"));
		item1.Attributes.Add(new ItemAttribute("_LoadedAmmoID", "ammo762_39"));
		item1.Attributes.Add(new ItemAttribute("_LoadedAmmos", 30));
		item1.Attributes.Add(new ItemAttribute("_IsRanged", true));
		item1.BuildIndex();

		Item item2 = new Item();
		item2.Name = "44 Magnum Revolver";
		item2.Description = "One of the most famous modern revolvers of all times. High damage with nice range, but packs a punch to the wielder.";
		item2.PrefabName = "44MagnumRevolver";
		item2.SpriteName = "44magnum";
		item2.Weight = 1.6f;
		item2.ID = "44magnum";
		item2.Type = ItemType.SideArm;
		item2.GridCols = 3;
		item2.GridRows = 2;
		item2.MaxStackSize = 1;
		item2.Attributes.Add(new ItemAttribute("_Muzzle Velocity", 100f));
		item2.Attributes.Add(new ItemAttribute("Impact", 5f));
		item2.Attributes.Add(new ItemAttribute("Accuracy", 0.5f));
		item2.Attributes.Add(new ItemAttribute("Range", 15f));
		item2.Attributes.Add(new ItemAttribute("Magazine Size", 6));
		item2.Attributes.Add(new ItemAttribute("Recoil", 0.7f));
		item2.Attributes.Add(new ItemAttribute("Rate of Fire", 2f));
		item2.Attributes.Add(new ItemAttribute("Handling", 0.8f));
		item2.Attributes.Add(new ItemAttribute("_Encumbrance", 0.1f));
		item2.Attributes.Add(new ItemAttribute("_Caliber", "7.62x39mm"));
		item2.Attributes.Add(new ItemAttribute("_LoadedAmmoID", "ammo762_39"));
		item2.Attributes.Add(new ItemAttribute("_LoadedAmmos", 6));
		item2.Attributes.Add(new ItemAttribute("_IsRanged", true));
		item2.BuildIndex();

		Item item3 = new Item();
		item3.Name = "Flak Jacket";
		item3.Description = "Light weight, kevlar based body armor.";
		item3.PrefabName = "FlakJacket";
		item3.SpriteName = "flakjacket";
		item3.Weight = 3.5f;
		item3.ID = "flakjacket";
		item3.Type = ItemType.Armor;
		item3.GridCols = 3;
		item3.GridRows = 4;
		item3.MaxStackSize = 1;
		item3.Attributes.Add(new ItemAttribute("Armor", 50f)); 
		item3.Attributes.Add(new ItemAttribute("Padding", 20f));
		item3.Attributes.Add(new ItemAttribute("Coverage", 0.6f));
		item3.Attributes.Add(new ItemAttribute("_ModelSuffix", "HalfArmor"));
		item3.Attributes.Add(new ItemAttribute("_IsFull", true));
		item3.Attributes.Add(new ItemAttribute("_TextureName", "ArmorTopWoodland"));
		item3.Attributes.Add(new ItemAttribute("_TextureName2", "ArmorBottomWoodland"));
		item3.BuildIndex();

		Item item4 = new Item();
		item4.Name = "Kevlar Helmet";
		item4.Description = "Polymer-based, lined with Kevlar. Offers basic protection against shrapnel and handgun bullets.";
		item4.PrefabName = "KevlarHelmet";
		item4.SpriteName = "kevlarhelmet";
		item4.Weight = 1.0f;
		item4.ID = "kevlarhelmet";
		item4.Type = ItemType.Helmet;
		item4.GridCols = 3;
		item4.GridRows = 3;
		item4.MaxStackSize = 1;
		item4.Attributes.Add(new ItemAttribute("Armor", 50f));
		item4.Attributes.Add(new ItemAttribute("Coverage", 0.5f));
		item4.Attributes.Add(new ItemAttribute("_hideHats", true));
		item4.BuildIndex();

		Item item5 = new Item();
		item5.Name = "Pipe Grenade";
		item5.Description = "Makeshift grenade made with plumbing material and gun powder.";
		item5.PrefabName = "PipeGrenade";
		item5.SpriteName = "pipegrenade";
		item5.Weight = 0.9f;
		item5.ID = "pipegrenade";
		item5.Type = ItemType.Thrown;
		item5.GridCols = 1;
		item5.GridRows = 2;
		item5.MaxStackSize = 1;
		item5.Attributes.Add(new ItemAttribute("Damage", 100f));
		item5.Attributes.Add(new ItemAttribute("Effective Radius", 5f));
		item5.BuildIndex();

		Item item6 = new Item();
		item6.Name = "Light Body Armor";
		item6.Description = "Light weight ceramic plate body armor.";
		item6.PrefabName = "LightArmor";
		item6.SpriteName = "lightarmor";
		item6.Weight = 4.3f;
		item6.ID = "lightarmor";
		item6.Type = ItemType.Armor;
		item6.GridCols = 3;
		item6.GridRows = 3;
		item6.MaxStackSize = 1;
		item6.Attributes.Add(new ItemAttribute("Armor", 50f)); 
		item6.Attributes.Add(new ItemAttribute("Padding", 20f));
		item6.Attributes.Add(new ItemAttribute("Coverage", 0.4f));
		item6.Attributes.Add(new ItemAttribute("_ModelSuffix", "HalfArmor"));
		item6.Attributes.Add(new ItemAttribute("_IsFull", false));
		item6.Attributes.Add(new ItemAttribute("_TextureName", "ArmorTopBlue"));
		item6.BuildIndex();

		Item item7 = new Item();
		item7.Name = "PASGT Ballistic Helmet";
		item7.Description = "Layered with aramid and polyethylene. Offers basic protection against shrapnel and handgun bullets.";
		item7.PrefabName = "PASGTHelmet";
		item7.SpriteName = "pasgthelmet";
		item7.Weight = 1.2f;
		item7.ID = "pasgthelmet";
		item7.Type = ItemType.Helmet;
		item7.GridCols = 3;
		item7.GridRows = 3;
		item7.MaxStackSize = 1;
		item7.Attributes.Add(new ItemAttribute("Armor", 60f));
		item7.Attributes.Add(new ItemAttribute("Coverage", 0.5f));
		item7.Attributes.Add(new ItemAttribute("_hideHats", true));
		item7.BuildIndex();

		Item item8 = new Item();
		item8.Name = "7.62x39mm SD";
		item8.Description = "Standard ammo for AK47.";
		item8.PrefabName = "Ammo762_39";
		item8.SpriteName = "ammo762_39";
		item8.Weight = 0.05f;
		item8.ID = "ammo762_39";
		item8.Type = ItemType.Ammo;
		item8.GridCols = 2;
		item8.GridRows = 1;
		item8.MaxStackSize = 100;
		item8.Attributes.Add(new ItemAttribute("_Caliber", "7.62x39mm"));
		item8.Attributes.Add(new ItemAttribute("_numberOfProjectiles", 1));
		item8.Attributes.Add(new ItemAttribute("Damage", 10f));
		item8.Attributes.Add(new ItemAttribute("Penetration", 20f));
		item8.BuildIndex();

		Item item9 = new Item();
		item9.Name = "12 Gauge Buckshot";
		item9.Description = "Ammo for shotguns.";
		item9.PrefabName = "Ammo12Shot";
		item9.SpriteName = "ammo12shot";
		item9.Weight = 0.05f;
		item9.ID = "ammo12shot";
		item9.Type = ItemType.Ammo;
		item9.GridCols = 2;
		item9.GridRows = 1;
		item9.MaxStackSize = 20;
		item9.Attributes.Add(new ItemAttribute("_Caliber", "12g"));
		item9.Attributes.Add(new ItemAttribute("_numberOfProjectiles", 6));
		item9.Attributes.Add(new ItemAttribute("Damage", 10f));
		item9.Attributes.Add(new ItemAttribute("Penetration", 10f));
		item9.BuildIndex();

		Item item10 = new Item();
		item10.Name = "Pump Action Hunting Shotgun";
		item10.Description = "Shotgun used for hunting doves.";
		item10.PrefabName = "HuntingShotgun";
		item10.SpriteName = "huntingshotgun";
		item10.Weight = 5.6f;
		item10.ID = "huntingshotgun";
		item10.Type = ItemType.PrimaryWeapon;
		item10.GridCols = 9;
		item10.GridRows = 3;
		item10.MaxStackSize = 1;
		item10.Attributes.Add(new ItemAttribute("_Muzzle Velocity", 100f));
		item10.Attributes.Add(new ItemAttribute("Impact", 5f));
		item10.Attributes.Add(new ItemAttribute("Accuracy", 0.4f));
		item10.Attributes.Add(new ItemAttribute("Range", 12f));
		item10.Attributes.Add(new ItemAttribute("Magazine Size", 5));
		item10.Attributes.Add(new ItemAttribute("Recoil", 0.8f));
		item10.Attributes.Add(new ItemAttribute("Rate of Fire", 1f));
		item10.Attributes.Add(new ItemAttribute("Handling", 0.5f));
		item10.Attributes.Add(new ItemAttribute("_Encumbrance", 0.13f));
		item10.Attributes.Add(new ItemAttribute("_Caliber", "12g"));
		item10.Attributes.Add(new ItemAttribute("_LoadedAmmoID", "ammo12shot"));
		item10.Attributes.Add(new ItemAttribute("_LoadedAmmos", 5));
		item10.Attributes.Add(new ItemAttribute("_IsRanged", true));
		item10.BuildIndex();

		Item item11 = new Item();
		item11.Name = "Dragunov Sniper Rifle";
		item11.Description = "Semi-automatic marksman rifle.";
		item11.PrefabName = "SVD";
		item11.SpriteName = "svd";
		item11.Weight = 6f;
		item11.ID = "svd";
		item11.Type = ItemType.PrimaryWeapon;
		item11.GridCols = 10;
		item11.GridRows = 3;
		item11.MaxStackSize = 1;
		item11.Attributes.Add(new ItemAttribute("_Muzzle Velocity", 150f));
		item11.Attributes.Add(new ItemAttribute("Impact", 15f));
		item11.Attributes.Add(new ItemAttribute("Accuracy", 1f));
		item11.Attributes.Add(new ItemAttribute("Range", 40f));
		item11.Attributes.Add(new ItemAttribute("Magazine Size", 10));
		item11.Attributes.Add(new ItemAttribute("Recoil", 1.5f));
		item11.Attributes.Add(new ItemAttribute("Rate of Fire", 1.2f));
		item11.Attributes.Add(new ItemAttribute("Handling", 0.5f));
		item11.Attributes.Add(new ItemAttribute("_Encumbrance", 0.13f));
		item11.Attributes.Add(new ItemAttribute("_Caliber", "7.62x54mmr"));
		item11.Attributes.Add(new ItemAttribute("_LoadedAmmoID", "ammo762_54r"));
		item11.Attributes.Add(new ItemAttribute("_LoadedAmmos", 10));
		item11.Attributes.Add(new ItemAttribute("_IsRanged", true));
		item11.BuildIndex();

		Item item12 = new Item();
		item12.Name = "7.62x54mmr";
		item12.Description = "Rimmed round for SVD";
		item12.PrefabName = "Ammo762_54r";
		item12.SpriteName = "ammo762_54r";
		item12.Weight = 0.05f;
		item12.ID = "ammo762_54r";
		item12.Type = ItemType.Ammo;
		item12.GridCols = 2;
		item12.GridRows = 1;
		item12.MaxStackSize = 40;
		item12.Attributes.Add(new ItemAttribute("_Caliber", "7.62x54mmr"));
		item12.Attributes.Add(new ItemAttribute("_numberOfProjectiles", 1));
		item12.Attributes.Add(new ItemAttribute("Damage", 50f));
		item12.Attributes.Add(new ItemAttribute("Penetration", 40f));
		item12.BuildIndex();

		Item item13 = new Item();
		item13.Name = "Machete";
		item13.Description = "A broad blade for clearing paths and melee combat. Best for unarmored targets.";
		item13.PrefabName = "Machete";
		item13.SpriteName = "machete";
		item13.Weight = 1f;
		item13.ID = "machete";
		item13.Type = ItemType.PrimaryWeapon;
		item13.GridCols = 6;
		item13.GridRows = 2;
		item13.MaxStackSize = 1;
		item13.Attributes.Add(new ItemAttribute("Sharp Damage", 30f));
		item13.Attributes.Add(new ItemAttribute("Blunt Damage", 30f));
		item13.Attributes.Add(new ItemAttribute("_IsRanged", false));
		item13.BuildIndex();

		switch(itemID)
		{
		case "ak47":
			return item1;
			break;
		case "44magnum":
			return item2;
			break;
		case "flakjacket":
			return item3;
			break;
		case "kevlarhelmet":
			return item4;
			break;
		case "pipegrenade":
			return item5;
			break;
		case "lightarmor":
			return item6;
			break;
		case "pasgthelmet":
			return item7;
			break;
		case "ammo762_39":
			return item8;
			break;
		case "ammo12shot":
			return item9;
			break;
		case "huntingshotgun":
			return item10;
			break;
		case "svd":
			return item11;
			break;
		case "ammo762_54r":
			return item12;
			break;
		case "machete":
			return item13;
			break;
		}

		return null;
	}
}
