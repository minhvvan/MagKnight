using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용법:
/// Addresses와 동일하게 PlaySFX, PlayBGM의 인자에 사용하면 됩니다.
/// 새 Audio를 AudioManager에 추가한 후,
/// 카테고리에 맞게 변수를 작성하고 해당 파일명을 문자열로 작성해주시면 됩니다.
/// List로 관리되는 영역은 연속재생되는 사운드의 자연스러움을 위해 같은 용도의 사운드를 여러개 넣은 경우입니다.(ex. 발소리)
/// 그렇게 쓰일 목적의 사운드들은 그렇게 추가해서 관리하면 됩니다.
/// </summary>
public static class AudioBase
{
    public static class BGM
    {
        public static class BaseCamp
        { 
            public const string Main = "AMBIENCE_Ferry_Car_Deck_02_loop_stereo";
            public const string Environment  = "LVSD-0006_04_SealedSanctuary_loop";
        }

        public static class Dungeon
        {
            public static class Common
            {
                
            }
            
            public static class Stage1
            {
                public const string Environment  = "AMBIENCE_SciFi_Large_Space_Hangar_Deep_Smooth_loop_stereo";
                public const string MainBattle = "Epikton - Acceleration Zone";
                public const string Boss = "Epikton - Artery Of Steel";
            }
        }
    }
    
    public static class SFX
    {
        public static class Common
        {
            public const string Alarm1 = "ALARM_Distorted_Echo_loop_stereo";
            public const string Alarm2 = "ALARM_Submarine_loop_stereo";
            public const string Light = "magic_flame_of_light_04";
            public const string ChaimBell = "MAGIC_SPELL_Attacking_Climbing_Bells_stereo";
            public const string DigitalNotify = "NOTIFICATION_Digital_06_mono";
        }

        public static class Item
        {
            public const string ItemCreate = "magic_light_bubble_03";
            public const string LootCreate = "ELEVATOR_Startup_01_mono";
        }
        
        public static class Player
        {
            public static class Attack
            {
                public static readonly List<string> Hit =new List<string>()
                {
                    "sword_impact_body",
                    "water_spell_impact_hit_04",
                    "water_spell_impact_hit_05",
                    "water_spell_impact_hit_06",
                };
                
                public static readonly List<string> Hurt =new List<string>()
                {
                    "bullet_impact_ice_01",
                    "bullet_impact_ice_02",
                    "bullet_impact_ice_03",
                    "bullet_impact_ice_04",
                    "bullet_impact_ice_05",
                    "bullet_impact_ice_06",
                    "bullet_impact_ice_07",
                    "bullet_impact_ice_08",
                    "bullet_impact_ice_09"
                };
                
                public static readonly List<string> Critical =new List<string>()
                {
                    "gun_silenced_rifle1_shot_01",
                    "gun_silenced_rifle1_shot_02",
                    "gun_silenced_rifle1_shot_03",
                    "gun_silenced_rifle1_shot_04"
                };
                
                public static readonly List<string> Swing =new List<string>()
                {
                    "BLUNT_Swing_04_mono",
                    "BLUNT_Swing_07_mono"
                };
                
                public static readonly List<string> Parry =new List<string>()
                {
                    "sci-fi_electric_pulse_hum_01",
                    "sci-fi_shield_power_deflect_boom_02"
                };
                
                public static readonly List<string> Bow =new List<string>()
                {
                    "bow_crossbow_arrow_shoot_type1_01",
                    "bow_crossbow_arrow_shoot_type1_02",
                    "bow_crossbow_arrow_shoot_type1_03",
                    "bow_crossbow_arrow_shoot_type1_04",
                    "bow_crossbow_arrow_shoot_type1_05",
                    "bow_crossbow_arrow_shoot_type1_06",
                    "bow_crossbow_arrow_shoot_type1_07",
                    "bow_crossbow_arrow_shoot_type1_08"
                };                
                
                public static readonly List<string> BowStretch =new List<string>()
                {
                    "bow_crossbow_arrow_draw_stretch1_04"
                };
                
                public static readonly List<string> Arrow =new List<string>()
                {
                    "bullet_leave_barrel_01",
                    "bullet_leave_barrel_02",
                    "bullet_leave_barrel_03",
                    "bullet_leave_barrel_04",
                    "bullet_leave_barrel_05"
                };
                
                public static readonly List<string> ArrowE =new List<string>()
                {
                    "bullet_leave_barrel_effect_01",
                    "bullet_leave_barrel_effect_02",
                    "bullet_leave_barrel_effect_03",
                    "bullet_leave_barrel_effect_04",
                    "bullet_leave_barrel_effect_05"
                };
            }

            public static class Movement
            {
                public static readonly List<string> Step =new List<string>()
                {
                    "footstep_concrete_walk_11",
                    "footstep_concrete_walk_12",
                    "footstep_concrete_walk_13",
                    "footstep_concrete_walk_14",
                    "footstep_concrete_walk_15",
                    "footstep_concrete_walk_16",
                    "footstep_concrete_walk_17",
                    "footstep_concrete_walk_18"
                };
                
                public static readonly List<string> Jump =new List<string>()
                {
                    "foley_cloth_light_fast_movement_12",
                    "foley_cloth_light_fast_movement_13",
                    "foley_cloth_light_fast_movement_14",
                    "foley_cloth_light_fast_movement_15"
                };

                
                public static readonly List<string> Land =new List<string>()
                {
                    "footstep_concrete_land_v2_02",
                    "footstep_concrete_land_v2_03",
                    "footstep_concrete_land_v2_04"
                };
                
                public static readonly List<string> Avoid =new List<string>()
                {
                    "foley_jump_movement_throw_03",
                    "foley_jump_movement_throw_04",
                    "foley_jump_movement_throw_05"
                };
            }

            public static class Health
            {
                public const string Recovery = "magic_light_bubble_05";
            }

            public static class Magnetic
            {
                public const string AimActivate = "movie_camera_vintage_lever_07";
                public const string AimDeactivate = "movie_camera_vintage_lever_09";
                
                public const string TargetLock = "movie_camera_vintage_lever_05";
                public const string ActiveMagnet = "potion_flask_mana_collect_04";
                
                public const string MagneticForce = "sci-fi_electric_pulse_hum_02";
                
                public const string MagneticSwitch1 = "magic_light_bubble_01";
                public const string MagneticSwitch2 = "sci-fi_shield_power_on_impact_02";
            }
        }

        public static class Enemy
        {
            public static class Awake
            {
                public const string PowerOn = "ROBOTIC_Servo_Large_Dual_Servos_Open_mono";
            }

            public static class Dead
            {
                public const string SignalLost = "retro_fail_sound_05";
                public const string PowerDown1 = "sci-fi_electric_pulse_power_down_02";
                public const string PowerDown2 = "sci-fi_electric_pulse_power_down_03";
            }
            
            public static class Attack
            {
                public static class AttackAlert
                {
                    public const string Alarm1 = "alarm_siren_warning_01";
                }

                public static class Cannon
                {
                    public const string ShotH1 = "gun_grenade_launcher_shot_02";
                    public const string ShotH2 = "gun_grenade_launcher_shot_03";
                    public const string ShotP1 = "PLASMA_CANNON_Zap_Dropping_Tail_mono";
                }

                public static class ShortVulcan
                {
                    public const string BurstShot = "gun_submachine_auto_shot_00_automatic_preview_01";
                }
                
                public static readonly List<string> HurtH =new List<string>()
                {
                    "bullet_impact_metal_heavy_01",
                    "bullet_impact_metal_heavy_02",
                    "bullet_impact_metal_heavy_03",
                    "bullet_impact_metal_heavy_04",
                    "bullet_impact_metal_heavy_05",
                    "bullet_impact_metal_heavy_06",
                    "bullet_impact_metal_heavy_07",
                    "bullet_impact_metal_heavy_08"
                };
                
                public static readonly List<string> HurtL =new List<string>()
                {
                    "bullet_impact_metal_light_01",
                    "bullet_impact_metal_light_02",
                    "bullet_impact_metal_light_03",
                    "bullet_impact_metal_light_04",
                    "bullet_impact_metal_light_05",
                    "bullet_impact_metal_light_06",
                    "bullet_impact_metal_light_07",
                    "bullet_impact_metal_light_08"
                };
            }

            public static class Movement
            {
                public const string Step = "metal_robot_impact_med_step_01";
                public const string Metal1 = "ROBOTIC_Mech_Movement_01_Footstep_stereo";
                public const string Metal2 = "ROBOTIC_Mech_Movement_03_Clean_Footstep_Fast_stereo";
                public const string Metal3 = "ROBOTIC_Mech_Movement_03_Clean_Footstep_stereo";
            }

            public static class Voice
            {
                public static class BlaBla
                {
                    public static readonly List<string> Robot =new List<string>()
                    {
                        "sci-fi_driod_robot_emote_01",
                        "sci-fi_driod_robot_emote_03",
                        "sci-fi_driod_robot_emote_05",
                        "sci-fi_driod_robot_emote_06",
                        "sci-fi_driod_robot_emote_07",
                        "sci-fi_driod_robot_emote_08",
                        "sci-fi_driod_robot_emote_09",
                        "sci-fi_driod_robot_emote_10",
                        "sci-fi_driod_robot_emote_11",
                        "sci-fi_driod_robot_emote_12"
                    };
                }
            }
        }

        public static class NPC
        {
            public static readonly List<string> Talk =new List<string>()
            {
                "cartoon_electronic_computer_code_03",
                "cartoon_electronic_computer_code_04",
                "cartoon_electronic_computer_code_05",
                "cartoon_electronic_computer_code_06",
                "cartoon_electronic_computer_code_07",
                "cartoon_electronic_computer_code_08"
            };
        }
        
        public static class Room
        {
            public const string GameOver = "MUSIC_EFFECT_Solo_Harp_Negative_02_stereo";
            public const string NextRoom = "sci-fi_shield_device_small_02";
            public const string RoomClear = "sci-fi_shield_device_power_up_01";
            public const string StageClear = "retro__musical_stinger_03";
        }

        public static class UI
        {
            public static class Menu
            {
                public const string Move = "gun_submachine_auto_magazine_safety_switch_02";
                public const string Select = "gun_submachine_auto_magazine_load_01";
                public const string Cancel = "gun_submachine_auto_magazine_safety_switch_01";
            }

            public static class FieldItem
            {
                public const string BuyItem = "COINS_Rattle_04_mono";
                public const string GetItem = "foley_soldier_gear_equipment_rattle_movement_light_01";
                public const string DropItem = "foley_soldier_gear_equipment_movement_grab_item_01";
                public const string Dismantle = "dirt_rice_pouring_gravel_debris_02";
            }

            public static class Artifact
            {
                public const string PickUp = "foley_object_grab_pickup_02";
                public const string Drop = "foley_object_grab_pickup_01";
                public const string Swap = "item_pickup_swipe_01";
            }
        }
    }
}
