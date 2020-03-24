using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public class SyncFlyingCameraSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Entities
				.WithAll<FlyingCameraController>()
				.WithoutBurst()
				.ForEach((Transform t, ref Translation translation) =>
				{
					translation.Value = t.position;
				}).Run();
		}
	}
}
