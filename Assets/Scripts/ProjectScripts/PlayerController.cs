﻿using UnityEngine;
using System.Collections;

public class PlayerController : IController
{
	public int PlayerIndex { get; private set; }

	public InputDevice playerDevice { get; private set; }
	
	public Fighter fighter;
	bool isPlayerBound;
	int curTarget;

	void Awake ()
	{
		fighter = gameObject.GetComponent<Fighter> ();
		isPlayerBound = false;
		ResetTargetIndex ();
		HighlightArrow (false);

		BindPlayer (0, InputDevices.GetAllInputDevices () [(int)InputDevices.ControllerTypes.Keyboard]);
	}
	
	void Update ()
	{
		HighlightArrow (fighter.target != null);
	}

	public override void Think ()
	{
		if (!isPlayerBound) {
			return;
		}

		TryPause ();
		if (!GameManager.Instance.IsPaused) {
			TryMove ();
			TryDodge ();
			TrySwitchTarget ();
			TryAttack ();
			TryBlock ();
			TryBandage ();
			TryDebugs ();
		}
	}

	void TryPause ()
	{
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (GameManager.Instance.IsPaused) {
				GameManager.Instance.RequestUnpause ();
			} else {
				GameManager.Instance.RequestPause ();
			}
		}
	}

	/*
	 * Use to forget the current target. Usually the Fighter should keep track of
	 * what the target is and the controller should keep track of what the player or
	 * AI wants the target to be. This method is implemented just to allow Sprint to
	 * reset the target to the start of the array.
	 */
	public void ResetTargetIndex ()
	{
		curTarget = 0;
	}

	/*
	 * Apply movement in the Player's desired directions according to the various speed
	 * and movement variables.
	 */
	void TryMove ()
	{
		// Get input values
		float horizontal = 0.0f, vertical = 0.0f;
		horizontal = RBInput.GetAxisRawForPlayer (InputStrings.HORIZONTAL, PlayerIndex, playerDevice);
		vertical = RBInput.GetAxisRawForPlayer (InputStrings.VERTICAL, PlayerIndex, playerDevice);
		
		Vector3 direction = new Vector3 (horizontal, 0.0f, vertical);
		
		if (direction != Vector3.zero) {
			if (RBInput.GetButtonForPlayer (InputStrings.SPRINT, PlayerIndex, playerDevice)) {
				fighter.Sprint (direction);
			} else {
				fighter.Run (direction);
			}
		}
	}
	
	/*
	 * If no target is selected, pick the first using the ENEMY tag. If one is selected
	 * already, choose the next until the end is reached at which point, select none. This
	 * will need to be refactored to be smarter later, i.e. choose the closest first, then
	 * switch further away. Also this should be a hold down button to keep lock, release to
	 * unlock behavior as well but right now it just tabs through.
	 */
	void TrySwitchTarget ()
	{
		if (RBInput.GetButtonDownForPlayer (InputStrings.TARGET, PlayerIndex, playerDevice)) {
			GameObject [] enemies = GameObject.FindGameObjectsWithTag (Tags.ENEMY);

			// Toggle to nothing when user has tabbed through the targets
			if (curTarget >= enemies.Length) {
				ResetTargetIndex ();
				fighter.LoseTarget ();
				return;
			}

			// Select the next target
			HighlightArrow (true);
			fighter.LockOnTarget (enemies [curTarget].transform);
			curTarget++;
		}
	}

	void TryAttack ()
	{
		bool isAttack = RBInput.GetButtonDownForPlayer (InputStrings.FIRE, PlayerIndex, playerDevice);
		if (isAttack) {
			fighter.SwingWeapon (Fighter.AttackType.Weak);
		} else {
			bool isHeavyAttack = Input.GetKeyDown (KeyCode.Mouse1);
			if (isHeavyAttack) {
				fighter.SwingWeapon (Fighter.AttackType.Strong);
			}
		}
		bool test = Input.GetKeyDown (KeyCode.Backspace);
		if (test) {
			int i = 0;
			while (i < 1000) {
				Debug.Log ("Yes or no:" + RBRandom.PercentageChance (24.5f).ToString ());
				i++;
			}
		}
	}

	void TryDodge ()
	{
		// Get input values
		float horizontal = 0.0f, vertical = 0.0f;
		horizontal = RBInput.GetAxisRawForPlayer (InputStrings.HORIZONTAL, PlayerIndex, playerDevice);
		vertical = RBInput.GetAxisRawForPlayer (InputStrings.VERTICAL, PlayerIndex, playerDevice);
		
		// If player isn't standing still and hits dodge button, let's dodge!
		if (RBInput.GetButtonDownForPlayer (InputStrings.DODGE, PlayerIndex, playerDevice) &&
			(horizontal != 0 || vertical != 0)) {
			fighter.Dodge (new Vector3 (horizontal, 0.0f, vertical));
		}
	}
	
	/*
	 * Set fighter to blocking or unblocking depending on button up or down.
	 */
	void TryBlock ()
	{
		if (RBInput.GetAxisForPlayer (InputStrings.BLOCK, PlayerIndex, playerDevice) == 1 ||
			RBInput.GetButtonForPlayer (InputStrings.BLOCK, PlayerIndex, playerDevice)) {
			fighter.Block ();
		} else if (RBInput.GetAxisForPlayer (InputStrings.BLOCK, PlayerIndex, playerDevice) == 0 ||
			RBInput.GetButtonUpForPlayer (InputStrings.BLOCK, PlayerIndex, playerDevice)) {
			if (fighter.IsBlocking) {
				fighter.UnBlock ();
			}
		}
	}
	
	/*
	 * Set the fighter to bandaging state and start bandaging.
	 */
	void TryBandage ()
	{
		if (RBInput.GetButtonForPlayer (InputStrings.BANDAGE, PlayerIndex, playerDevice)) {
			fighter.Bandage ();
		} else {
			fighter.InterruptBandage ();
		}
	}

	/*
	 * Reads input and handles action for all debug functions
	 */
	void TryDebugs ()
	{
	}

	public void BindPlayer (int index, InputDevice device)
	{
		isPlayerBound = true;

		PlayerIndex = index;
		playerDevice = device;
		fighter.SetHuman(true);
	}

	/*
	 * Debug method that highlights the Arrow or not. Pass in True to highlight,
	 * False to not highlight it.
	 */
	void HighlightArrow (bool trueFalse)
	{
		Component[] renderers = GameObject.Find ("Arrow").GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			if (trueFalse) {
				renderer.material.color = Color.red;
			} else {
				renderer.material.color = Color.blue;
			}
			//renderer.enabled = trueFalse;
		}
	}
}