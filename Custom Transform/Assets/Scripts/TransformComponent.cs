using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomTransform.ECS
{
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    [GenerateAuthoringComponent]
    public struct TransformComponent : IComponentData
    {
        public float2 position;
        public quaternion rotation;
        public float scale;

        public float3 Position3 => new float3(position.x, position.y, 0.0f);

        public void Translate(float x, float y)
        {
            position.x += x;
            position.y += y;
        }

        public void Rotate(float x, float y, float z)
        {
            Quaternion rot = rotation;
            rot.eulerAngles += new Vector3(x, y, z);
            rotation = rot;
        }
    }
}
