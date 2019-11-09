using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
public class SystemTestManager : MonoBehaviour
{
    public const int numBodies = 1000;
    public int currentNeighborCheckIndex = 0;
    [SerializeField] private Mesh bodyMesh;
    [SerializeField] private Material bodyMaterial;
    
    private EntityArchetype orbitalBodyArchetype;
    private EntityManager entityManager = null;

    void Start()
    {
        entityManager = World.Active.EntityManager;
        orbitalBodyArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(LocalToWorld),
            //typeof(Rotation),
            //typeof(Scale),
            typeof(RenderMesh),
            typeof(VelocityComponent),
            typeof(MassComponent),
            typeof(OrbitalBodyNeighborComponent),
            typeof(OrbitalBodyTagComponent)
        );

        setupOrbitalBodies();
        DoOrbitalBodyGravityNeighborUpdate();
        DoOrbitalBodyGravity();
        DoOrbitalBodyMovement();
    }

    private void FixedUpdate()
    {
        DoOrbitalBodyGravityNeighborUpdate();
        DoOrbitalBodyGravity();
        DoOrbitalBodyMovement();
    }

    private void setupOrbitalBodies()
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(numBodies,Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype,bodiesTemp);
        

        for(int i = 0; i < numBodies; i++)
        {
            Vector3 pos = UnityEngine.Random.insideUnitSphere * 100.0f;
            pos.y *= 0.1f;
            entityManager.SetComponentData(bodiesTemp[i],new Translation{Value = UnityEngine.Random.insideUnitSphere * 100.0f });
            //entityManager.SetComponentData(bodiesTemp[i],new VelocityComponent{Value = new float3(pos.z * 0.001f, 0,pos.x*0.001f)});
            entityManager.SetComponentData(bodiesTemp[i], new VelocityComponent { Value = new float3(0,0,0) });
            entityManager.SetComponentData(bodiesTemp[i],new MassComponent{Value = 1});
            entityManager.SetSharedComponentData(bodiesTemp[i],new RenderMesh{mesh = bodyMesh, material = bodyMaterial});
        }

        bodiesTemp.Dispose();
    }

    private void DoOrbitalBodyMovement()
    {
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent));
        var job = new OrbitalBodyMoveJob()
        {

        };
        Unity.Jobs.JobHandle jh = job.Schedule(q);

        jh.Complete();
    }

    private void DoOrbitalBodyGravity()
    {
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent));
        NativeArray<Entity> entities = q.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> translations = q.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<MassComponent> massComponents = q.ToComponentDataArray<MassComponent>(Allocator.TempJob);


        var job = new OrbitalBodyGravityJob()
        {
            allBodies = entities,
            allTranslations = translations,
            allMasses = massComponents
        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);

        jh.Complete();
        entities.Dispose();
        translations.Dispose();
        massComponents.Dispose();
    }
    
    private void DoOrbitalBodyGravityNeighborUpdate()
    {
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent),typeof(Translation),typeof(MassComponent),typeof(VelocityComponent),typeof(OrbitalBodyTagComponent));
        NativeArray<Entity> entities = q.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> translations = q.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<MassComponent> massComponents = q.ToComponentDataArray<MassComponent>(Allocator.TempJob);
        

        var job = new OrbitalBodyGravityNeighborJob()
        {
            allBodies = entities,
            allTranslations = translations,
            allMasses = massComponents
        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);
        
        jh.Complete();
        entities.Dispose();
        translations.Dispose();
        massComponents.Dispose();
    }

}
