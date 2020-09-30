using CustomTransform.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace CustomTransform.ECS
{
    [UpdateBefore(typeof(TransformSystem))]
    public class ParentSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //Добавление инфы по новому родителю
            Entities
                .WithNone<PreviousParent>()
                .ForEach((Entity entity, in ParentComponent parent) =>
                {
                    DynamicBuffer<Child> buffer;
                    if (!EntityManager.HasComponent<Child>(parent.Value))
                    {
                        buffer = EntityManager.AddBuffer<Child>(parent.Value);
                        buffer.Add(entity);
                    }
                    else
                    {
                        buffer = EntityManager.GetBuffer<Child>(parent.Value);
                        if (buffer.Reinterpret<Entity>().AsNativeArray().Contains(entity))
                        {
                            EntityManager.AddComponent(entity, typeof(PreviousParent));
                            EntityManager.SetComponentData(entity, new PreviousParent { Value = parent.Value });
                        }
                        else
                        {
                            buffer.Add(entity);
                        }
                    }
                }).WithoutBurst().WithStructuralChanges().Run();

            //Удаление остаточной инфы о родителе
            Entities
                .WithNone<ParentComponent>()
                .ForEach((Entity entity, in PreviousParent previousParent) =>
                {
                    bool canRemovePrevParent = FreePreviousParent(entity, previousParent.Value);

                    if (canRemovePrevParent)
                        EntityManager.RemoveComponent<PreviousParent>(entity);

                }).WithoutBurst().WithStructuralChanges().Run();

            //Обновление инфы по родителю
            Entities
                .ForEach(
                (Entity entity, in ParentComponent parent, in PreviousParent previousParent) =>
                {
                    if (parent.Value != previousParent.Value)
                    {
                        bool canRemovePrevParent = FreePreviousParent(entity, previousParent.Value);

                        if (canRemovePrevParent)
                        {
                            EntityManager.SetComponentData(entity, new PreviousParent { Value = parent.Value });
                        }
                        else
                        {
                            DynamicBuffer<Child> buffer;
                            if (!EntityManager.HasComponent<Child>(parent.Value))
                                buffer = EntityManager.AddBuffer<Child>(parent.Value);
                            else
                                buffer = EntityManager.GetBuffer<Child>(parent.Value);

                            buffer.Add(entity);
                        }
                    }
                }).WithoutBurst().Run();

            //Удаление остаточной инфы о детях
            Entities
                .ForEach((Entity entity, DynamicBuffer<Child> buffer) =>
                {
                    if (buffer.Length == 0)
                        EntityManager.RemoveComponent<Child>(entity);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private bool FreePreviousParent(Entity child, Entity previousParent)
        {
            if (EntityManager.Exists(previousParent) && EntityManager.HasComponent<Child>(previousParent))
            {
                DynamicBuffer<Child> buffer = EntityManager.GetBuffer<Child>(previousParent);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == child)
                    {
                        buffer.RemoveAt(i);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
