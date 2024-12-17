using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainMeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    [Range(16, 256)]
    public int xSize = 20;
    [Range(16, 256)]
    public int zSize = 20;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize+1) * (zSize+1)]; // quantidade de vertices é sempre a quantidade de quadrantes que o mesh vai ter + 1
        
        for (int cc = 0, ii=0; ii <= zSize; ii++)
        {
            for(int jj=0; jj <= xSize; jj++)
            {
                vertices[cc] = new Vector3(jj, 0, ii);
                cc++;
            }
        }

        int vert = 0;
        int tris = 0;
        for (int xx = 0; xx < xSize; xx++)
        {
            triangles = new int[6];
            triangles[0] = xx;
            triangles[1] = xSize + 1;
            triangles[2] = xx+1;

            triangles[3] = xx+1;
            triangles[4] = xSize + 1;
            triangles[5] = xSize + 2;
            vert++;
            tris += 6;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals(); // para que o mesh reaga corretamente à iluminação do objeto
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;
        for(int ii=0; ii < vertices.Length; ii++)
        {
            Gizmos.DrawSphere(vertices[ii], .1f);
        }
    }
}
