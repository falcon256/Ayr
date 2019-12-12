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
        float3 delta = vel.Value;
        delta.y *= 0.99f;
        delta.y -= trabs.Value.y * 0.01f;
        trabs.Value += delta;
    }
}
