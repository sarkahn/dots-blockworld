using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct LinkToEntity : IComponentData
	{
		public Entity target;
	} 
}
