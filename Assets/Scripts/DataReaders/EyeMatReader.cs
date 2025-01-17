using System.Xml;
using Eyelink.Structs;
using System;
using UnityEngine;
using VirtualMaze.Assets.Scripts.Utils;
using VirtualMaze.Assets.Utils;

public class EyeMatReader : EyeDataReader {
    private AllFloatData currentData = null;

    private EyelinkMatFile file;
    private int currentTime = -1;
    private int stateIndex = 0;

    private double lastTriggerTime = -1;

    //SessionTrigger state = SessionTrigger.TrialStartedTrigger;
    double stateTime;

    Interval nextFixation;

    int fixationIndex = 0;
    

    public EyeMatReader(string filePath) {
        file = new EyelinkMatFile(filePath);
        // -1 since matlab is 1 based array, -1 again to act as a null value

        currentTime = (int)GetStateTime(stateIndex);
        stateTime = GetStateTime(stateIndex + 1);

        //additional 1 second for fadeout
        lastTriggerTime = GetStateTime(file.trial_index.GetLength(1) * 3 - 1) + 1000;
        
        //get the first fixation
        nextFixation = new Interval((int)file.fixationStarts[fixationIndex],(int)file.fixationEnds[fixationIndex]);

    }

    //datatype is unused for .mat file as they do not contain thee information
    public AllFloatData GetCurrentData(DataTypes dataType) {
        return currentData;
    }

    public AllFloatData GetNextData() {

        if (currentData == null) {
            //do not need to decrement because the decrement is done in the constuctor
            currentData = new MessageEvent(file.timestamps[0, currentTime], parseTrialCode(GetStateCode(stateIndex)), DataTypes.MESSAGEEVENT);

            //undo the increment of index to simulate a message event within the data
            currentTime--;
        }
        else {

            currentTime++;

            if (currentTime >= stateTime) {
                stateIndex++;

                if (stateIndex < file.trial_index.GetLength(1) * 3 - 1) {
                    stateTime = GetStateTime(stateIndex + 1);
                }
                else {
                    stateTime = float.MaxValue;
                }

                currentData = new MessageEvent(file.timestamps[0, currentTime], parseTrialCode(GetStateCode(stateIndex)), DataTypes.MESSAGEEVENT);

                //undo the increment of index to simulate a message event within the data
                currentTime--;
            }
            else {
                if (currentTime >= lastTriggerTime) {
                    currentData = new FEvent(1, DataTypes.NO_PENDING_ITEMS);
                }
                    
                else {
                    float gx = file.eyePos[0, currentTime];
                    float gy = file.eyePos[1, currentTime];
                    //if (float.IsNaN(gx)) {
                    //    gx = 100_000_000f;
                    //}

                    //if (float.IsNaN(gy)) {
                    //    gy = 100_000_000f;
                    //}
                    
                    // This line decides if the data type should be fixation start/end or just sample type

                    uint currentTimeStamp = file.timestamps[0, currentTime]; 
                    // I do not know why but we need to index here
                    // I am not sure what currentTime actually is and am afraid to touch
                    // -- Xavier, 1 Jan 2024
                    DataTypes sampleType = GetDataType(currentTimeStamp);
                    // TODO : add check for if either is NaN

                    currentData = new Fsample(currentTimeStamp, gx, gy, sampleType);
                }
            }
        }
        return currentData;
    }

    private double GetStateTime(int stateIndex) {
        // -1 since matlab is 1 based array
        return file.trial_index[stateIndex % 3, stateIndex / 3] - 1;
    }

    private int GetStateCode(int stateIndex) {
        return file.trial_codes[stateIndex % 3, stateIndex / 3];
    }

    private void getCorrectInterval(uint currentTimeStamp) {
        while (currentTimeStamp > nextFixation.End && fixationIndex < file.fixationStarts.Length) {
            nextFixation = new Interval((int)file.fixationStarts[fixationIndex],(int)file.fixationEnds[fixationIndex]);
            fixationIndex += 1;
        }
    }

    private DataTypes GetDataType(uint currentTimeStamp) {
        getCorrectInterval(currentTimeStamp);
        if (IsFixationStart(currentTimeStamp)) {
            return DataTypes.SAMPLESTARTFIX;
        } else if (IsFixationEnd(currentTimeStamp)) {
            return DataTypes.SAMPLEENDFIX;
        } else {
            return DataTypes.SAMPLE_TYPE;
        }
    }
    private bool IsFixationStart(uint eventTime) {
        return eventTime == nextFixation.Start;
    }

    private bool IsFixationEnd(uint eventTime) {
        return eventTime == nextFixation.End;
    }

    private string parseTrialCode(int code) {
        Debug.Log(code);
        SessionTrigger trigger = (SessionTrigger)(code - (code % 10));
        switch (trigger) {
            case SessionTrigger.CueOffsetTrigger:
                return $"Cue Offset {code}";
            case SessionTrigger.TrialStartedTrigger:
                return $"Start Trial {code}";
            case SessionTrigger.TrialEndedTrigger:
                return $"End Trial {code}";
            case SessionTrigger.TimeoutTrigger:
                return $"Timeout {code}";
            case SessionTrigger.ExperimentVersionTrigger:
                return $"Trigger Version {(int)trigger + GameController.versionNum}";
            default:
                throw new NotSupportedException($"EyeMatReader::Unknown code {code}");
        }
    }

    public void Dispose() {
        file.Dispose();
    }
}
