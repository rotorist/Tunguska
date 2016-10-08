﻿using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(AnimationEventHandler))]
[RequireComponent(typeof(LeftHandIKControl))]
[RequireComponent(typeof(HeadIKControl))]
[RequireComponent(typeof(AimIK))]




public class CharacterReference : MonoBehaviour
{

	public GameObject HelmetMount;
	public GameObject RightHandWeaponMount;
	public GameObject TorsoWeaponMount;
	public GameObject SlingWeaponMount;
	public GameObject HolsterWeaponMount;
	public GameObject CurrentWeapon;
	public GameObject Eyes;
	public Character ParentCharacter;
	public FlashLight Flashlight;
	public Weapon FixedMeleeLeft;
	public Weapon FixedMeleeRight;
	public BoxCollider DeathCollider;
	public CapsuleCollider LiveCollider;
}
