using BlockWorld;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    [AlwaysSynchronizeSystem]
    [AlwaysUpdateSystem]
    public class PlaceBlockTestSystem : JobComponentSystem
    {
        EntityQuery _chunkQuery;

        int3 _currentBlockPos = new int3();

        void MakeChunk(int3 worldPos)
        {
            int3 chunkIndex = GridMath.Grid3D.CellIndexFromWorldPos(worldPos, Constants.BlockChunks.Size);
            _chunkQuery.SetSharedComponentFilter<BlockChunkIndex>(chunkIndex);
            if (_chunkQuery.CalculateEntityCount() > 0)
                return;

            Debug.Log($"Generating chunk {chunkIndex}");

            var arch = EntityManager.CreateArchetype(typeof(Block));

            var e = EntityManager.CreateEntity();
            EntityManager.SetName(e, $"Chunk {chunkIndex.ToString()}");
            EntityManager.AddSharedComponentData<BlockChunkIndex>(e, chunkIndex);

            var b = EntityManager.AddBuffer<BlockBuffer>(e);
            b.ResizeUninitialized(Constants.BlockChunks.Volume);

            var bufferArray = b.Reinterpret<Entity>().AsNativeArray();
            EntityManager.CreateEntity(arch, bufferArray);

            var blockQuery = GetEntityQuery(
                ComponentType.ReadWrite<Block>());

            EntityManager.AddSharedComponentData<BlockChunkIndex>(blockQuery, chunkIndex);
        }


        void PlaceBlock(int x, int y, int z, int blockType) => PlaceBlock(new int3(x, y, z), blockType);
        void PlaceBlock(int3 worldPos, int blockType)
        {
            MakeChunk(worldPos);
            var e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new PlaceBlock { pos = worldPos, blockType = blockType });
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _chunkQuery = GetEntityQuery(
                ComponentType.ReadOnly<BlockChunkIndex>(),
                ComponentType.ReadWrite<BlockBuffer>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (Input.GetButtonDown("Jump"))
                //{
                //    PlaceBlock(_currentBlockPos + new int3(0, 0, 0), 6);
                //    PlaceBlock(_currentBlockPos + new int3(1, 0, 0), 15);
                //    _currentBlockPos.x += Constants.ChunkSizeX;

                for (int i = 0; i < 5; ++i)
                {
                    for (int j = 0; j < 4; ++j)
                        PlaceBlock(_currentBlockPos + new int3(j, 0, 0), _currentBlockPos.x);
                    _currentBlockPos.x += Constants.BlockChunks.SizeX;
                }


            //if( Input.GetButtonDown("Fire1") )
            //{
            //    _chunkQuery.SetSharedComponentFilter<ChunkIndex>(new int3(0, 0, 0));
            //    var blockBuffer = EntityManager.GetBuffer<BlockBuffer>(_chunkQuery.GetSingletonEntity());
            //    int3 p = new int3(0, 0, 0);
            //    var block = EntityManager.GetComponentData<Block>(blockBuffer[0]);

            //    Debug.Log($"Block at {p.ToString()} : {block.type} ");
            //}

            return default;
        }
    }
}