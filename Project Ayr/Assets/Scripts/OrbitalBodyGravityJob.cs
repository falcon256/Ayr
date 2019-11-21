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
public struct OrbitalBodyGravityJob : IJobForEach<Translation,VelocityComponent,MassComponent,OrbitalBodyTagComponent, OrbitalBodyNeighborComponent>
{
    [BurstCompile]
    public void Execute ([ReadOnly]ref Translation trabs,ref VelocityComponent vel, [ReadOnly]ref MassComponent mass, [ReadOnly]ref OrbitalBodyTagComponent tag, [ReadOnly]ref OrbitalBodyNeighborComponent neighbors)
    {
        //inop
        /*
        float3 velMod = float3.zero;
        float3 n1Dif = math.normalize(neighbors.neightbor1Position-trabs.Value);
        float3 n2Dif = math.normalize(neighbors.neightbor2Position-trabs.Value);
        float3 n3Dif = math.normalize(neighbors.neightbor3Position-trabs.Value);
        float3 n4Dif = math.normalize(neighbors.neightbor4Position-trabs.Value);

        velMod += (n1Dif * neighbors.neighborInfluence1);
        if(neighbors.neighborEntities1!=neighbors.neighborEntities2)
            velMod += (n1Dif * neighbors.neighborInfluence2);
        if (neighbors.neighborEntities2 != neighbors.neighborEntities3)
            velMod += (n1Dif * neighbors.neighborInfluence3);
        if (neighbors.neighborEntities3 != neighbors.neighborEntities4)
            velMod += (n1Dif * neighbors.neighborInfluence4);


        

        vel.Value += velMod*100000.0f;

    */
    }
}
