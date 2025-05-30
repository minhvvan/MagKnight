using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const int MaxRooms = 5;
    public const int MinRooms = 2;
    public const int MaxTreasureRoomCount = 2;
    public const int MaxArtifacts = 9;

    #region SaveDataKey

    public const string PlayerData = "PlayerData";
    public const string CurrentRun = "CurrentRunData";
    

    public const string MasterVolume = "Master";
    public const string BGMVolume = "BGM";
    public const string SFXVolume = "SFX";
    public const string BGMMute = "BGMMute";
    public const string SFXMute = "SFXMute";

    public const string MouseXSensitivity = "MouseX";
    public const string MouseYSensitivity = "MouseY";

    #endregion
    
    #region SceneName
    public const string BaseCamp = "BaseCampScene";
    public const string Start = "start";
    #endregion

    #region AnimationName

    public const string CrateOpen = "CrateOpen";
    public const string CrateClose = "CrateClose";

    #endregion

}
