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
    public const int numBodies = 2500;
    public int currentNeighborCheckIndex = 0;
    [SerializeField] private Mesh bodyMesh;
    [SerializeField] private Material bodyMaterial;
    public BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private EntityArchetype orbitalBodyArchetype;
    private EntityManager entityManager = null;
    public EntityCommandBuffer.Concurrent CommandBuffer;
    void Start()
    {
        entityManager = World.Active.EntityManager;
        orbitalBodyArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(LocalToWorld),
            //typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(VelocityComponent),
            typeof(MassComponent),
            typeof(OrbitalBodyNeighborComponent),
            typeof(OrbitalBodyTagComponent)
        );
        m_EntityCommandBufferSystem = World.Active.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        setupOrbitalBodies();
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation),typeof(Scale), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent));
        DoOrbitalBodyGravityNeighborUpdate(q);
        //DoOrbitalBodyGravity(q);
        DoOrbitalBodyMovement(q);
    }

    private void FixedUpdate()
    {
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation), typeof(Scale), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent));
        DoOrbitalBodyGravityNeighborUpdate(q);
        //DoOrbitalBodyGravity(q);
        DoOrbitalBodyMovement(q);
        DoOrbitalBodyObjectRenderingUpdate(q);




    }

    public void spawnEntity(Transform trans)
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(1, Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype, bodiesTemp);
        entityManager.SetComponentData(bodiesTemp[0], new Translation { Value = trans.position });
        entityManager.SetComponentData(bodiesTemp[0], new VelocityComponent { Value = trans.forward*0.1f });
        entityManager.SetComponentData(bodiesTemp[0], new Scale { Value = 0.1f });
        entityManager.SetComponentData(bodiesTemp[0], new MassComponent { Value = (0.1f + UnityEngine.Random.Range(0.0f, 0.1f)) });
        entityManager.SetSharedComponentData(bodiesTemp[0], new RenderMesh { mesh = bodyMesh, material = bodyMaterial });
        bodiesTemp.Dispose();
    }


    private void setupOrbitalBodies()
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(numBodies,Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype,bodiesTemp);

        entityManager.SetComponentData(bodiesTemp[0], new Translation { Value = float3.zero });
        entityManager.SetComponentData(bodiesTemp[0], new VelocityComponent { Value = float3.zero });
        entityManager.SetComponentData(bodiesTemp[0], new Scale { Value = 1.0f });
        entityManager.SetComponentData(bodiesTemp[0], new MassComponent { Value = (10.0f + UnityEngine.Random.Range(0.0f, 100.0f)) });
        entityManager.SetSharedComponentData(bodiesTemp[0], new RenderMesh { mesh = bodyMesh, material = bodyMaterial });

        for (int i = 1; i < numBodies; i++)
        {
            Vector3 pos = UnityEngine.Random.onUnitSphere * 1000.0f;
            pos.y *= 0.01f;
            entityManager.SetComponentData(bodiesTemp[i],new Translation{Value = pos});
            entityManager.SetComponentData(bodiesTemp[i],new VelocityComponent{Value =math.normalize( new float3(-pos.z, 0,pos.x)*100.0f)/((pos.magnitude/100.0f)+1.0f) });
            entityManager.SetComponentData(bodiesTemp[i],new Scale { Value = 1.0f });
            entityManager.SetComponentData(bodiesTemp[i],new MassComponent{Value = (1.0f+UnityEngine.Random.Range(0.0f,1.0f))});
            entityManager.SetSharedComponentData(bodiesTemp[i],new RenderMesh{mesh = bodyMesh, material = bodyMaterial});
        }

        bodiesTemp.Dispose();
    }

    private void DoOrbitalBodyMovement(EntityQuery q)
    {
        
        var job = new OrbitalBodyMoveJob()
        {

        };
        Unity.Jobs.JobHandle jh = job.Schedule(q);

        jh.Complete();
    }

    private void DoOrbitalBodyGravity(EntityQuery q)
    {
        
        var job = new OrbitalBodyGravityJob()
        {
           
        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);
        jh.Complete();
    }
    
    private void DoOrbitalBodyGravityNeighborUpdate(EntityQuery q)
    {
        
        NativeArray<Entity> entities = q.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> translations = q.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<VelocityComponent> velocities = q.ToComponentDataArray<VelocityComponent>(Allocator.TempJob);
        NativeArray<MassComponent> massComponents = q.ToComponentDataArray<MassComponent>(Allocator.TempJob);
        

        var job = new OrbitalBodyGravityNeighborJob()
        {
            CommandBuffer = CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            allBodies = entities,
            allTranslations = translations,
            allVelocities = velocities,
            allMasses = massComponents
        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jh);
        jh.Complete();
        entities.Dispose();
        translations.Dispose();
        velocities.Dispose();
        massComponents.Dispose();
    }
    
    private void DoOrbitalBodyObjectRenderingUpdate(EntityQuery q)
    {
        var job = new OrbitalBodyRenderingUpdateJob()
        {

        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);
        jh.Complete();
    }
}
