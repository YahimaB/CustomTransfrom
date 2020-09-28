using System;
using Unity.Entities;
using Unity.Transforms;

[Serializable]
[WriteGroup(typeof(LocalToWorld))]
public struct ParentComponent : IComponentData
{
    public Entity Value;
}

[Serializable]
public struct PreviousParent : ISystemStateComponentData, IComponentData
{
    public Entity Value;
}
