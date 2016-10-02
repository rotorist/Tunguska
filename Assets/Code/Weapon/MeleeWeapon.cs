using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon
{
	public bool IsSwung;


	private BoxCollider _collider;



	void OnCollisionEnter(Collision collision) 
	{
		if(!IsSwung)
		{
			return;
		}
		//Debug.Log("melee weapon hit: attacker " + Attacker.name);

		Character hitCharacter = collision.collider.GetComponent<Character>();
		Vector3 pos = collision.contacts[0].point;
		Vector3 normal = collision.contacts[0].normal;

		//Debug.Log("hit collider is " + collision.collider.name);
		if(hitCharacter == Attacker)
		{
			return;
		}

		if(hitCharacter == null)
		{
			//show sparks or dust
			GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BImpact Metal", 0, FXType.BulletImpact);
			impact.transform.position = transform.position;
			impact.transform.rotation = Quaternion.LookRotation(normal);
		}

		if(hitCharacter != null && hitCharacter.Faction == Attacker.Faction)
		{
			//return;
		}

		if(hitCharacter != null)
		{
			//Debug.Log("hit somebody! " + hitCharacter.name);
			bool isBlocked = hitCharacter.SendMeleeDamage(1, 1, normal, Attacker);

			if(isBlocked)
			{
				GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BImpact Metal", 0, FXType.BulletImpact);
				impact.transform.position = transform.position;
				impact.transform.rotation = Quaternion.LookRotation(normal);

				if(Attacker == GameManager.Inst.PlayerControl.SelectedPC)
				{
					GameManager.Inst.CameraShaker.TriggerScreenShake(0.05f, 0.04f);

				}

			}
			else
			{
				GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BloodSmoke", 0, FXType.BulletImpact);
				impact.transform.position = pos;
				impact.transform.rotation = Quaternion.LookRotation(this.transform.forward);

				if(Attacker == GameManager.Inst.PlayerControl.SelectedPC)
				{
					if(GameManager.Inst.PlayerControl.SelectedPC.IsComboAttack())
					{
						GameManager.Inst.CameraShaker.TriggerTempSlow(0.08f);
						GameManager.Inst.CameraShaker.TriggerZoomShake(0.1f, 0.55f);
					}
					else
					{
						GameManager.Inst.CameraShaker.TriggerZoomShake(0.15f, 0.55f);
					}
				
				}
			}
		}
	}

	public override void Rebuild (WeaponCallBack callBack, Item weaponItem)
	{
		this.WeaponItem = weaponItem;
		_collider = GetComponent<BoxCollider>();
		_collider.enabled = false;

	}

	public void SwingStart()
	{
		_collider.enabled = true;
		IsSwung = true;
	}

	public void SwingStop()
	{
		_collider.enabled = false;
		IsSwung = false;
	}
}
