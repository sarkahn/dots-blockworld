using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

using static UnityEngine.Mesh;

namespace TestingMeshDataAPI
{
    struct MakeMeshJob : IJob
    {
        public MeshData MeshData;

        public void Execute()
        {
            // Note - Unity meshes typically lay out all vertex buffer data in a single stream, stream 0
            // IE: Position/Normal/Tangent/Color/TexCoords/BlendWeight/BlendIndices in order

            // Here we use stream 0 for position only and separate normals into stream 1
            MeshData.SetVertexBufferParams(4,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));

            float3 right = new float3(1, 0, 0);
            float3 up = new float3(0, 1, 0);

            float3 bl = 0;


            // 1--2
            // | /|
            // |/ |
            // 0--3
            var positions = MeshData.GetVertexData<float3>();
            positions[0] = bl;
            positions[1] = bl + up;
            positions[2] = bl + up + right;
            positions[3] = bl + right;

            MeshData.SetIndexBufferParams(6, IndexFormat.UInt16);
            var indices = MeshData.GetIndexData<ushort>();
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            MeshData.subMeshCount = 1;
            MeshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
        }
    }

    public class TestMeshDataAPI : MonoBehaviour
    {
        [SerializeField]
        int _numToCreate = 3;

        bool _regenerate;

        Mesh.MeshDataArray _meshDataArray;
        JobHandle? _job = null;

        List<GameObject> _meshGOs = new List<GameObject>();

        Material _mat;

        private void OnEnable()
        {
            _regenerate = true;

            _mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        }

        private void Update()
        {
            if( _regenerate && _job == null )
            {
                _regenerate = false;
                _job = default(JobHandle);

                _meshDataArray = Mesh.AllocateWritableMeshData(_numToCreate);

                for (int i = 0; i < _numToCreate; ++i)
                {
                    var meshJob = new MakeMeshJob
                    {
                        MeshData = _meshDataArray[i]
                    }.Schedule();
                    _job = JobHandle.CombineDependencies(_job.Value, meshJob);
                }

                JobHandle.ScheduleBatchedJobs();

                UpdateGameObjectCount();

                return;
            }

            if( _job != null && _job.Value.IsCompleted )
            {
                _job.Value.Complete();

                var meshes = _meshGOs.Select(go => go.GetComponent<MeshFilter>().sharedMesh).ToArray();

                Mesh.ApplyAndDisposeWritableMeshData(_meshDataArray, meshes);

                foreach(Mesh mesh in meshes )
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                }

                _job = null;
            }
        }

        void UpdateGameObjectCount()
        {
            while(_meshGOs.Count > _numToCreate)
            {
                int lastIndex = _meshGOs.Count - 1;
                Destroy(_meshGOs[lastIndex].GetComponent<MeshFilter>().sharedMesh);
                Destroy(_meshGOs[lastIndex]);
                _meshGOs.RemoveAt(lastIndex);
            }

            while( _meshGOs.Count < _numToCreate)
            {
                var go = new GameObject($"Mesh {_meshGOs.Count}", typeof(MeshFilter), (typeof(MeshRenderer)));
                _meshGOs.Add(go);
                go.GetComponent<MeshFilter>().sharedMesh = new Mesh();
                go.GetComponent<MeshRenderer>().sharedMaterial = _mat;
            }

            int size = (int)(math.ceil(math.sqrt(_meshGOs.Count + 1)));
            for( int x = 0; x < size; ++x )
            {
                for( int y = 0; y < size; ++y )
                {
                    int i = size * y + x;
                    if (i >= _meshGOs.Count)
                        break;

                    _meshGOs[i].transform.position = new Vector3(x * 2, y * 2, 0);
                }
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
                return;

            _numToCreate = math.max(_numToCreate, 1);

            _regenerate = true;
        }
    }
}