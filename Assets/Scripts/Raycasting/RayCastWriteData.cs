using System;
using System.Text;
using VirtualMaze.Assets.Scripts.Utils;
using UnityEngine;

namespace VirtualMaze.Assets.Scripts.Raycasting
{
    /// <summary>
    /// Represents data generated by a raycast, with optional properties.
    /// </summary>
    public struct RayCastWriteData
    {
        /// <summary>
        /// Flag used to mark the end of a frame in the data.
        /// </summary>
        public static string END_OF_FRAME_FLAG = "F";

        /// <summary>
        /// Gets the formatted string representation of the raycast data.
        /// </summary>
        public string DataString
        {
            get
            {
                return _GetDataString();
            }
        }

        /// <summary>
        /// Default delimiter used to separate values in the formatted string.
        /// </summary>
        public static readonly string DEFAULT_DELIMITER = ",";
        
        /// <summary>
        /// Gets the delimiter used to separate values in the formatted string.
        /// </summary>
        public string Delimiter { get; private set; }

        /// <summary>
        /// Type of the raycast data (optional).
        /// </summary>
        public Optional<DataTypes> Type { get; private set; }

        /// <summary>
        /// Time of the raycast data (optional).
        /// </summary>
        public Optional<uint> Time { get; private set; }

        /// <summary>
        /// Object name associated with the raycast data (optional).
        /// </summary>
        public Optional<string> ObjName { get; private set; }

        /// <summary>
        /// Center offset of the raycast data (optional).
        /// </summary>
        public Optional<Vector2> CenterOffset { get; private set; }

        /// <summary>
        /// Hit object location of the raycast data (optional).
        /// </summary>
        public Optional<Vector3> HitObjLocation { get; private set; }

        /// <summary>
        /// Raw gaze data of the raycast (optional).
        /// </summary>
        public Optional<Vector2> RawGaze { get; private set; }

        /// <summary>
        /// Subject location of the raycast data (optional).
        /// </summary>
        public Optional<Vector3> SubjectLoc { get; private set; }

        /// <summary>
        /// Subject rotation of the raycast data (optional).
        /// </summary>
        public Optional<float> SubjectRotation { get; private set; }

        /// <summary>
        /// Indicates if the current sample is the last in the frame (optional).
        /// </summary>
        public Optional<bool> IsLastSampleInFrame { get; private set; }

        /// <summary>
        /// Angular offset of the raycast data (optional).
        /// </summary>
        public Optional<Vector2> AngularOffset { get; private set; }

        /// <summary>
        /// Constructor for creating an instance of RayCastWriteData.
        /// </summary>
        /// <param name="type">Type of the raycast data (optional).</param>
        /// <param name="time">Time of the raycast data (optional).</param>
        /// <param name="objName">Object name associated with the raycast data (optional).</param>
        /// <param name="centerOffset">Center offset of the raycast data (optional).</param>
        /// <param name="hitObjLocation">Hit object location of the raycast data (optional).</param>
        /// <param name="rawGaze">Raw gaze data of the raycast (optional).</param>
        /// <param name="subjectLoc">Subject location of the raycast data (optional).</param>
        /// <param name="subjectRotation">Subject rotation of the raycast data (optional).</param>
        /// <param name="isLastSampleInFrame">Indicates if the current sample is the last in the frame (optional).</param>
        /// <param name="angularOffset">Angular offset of the raycast data (optional).</param>
        /// <param name="delimiter">Delimiter used to separate values in the formatted string (optional).</param>
        /// <remarks>
        /// It is recommended to use the RayCastWriteDataBuilder for constructing instances of RayCastWriteData.
        /// </remarks>
        public RayCastWriteData(
            Optional<DataTypes> type = default,
            Optional<uint> time = default,
            Optional<string> objName = default,
            Optional<Vector2> centerOffset = default,
            Optional<Vector3> hitObjLocation = default,
            Optional<Vector2> rawGaze = default,
            Optional<Vector3> subjectLoc = default,
            Optional<float> subjectRotation = default,
            Optional<bool> isLastSampleInFrame = default,
            Optional<Vector2> angularOffset = default,
            string delimiter = null)
        {
            Type = type;
            Time = time;
            ObjName = objName;
            CenterOffset = centerOffset;
            HitObjLocation = hitObjLocation;
            RawGaze = rawGaze;
            SubjectLoc = subjectLoc;
            SubjectRotation = subjectRotation;
            IsLastSampleInFrame = isLastSampleInFrame;
            AngularOffset = angularOffset;
            Delimiter = delimiter ?? DEFAULT_DELIMITER;
            // use default if null was supplied
        }

        private string _GetDataString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            AppendOptionalData(stringBuilder, Type);
            AppendOptionalData(stringBuilder, Time);
            AppendOptionalData(stringBuilder, ObjName);
            AppendOptionalData(stringBuilder, CenterOffset);
            AppendOptionalData(stringBuilder, HitObjLocation);
            AppendOptionalData(stringBuilder, RawGaze);
            AppendOptionalData(stringBuilder, SubjectLoc);
            AppendOptionalData(stringBuilder, SubjectRotation);
            AppendOptionalData(stringBuilder, IsLastSampleInFrame);
            AppendOptionalData(stringBuilder, AngularOffset);

            // Remove the last delimiter if the string is not empty
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length -= Delimiter.Length;
            }

            return stringBuilder.ToString();
        }

        private void AppendOptionalData<T>(StringBuilder stringBuilder, Optional<T> optional)
        {
            if (optional.HasValue)
            {
                string valueString = OptionalValueToString(optional.Value);
                stringBuilder.Append($"{valueString}{Delimiter}");
            }
        }

        private string OptionalValueToString<T>(T value)
        {
            if (value is Vector3 vector3)
            {
                return Vector3ToString(vector3);
            }
            else if (value is Vector2 vector2)
            {
                return Vector2ToString(vector2);
            }
            else
            {
                return value.ToString();
            }
        }

        private string Vector3ToString(Vector3 v)
        {
            return $"{v.x}{Delimiter}{v.y}{Delimiter}{v.z}";
        }

        private string Vector2ToString(Vector2 v)
        {
            return $"{v.x}{Delimiter}{v.y}";
        }
    }
}
