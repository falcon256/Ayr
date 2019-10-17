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
            typeof(MassComponent)
        );

        setupOrbitalBodies();
    }

    private void setupOrbitalBodies()
    {
        NativeArray<Entity> bodiesTemp = new NativeArray<Entity>(numBodies,Allocator.Temp);
        entityManager.CreateEntity(orbitalBodyArchetype,bodiesTemp);

        for(int i = 0; i < numBodies; i++)
        {
            entityManager.SetComponentData(bodiesTemp[i],new Translation{Value = new float3(0,i*1.0f,0)});
            entityManager.SetComponentData(bodiesTemp[i],new VelocityComponent{Value = new float3(i*0.1f,0,0)});
            entityManager.SetComponentData(bodiesTemp[i],new MassComponent{Value = 1});
            entityManager.SetSharedComponentData(bodiesTemp[i],new RenderMesh{mesh = bodyMesh, material = bodyMaterial});
        }

        bodiesTemp.Dispose();
    }

    private void DoOrbitalBodyMovement()
    {

    }

    private void DoOrbitalBodyGravity()
    {

    }
    
    private void DoOrbitalBodyGravityNeighborUpdate()
    {

    }

}
