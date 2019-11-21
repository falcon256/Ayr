using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

[BurstCompile]
public struct OrbitalBodyRenderingUpdateJob : IJobForEach<Translation, Scale, VelocityComponent, MassComponent, OrbitalBodyTagComponent, OrbitalBodyNeighborComponent>
{
    [BurstCompile]
    public void Execute([ReadOnly]ref Translation c0, ref Scale c1, [ReadOnly]ref VelocityComponent c2, [ReadOnly]ref MassComponent c3, [ReadOnly]ref OrbitalBodyTagComponent c4, [ReadOnly]ref OrbitalBodyNeighborComponent c5)
    {
        c1.Value = math.sqrt((c3.Value * 0.62035f * math.PI) / 6.0f);
    }
}
