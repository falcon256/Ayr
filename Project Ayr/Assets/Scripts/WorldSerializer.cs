using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities.Serialization;


public class WorldSerializer 
{
    //GameObject gos;
    public void doSave()
    {
        
        int numWorlds = World.AllWorlds.Count;
        World[] worlds = new World[numWorlds];
        World.AllWorlds.CopyTo(worlds,0);
        for(int i = 0; i < numWorlds; i++)
        {
            doWorldSerialization(worlds[i], i);
        }
    }

    public void doLoad()
    {
        World.Active.EntityManager.PrepareForDeserialize();
        doWorldDeserialization(Application.dataPath + "/save/" + 0 + ".wrld");
    }

    public void doWorldSerialization(World world, int index)
    {
        string path = Application.dataPath + "/save/";

        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);

        string targetLocation = path + index + ".wrld";
        BinaryWriter writer = new StreamBinaryWriter(targetLocation);   
        int[] serializedComponents;      
        //Unity.Entities.Serialization.SerializeUtilityHybrid.Serialize(World.Active.EntityManager, writer, out gos);
        
        Unity.Entities.Serialization.SerializeUtility.SerializeWorld(world.EntityManager, writer, out serializedComponents);
        if (serializedComponents.Length > 0)
        {
            Debug.LogError("You are trying to serialize shared fekking components!");

            Unity.Entities.Serialization.SerializeUtility.SerializeSharedComponents(world.EntityManager, writer, serializedComponents);
        }
        //Debug.Log("" + serializedComponents.Length);
        
        writer.Dispose();

    }

    public World doWorldDeserialization(string target)
    {
        BinaryReader reader = new StreamBinaryReader(target);
        //Unity.Entities.Serialization.SerializeUtilityHybrid.Deserialize(World.Active.EntityManager, reader, gos);
        ExclusiveEntityTransaction transaction = World.Active.EntityManager.BeginExclusiveEntityTransaction();
        //int numSC = Unity.Entities.Serialization.SerializeUtility.DeserializeSharedComponents(World.Active.EntityManager, reader);
        //Debug.Log("NumSC=" + numSC);
        //Unity.Entities.Serialization.SerializeUtility.DeserializeWorld(transaction, reader, numSC);
        Unity.Entities.Serialization.SerializeUtility.DeserializeWorld(transaction, reader, 0);
        World.Active.EntityManager.EndExclusiveEntityTransaction();
        return World.Active;
    }

}
