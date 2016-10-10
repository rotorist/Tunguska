using UnityEngine;
using System.Collections;

public class FootKickCollider : MonoBehaviour 
{
	public Character Attacker;
	public bool IsActive;


	public void SetActive(bool isActive)
	{
		transform.GetComponent<BoxCollider>().enabled = isActive;
		IsActive = isActive;
	}

	void OnCollisionEnter(Collision collision) 
	{
		if(!IsActive)
		{
			return;
		}

		Character hitCharacter = collision.collider.GetComponent<Character>();
		Vector3 pos = collision.contacts[0].point;
		Vector3 normal = collision.contacts[0].normal;

		//Debug.Log("KICKING hit collider is " + collision.collider.name);
		if(hitCharacter == Attacker)
		{
			return;
		}
			

		if(hitCharacter != null && hitCharacter.Faction == Attacker.Faction)
		{
			//return;
		}

		if(hitCharacter != null)
		{
			//Debug.Log("Kicked somebody! " + hitCharacter.name);
			hitCharacter.SendMeleeDamage(0, 0, normal, Attacker, 0);


		}
	}
}
