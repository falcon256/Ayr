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

public struct OrbitalBodyGravityNeighborJob : IJobForEach<Translation,VelocityComponent,MassComponent,OrbitalBodyNeighborComponent,OrbitalBodyTagComponent,TemperatureComponent>
{
    [NativeDisableParallelForRestriction] public NativeArray<TemperatureComponent> allTemperatures;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Entity> allBodies;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<Translation> allTranslations;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<VelocityComponent> allVelocities;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<MassComponent> allMasses;
    public EntityCommandBuffer.Concurrent CommandBuffer;

    // Gravitational Constant. Units dyne-cm^2/g^2
    public const float GRAV = 6.6720e-08f;

    [BurstCompile]
    public void Execute ([ReadOnly]ref Translation trans, ref VelocityComponent vel, ref MassComponent mass,ref OrbitalBodyNeighborComponent neighbors, [ReadOnly]ref OrbitalBodyTagComponent tag, ref TemperatureComponent tempK)
    {
        //my other 5 body gravity is broken so I said screw it and did nbody.
        float3 vecDif = float3.zero;
        int bodyCount = allBodies.Length;

        if(math.length(trans.Value)>1000.0f)
        {
            vecDif -= trans.Value * 0.000001f;//bring them back into the system if they are way off.
        }

        for (int i = 0; i < bodyCount - 1; i++)
        {
            Entity ebody = allBodies[i];
            float emass = allMasses[i].Value;
            float3 eposition = allTranslations[i].Value;
            float3 evel = allVelocities[i].Value;
            if (math.distancesq(eposition, trans.Value) < 0.01f)
                continue;

            float influence = 100000.0f * GRAV * ((emass / (mass.Value + 1.0f)) / (math.distancesq(eposition, trans.Value) * 0.2f));
            vecDif += math.normalize(eposition - trans.Value) * influence;
            float distsq = math.distancesq(eposition, trans.Value);
            if (math.sqrt(distsq) < math.sqrt(((emass + mass.Value) * 0.62035f * math.PI) / 6.0f) && mass.Value > emass)
            {
                float totalMass = mass.Value += emass;
                float3 vel1 = mass.Value * vel.Value;
                float3 vel2 = emass * evel;
                vel.Value = (vel1 + vel2) / totalMass;
                mass.Value = totalMass;
                CommandBuffer.DestroyEntity(i, ebody);
                neighbors.neighborInfluence1 = 0;
                neighbors.neightbor1Position = float3.zero;

                

                return;
            }
        }
        tempK.Value += math.length(vecDif) / (mass.Value + 1.0f);
        vel.Value += vecDif;



            /*
            bool requiresUpdate = false;
            //let's grab our neighbors influence and location data.
            if (neighbors.neighborInfluence1 > 0 && neighbors.neighborInfluence2 > 0 && neighbors.neighborInfluence3 > 0 && neighbors.neighborInfluence4 > 0)
            {
                int bodyCount = allBodies.Length;
                for (int i = 0; i < bodyCount - 1; i++)
                {
                    Entity ebody = allBodies[i];               
                    float emass = allMasses[i].Value;
                    float3 eposition = allTranslations[i].Value;
                    if (ebody == neighbors.neighborEntities1 && neighbors.neighborEntities1 != Entity.Null)
                    {
                        float distsq = math.distancesq(eposition, trans.Value);
                        float influence = GRAV * ((emass / (mass.Value + 1.0f)) / (math.distancesq(eposition, trans.Value) * 0.1f));
                        neighbors.neightbor1Position = eposition;
                        neighbors.neighborInfluence1 = influence;
                        if(math.sqrt(distsq)< (((emass + mass.Value) * 0.62035f * math.PI) / 6.0f) && mass.Value > emass)
                        {
                            mass.Value += emass;
                            CommandBuffer.DestroyEntity(i ,ebody);
                            neighbors.neighborInfluence1 = 0;
                            neighbors.neightbor1Position = float3.zero;
                            return;
                        }
                    }
                    if (ebody == neighbors.neighborEntities2 && neighbors.neighborEntities2 != Entity.Null && neighbors.neighborEntities1!=neighbors.neighborEntities2)
                    {
                        float distsq = math.distancesq(eposition, trans.Value);
                        float influence = GRAV * ((emass / (mass.Value + 1.0f)) / (math.distancesq(eposition, trans.Value) * 0.1f));
                        neighbors.neightbor2Position = eposition;
                        neighbors.neighborInfluence2 = influence;
                        if (math.sqrt(distsq) < (((emass + mass.Value) * 0.62035f * math.PI) / 6.0f) && mass.Value > emass)
                        {
                            mass.Value += emass;
                            CommandBuffer.DestroyEntity(i, ebody);
                            neighbors.neighborInfluence2 = 0;
                            neighbors.neightbor2Position = float3.zero;
                            return;
                        }
                    }
                    if (ebody == neighbors.neighborEntities3 && neighbors.neighborEntities3 != Entity.Null && neighbors.neighborEntities2 != neighbors.neighborEntities3)
                    {
                        float distsq = math.distancesq(eposition, trans.Value);
                        float influence = GRAV * ((emass / (mass.Value + 1.0f)) / (math.distancesq(eposition, trans.Value) * 0.1f));
                        neighbors.neightbor3Position = eposition;
                        neighbors.neighborInfluence3 = influence;
                        if (math.sqrt(distsq) < (((emass + mass.Value) * 0.62035f * math.PI) / 6.0f) && mass.Value > emass)
                        {
                            mass.Value += emass;
                            CommandBuffer.DestroyEntity(i, ebody);
                            neighbors.neighborInfluence3 = 0;
                            neighbors.neightbor3Position = float3.zero;
                            return;
                        }
                    }
                    if (ebody == neighbors.neighborEntities4 && neighbors.neighborEntities4 != Entity.Null && neighbors.neighborEntities3 != neighbors.neighborEntities4)
                    {
                        float distsq = math.distancesq(eposition, trans.Value);
                        float influence = GRAV * ((emass / (mass.Value + 1.0f)) / (math.distancesq(eposition, trans.Value) * 0.1f));
                        neighbors.neightbor4Position = eposition;
                        neighbors.neighborInfluence4 = influence;
                        if (math.sqrt(distsq) < (((emass + mass.Value) * 0.62035f * math.PI) / 6.0f) && mass.Value > emass)
                        {
                            mass.Value += emass;
                            CommandBuffer.DestroyEntity(i, ebody);
                            neighbors.neighborInfluence4 = 0;
                            neighbors.neightbor4Position = float3.zero;
                            return;
                        }
                    }
                }


                if (neighbors.neighborInfluence1 <= neighbors.neighborInfluence2 ||
                   neighbors.neighborInfluence2 <= neighbors.neighborInfluence3 ||
                   neighbors.neighborInfluence3 <= neighbors.neighborInfluence4)
                {
                    requiresUpdate = true;
                }




            }
            else
            {
                requiresUpdate = true;
            }


            //first we need to determine if we need to update this entities neighbors




            if (!requiresUpdate)
                return;
            int max = allBodies.Length;
            for (int i = 0; i < max - 1; i++)
            {
                Entity ebody = allBodies[i];
                float emass = allMasses[i].Value;
                float3 eposition = allTranslations[i].Value;

                if (math.distancesq(eposition, trans.Value) < 0.01f)
                    continue;

                float influence = GRAV * ((emass / (mass.Value+1.0f)) / (math.distancesq(eposition, trans.Value)*0.1f));



                if(influence>neighbors.neighborInfluence1)
                {
                    neighbors.neighborInfluence2 = neighbors.neighborInfluence1;
                    neighbors.neighborEntities2 = neighbors.neighborEntities1;
                    neighbors.neightbor2Position = neighbors.neightbor1Position;
                    neighbors.neighborInfluence1 = influence;
                    neighbors.neighborEntities1 = ebody;
                    neighbors.neightbor1Position = eposition;

                    influence = neighbors.neighborInfluence2;
                    ebody = neighbors.neighborEntities2;
                    eposition = neighbors.neightbor2Position;
                }

                if (influence > neighbors.neighborInfluence2)
                {
                    neighbors.neighborInfluence3 = neighbors.neighborInfluence2;
                    neighbors.neighborEntities3 = neighbors.neighborEntities2;
                    neighbors.neightbor3Position = neighbors.neightbor2Position;
                    neighbors.neighborInfluence2 = influence;
                    neighbors.neighborEntities2 = ebody;
                    neighbors.neightbor2Position = eposition;

                    influence = neighbors.neighborInfluence3;
                    ebody = neighbors.neighborEntities3;
                    eposition = neighbors.neightbor3Position;
                }

                if (influence > neighbors.neighborInfluence3)
                {
                    neighbors.neighborInfluence4 = neighbors.neighborInfluence3;
                    neighbors.neighborEntities4 = neighbors.neighborEntities3;
                    neighbors.neightbor4Position = neighbors.neightbor3Position;
                    neighbors.neighborInfluence3 = influence;
                    neighbors.neighborEntities3 = ebody;
                    neighbors.neightbor3Position = eposition;

                    influence = neighbors.neighborInfluence4;
                    ebody = neighbors.neighborEntities4;
                    eposition = neighbors.neightbor4Position;
                }

                if (influence > neighbors.neighborInfluence4)
                {
                    neighbors.neighborInfluence4 = influence;
                    neighbors.neighborEntities4 = ebody;
                    neighbors.neightbor4Position = eposition;
                }
            }
            /*
            //debug code
            if (neighbors.neighborInfluence1 > 0 && neighbors.neighborInfluence2 > 0 && neighbors.neighborInfluence3 > 0 && neighbors.neighborInfluence4 > 0)
            {
                if (neighbors.neighborEntities1 == neighbors.neighborEntities2)
                    Debug.LogError(neighbors.neighborEntities1 + "=" + neighbors.neighborEntities2);
                if (neighbors.neighborEntities2 == neighbors.neighborEntities3)
                    Debug.LogError(neighbors.neighborEntities2 + "=" + neighbors.neighborEntities3);
                if (neighbors.neighborEntities3 == neighbors.neighborEntities4)
                    Debug.LogError(neighbors.neighborEntities3 + "=" + neighbors.neighborEntities4);

            }*/
        }
    }
