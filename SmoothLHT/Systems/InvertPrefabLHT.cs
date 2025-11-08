using System.Linq;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.City;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Systems
{
    public partial class InvertPrefabLHT : GameSystemBase
    {
        private PrefabSystem prefabSystem;
        private CityConfigurationSystem cityConfigurationSystem;
        private EntityQuery allAssets;
        private bool isAllInverted;

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
            cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
            allAssets = SystemAPI.QueryBuilder().WithAllRW<PrefabData>().Build();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            if (!cityConfigurationSystem.leftHandTraffic || isAllInverted) return;
            var allAssetEntities = allAssets.ToEntityArray(Allocator.Temp);
            log.Info($"Loaded {allAssetEntities.Length} assets");
            foreach (var entity in allAssetEntities)
            {
                if (!prefabSystem.TryGetPrefab(entity, out PrefabBase prefab) ||
                    prefab is not (BuildingPrefab or BuildingExtensionPrefab) ||
                    ASSETS_PREFIX_NOT_INVERTED.Any(prefab.name.StartsWith) ||
                    !prefab.TryGet(out ObjectSubNets subNets) ||
                    subNets is null ||
                    subNets.m_InvertWhen.Equals(NetInvertMode.LefthandTraffic)
                   ) continue;

                subNets.m_InvertWhen = NetInvertMode.LefthandTraffic;
                prefabSystem.UpdatePrefab(prefab);
                log.Info($"Inverted {prefab.name}");
            }

            isAllInverted = true;
        }
    }
}