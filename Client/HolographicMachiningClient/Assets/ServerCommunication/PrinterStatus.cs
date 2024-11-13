using System.Collections;
using System.Numerics;
using UnityEngine;

namespace ServerCommunication
{
    public struct PrinterStatus
    {
        public float bed_temp;
        public float bed_target_temp;
        public float bed_power;

        public float extruder_temp;
        public float extruder_target_temp;
        public float extruder_power;

        public float fan_speed;
        
        public float[] toolhead_position;

        public GameObject currentPreview;
        public string currentGCodeName;
    }
}