using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	/// <summary>
	/// This in combination with a <see cref="RegionIndex"/> component signifies when and where a region
	/// should be loaded. Processed by <see cref="LoadRegionSystem"/>.
	/// </summary>
	public struct LoadRegion : IComponentData
	{
	} 
}
