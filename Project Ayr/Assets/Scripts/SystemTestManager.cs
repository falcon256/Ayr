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
    [SerializeField] private Gradient colorGradient;

    public BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private EntityArchetype orbitalBodyArchetype;
    private EntityManager entityManager = null;
    public EntityCommandBuffer.Concurrent CommandBuffer;
    private int intermittentUpdate = 0;
    private int intermittentUpdateFreq = 100;
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
            typeof(OrbitalBodyTagComponent),
            typeof(TemperatureComponent)
        );


        m_EntityCommandBufferSystem = World.Active.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        setupOrbitalBodies();
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation),typeof(Scale), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent), typeof(TemperatureComponent));
        DoOrbitalBodyGravityNeighborUpdate(q);
        DoOrbitalBodyMovement(q);
        q.Dispose();
    }

    private void FixedUpdate()
    {
        EntityQuery q = entityManager.CreateEntityQuery(typeof(OrbitalBodyNeighborComponent), typeof(Translation), typeof(Scale), typeof(MassComponent), typeof(VelocityComponent), typeof(OrbitalBodyTagComponent), typeof(TemperatureComponent));
        DoOrbitalBodyGravityNeighborUpdate(q);
        DoOrbitalBodyMovement(q);
        DoOrbitalBodyObjectRenderingUpdate(q);
        if (intermittentUpdate++ >= intermittentUpdateFreq)
        {
            
            updateTemperatureColoration(q);
            intermittentUpdate=0;
        }
        q.Dispose();


    }

    private void updateTemperatureColoration(EntityQuery q)
    {
        NativeArray<Entity> entities = q.ToEntityArray(Allocator.TempJob);
        
        NativeArray<TemperatureComponent> temperatureComponents = q.ToComponentDataArray<TemperatureComponent>(Allocator.TempJob);
        
        for(int i = 0; i < entities.Length; i++)
        {
            RenderMesh m = entityManager.GetSharedComponentData<RenderMesh>(entities[i]);


            float temp = temperatureComponents[i].Value;
            
            if (temp> 373.15f)
            {
                Color32 emission = new Color32((byte)math.clamp(((temp - 1000.0f) * 0.1f), 0, 255), (byte)math.clamp(((temp - 1500.0f) * 0.15f), 0, 255), (byte)math.clamp(((temp - 2000.0f) * 0.2f), 0, 255), 255);
                m.material.EnableKeyword("_EMISSION");
                m.material.color = Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(255, 255, 255, 255), math.clamp(temp / 1000.0f, 0, 1.0f));
                m.material.SetColor("_EmissionColor", emission);

            }
            else
            {
                m.material.color = colorGradient.Evaluate(temp / 373.15f);
            }
            
            
            
        }
        //Debug.Log(entities.Length);
        temperatureComponents.Dispose();
        entities.Dispose();
        
        
    }

    public void spawnEntity(Transform trans)
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(1, Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype, bodiesTemp);
        entityManager.SetComponentData(bodiesTemp[0], new Translation { Value = trans.position });
        entityManager.SetComponentData(bodiesTemp[0], new VelocityComponent { Value = trans.forward*0.1f });
        entityManager.SetComponentData(bodiesTemp[0], new Scale { Value = 0.1f });
        entityManager.SetComponentData(bodiesTemp[0], new MassComponent { Value = (0.1f + UnityEngine.Random.Range(0.0f, 0.1f)) });
        entityManager.SetComponentData(bodiesTemp[0], new TemperatureComponent { Value = (300.0f + UnityEngine.Random.Range(-300.0f, 300.0f)) });
        entityManager.SetSharedComponentData(bodiesTemp[0], new RenderMesh { mesh = bodyMesh, material = new Material(bodyMaterial) });
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
        entityManager.SetComponentData(bodiesTemp[0], new TemperatureComponent { Value = (300.0f + UnityEngine.Random.Range(-300.0f, 300.0f)) });
        entityManager.SetSharedComponentData(bodiesTemp[0], new RenderMesh { mesh = bodyMesh, material = new Material(bodyMaterial) });

        for (int i = 1; i < numBodies; i++)
        {
            Vector3 pos = UnityEngine.Random.onUnitSphere * 1000.0f;
            pos.y *= 0.01f;
            entityManager.SetComponentData(bodiesTemp[i],new Translation{Value = pos});
            entityManager.SetComponentData(bodiesTemp[i],new VelocityComponent{Value =math.normalize( new float3(-pos.z, 0,pos.x)*100.0f)/((pos.magnitude/100.0f)+1.0f) });
            entityManager.SetComponentData(bodiesTemp[i],new Scale { Value = 1.0f });
            entityManager.SetComponentData(bodiesTemp[i],new MassComponent{Value = (1.0f+UnityEngine.Random.Range(0.0f,1.0f))});
            entityManager.SetComponentData(bodiesTemp[i], new TemperatureComponent { Value = (300.0f + UnityEngine.Random.Range(-300.0f, 300.0f)) });
            entityManager.SetSharedComponentData(bodiesTemp[i],new RenderMesh{mesh = bodyMesh, material = new Material(bodyMaterial) });
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
        NativeArray<TemperatureComponent> tempComponents = q.ToComponentDataArray<TemperatureComponent>(Allocator.TempJob);

        var job = new OrbitalBodyGravityNeighborJob()
        {
            CommandBuffer = CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            allBodies = entities,
            allTranslations = translations,
            allVelocities = velocities,
            allMasses = massComponents,
            allTemperatures =tempComponents
        };

        Unity.Jobs.JobHandle jh = job.Schedule(q);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jh);
        jh.Complete();
        entities.Dispose();
        translations.Dispose();
        velocities.Dispose();
        massComponents.Dispose();
        tempComponents.Dispose();
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
