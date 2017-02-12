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

	public void CreateChunk(byte[,,] chunk, float blockSize) {
		Init ();

		this.chunkSize = chunk.GetLength (0);
		this.chunkHalfSize = this.chunkSize / 2;
		this.blockSize = blockSize;
		this.blockHalfSize = blockSize * 0.5f;
		this.chunk = chunk;

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

	public void RemoveBlock(int x, int y, int z) {
		if (IsInBounds(x, y, z)) {
			chunk [x, y, z] = 0;
		}

		CreateChunk (chunk, blockSize);

		Debug.Log (string.Format ("Removed block at X: {0} Y: {1} Z:{2}", x, y, z));
	}

	public void RemoveBlock(Vector3 position) {
		RemoveBlock ((int)position.x, (int)position.y, (int)position.z);
	}

	public float GetSize() {
		return chunkSize * blockSize;
	}

	private void Init() {
		vertices.Clear ();
		uvs.Clear ();
		triangles.Clear ();
	}

	private bool IsInBounds(int x, int y, int z) {
		return x >= 0 && x < chunkSize && y >= 0 && y < chunkSize && z >= 0 && z < chunkSize;
	}

	private Vector3 GetWorldPosition(int x, int y, int z) {
		int px = (int)(x * blockSize - chunkHalfSize * blockSize);
		int py = (int)(y * blockSize - chunkHalfSize * blockSize);
		int pz = (int)(z * blockSize - chunkHalfSize * blockSize);

		return new Vector3 (px, py, pz);
	}

	private Vector3 GetWorldPosition(Vector3 position) {
		return GetWorldPosition ((int)position.x, (int)position.y, (int)position.z);
	}

	private BlockNeighbourInfo GetNeighbourBlockInfo(int x, int y, int z) {
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
		BlockNeighbourInfo blockNeighbourInfo = GetNeighbourBlockInfo (x, y, z);
		Vector3 worldPosition = GetWorldPosition (x, y, z);

		int px = (int) worldPosition.x;
		int py = (int) worldPosition.y;
		int pz = (int) worldPosition.z;

		if (!blockNeighbourInfo.Front)
			CreateFrontFace (px, py, pz);

		if (!blockNeighbourInfo.Back)
			CreateBackFace (px, py, pz);

		if (!blockNeighbourInfo.Left)
			CreateLeftFace (px, py, pz);

		if (!blockNeighbourInfo.Right)
			CreateRightFace (px, py, pz);

		if (!blockNeighbourInfo.Top)
			CreateTopFace (px, py, pz);

		if (!blockNeighbourInfo.Bottom)
			CreateBottomFace (px, py, pz);
	}

	private void CreateLeftFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z+blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateRightFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateTopFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateBottomFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z+blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateFrontFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z-blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z-blockHalfSize));

		CreateUvs ();
		CreateTriangles ();
	}

	private void CreateBackFace(int x, int y, int z) {
		vertices.Add (new Vector3 (x+blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y+blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x-blockHalfSize, y-blockHalfSize, z+blockHalfSize));
		vertices.Add (new Vector3 (x+blockHalfSize, y-blockHalfSize, z+blockHalfSize));

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
