using System.Linq;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Systems
{
    public partial class InvertPrefabLHT : GameSystemBase
    {
        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;

        private static string[] ASSETS_PREFIX_NOT_INVERTED =
        {
            "Dome Parking Hall",
            "Aquaculture Area Placeholder -Water",
            "Offshore Oil Industry Placeholder",
            "Openwater Fish Farm Entrance",
            "Openwater Fishing Area Entrance",
            "BusStation01"
        };


        private static ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);


        protected override void OnCreate()
        {
            base.OnCreate();
            log.Info($"Initializing {nameof(InvertPrefabLHT)}");
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            allAssets = SystemAPI.QueryBuilder().WithAllRW<PrefabData>().Build();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            var allAssetEntities = allAssets.ToEntityArray(Allocator.Temp);
            log.Info($"Loaded {allAssetEntities.Length} assets");
            foreach (var entity in allAssetEntities)
            {
                prefabSystem.TryGetPrefab(entity, out PrefabBase prefab);
                if (prefab is BuildingPrefab or BuildingExtensionPrefab)
                {
                    if (ASSETS_PREFIX_NOT_INVERTED.Any(prefix => prefab.name.StartsWith(prefix)))
                    {
                        continue;
                    }

                    prefab.TryGet(out ObjectSubNets subNets);
                    if (subNets is null)
                    {
                        continue;
                    }

                    subNets.m_InvertWhen = NetInvertMode.LefthandTraffic;
                    prefabSystem.UpdatePrefab(prefab);
                    log.Info($"Inverted {prefab.name}");
                }
            }

            base.OnGameLoadingComplete(purpose, mode);
        }
    }
}