using System;
using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class RewardsController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        public string portNum;
        public int rewardDurationMilliSecs;
        public float requiredViewAngle;
        public float requiredDistance;
        public float proximityDistance;
        public float directionErrorDistance;
        public bool enableRewardAreaError;
        public bool enableNonTargetRaycast;
        public float rewardAreaErrorTime;
        public bool playSound;

        public Settings(string portNum,
            int rewardDurationMilliSecs,
            float requiredViewAngle,
            float requiredDistance,
            float proximityDistance,
            float directionErrorDistance,
            bool enableRewardAreaError,
            bool enableNonTargetRaycast,
            float rewardAreaErrorTime,
            bool playSound)
        {
            this.proximityDistance = proximityDistance;
            this.portNum = portNum;
            this.rewardDurationMilliSecs = rewardDurationMilliSecs;
            this.requiredViewAngle = requiredViewAngle;
            this.requiredDistance = requiredDistance;
            this.directionErrorDistance = directionErrorDistance;
            this.enableRewardAreaError = enableRewardAreaError;
            this.enableNonTargetRaycast = enableNonTargetRaycast;
            this.rewardAreaErrorTime = rewardAreaErrorTime;
            this.playSound = playSound;
        }
    }

    public string portNum;
    public int rewardDurationMilliSecs;
    public bool enableRewardAreaError;
    public bool enableNonTargetRaycast;
    public float rewardAreaErrorTime { get; set; }

    public bool playSound;
    private const int buadRate = 9600;
    private static SerialPort rewardSerial;

    public bool IsPortOpen { get; private set; }

    public bool RewardValveOn() {
        //dispose if exists
        if (rewardSerial != null) {
            rewardSerial.Close();
            rewardSerial.Dispose();
        }

        //open port
        try {
            rewardSerial = new SerialPort(portNum, buadRate);
            rewardSerial.ReadTimeout = 60;
            rewardSerial.DtrEnable = true;
            rewardSerial.RtsEnable = true;

            rewardSerial.Open();
        }
        catch {
            return false;
        }
        IsPortOpen = true;
        return true;
    }

    public void RewardValveOff() {
        if (rewardSerial != null) {
            rewardSerial.Close();
            rewardSerial.Dispose();
            rewardSerial = null;
        }
        IsPortOpen = false;
    }

    public void Reward() {
        StartCoroutine(RewardRoutine());
        Debug.Log("Reward Given");
    }

    private IEnumerator RewardRoutine() {
        //PlayerAudio.instance.PlayRewardClip ();
        RewardValveOn();
        //delay for rewardTime seconds
        // TODO: Play sound
        yield return new WaitForSecondsRealtime(rewardDurationMilliSecs / 1000.0f);
        RewardValveOff();
    }

    void OnApplicationQuit() {
        //prevent blocking of serial port
        RewardValveOff();
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(portNum, rewardDurationMilliSecs, RewardArea.RequiredViewAngle, RewardArea.RequiredDistance,
            RewardArea.ProximityDistance, DirectionError.distanceRange, enableRewardAreaError, enableNonTargetRaycast, rewardAreaErrorTime, playSound);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings("", 1000, 45f, 1f, 1f, 0.5f, false, false, -1f,false); //1000, 0.8f, 1f, 1f, 0.5f
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        //turn the reward off before settings are changed
        RewardValveOff();

        Settings settings = (Settings)loadedSettings;

        portNum = settings.portNum;
        rewardDurationMilliSecs = settings.rewardDurationMilliSecs;
        RewardArea.RequiredViewAngle = settings.requiredViewAngle;
        RewardArea.ProximityDistance = settings.proximityDistance;
        RewardArea.RequiredDistance = settings.requiredDistance;
        DirectionError.distanceRange = settings.directionErrorDistance;
        enableRewardAreaError = settings.enableRewardAreaError;
        enableNonTargetRaycast = settings.enableNonTargetRaycast;
        rewardAreaErrorTime = settings.rewardAreaErrorTime;
        playSound = settings.playSound;
    }
}
