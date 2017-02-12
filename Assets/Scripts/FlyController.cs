using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyController : MonoBehaviour {

	[SerializeField] private float speed = 0.5f;
	private bool lookEnabled;

	private void Awake() {
		lookEnabled = false;
	}

	private void Update () {
		CheckLookEnabled ();

		if (lookEnabled) {
			Camera.main.transform.Rotate (new Vector3 (-Input.GetAxis ("Mouse Y"), Input.GetAxis ("Mouse X"), 0) * 2.0f);
			Vector3 currentRot = Camera.main.transform.rotation.eulerAngles;
			currentRot.z = 0;
			Camera.main.transform.rotation = Quaternion.Euler (currentRot);
		}

		if (Input.GetKey(KeyCode.W)) {
			Camera.main.transform.position += Camera.main.transform.forward * speed;
		}
		if (Input.GetKey(KeyCode.S)) {
			Camera.main.transform.position -= Camera.main.transform.forward * speed;
		}
		if (Input.GetKey(KeyCode.A)) {
			Camera.main.transform.position -= Camera.main.transform.right * speed;
		}
		if (Input.GetKey(KeyCode.D)) {
			Camera.main.transform.position += Camera.main.transform.right * speed;
		}
	}

	private void CheckLookEnabled() {
		if (Input.GetMouseButtonDown (1))
			lookEnabled = true;
		if (Input.GetMouseButtonUp (1))
			lookEnabled = false;
	}
}
