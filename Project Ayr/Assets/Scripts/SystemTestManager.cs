using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using TMPro;

public class SystemTestManager : MonoBehaviour
{
    public const int numBodies = 1000;
    public int currentNeighborCheckIndex = 0;
    [SerializeField] private Mesh bodyMesh;
    [SerializeField] private Material bodyMaterial;
    [SerializeField] private Gradient colorGradient;
    public GameObject handText = null;
    private string handTextStart = "";
    public BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private EntityArchetype orbitalBodyArchetype;
    private EntityManager entityManager = null;
    public EntityCommandBuffer.Concurrent CommandBuffer;
    private int intermittentUpdate = 0;
    private int intermittentUpdateFreq = 10;
    private int framesWithGoodPlanet = 0;
    private float solarRadiation = 0.0f;

    void Start()
    {
        entityManager = World.Active.EntityManager;
        handTextStart = handText.GetComponent<TMPro.TextMeshProUGUI>().text;
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
        entityManager = World.Active.EntityManager;
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

        if(framesWithGoodPlanet>1000)
        {
            handText.GetComponent<TMPro.TextMeshProUGUI>().text = "You win.";
        }
        else if(framesWithGoodPlanet > 1)
        {
            handText.GetComponent<TMPro.TextMeshProUGUI>().text = "Ticks until winning: "+(1000-framesWithGoodPlanet);
        }
        else
        {
            handText.GetComponent<TMPro.TextMeshProUGUI>().text = handTextStart;
        }

    }

    private void updateTemperatureColoration(EntityQuery q)
    {
        NativeArray<Entity> entities = q.ToEntityArray(Allocator.TempJob);
        
        NativeArray<TemperatureComponent> temperatureComponents = q.ToComponentDataArray<TemperatureComponent>(Allocator.TempJob);
        NativeArray<MassComponent> massComponents = q.ToComponentDataArray<MassComponent>(Allocator.TempJob);
        NativeArray<Translation> translations = q.ToComponentDataArray<Translation>(Allocator.TempJob);
        bool found = false;
        
        for (int i = 0; i < entities.Length; i++)
        {
            RenderMesh m;
            if(entityManager.HasComponent<RenderMesh>(entities[i]))
                m = entityManager.GetSharedComponentData<RenderMesh>(entities[i]);
            else
            {
                continue;
            }

            
            float temp = temperatureComponents[i].Value;
            float rad = massComponents[i].Value * temp;
            
            if (solarRadiation < rad)
                solarRadiation = rad;

            //float dist = math.lengthsq(translations[i].Value); //inverse square mofos
            //temperatureComponents[i].Value += rad / dist; //seems we can't update this yet, TODO do this in a system.

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
            
            if(massComponents[i].Value>10.0f&&temp<373.0f&&temp>273.0f)
            {
                found = true;
            }
            
            
        }

        if (found)
            framesWithGoodPlanet++;
        else
            framesWithGoodPlanet = 0;

        //Debug.Log(entities.Length);
        temperatureComponents.Dispose();
        translations.Dispose();
        massComponents.Dispose();
        entities.Dispose();
        
        
    }

    public void spawnEntity(Transform trans, float temp)
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(1, Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype, bodiesTemp);
        entityManager.SetComponentData(bodiesTemp[0], new Translation { Value = trans.position + (trans.forward * UnityEngine.Random.Range(5.0f, 10.0f)) + (UnityEngine.Random.insideUnitSphere * 1.0f) });
        entityManager.SetComponentData(bodiesTemp[0], new VelocityComponent { Value = (trans.forward * UnityEngine.Random.Range(0.0f, 0.01f))+(UnityEngine.Random.insideUnitSphere*0.000001f) });
        entityManager.SetComponentData(bodiesTemp[0], new Scale { Value = 0.01f });
        entityManager.SetComponentData(bodiesTemp[0], new MassComponent { Value = (0.1f + UnityEngine.Random.Range(0.0f, 0.1f)) });
        entityManager.SetComponentData(bodiesTemp[0], new TemperatureComponent { Value = (temp + UnityEngine.Random.Range(0.0f, 1.0f)) });
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
        entityManager.SetComponentData(bodiesTemp[0], new MassComponent { Value = (100.0f + UnityEngine.Random.Range(0.0f, 100.0f)) });
        entityManager.SetComponentData(bodiesTemp[0], new TemperatureComponent { Value = (3000.0f + UnityEngine.Random.Range(-300.0f, 300.0f)) });
        entityManager.SetSharedComponentData(bodiesTemp[0], new RenderMesh { mesh = bodyMesh, material = new Material(bodyMaterial) });

        for (int i = 1; i < numBodies; i++)
        {
            Vector3 pos = UnityEngine.Random.onUnitSphere * 1000.0f;
            pos.y *= 0.01f;
            entityManager.SetComponentData(bodiesTemp[i],new Translation{Value = pos});
            entityManager.SetComponentData(bodiesTemp[i],new VelocityComponent{Value =math.normalize( new float3(-pos.z, 0,pos.x)*100.0f)/((pos.magnitude/10.0f)+1.0f) });
            entityManager.SetComponentData(bodiesTemp[i],new Scale { Value = 1.0f });
            entityManager.SetComponentData(bodiesTemp[i],new MassComponent{Value = (10.0f+UnityEngine.Random.Range(0.0f,10.0f))});
            entityManager.SetComponentData(bodiesTemp[i], new TemperatureComponent { Value = (0.0f + UnityEngine.Random.Range(0.0f, 3.0f)) });
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

    public void serializeWorld()
    {      
        EntityQuery eq = World.Active.EntityManager.CreateEntityQuery(typeof(RenderMesh));
        World.Active.EntityManager.RemoveComponent<RenderMesh>(eq);
        //NativeArray<Entity> entities = eq.ToEntityArray(Allocator.TempJob);
        //for (int i = 0; i < entities.Length; i++)
        //{
        //    World.Active.EntityManager.RemoveComponent(entities[i], typeof(RenderMesh));
        //}
        Debug.Log("Shared Components Remaining During Save: "+entityManager.GetSharedComponentCount());
        

        WorldSerializer ws = new WorldSerializer();
        ws.doSave();        
        //entities.Dispose();              
        World.Active.EntityManager.DestroyEntity(eq);
        eq.Dispose();
        deserializeWorld();
    }

    public void deserializeWorld()
    {
        EntityQuery eq = World.Active.EntityManager.CreateEntityQuery(typeof(RenderMesh));
        World.Active.EntityManager.DestroyEntity(eq);
        WorldSerializer ws = new WorldSerializer();
        ws.doLoad();
        EntityQuery eq2 = World.Active.EntityManager.CreateEntityQuery(typeof(OrbitalBodyTagComponent));
        NativeArray<Entity> entities = eq2.ToEntityArray(Allocator.TempJob);
        World.Active.EntityManager.AddComponent(entities, typeof(RenderMesh));
        for (int i = 0; i < entities.Length; i++)
        {
            entityManager.SetSharedComponentData(entities[i], new RenderMesh { mesh = bodyMesh, material = new Material(bodyMaterial) });
        }
        entities.Dispose();
        eq.Dispose();
        eq2.Dispose();
    }
}
