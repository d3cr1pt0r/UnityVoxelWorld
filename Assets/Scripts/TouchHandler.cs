using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TouchHandler : MonoBehaviour {

	[SerializeField] private WorldGenerator worldGenerator = null;

	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if (Physics.Raycast (ray, out hit, 100.0f)) {
				Vector3 p = hit.point - hit.normal / 2.0f;
				Vector3 blockPos = worldGenerator.WorldToBlockPosition (p);

				Chunk chunk = worldGenerator.GetChunk (p);
				chunk.RemoveBlock (blockPos);
			}
		}

		if (Input.GetMouseButton (2)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if (Physics.Raycast (ray, out hit, 100.0f)) {
				Vector3 p = hit.point - hit.normal / 2.0f;
				Vector3 chunkPos = worldGenerator.WorldToChunkPosition (p);
				Vector3 blockPos = worldGenerator.WorldToBlockPosition (p);

				Chunk chunk = worldGenerator.GetChunk (p);

				Debug.Log (hit.point);
				Debug.Log (blockPos);
//				Debug.Log(chunkPos);
				Debug.DrawLine (ray.origin, hit.point);
			}
		}
	}

}
