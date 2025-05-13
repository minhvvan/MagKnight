using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Addresses
{
    public static class Data
    {
        public static class Player
        {
            public const string Stat = "data/player/stat";
        }

        public static class Weapon
        {
            public const string Katana = "data/weapon/katana";
            public const string Dictionary = "data/weapon/dictionary";
        }

        public static class Room
        {
            public const string Floor = "data/floor";
            public const string RoomData = "data/room";
            public const string NavMeshDataBattle  = "data/navmesh/battle";
            public const string NavMeshDataBoss  = "data/navmesh/boss";
        }

        public static class Magnetic
        {
            public const string MagneticSetupData = "data/magnetic/setup";
        }

        public static class Common
        {
            public const string SceneData = "data/common/sceneData";
        }

        public static class Item
        {
            public const string ItemListData = "data/itemList";
        }

        public static class FX
        {
            public const string VFXLOADER = "data/fx/vfxloader";
        }
        
        public static class Artifact
        {
            public const string ArtifactMappingData = "data/artifactMapping";
        }        
        
        public static class Interact
        {
            public const string InteractText = "data/interactText";
        }
    }
    
    public static class Prefabs
    {
    }
    
    public static class NavMeshAddressLoader
    {
        public static string GetPath(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.BattleRoom: return "data/navmesh/battle";
                case RoomType.BossRoom: return "data/navmesh/boss";
                default: return string.Empty;
            }
        }
    }
}