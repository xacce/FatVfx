using Unity.Entities;
using UnityEngine.VFX;

namespace FatVfx
{
  
    [InternalBufferCapacity(0)]
    public partial struct FatVfxDirectEventElement : IBufferElementData
    {
        public int eventId;
        public int particlesCount;
        public int index;
        public int indexPropId;
    }

    public partial struct FatVfxAsset : IComponentData
    {
        public UnityObjectRef<VisualEffectAsset> vfx;
    }
}