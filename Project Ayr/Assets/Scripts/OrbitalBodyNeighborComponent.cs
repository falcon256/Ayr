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
public struct OrbitalBodyNeighborComponent : IComponentData
{
    public Entity neighborEntities1;
    public float3 neightbor1Position;
    public float neighborInfluence1;
    public Entity neighborEntities2;
    public float3 neightbor2Position;
    public float neighborInfluence2;
    public Entity neighborEntities3;
    public float3 neightbor3Position;
    public float neighborInfluence3;
    public Entity neighborEntities4;
    public float3 neightbor4Position;
    public float neighborInfluence4;

}
