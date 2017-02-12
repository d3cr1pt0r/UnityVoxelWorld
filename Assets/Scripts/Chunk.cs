using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

	[SerializeField] private MeshFilter meshFilter = null;
	[SerializeField] private MeshCollider meshCollider = null;

	private Mesh mesh = null;
	private List<Vector3> vertices = null;
	private List<Vector2> uvs = null;
	private List<int> triangles = null;

	private byte[,,] chunk = null;
	private int chunkSize = 16;
	private int chunkHalfSize = 8;
	private float blockSize = 1.0f;
	private float blockHalfSize = 0.5f;

	private void Awake() {
		mesh = new Mesh ();
		vertices = new List<Vector3> ();
		uvs = new List<Vector2> ();
		triangles = new List<int> ();
	}

	private void Init() {
		meshCollider.sharedMesh = null;
		mesh.Clear (false);
		vertices.Clear ();
		uvs.Clear ();
		triangles.Clear ();
	}

	public void GenerateChunk(Vector3 position, int chunkSize, float blockSize, bool empty=false) {
		this.chunkSize = chunkSize;
		this.chunkHalfSize = chunkSize / 2;
		this.blockSize = blockSize;
		this.blockHalfSize = blockSize * 0.5f;

		if (!empty) {
			GenerateNoiseChunk (position, chunkSize);
		} else {
			GenerateEmptyChunk (chunkSize);
		}

		GenerateChunkMesh ();
	}

	private void GenerateChunkMesh() {
		Init ();

		for (int z = 0; z < chunkSize; z++) {
			for (int y = 0; y < chunkSize; y++) {
				for (int x = 0; x < chunkSize; x++) {
					if (chunk [x, y, z] > 0) {
						CreateBlock (x, y, z);
					}
				}
			}
		}

		CreateMesh ();
	}

	public void SetBlock(int x, int y, int z, byte type) {
		if (IsInBounds (x, y, z)) {
			chunk [x, y, z] = type;
			GenerateChunkMesh ();

			Debug.Log (string.Format ("SetBlock() X: {0} Y: {1} Z: {2} Type: {3}", x, y, z, type));
		} else {
			Debug.Log (string.Format ("SetBlock() out of bounds X: {0} Y: {1} Z: {2}", x, y, z));
		}
	}

	public void SetBlock(Vector3 position, byte type) {
		SetBlock ((int)position.x, (int)position.y, (int)position.z, type);
	}

	private void GenerateEmptyChunk(int chunkSize) {
		chunk = new byte[chunkSize, chunkSize, chunkSize];

		for (int z = 0; z < chunkSize; z++) {
			for (int y = 0; y < chunkSize; y++) {
				for (int x = 0; x < chunkSize; x++) {
					chunk [x, y, z] = 0;
				}
			}
		}
	}

	private void GenerateNoiseChunk(Vector3 position, int chunkSize) {
		chunk = new byte[chunkSize, chunkSize, chunkSize];

		for (int z = 0; z < chunkSize; z++) {
			float noiseZ = (float)(position.z + z * blockSize) / 50.0f;

			for (int y = 0; y < chunkSize; y++) {
				float noiseY = (float)(position.y + y * blockSize) / 50.0f;

				for (int x = 0; x < chunkSize; x++) {
					float noiseX = (float)(position.x + x * blockSize) / 50.0f;
					float noiseValue = SimplexNoise.Noise.Generate (noiseX, noiseY, noiseZ);

					noiseValue += (10.0f - (float)y) / 10.0f;
					chunk [x, y, z] = noiseValue > 0.2f ? (byte)1 : (byte)0;

					if (y < 1) {
						chunk [x, y, z] = 1;
					}
				}
			}
		}
	}

	private bool IsInBounds(int x, int y, int z) {
		return x >= 0 && x < chunkSize && y >= 0 && y < chunkSize && z >= 0 && z < chunkSize;
	}

	// gets chunk position transformed from 0-chunkSize range to -halfChunkSize - halfChunkSize
	private Vector3 GetChunkSpacePosition(int x, int y, int z) {
		int px = (int)(x * blockSize - chunkHalfSize * blockSize);
		int py = (int)(y * blockSize - chunkHalfSize * blockSize);
		int pz = (int)(z * blockSize - chunkHalfSize * blockSize);

		return new Vector3 (px, py, pz);
	}

	private Vector3 GetChunkSpacePosition(Vector3 position) {
		return GetChunkSpacePosition ((int)position.x, (int)position.y, (int)position.z);
	}

	private BlockNeighbourInfo GetBlockNeighbourInfo(int x, int y, int z) {
		BlockNeighbourInfo blockNeighbourInfo = new BlockNeighbourInfo ();

		blockNeighbourInfo.Front = IsInBounds (x, y, z - 1) && chunk [x, y, z - 1] > 0;
		blockNeighbourInfo.Back = IsInBounds (x, y, z + 1) && chunk [x, y, z + 1] > 0;
		blockNeighbourInfo.Left = IsInBounds (x - 1, y, z) && chunk [x - 1, y, z] > 0;
		blockNeighbourInfo.Right = IsInBounds (x + 1, y, z) && chunk [x + 1, y, z] > 0;
		blockNeighbourInfo.Top = IsInBounds (x, y + 1, z) && chunk [x, y + 1, z] > 0;
		blockNeighbourInfo.Bottom = IsInBounds (x, y - 1, z) && chunk [x, y - 1, z] > 0;

		return blockNeighbourInfo;
	}

	private void CreateMesh() {
		mesh.Clear ();
		mesh.vertices = vertices.ToArray ();
		mesh.uv = uvs.ToArray ();
		mesh.triangles = triangles.ToArray ();

		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}

	private void CreateBlock(int x, int y, int z) {
		BlockNeighbourInfo blockNeighbourInfo = GetBlockNeighbourInfo (x, y, z);
		Vector3 worldPosition = GetChunkSpacePosition (x, y, z);

		if (!blockNeighbourInfo.Front)
			CreateFrontFace (worldPosition);

		if (!blockNeighbourInfo.Back)
			CreateBackFace (worldPosition);

		if (!blockNeighbourInfo.Left)
			CreateLeftFace (worldPosition);

		if (!blockNeighbourInfo.Right)
			CreateRightFace (worldPosition);

		if (!blockNeighbourInfo.Top)
			CreateTopFace (worldPosition);

		if (!blockNeighbourInfo.Bottom)
			CreateBottomFace (worldPosition);
	}

	private void CreateLeftFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateRightFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateTopFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateBottomFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateFrontFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateBackFace(Vector3 position) {
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y+blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x-blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));
		vertices.Add (new Vector3 (position.x+blockHalfSize, position.y-blockHalfSize, position.z+blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateUvs() {
		uvs.Add (new Vector2 (0, 1));
		uvs.Add (new Vector2 (1, 1));
		uvs.Add (new Vector2 (1, 0));
		uvs.Add (new Vector2 (0, 0));
	}

	private void CreateTriangles() {
		int verticesCount = vertices.Count;

		triangles.Add(verticesCount-4);
		triangles.Add(verticesCount-3);
		triangles.Add(verticesCount-2);

		triangles.Add(verticesCount-4);
		triangles.Add(verticesCount-2);
		triangles.Add(verticesCount-1);
	}
}
