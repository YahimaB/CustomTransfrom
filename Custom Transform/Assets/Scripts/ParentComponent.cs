using System;
using Unity.Entities;
using Unity.Transforms;

namespace CustomTransform.ECS
{
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

    [Serializable]
    public struct Child : ISystemStateBufferElementData, IBufferElementData
    {
        public Entity Value;

        public static implicit operator Entity(Child e)
        {
            return e.Value;
        }

        public static implicit operator Child(Entity e)
        {
            return new Child { Value = e };
        }
    }
}
