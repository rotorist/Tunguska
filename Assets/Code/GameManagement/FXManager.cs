using UnityEngine;
using System.Collections;

//this manager's purpose is to keep track of 
//created FX and remove them
public class FXManager : MonoBehaviour 
{

	private GameObject [] _bulletHoles;
	private float [] _bulletHoleTimers;

	public void Initialize(int maxBulletHoles)
	{
		_bulletHoles = new GameObject[maxBulletHoles];
		_bulletHoleTimers = new float[maxBulletHoles];
		TimerEventHandler.OnOneSecondTimer += ManagerPerSecondUpdate;
	}

	public void ManagerPerSecondUpdate()
	{
		//remove fx with ttl = 0
		for(int i=0; i<_bulletHoles.Length; i++)
		{
			if(_bulletHoles[i] != null)
			{
				_bulletHoleTimers[i] -= 1;
				if(_bulletHoleTimers[i] <= 0)
				{
					_bulletHoles[i].GetComponent<WFX_BulletHoleDecal>().StartFadeOut();
					_bulletHoles[i] = null;
				}
			}
		}
	}

	public GameObject LoadFX(string fxName, float ttl, FXType type)
	{
		GameObject fx = GameObject.Instantiate(Resources.Load(fxName) as GameObject);

		int index = 0;

		switch(type)
		{
		case FXType.BulletHole:
			index = FindEmptySlot(_bulletHoles, _bulletHoleTimers);
			if(_bulletHoles[index] != null)
			{
				_bulletHoles[index].GetComponent<WFX_BulletHoleDecal>().StartFadeOut();
			}
			_bulletHoles[index] = fx;
			_bulletHoleTimers[index] = ttl;
			break;
		case FXType.BulletImpact:
			//don't need to do anything; autodestruct
			break;
		}

		return fx;
	}
	


	private int FindEmptySlot(GameObject [] fxList, float [] timerList)
	{
		//find an empty slot
		for(int i=0; i<fxList.Length; i++)
		{
			if(fxList[i] == null)
			{
				return i;
			}
		}
		
		//if no null slots, find slot with oldest FX
		float timer = timerList[0];
		int index = 0;
		for(int i=0; i<fxList.Length; i++)
		{
			if(timerList[i] < timer)
			{
				timer = timerList[i];
				index = i;
			}
		}

		return index;
	}


}
