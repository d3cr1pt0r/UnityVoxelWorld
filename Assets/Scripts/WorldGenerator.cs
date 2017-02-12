using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

	[SerializeField] private Chunk chunk = null;
	[SerializeField] private Transform playerTransform = null;
	[SerializeField] private int chunkViewDistance = 5;
	[SerializeField] private int chunkSize = 16;
	[SerializeField] private float blockSize = 1.0f;
//	[SerializeField] private int seed = 0;

	private Dictionary<Vector3, Chunk> world;
	private Chunk tmpChunk;

	private void Awake() {
		world = new Dictionary<Vector3, Chunk> ();
		tmpChunk = null;
	}

	private void Update() {
		CheckForNewChunks ();
	}

	private void CheckForNewChunks() {
		Vector3 chunkPos = WorldToChunkPosition (playerTransform.position);

		for (int z = -chunkViewDistance; z <= chunkViewDistance; z++) {
			for (int y = -chunkViewDistance; y <= chunkViewDistance; y++) {
				for (int x = -chunkViewDistance; x <= chunkViewDistance; x++) {
					int viewerChunkX = (int)chunkPos.x + x;
					int viewerChunkY = (int)chunkPos.y + y;
					int viewerChunkZ = (int)chunkPos.z + z;

					// temporary disable chunk generation on y-axis
					viewerChunkY = 0;

					Vector3 viewerChunkPos = new Vector3 (viewerChunkX, viewerChunkY, viewerChunkZ);
					AddChunk (viewerChunkPos);
				}
			}
		}
	}

	private Chunk CreateChunk(Vector3 position, bool empty=false) {
		Vector3 worldPosition = position * blockSize * chunkSize;

		Chunk chunkInstance = Instantiate (chunk, worldPosition, Quaternion.identity);
		chunkInstance.GenerateChunk (worldPosition, chunkSize, blockSize, empty);
		chunkInstance.transform.SetParent (transform);

		return chunkInstance;
	}

	// gets world position transformed into 1 unit sized 
	public Vector3 WorldToChunkPosition(Vector3 worldPosition) {
		float chunkSizeTimesBlockSize = chunkSize * blockSize;

		int x = Mathf.RoundToInt ((worldPosition.x + blockSize * 0.5f) / chunkSizeTimesBlockSize);
		int y = Mathf.RoundToInt ((worldPosition.y + blockSize * 0.5f) / chunkSizeTimesBlockSize);
		int z = Mathf.RoundToInt ((worldPosition.z + blockSize * 0.5f) / chunkSizeTimesBlockSize);

		return new Vector3 (x, y, z);
	}

	// transforms world position to a position used in local chunk space (RANGE: x=0-chunkSize y=0-chunkSize, z=0-chunkSize)
	// this position is used to get the block from the chunk using the chunks local coordinate space
	public Vector3 WorldToBlockPosition(Vector3 worldPosition) {
		Vector3 chunkPosition = WorldToChunkPosition (worldPosition);

		float preCalc1 = blockSize * 0.5f + (chunkSize * 0.5f) * blockSize;
		float chunkSizeTimesBlockSize = chunkSize * blockSize;

		int x = (int) (((worldPosition.x + preCalc1) - chunkPosition.x * chunkSizeTimesBlockSize) / blockSize);
		int y = (int) (((worldPosition.y + preCalc1) - chunkPosition.y * chunkSizeTimesBlockSize) / blockSize);
		int z = (int) (((worldPosition.z + preCalc1) - chunkPosition.z * chunkSizeTimesBlockSize) / blockSize);

		return new Vector3 (x, y, z);
	}

	public void AddChunk(Vector3 position, bool empty=false) {
		if (!world.TryGetValue (position, out tmpChunk)) {
			Chunk c = CreateChunk (position, empty);
			world.Add (position, c);

			Debug.Log (string.Format ("AddChunk() Pos: {0} Empty: {1}", position, empty));
		}
	}

	public Chunk GetChunk(Vector3 worldPosition) {
		Vector3 chunkPosition = WorldToChunkPosition (worldPosition);

		if (world.TryGetValue (chunkPosition, out tmpChunk)) {
			return tmpChunk;
		}

		return null;
	}

}
