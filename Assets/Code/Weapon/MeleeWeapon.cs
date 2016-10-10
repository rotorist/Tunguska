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
			bool isBlocked = hitCharacter.SendMeleeDamage(30, 10, normal, Attacker, 0.5f);

			if(isBlocked)
			{
				GameObject impact = GameManager.Inst.FXManager.LoadFX("WFX_BImpact Metal", 0, FXType.BulletImpact);
				impact.transform.position = transform.position;
				impact.transform.rotation = Quaternion.LookRotation(normal);

				if(Attacker == GameManager.Inst.PlayerControl.SelectedPC)
				{
					GameManager.Inst.CameraShaker.TriggerScreenShake(0.05f, 0.08f);

				}

			}
			else
			{
				GameObject impact = null;

				if(pos.y > hitCharacter.transform.position.y + collision.collider.bounds.size.y * 0.75f)
				{
					impact = GameManager.Inst.FXManager.LoadFX("MeleeBlood1", 1, FXType.BloodSpatter);
					impact.transform.position = pos - new Vector3(0, 0.5f, 0);
				}
				else
				{
					impact = GameManager.Inst.FXManager.LoadFX("MeleeBlood2", 1, FXType.BloodSpatter);
					impact.transform.position = pos;
				}

				impact.transform.parent = hitCharacter.transform;
				impact.transform.rotation = Quaternion.LookRotation(normal);

				if(Attacker == GameManager.Inst.PlayerControl.SelectedPC)
				{
					if(GameManager.Inst.PlayerControl.SelectedPC.IsComboAttack())
					{
						GameManager.Inst.CameraShaker.TriggerTempSlow(0.08f, 300f);

					}
					else
					{
						GameManager.Inst.CameraShaker.TriggerTempSlow(0.03f, 600f);
					}

					GameManager.Inst.CameraShaker.TriggerScreenShake(0.04f, 0.09f);
					//GameManager.Inst.CameraShaker.TriggerDirectionalShake(0.1f, 0.08f, new Vector3(0.1f, 0.1f, 0));
				
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
