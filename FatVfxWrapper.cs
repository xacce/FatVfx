using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.VFX;

namespace FatVfx
{
    public partial struct FatVfxDataRuntime : ICleanupComponentData
    {
        public int index;
        public UnityObjectRef<VisualEffect> runtimeEffect;
        public bool IsReady() => index != -1;
    }

    public partial struct FatVfxData : IComponentData
    {
        public FixedList32Bytes<int> buffersSize;
        public FixedList32Bytes<int> buffersCapacity;
        public UnityObjectRef<VisualEffectAsset> vfx;
    }


    public partial class FatVfxWrapper : IDisposable
    {
        public static readonly int count1 = Shader.PropertyToID("bufferCount_0");
        public static readonly int count2 = Shader.PropertyToID("bufferCount_1");
        public static readonly int initializedEvt = Shader.PropertyToID("Initialized");

        private class Registered
        {
            public VisualEffect vfx;
            public GraphicsBuffer[] b1 = Array.Empty<GraphicsBuffer>();
            public EntityQuery query;
        }

        private Registered[] _registereds;
        private NativeQueue<int> _free;


        public FatVfxWrapper(int buffersCapacity = 100)
        {
            _registereds = new Registered[buffersCapacity];
            _free = new NativeQueue<int>(Allocator.Persistent);
            for (int i = 0; i < _registereds.Length; i++)
            {
                _free.Enqueue(i);
            }
        }


        public void Dispose()
        {
            _free.Dispose();
            for (int i = 0; i < _registereds.Length; i++)
            {
                if (_registereds[i] != null && _registereds[i].b1.Length > 0)
                {
                    for (int j = 0; j < _registereds[i].b1.Length; j++)
                    {
                        _registereds[i].b1[j].Release();
                    }
                }
            }
        }

        public void Update<T1, T2>(SystemBase state, EntityQuery query) where T1 : unmanaged, IBufferElementData where T2 : unmanaged, IBufferElementData
        {
            Profiler.BeginSample("Update fat vfx");
            var eventsHandle = state.GetBufferTypeHandle<FatVfxDirectEventElement>(isReadOnly: false);
            var b1h = state.GetBufferTypeHandle<T1>(isReadOnly: true);
            var b2h = state.GetBufferTypeHandle<T2>(isReadOnly: true);
            var fatVfxHandle = state.GetComponentTypeHandle<FatVfxData>(isReadOnly: true);
            var fatVfxRuntimeHandle = state.GetComponentTypeHandle<FatVfxDataRuntime>(isReadOnly: false);
            var ltwHandle = state.GetComponentTypeHandle<LocalToWorld>(isReadOnly: true);
            var chunks = query.ToArchetypeChunkArray(Allocator.Temp);

            foreach (var chunk in chunks)
            {
                var numEntities = chunk.Count;

                var b1 = chunk.GetBufferAccessor(ref b1h);
                var b2 = chunk.GetBufferAccessor(ref b2h);
                var eventsAccessor = chunk.GetBufferAccessor(ref eventsHandle);
                var fats = chunk.GetNativeArray(ref fatVfxHandle);
                var fatsRuntime = chunk.GetNativeArray(ref fatVfxRuntimeHandle);
                var ltws = chunk.GetNativeArray(ref ltwHandle);

                for (int j = 0; j < numEntities; j++)
                {
                    var fat = fats[j];
                    var fatRuntme = fatsRuntime[j];
                    if (fatRuntme.index == -1)
                    {
                        if (!Register(ref fatRuntme, in fat)) continue;
                        var vfx = _registereds[fatRuntme.index].vfx;
                        vfx.SetInt(count1, b1[j].Length);
                        vfx.SetInt(count2, b2[j].Length);
                        vfx.SendEvent(initializedEvt);
                        fatsRuntime[j] = fatRuntme;
                        Debug.Log("Registered new fat vfx");
                    }

                    var ltw = ltws[j];
                    var r = _registereds[fatRuntme.index];
                    r.b1[0].SetData(b1[j].AsNativeArray());
                    r.b1[1].SetData(b2[j].AsNativeArray());
                    r.vfx.gameObject.transform.SetPositionAndRotation(ltw.Position, ltw.Rotation);
                    var events = eventsAccessor[j];
                    if (!events.IsEmpty)
                    {
                        for (int i = 0; i < events.Length; i++)
                        {
                            var evt = events[i];
                            var attrs = r.vfx.CreateVFXEventAttribute();
                            attrs.SetFloat(spawnCount, evt.particlesCount);
                            attrs.SetInt(evt.indexPropId, evt.index);
                            r.vfx.SendEvent(evt.eventId, attrs);
                            Debug.Log("Event was sent");
                        }

                        events.Clear();
                    }
                }
            }

            Profiler.EndSample();
        }

        public readonly static int spawnCount = Shader.PropertyToID("spawnCount");

        private bool Register(ref FatVfxDataRuntime runtime, in FatVfxData fatVfxData)
        {
            if (!_free.TryDequeue(out int index)) return false;
            var registered = new Registered();
            // var fatVfx = new FatVfxManaged();
            var goVfx = new GameObject();
            var vfx = goVfx.AddComponent<VisualEffect>();
            runtime.runtimeEffect = vfx;
            registered.vfx = vfx;
            registered.b1 = new GraphicsBuffer[fatVfxData.buffersSize.Length];
            vfx.visualEffectAsset = fatVfxData.vfx;
            for (int i = 0; i < fatVfxData.buffersCapacity.Length; i++)
            {
                var capacity = fatVfxData.buffersCapacity[i];
                var size = fatVfxData.buffersSize[i];
                registered.b1[i] = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, size);
                vfx.SetGraphicsBuffer($"buffer_{i}", registered.b1[i]);
            }

            runtime.index = index;

            _registereds[index] = registered;
            return true;
        }
    }
}