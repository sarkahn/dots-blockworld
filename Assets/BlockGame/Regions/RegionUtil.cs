using BlockGame.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BlockGame.Regions
{



    public static class RegionUtil
    {


        public static Entity CreateRegion(EntityCommandBuffer ecb)
        {
            var region = ecb.CreateEntity();
            return default;
        }

        //public static Entity CreateRegion<T>(T creator) where T : 
    }
}
