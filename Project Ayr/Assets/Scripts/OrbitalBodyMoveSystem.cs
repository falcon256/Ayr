using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
public class OrbitalBodyMoveSystem : JobComponentSystem
{
    //depricated
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        
        return inputDeps;
    }


    [BurstCompile]
    public struct OrbitalBodyGravityJob : IJobForEach<Translation, VelocityComponent, MassComponent, OrbitalBodyTagComponent, OrbitalBodyNeighborComponent>
    {
        public void Execute(ref Translation trabs, ref VelocityComponent vel, ref MassComponent mass, ref OrbitalBodyTagComponent tag, ref OrbitalBodyNeighborComponent neighbors)
        {
            
            //Continue from here
            //https://forum.unity.com/threads/getting-component-directly-from-entity-in-a-job.557350/#post-3692527
            //ComponentDataFromEntity<Translation> myTypeFromEntity = Unity.Entities.

        }

    }

}
