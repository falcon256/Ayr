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
    public const float GRAV = 6.6720e-08f;

    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Entity> allBodies;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Translation> allTranslations;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<MassComponent> allMasses;

    public void Execute ([ReadOnly]ref Translation trabs,ref VelocityComponent vel, [ReadOnly]ref MassComponent mass, [ReadOnly]ref OrbitalBodyTagComponent tag, [ReadOnly]ref OrbitalBodyNeighborComponent neighbors)
    {
        //first find our neighbors and set our needed data.
        //this should be done in a seperate job system, temporarily here just to get it working for that damn milestone.
        float3 n1pos = float3.zero;
        float n1Mass = 0.0f;
        bool hasn1 = false;
        float3 n2pos = float3.zero;
        float n2Mass = 0.0f;
        bool hasn2 = false;
        float3 n3pos = float3.zero;
        float n3Mass = 0.0f;
        bool hasn3 = false;
        float3 n4pos = float3.zero;
        float n4Mass = 0.0f;
        bool hasn4 = false;

        float3 velMod = float3.zero;

        for (int i = 0; i < allBodies.Length; i++)
        {
            if(neighbors.neighborEntities1==allBodies[i])
            {
                n1pos = allTranslations[i].Value;
                n1Mass = allMasses[i].Value;
                hasn1 = true;
            }
            if (neighbors.neighborEntities2 == allBodies[i])
            {
                n2pos = allTranslations[i].Value;
                n2Mass = allMasses[i].Value;
                hasn2 = true;
            }
            if (neighbors.neighborEntities3 == allBodies[i])
            {
                n3pos = allTranslations[i].Value;
                n3Mass = allMasses[i].Value;
                hasn3 = true;
            }
            if (neighbors.neighborEntities4 == allBodies[i])
            {
                n4pos = allTranslations[i].Value;
                n4Mass = allMasses[i].Value;
                hasn4 = true;
            }
        }
        
        if(hasn1)
        {
            float3 dist = (n1pos - trabs.Value);
            float fdist = math.lengthsq(dist);
            if (fdist > 1.0f)
            {
                float influence = 10000.0f * GRAV * ((mass.Value * n1Mass) / fdist);
                velMod += math.normalize(dist) * influence;
            }
        }
        if (hasn2)
        {
            float3 dist = (n2pos - trabs.Value);
            float fdist = math.lengthsq(dist);
            if (fdist > 1.0f)
            {
                float influence = 10000.0f * GRAV * ((mass.Value * n2Mass) / fdist);
                velMod += math.normalize(dist) * influence;
            }
        }
        if (hasn3)
        {
            float3 dist = (n3pos - trabs.Value);
            float fdist = math.lengthsq(dist);
                if (fdist > 1.0f)
                {
                    float influence = 10000.0f * GRAV * ((mass.Value * n3Mass) / fdist);
                    velMod += math.normalize(dist) * influence;
                }
        }
        if (hasn4)
        {
            float3 dist = (n4pos - trabs.Value);
            float fdist = math.lengthsq(dist);
                    if (fdist > 1.0f)
                    {
                        float influence = 10000.0f * GRAV * ((mass.Value * n4Mass) / fdist);
                        velMod += math.normalize(dist) * influence;
                    }
        }

        vel.Value += velMod;

        //Continue from here
        //https://forum.unity.com/threads/getting-component-directly-from-entity-in-a-job.557350/#post-3692527
        //ComponentDataFromEntity<Translation> myTypeFromEntity = Unity.Entities.

    }
}
