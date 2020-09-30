using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CustomTransform.ECS
{
    public class TransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //Обновление мировых позиций для "верхних" сущностей
            Entities
                .WithNone<ParentComponent, PreviousParent>()
                .ForEach((Entity entity, ref LocalToWorld localToWorld, in TransformComponent transform) => //change ref Trans to in Trans
                    {
                        var scale = float4x4.Scale(transform.scale);
                        localToWorld = new LocalToWorld
                        {
                            Value = math.mul(new float4x4(transform.rotation, transform.Position3), scale)
                        };
                        UpdateChildTransform(entity, localToWorld);
                    }).WithoutBurst().WithStructuralChanges().Run();


            //Обновление мировых позиций для сущностей, которые потеряли родителей
            Entities
                .WithNone<ParentComponent>()
                .ForEach((Entity entity, ref LocalToWorld localToWorld, ref TransformComponent transform, in PreviousParent parent) =>
                    {
                        var scale = math.length(localToWorld.Value.c0.xyz);
                        transform = new TransformComponent
                        {
                            position = new float2(localToWorld.Value.c3.x, localToWorld.Value.c3.y),
                            rotation = new quaternion(localToWorld.Value),
                            scale = scale
                        };
                        UpdateChildTransform(entity, localToWorld);
                    }).WithoutBurst().WithStructuralChanges().Run();
        }

        //Обновление мировых позиций для сущностей с родителями
        private void UpdateChildTransform(Entity parent, in LocalToWorld parentLTW)
        {
            DynamicBuffer<Child> children;
            if (!EntityManager.HasComponent<Child>(parent) || (children = EntityManager.GetBuffer<Child>(parent)).Length == 0)
                return;

            foreach (Entity child in children)
            {
                if (!EntityManager.HasComponent<TransformComponent>(child) || !EntityManager.HasComponent<LocalToWorld>(child))
                    continue;

                var transform = EntityManager.GetComponentData<TransformComponent>(child);
                var localToWorld = EntityManager.GetComponentData<LocalToWorld>(child);

                var scale = float4x4.Scale(transform.scale);
                var localToParent = math.mul(new float4x4(transform.rotation, transform.Position3), scale);

                if (!EntityManager.HasComponent<PreviousParent>(child)) //Только появился родитель - перенос из мира в локал
                {
                    var parentScale = math.length(parentLTW.Value.c0.xyz);
                    var newLocalToParent = math.mul(math.inverse(parentLTW.Value), localToParent);

                    transform = new TransformComponent
                    {
                        position = new float2(newLocalToParent.c3.x, newLocalToParent.c3.y),
                        rotation = new quaternion(newLocalToParent),
                        scale = transform.scale / parentScale
                    };
                }
                else if (EntityManager.GetComponentData<PreviousParent>(child).Value == parent) //Родитель тот же, что и на прошлом фрейме
                {
                    localToWorld.Value = math.mul(parentLTW.Value, localToParent);
                }
                else //Сменился родитель - перенос из локал1 в локал2
                {
                    var parentScale = math.length(parentLTW.Value.c0.xyz);

                    var newLocalToParent = math.mul(math.inverse(parentLTW.Value), localToWorld.Value);
                    var worldScale = math.length(localToWorld.Value.c0.xyz);

                    transform = new TransformComponent
                    {
                        position = new float2(newLocalToParent.c3.x, newLocalToParent.c3.y),
                        rotation = new quaternion(newLocalToParent),
                        scale = worldScale / parentScale
                    };
                }
                EntityManager.SetComponentData(child, transform);
                EntityManager.SetComponentData(child, localToWorld);

                UpdateChildTransform(child, localToWorld);
            }
        }
    }
}
