using System;
using Unity.Entities;
using UnityEngine;

namespace FatVfx
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct FatVfxCleanupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbS = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbS.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (runtimeRo, entity) in SystemAPI.Query<RefRO<FatVfxDataRuntime>>().WithNone<FatVfxData>().WithEntityAccess())
            {
                var runtime = runtimeRo.ValueRO;
                if (runtime.runtimeEffect.IsValid())
                {
                    GameObject.Destroy(runtime.runtimeEffect.Value.gameObject);
                    ecb.RemoveComponent<FatVfxDataRuntime>(entity);
                }
            }
        }
    }
}