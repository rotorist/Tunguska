using UnityEngine;
using System.Collections;

public class CharacterStatus
{
	public float WalkSpeed;
	public float StrafeSpeed;
	public float RunSpeed;
	public float SprintSpeed;

	public float WalkSpeedModifier; //0.6 to 1.5
	public float RunSpeedModifier; //0.9 to 1.2
	public float SprintSpeedModifier; //0.9 to 1.1
	public float StrafeSpeedModifier; //0.8 to 1.2

	public float MaxArmFatigue;
	public float ArmFatigue;

	public float MaxHealth;
	public float Health;

	public float MaxCarryWeight;
	public float CarryWeight;

	public float EyeSight;

	public int Intelligence; //0, 1, 2


	public void Initialize()
	{
		WalkSpeed = 1.2f;
		StrafeSpeed = 1.75f;
		RunSpeed = 4f;
		SprintSpeed = 5.2f;

		ArmFatigue = 0;
		MaxArmFatigue = 5;

		ResetSpeedModifier();


		MaxHealth = 160;
		Health = 160;

		EyeSight = 1.5f;

		Intelligence = 2;
	}

	public void ResetSpeedModifier()
	{
		WalkSpeedModifier = 0.8f;
		RunSpeedModifier = 1.1f;
		SprintSpeedModifier = 1.1f;
		StrafeSpeedModifier = 1.2f;
	}

}
