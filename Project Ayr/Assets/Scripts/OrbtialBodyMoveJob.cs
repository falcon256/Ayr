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
public struct OrbitalBodyMoveJob : IJobForEach<Translation,VelocityComponent,MassComponent,OrbitalBodyTagComponent>
{
    public void Execute (ref Translation trabs, [ReadOnly]ref VelocityComponent vel, [ReadOnly]ref MassComponent mass, [ReadOnly]ref OrbitalBodyTagComponent tag)
    {
        trabs.Value += vel.Value;
    }
}
