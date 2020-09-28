using CustomTransform.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CustomTransform.ECS
{
    [UpdateAfter(typeof(TransformSystem))]
    public class ParentSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //Удаление остаточной инфы о родителе
            Entities
                .WithAll<PreviousParent>()
                .WithNone<ParentComponent>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    EntityManager.RemoveComponent<PreviousParent>(entity);
                }).WithoutBurst().WithStructuralChanges().Run();

            //Обновление инфы по родителю
            Entities
                .ForEach(
                (Entity entity, int entityInQueryIndex, in ParentComponent parent, in PreviousParent previousParent) =>
                {
                    if (parent.Value != previousParent.Value)
                    {
                        EntityManager.SetComponentData(entity, new PreviousParent { Value = parent.Value });
                    }
                }).WithoutBurst().Run();

            //Добавление инфы по новому родителю
            Entities
                .WithNone<PreviousParent>()
                .ForEach((Entity entity, int entityInQueryIndex, in ParentComponent parent) =>
                {
                    EntityManager.AddComponent(entity, typeof(PreviousParent));
                    EntityManager.SetComponentData(entity, new PreviousParent { Value = parent.Value });
                }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}
