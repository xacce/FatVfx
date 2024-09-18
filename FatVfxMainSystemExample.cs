// using Unity.Collections;
// using Unity.Entities;
//
// namespace FatVfx
// {
//     public partial class FatVfxMainSystemExample : SystemBase
//     {
//         private FatVfxWrapper _wrappers;
//         private EntityQuery _eveFatQuery1;
//         private EntityQuery _eveFatQuery2;
//
//         protected override void OnCreate()
//         {
//             _wrappers = new FatVfxWrapper(100);
//             _eveFatQuery1 = new EntityQueryBuilder(Allocator.Persistent).WithAll<FatVfxData, UrComponent1>().Build(this);
//             _eveFatQuery2 = new EntityQueryBuilder(Allocator.Persistent).WithAll<FatVfxData, UrComponent2>().Build(this);
//         }
//
//         protected override void OnUpdate()
//         {
//             _wrappers.Update<UrBufferStruct1, UrBufferStruct2>(this, _eveFatQuery1);
//             _wrappers.Update<UrBufferStruct1ForSecondFat, UrBufferStruct2ForSecondFat>(this, _eveFatQuery2);
//         }
//
//         protected override void OnDestroy()
//         {
//             _wrappers?.Dispose();
//         }
//     }
// }