﻿using UnityEngine;
using System.Collections;

/*
 * Handles casting between this object and it's position in the previous frame. It then
 * sends a callback for each object hit.
 */
public class AttackCast : MonoBehaviour
{
	// GameObjects that show the two cast positions - start and end
	GameObject debugCurrentPosition;
	GameObject debugLastPosition;

	// Track last position of this game object
	Vector3 lastFramePosition;

	// The radius for this attack sphere
	public float radius;

	// The bits this attack should hit
	public LayerMask hitLayer;

	// Flags for showing debug information
	public bool debugShowCasts;
	public bool debugShowHits;

	void OnEnable ()
	{
		// TODO: Could support cast to source position, like I did in Ben10
		lastFramePosition = transform.position;
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 direction = (transform.position - lastFramePosition).normalized;
		float distance = Vector3.Distance (lastFramePosition, transform.position);
		RaycastHit[] hits;
		hits = Physics.SphereCastAll (lastFramePosition, radius, direction, distance, hitLayer);
		ReportHits (hits);
		RenderDebugCasts ();

		// Remember this position as the last
		lastFramePosition = transform.position;
	}

	void RenderDebugCasts ()
	{
		//TODO: Could wrap a precompile flag to never show debugs in release
		if (!debugShowCasts) {
			return;
		}

		// Spawn debug objects if they haven't been spawned
		if (debugCurrentPosition == null) {
			SpawnDebugObjects ();
		}

		// Update debug line. Wish this was a cylinder.
		Debug.DrawLine (transform.position, lastFramePosition);

		// Update gizmo, if we have one
		Gizmo gizmo = (Gizmo)GetComponent<Gizmo> ();
		if (gizmo != null) {
			gizmo.gizmoSize = radius;
			gizmo.enabled = debugShowCasts;
		}

		// Make sure spheres represent current cast radius as a diameter
		debugLastPosition.transform.localScale = (Vector3.one * (radius * 2));
		debugCurrentPosition.transform.localScale = (Vector3.one * (radius * 2));

		// Hide spheres if debugs are off
		debugLastPosition.SetActive (debugShowCasts);
		debugCurrentPosition.SetActive (debugShowCasts);

		// Set the new positions of the spheres
		debugLastPosition.transform.position = lastFramePosition;
		debugCurrentPosition.transform.position = transform.position;
	}

	/*
	 * Spawn debug objects used to see where the casts are located
	 */
	void SpawnDebugObjects ()
	{
		Material debugMaterial = new Material (Shader.Find ("Transparent/Diffuse"));
		debugCurrentPosition = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		debugCurrentPosition.transform.position = transform.position;
		debugCurrentPosition.renderer.material = debugMaterial;
		debugCurrentPosition.renderer.material.color = new Color (0, 1.0f, 0, .3f);
		debugCurrentPosition.collider.enabled = false;
		debugCurrentPosition.transform.parent = transform;
		debugCurrentPosition.SetActive (debugShowCasts);

		debugLastPosition = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		debugLastPosition.transform.position = lastFramePosition;
		debugLastPosition.renderer.material = debugMaterial;
		debugLastPosition.renderer.material.color = new Color (1.0f, 1.0f, 0, .3f);
		debugLastPosition.collider.enabled = false;
		debugLastPosition.transform.parent = transform;
		debugLastPosition.SetActive (debugShowCasts);
	}

	void ReportHits (RaycastHit[] hits)
	{
		foreach (RaycastHit hit in hits) {
			OnHit (hit);
		}
	}

	void ReportHits (Collider[] colliders)
	{
		foreach (Collider collider in colliders) {
			OnHit (collider);
		}
	}

	void OnHit (RaycastHit hit)
	{
		if (debugShowHits) {
			Debug.DrawRay (hit.point, hit.normal, Color.red, 0.5f);
		}
		OnHit (hit.collider);
	}

	void OnHit (Collider hitCollider)
	{
		Debug.Log ("Hit! Object: " + hitCollider.name);
	}
}
