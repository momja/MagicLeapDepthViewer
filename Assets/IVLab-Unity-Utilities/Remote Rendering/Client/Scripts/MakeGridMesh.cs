using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeGridMesh : MonoBehaviour
{

    public int width = 512;
    public int height = 512;

    

    // Start is called before the first frame update
    void Start()
    {
        Mesh grid = new Mesh();
        grid.name = "Grid";

        Vector2 size = Vector2.one;
        Vector2 center = Vector2.zero;
        Vector2 extents = size / 2;

        Vector2 topLeft = center - extents;
        Vector2 bottomRight = center + extents;

        Vector3 [] verts = new Vector3[width * height];
        Vector2 [] uvs = new Vector2[width * height];

        int[] indices = new int[(width - 1) * (height - 1) * 4];
        int vertIndex = 0;
        Vector2 uvTopLeft = new Vector2(0, 0);
        Vector2 uvBottomRight = new Vector2(1, 1);

        //topLeft = new Vector2(-0.25f, -0.25f);
        //bottomRight = new Vector2(0.25f, 0.25f);

        // uvTopLeft = new Vector2(0.25f, 0.25f);
        // uvBottomRight = new Vector2(0.75f, 0.75f);
        for (int r = 0; r < height; r++)
        {
            for(int c = 0; c < width; c++)
            {
                float px = c / (width - 0f);
                float py = r / (height - 0f);
                float x = Mathf.Lerp(topLeft.x, bottomRight.x, px);
                float y = Mathf.Lerp(topLeft.y, bottomRight.y, py);
                float u = Mathf.Lerp(uvTopLeft.x, uvBottomRight.x, px);
                float v = Mathf.Lerp(uvTopLeft.y, uvBottomRight.y, py);

                verts[vertIndex] = new Vector3(x, y, 0);
                uvs[vertIndex] = new Vector2(u, v);
                vertIndex++;
            }
        }

        int i = 0;
        for (int r = 0; r < height - 1; r++)
        {
            for (int c = 0; c < width - 1; c++)
            {
                indices[i++] = (r + 1) * height + (c);

                indices[i++] = (r + 1) * height + (c + 1);

                indices[i++] = r * height + (c + 1);

                indices[i++] = r * height + c;
            }
        }

        grid.SetVertices(verts);
        grid.SetUVs(0,uvs);
        grid.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        grid.SetIndices(indices, MeshTopology.Quads, 0);
        grid.RecalculateNormals();
        grid.RecalculateBounds();
        grid.RecalculateTangents();
        grid.Optimize();
        grid.UploadMeshData(false);
        Debug.Log("Assigning grid");
        GetComponent<MeshFilter>().mesh = grid;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
