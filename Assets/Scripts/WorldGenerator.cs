using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

	[SerializeField] private Chunk chunk = null;
	[SerializeField] private int chunkViewDistance = 5;
	[SerializeField] private int chunkSize = 16;
	[SerializeField] private float blockSize = 1.0f;
	[SerializeField] private int seed = 0;

	private Dictionary<Vector3, Chunk> world;
	private Chunk tmpChunk;

	private void Awake() {
		world = new Dictionary<Vector3, Chunk> ();
		tmpChunk = null;

//		CheckForNewChunks ();
	}

	private void Update() {
		CheckForNewChunks ();
	}

	private void CheckForNewChunks() {
		Vector3 playerPosition = Camera.main.transform.position;
		Vector3 chunkPos = WorldToChunkPosition (playerPosition);

		for (int z = -chunkViewDistance; z <= chunkViewDistance; z++) {
			for (int y = -chunkViewDistance; y <= chunkViewDistance; y++) {
				for (int x = -chunkViewDistance; x <= chunkViewDistance; x++) {
					int viewerChunkX = (int)chunkPos.x + x;
					int viewerChunkY = (int)chunkPos.y + y;
					int viewerChunkZ = (int)chunkPos.z + z;

					// temporary disable chunk generation on y-axis
					viewerChunkY = 0;

					Vector3 viewerChunkPos = new Vector3 (viewerChunkX, viewerChunkY, viewerChunkZ);

					if (!world.TryGetValue (viewerChunkPos, out tmpChunk)) {
						Chunk chunk = CreateChunk (viewerChunkX, viewerChunkY, viewerChunkZ);
						world.Add (viewerChunkPos, chunk);
					}
				}
			}
		}
	}

	private Chunk CreateChunk(int x, int y, int z) {
		Vector3 worldPosition = new Vector3 (x * blockSize * chunkSize, y * blockSize * chunkSize, z * blockSize * chunkSize);

		Chunk chunkInstance = Instantiate (chunk, worldPosition, Quaternion.identity);
		chunkInstance.CreateChunk (GenerateSimplexNoiseChunk (worldPosition), blockSize);
		chunkInstance.transform.SetParent (transform);

		return chunkInstance;
	}

	private byte[,,] GenerateSimplexNoiseChunk(Vector3 position) {
		byte[,,] chunk = new byte[chunkSize, chunkSize, chunkSize];

		for (int z = 0; z < chunkSize; z++) {
			float noiseZ = (float)(position.z + z * blockSize) / 50.0f;
			for (int y = 0; y < chunkSize; y++) {
				float noiseY = (float)(position.y + y * blockSize) / 50.0f;
				for (int x = 0; x < chunkSize; x++) {
					float noiseX = (float)(position.x + x * blockSize) / 50.0f;
					float noiseValue = SimplexNoise.Noise.Generate (noiseX, noiseY, noiseZ);
					noiseValue += (10.0f - (float)y) / 10.0f;
					chunk [x, y, z] = noiseValue > 0.2f ? (byte)1 : (byte)0;
					if (y <= 2) {
						chunk [x, y, z] = 1;
					}
				}
			}
		}

		return chunk;
	}

	public float GetBlockSize() {
		return blockSize;
	}

	public Vector3 WorldToChunkPosition(Vector3 worldPosition) {
		float preMultiply = chunkSize * blockSize;

		int x = Mathf.RoundToInt ((worldPosition.x + blockSize * 0.5f) / preMultiply);
		int y = Mathf.RoundToInt ((worldPosition.y + blockSize * 0.5f) / preMultiply);
		int z = Mathf.RoundToInt ((worldPosition.z + blockSize * 0.5f) / preMultiply);

		return new Vector3 (x, y, z);
	}

	public Vector3 WorldToBlockPosition(Vector3 worldPosition) {
		Vector3 chunkPosition = WorldToChunkPosition (worldPosition);

		float preCalc1 = blockSize * 0.5f + (chunkSize * 0.5f) * blockSize;
		float preCalc2 = chunkSize * blockSize;

		int x = (int) (((worldPosition.x + preCalc1) - chunkPosition.x * preCalc2) / blockSize);
		int y = (int) (((worldPosition.y + preCalc1) - chunkPosition.y * preCalc2) / blockSize);
		int z = (int) (((worldPosition.z + preCalc1) - chunkPosition.z * preCalc2) / blockSize);

		return new Vector3 (x, y, z);
	}

	public Chunk GetChunk(Vector3 worldPosition) {
		Vector3 chunkPosition = WorldToChunkPosition (worldPosition);

		if (world.TryGetValue (chunkPosition, out tmpChunk)) {
			return tmpChunk;
		}

		return null;
	}

}
