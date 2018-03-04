﻿using UnityEngine;

public class Destruction : MonoBehaviour
{
	[Space (7)]
	[Header ("State")]
	[Space (2)]
	[Tooltip ("Whether the object is unbroken")]
	public bool together = true;
	[Tooltip ("Whether the object starts broken")]
	public bool startBroken = false;

	[Space (7)]
	[Header ("General Settings")]
	[Space (2)]
	[Tooltip ("The higher this is, the more will break off in an impact")]
	public float breakageMultiplier = 0.3f;
	[Tooltip ("How resistant the object initially is to breakage.")]
	public float strength = 0.3f;

	[Space (7)]
	[Header ("Breaking on Collision")]
	[Space (2)]
	[Tooltip ("Whether the object breaks when it collides with something")]
	public bool breakOnCollision = true;
	[Tooltip ("The minimum relative velocity to break the object")]
	public float velocityToBreak = 1;
	// Velocity required to break object

	[Space (7)]
	[Header ("Breaking when nothing underneath")]
	[Space (2)]
	[Tooltip ("Whether the object breaks when there's nothing underneath supporting it")]
	public bool breakOnNoSupports = false;
	[Tooltip ("The length of the raycast used to check for supports underneath")]
	public float raycastLength = 1f;

	[Space (7)]
	[Header ("Sound On Break")]
	[Space (2)]
	[Tooltip ("Whether the object makes a sound when it breaks")]
	public bool soundOnBreak = false;
	[Tooltip ("An array of sounds for the object to make when it breaks (A random one will be selected)")]
	public AudioClip[] clips;

	[Space (7)]
	[Header ("Particles On Break")]
	[Space (2)]
	[Tooltip ("Whether the object makes particles when it breaks")]
	public bool particlesOnBreak = false;

	[SerializeField] string collisionTag = "Player";

	//Private vars
	AudioSource src;
	ParticleSystem psys;

	Vector3 spherePoint = Vector3.zero;
	float sphereRadius = 0f;

	Rigidbody _rigidbody;
	Collider coll;
	Rigidbody[] rigids;
	MeshRenderer[] renderers;

	void Start ()
	{
		//Get the rigidbodies
		renderers = gameObject.GetComponentsInChildren<MeshRenderer> ();
		rigids = gameObject.GetComponentsInChildren<Rigidbody> ();
		SetRenderersEnabled (false);
		coll = GetComponent<Collider> ();
		_rigidbody = GetComponent<Rigidbody> ();

		together = !startBroken;
		SetPiecesKinematic (together);

		if (soundOnBreak) {
			SetupSound ();
		}
		if (particlesOnBreak) {
			SetupParticles ();
		}
	}

	void SetRenderersEnabled (bool value)
	{
		for (int i = 1; i < renderers.Length; i++) {
			renderers [i].enabled = value;
		}
	}

	void SetupSound ()
	{
		//Get the audio source or create one
		src = GetComponent<AudioSource> ();
		if (src == null) {
			src = gameObject.AddComponent<AudioSource> ();
		}

		//Add a random audio clip to it
		src.clip = clips [Random.Range (0, clips.Length - 1)];
	}

	void SetupParticles ()
	{
		// Get the particle system or create one
		psys = GetComponent<ParticleSystem> ();
		if (psys == null) {
			psys = gameObject.AddComponent<ParticleSystem> ();
		}

		//This doesn't seem to do anything b/c the gameobject is not active
		psys.Stop ();
	}

	void Update ()
	{
		/* Broken object should follow unbroken one to prevent them from
         * being in the wrong place when they switch */
		//brokenObj.transform.position = togetherObj.transform.position;

		//Make sure the right object is active
		//togetherObj.SetActive(together);
		//brokenObj.SetActive(!together);
		if (!together) {
			Break ();
		}

		if (breakOnNoSupports) {
			CheckForSupports ();
		}
	}

	void CheckForSupports ()
	{
		//Check downwards for supports
		if (!Physics.Raycast (transform.position, Vector3.down, raycastLength)) {
			Break ();
		}
	}

	void DoCollisionLogic (Collision collision)
	{
		//Otherwise, if velocity is strong enough to break some bits but not others...
		//Get the impact point
		spherePoint = collision.contacts [0].point;
		//Get the radius within which we'll break pieces
		sphereRadius = collision.relativeVelocity.magnitude / velocityToBreak * breakageMultiplier;
		//Turn on Colliders so that Physics.OverlapSphere will work
		//Turn on Renderer for pieces
		foreach (Rigidbody rigid in rigids) {
			rigid.GetComponent<Collider> ().enabled = true;
		}
		Collider[] pieces = Physics.OverlapSphere (spherePoint, sphereRadius, 1 << 8);
		//Make the broken-off pieces non-kinematic
		foreach (Collider piece in pieces) {
			Rigidbody rigid = piece.GetComponent<Rigidbody> ();
			rigid.isKinematic = false;
		}
		//And turn off the Colliders of the not broken-off pieces 
		foreach (Rigidbody rigid in rigids) {
			rigid.GetComponent<Collider> ().enabled = !rigid.isKinematic;
		}
	}

	void OnCollisionEnter (Collision collision)
	{
		if (collision.transform.CompareTag (collisionTag) && breakOnCollision) {
			//Only break if relative velocity is high enough
			if (collision.relativeVelocity.magnitude > velocityToBreak) {
				Break ();
				ScoreBoard.IncreaseScore (collision.gameObject.tag, 1);
			} else if (collision.relativeVelocity.magnitude / velocityToBreak > strength) {
				DoCollisionLogic (collision);
			}
		}
	}

	public void Break ()
	{
		SetPiecesKinematic (false);
		SetRenderersEnabled (true);
		renderers [0].gameObject.SetActive (false);
		//Play the sound
		if (soundOnBreak) {
			src.Play ();
		}
		//Show particles
		if (particlesOnBreak) {
			psys.Play ();
		}
	}

	void SetPiecesKinematic (bool valueIn)
	{
		foreach (Rigidbody rigid in rigids) {
			rigid.isKinematic = valueIn;
			rigid.GetComponent<Collider> ().enabled = !valueIn;
		}
		coll.enabled = valueIn;
		_rigidbody.isKinematic = !valueIn;
	}

	public void BreakWithExplosiveForce (float force, float radius = 3)
	{
		Break ();

		//Add the explosive force to each rigidbody
		foreach (Rigidbody rigid in rigids) {
			rigid.AddExplosionForce (force, transform.position, radius);
		}
	}

}
