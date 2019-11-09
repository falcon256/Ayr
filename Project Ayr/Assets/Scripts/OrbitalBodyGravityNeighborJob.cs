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

public struct OrbitalBodyGravityNeighborJob : IJobForEach<Translation,VelocityComponent,MassComponent,OrbitalBodyNeighborComponent,OrbitalBodyTagComponent>
{

    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Entity> allBodies;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Translation> allTranslations;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<MassComponent> allMasses;

    // Gravitational Constant. Units dyne-cm^2/g^2
    public const float GRAV = 6.6720e-08f;

    [BurstCompile]
    public void Execute ([ReadOnly]ref Translation trans, [ReadOnly]ref VelocityComponent vel, [ReadOnly]ref MassComponent mass,ref OrbitalBodyNeighborComponent neighbors, [ReadOnly]ref OrbitalBodyTagComponent tag)
    {



        int max = allBodies.Length;
        for (int i = 0; i < max - 1; i++)
        {
            Entity ebody = allBodies[i];
            float emass = allMasses[i].Value;
            float3 eposition = allTranslations[i].Value;

            if (math.distancesq(eposition, trans.Value) < 0.001f)
                continue;

            float influence = GRAV * ((mass.Value * emass) / math.distancesq(eposition, trans.Value));

            

            if(influence>neighbors.neighborInfluence1)
            {
                neighbors.neighborInfluence2 = neighbors.neighborInfluence1;
                neighbors.neighborEntities2 = neighbors.neighborEntities1;
                neighbors.neighborInfluence1 = influence;
                neighbors.neighborEntities1 = ebody;

                influence = neighbors.neighborInfluence2;
                ebody = neighbors.neighborEntities2;
            }

            if (influence > neighbors.neighborInfluence2)
            {
                neighbors.neighborInfluence3 = neighbors.neighborInfluence2;
                neighbors.neighborEntities3 = neighbors.neighborEntities2;
                neighbors.neighborInfluence2 = influence;
                neighbors.neighborEntities2 = ebody;

                influence = neighbors.neighborInfluence3;
                ebody = neighbors.neighborEntities3;
            }

            if (influence > neighbors.neighborInfluence3)
            {
                neighbors.neighborInfluence4 = neighbors.neighborInfluence3;
                neighbors.neighborEntities4 = neighbors.neighborEntities3;
                neighbors.neighborInfluence3 = influence;
                neighbors.neighborEntities3 = ebody;

                influence = neighbors.neighborInfluence4;
                ebody = neighbors.neighborEntities4;
            }

            if (influence > neighbors.neighborInfluence4)
            {
                neighbors.neighborInfluence4 = influence;
                neighbors.neighborEntities4 = ebody;
            }
        }       
    }
}
