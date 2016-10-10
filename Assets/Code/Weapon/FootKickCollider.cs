using UnityEngine;
using System.Collections;

public class FootKickCollider : MonoBehaviour 
{
	public Character Attacker;

	void OnCollisionEnter(Collision collision) 
	{


		Character hitCharacter = collision.collider.GetComponent<Character>();
		Vector3 pos = collision.contacts[0].point;
		Vector3 normal = collision.contacts[0].normal;

		Debug.Log("KICKING hit collider is " + collision.collider.name);
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
			Debug.Log("Kicked somebody! " + hitCharacter.name);
			hitCharacter.SendMeleeDamage(0, 0, normal, Attacker, 0);


		}
	}
}
