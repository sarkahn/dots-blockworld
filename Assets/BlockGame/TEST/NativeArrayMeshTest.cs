using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class NativeArrayMeshTest : MonoBehaviour
{
    Mesh mesh;

    NativeList<float3> verts;
    NativeList<int> indices;
    NativeList<Color32> colors;

    [SerializeField]
    Material material;

    [SerializeField]
    int iterations = 500;

    private void OnEnable()
    {
        mesh = new Mesh();
        verts = new NativeList<float3>(Allocator.Persistent);
        indices = new NativeList<int>(Allocator.Persistent);
        colors = new NativeList<Color32>(Allocator.Persistent);

        verts.Add(new float3(0, 0, 0));
        verts.Add(new float3(0, 1, 0));
        verts.Add(new float3(1, 1, 0));
        verts.Add(new float3(1, 0, 0));

        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);

        for (int i = 0; i < 4; ++i)
            colors.Add(Color.blue);

        mesh.SetVertices<float3>(verts);
        mesh.SetIndices<int>(indices, MeshTopology.Triangles, 0);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    [ContextMenu("AddFaceMainThread")]
    void AddFaceMainThread()
    {
        int index = indices.Length;

        verts.Add(new float3(1, 0, 0));
        verts.Add(new float3(1, 1, 0));
        verts.Add(new float3(2, 1, 0));
        verts.Add(new float3(2, 0, 0));

        indices.Add(index + 0);
        indices.Add(index + 1);
        indices.Add(index + 2);
        indices.Add(index + 0);
        indices.Add(index + 2);
        indices.Add(index + 3);
        
        for (int i = 0; i < 4; ++i)
            colors.Add(Color.blue);

        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
    }

    [ContextMenu("AddFacesViaJob")]
    void AddFacesJob()
    {
        AddFaces(new float3(1, 0, 0));
    }

    [ContextMenu("ChangeFace")]
    void ChangeFace()
    {
        for( int i = 0; i < 4; ++i )
        verts[i] = verts[i] - new float3(1, 0, 0);

        mesh.SetVertices<float3>(verts);
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
    }

    void AddFaces(float3 pos)
    {
        var job = new AddFaceJob();
        job.origin = pos;
        job.verts = verts;
        job.indices = indices;
        job.colors = colors;
        job.rowWidth = 16;
        job.iterations = iterations;
        job.colA = Color.blue;
        job.colB = Color.red;
        var handle = job.Schedule();

        StartCoroutine(UploadJobChanges(mesh, handle));
    }

    IEnumerator UploadJobChanges(Mesh mesh, JobHandle job)
    {
        while (!job.IsCompleted)
            yield return null;

        job.Complete();

        mesh.SetVertices<float3>(verts);
        mesh.SetIndices<int>(indices, MeshTopology.Triangles, 0);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    struct AddFaceJob : IJob
    {
        public NativeList<float3> verts;
        public NativeList<int> indices;
        public NativeList<Color32> colors;

        public float3 origin;

        public int rowWidth;
        public int iterations;

        public Color colA;
        public Color colB;

        public void Execute()
        {
            for(int i = 0; i < iterations; ++i )
            {
                float3 p = origin + new float3(i % rowWidth, i / rowWidth, 0);
                verts.Add(p + new float3(0, 0, 0));
                verts.Add(p + new float3(0, 1, 0));
                verts.Add(p + new float3(1, 1, 0));
                verts.Add(p + new float3(1, 0, 0));

                indices.Add(i * 4 + 0);
                indices.Add(i * 4 + 1);
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 0);
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 3);

                for (int j = 0; j < 4; ++j)
                    colors.Add(Color.Lerp(colA, colB, (float)i / iterations));
            }

        }
    }

    private void OnDisable()
    {
        verts.Dispose();
        indices.Dispose();
        colors.Dispose();
        mesh.Clear();
    }
}
