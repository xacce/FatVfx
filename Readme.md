Need to work with massive VFX. Is not an implementation for spooling multiple VFXs.
Instead, it provides an interface for working with a single VFX, such as a complex and interactive projection.

Has one built-in system - cleanup.

Everything else is up to you.

Add `FatVfxAssetAuthoring` somewhere and specify your vfx asset.
Supports working with two* custom structured buffers. They are available by `buffer_0/buffer_1` the number of objects in them is available by `bufferCount_0/bufferCount_1`.

Then when you need to spool this heavy vfx add `FatVfxData` to it and specify the required data.

```csharp
var fatvfx = new FatVfxData()
{
    vfx = asset.vfx,
    buffersCapacity = new FixedList32Bytes<int>() { vfxDatas.Length, vfxPairs.Length },
    buffersSize = new FixedList32Bytes<int>() { UnsafeUtility.SizeOf<StructBuffer0>(), UnsafeUtility.SizeOf<StructBuffer1>() }
};
ecb.AddComponent(index, entity, new FatVfxDataRuntime() { index = -1 });
ecb.AddComponent(index, entity, fatvfx); 
```

Define in the project a system in which you will update all fat vfxes

Its contents can be like this for starters

```csharp
 public partial class FatVfxSystem : SystemBase
    {
        private FatVfxWrapper _wrappers;
        private EntityQuery _myFatQuery;

        protected override void OnCreate()
        {
            _wrappers = new FatVfxWrapper(100);
            _myFatQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<FatVfxData, MyCustomVfxData>().Build(this);
        }

        protected override void OnUpdate()
        {
            _wrappers.Update<StructBuffer0, StructBuffer1>(this, _myFatQuery);
        }

        protected override void OnDestroy()
        {
            _wrappers.Dispose();
        }
    }
```

You can change the data itself in the corresponding `DynamicBuffer<StructBuffer0>` `DynamicBuffer<StructBuffer1>`. Add them to the entity yourself.

The current implementation does not imply changing the size of the buffers, so your pipline for dealing with this is as follows:

1 - Fill buffers with source data
2 - Add `FatVfxDataRuntime` `FatVfxData` to entity
3 - Change data