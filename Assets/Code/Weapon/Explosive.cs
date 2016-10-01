using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosive : MonoBehaviour 
{
	public Character Attacker;
	public float Range;
	public float Damage;
	public AnimationCurve DamageModifier;
	public bool IsEnabled;

	public void TriggerExplosion()
	{

		GameObject explosion = GameObject.Instantiate(Resources.Load("WFX_Explosion StarSmoke")) as GameObject;
		explosion.transform.position = transform.position;

		RaycastHit [] hits = Physics.SphereCastAll(transform.position, Range, Vector3.up);
		if(hits.Length > 0)
		{
			foreach(RaycastHit hit in hits)
			{
				Character c = hit.collider.GetComponent<Character>();
				if(c != null && c.MyStatus.Health > 0)
				{
					//now do a raycast check
					RaycastHit checkHit;
					float colliderHeight = c.GetComponent<CapsuleCollider>().height;
					Vector3 rayTarget = c.transform.position + Vector3.up * colliderHeight * 0.5f;
					Ray ray = new Ray(transform.position, rayTarget - transform.position);
					if(Physics.Raycast(ray, out checkHit))
					{
						if(checkHit.collider.gameObject == c.gameObject)
						{
							//now apply damage
							float dist = Vector3.Distance(c.transform.position, transform.position);
							dist = Mathf.Clamp(dist / Range, 0, 1);
							float modifer = DamageModifier.Evaluate(dist);
							c.SendDamage(Damage * modifer, 100, checkHit.normal, Attacker, null);
						}
					}


				}
			}
		}

		//now send a disturbance to all human characters within sound range
		List<HumanCharacter> humans = GameManager.Inst.NPCManager.HumansInScene;
		foreach(HumanCharacter human in humans)
		{
			if(Vector3.Distance(transform.position, human.transform.position) <= 30)
			{
				human.MyAI.Sensor.OnReceiveDisturbance(0.85f, this, transform.position, Attacker);
			}
		}

		GameObject.Destroy(gameObject);
	}
}
