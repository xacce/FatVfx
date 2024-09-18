#if UNITY_EDITOR
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

namespace FatVfx.Authoring
{
    public class FatVfxAssetAuthoring : MonoBehaviour
    {
        [SerializeField] private VisualEffectAsset vfx;

        private class B : Baker<FatVfxAssetAuthoring>
        {
            public override void Bake(FatVfxAssetAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new FatVfxAsset { vfx = authoring.vfx });
                AddBuffer<FatVfxDirectEventElement>(e);
            }
        }
    }
}
#endif