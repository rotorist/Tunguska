using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour 
{
	public Character Attacker;
	public Vector3 InHandPosition;
	public Vector3 InHandAngles;
	public Vector3 InHolsterPosition;
	public Vector3 InHolsterAngles;
	public Transform AimTransform;
	public Transform ForeGrip;
	public bool IsRanged;
	public bool IsScoped;
	public Item WeaponItem;

	public delegate void WeaponCallBack();

	public virtual void Rebuild(WeaponCallBack callBack, Item weaponItem)
	{

	}

	public virtual void Refresh()
	{

	}

	public virtual float GetTotalLessMoveSpeed()
	{
		return 0;
	}
}
