using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace CustomTransform.ECS
{
    public class Testing : MonoBehaviour
    {
        [SerializeField]
        private Mesh mesh;

        [SerializeField]
        private Material material;

        EntityManager entityManager;
        EntityArchetype entityArchetype;

        // Start is called before the first frame update
        void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityArchetype = entityManager.CreateArchetype(
                typeof(TransformComponent),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(LocalToWorld)
            );

            StartCoroutine(CommandExecution());
        }

        private IEnumerator CommandExecution()
        {
            //ScalingShow
            var entity0 = SpawnEntity(-8f, 2f);
            var entity1 = SpawnEntity(-6f, 3f);
            Parentize(entity1, entity0);

            yield return new WaitForSeconds(3f);
            var enlarger = StartCoroutine(Enlarger(entity0));

            yield return new WaitForSeconds(8f);
            StopCoroutine(enlarger);
            Parentize(entity1, null);

            entityManager.DestroyEntity(entity0);

            yield return new WaitForSeconds(3f);
            var entity2 = SpawnEntity(-4f, 2f);
            Parentize(entity1, entity2);
            var minimizer = StartCoroutine(Minimizer(entity2));

            yield return new WaitForSeconds(5f);
            StopCoroutine(minimizer);

            //RotationShow
            var entity3 = SpawnEntity(3f, 2f);
            StartCoroutine(Rotator(entity3));

            var entity4 = SpawnEntity(6f, 2f);
            StartCoroutine(Rotator(entity4));

            var entity5 = SpawnEntity(8f, 2f);
            StartCoroutine(Rotator(entity5));

            yield return new WaitForSeconds(3f);
            Parentize(entity5, entity4);

            yield return new WaitForSeconds(3f);
            Parentize(entity4, entity3);

            yield return new WaitForSeconds(3f);
            Parentize(entity5, entity3);

            yield return new WaitForSeconds(3f);
            Parentize(entity4, null);

            yield return new WaitForSeconds(3f);
            Parentize(entity5, null);

            yield return null;
        }

        private IEnumerator Enlarger(Entity entity)
        {
            Debug.Log($"{entity} is now enlargening");
            while (true)
            {
                yield return new WaitForSeconds(1f);
                var transform = entityManager.GetComponentData<TransformComponent>(entity);
                transform.scale += 0.1f;
                entityManager.SetComponentData(entity, transform);
            }
        }

        private IEnumerator Minimizer(Entity entity)
        {
            Debug.Log($"{entity} is now minimizing");
            while (true)
            {
                yield return new WaitForSeconds(1f);
                var transform = entityManager.GetComponentData<TransformComponent>(entity);
                transform.scale -= 0.1f;
                entityManager.SetComponentData(entity, transform);
            }
        }

        private IEnumerator Rotator(Entity entity)
        {
            Debug.Log($"{entity} is now rotating");
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                var transform = entityManager.GetComponentData<TransformComponent>(entity);
                transform.Rotate(0, 0, 5);
                entityManager.SetComponentData(entity, transform);
            }
        }

        private Entity SpawnEntity(float x, float y)
        {
            var entity = entityManager.CreateEntity(entityArchetype);
            entityManager.SetComponentData(entity, new TransformComponent { position = new Vector2(x, y), scale = 1f });
            entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material });
            entityManager.SetComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });
            return entity;
        }

        private void Parentize(Entity child, Entity? parent)
        {
            if (parent == null)
            {
                entityManager.RemoveComponent<ParentComponent>(child);
                Debug.Log($"{child} has no parent anymore");
            }
            else
            {
                if (!entityManager.HasComponent<ParentComponent>(child))
                    entityManager.AddComponent<ParentComponent>(child);
                entityManager.SetComponentData(child, new ParentComponent { Value = (Entity)parent });
                Debug.Log($"{child} is now child of {parent}");
            }
        }

    }
}
