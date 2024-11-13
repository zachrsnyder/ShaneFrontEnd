using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json;
using UnityEngine.Serialization;

/*Whole goal of this system is that the Hololens only has to request information when it needs it.
 If there is an emergency or a threshold is reached, it will be detected by the server and sent as a notification.
 Therefore, the printer state on the hololens should only be updated when the information is being viewed.*/
namespace FarmhandStuff
{
    [Serializable()]
    public class Filament
    {
        public string Material_Name;
        public float First_Layer_Temp;
        public float Printing_Temp;
        public float Bed_Temp;
        public string Color;
        public float Required_Nozzle_Size;
        public bool Needs_Hardened_Nozzle;
        [FormerlySerializedAs("Weight")] public float Current_Weight;
    }
    
    [Serializable()]
    public class Printer
    {
        
        public string name;
        public Filament Current_Filament;
        public float Nozzle_Size;
        public bool Has_Hardened_Nozzle;
        public float current_temp_extruder;
        public float current_temp_bed;
        public float current_extruder_fan_power;
        public string current_gcode_name;
        public string current_state;

        public List<PrintableObject> available_prints = new List<PrintableObject>();
        
        
        public Printer(string name, Filament filament = null, float nozzleSize = 0.4f, bool hasHardenedNozzle = false)
        {
            this.name = name;
            this.Current_Filament = filament;
            this.Nozzle_Size = nozzleSize;
            this.Has_Hardened_Nozzle = hasHardenedNozzle;
        }
        
        //Expects a string of properly formatted json including the updated attributes
        public void update_printer(string json)
        {
            // Debug.Log(json);
            JsonUtility.FromJsonOverwrite(json, this);
            // this.current_filament.material_name = this.current_filament.material_name.Replace("b\'", "").Replace("\'", "");
            // this.current_filament.color = this.current_filament.color.Replace("b\'", "").Replace("\'", "");
        }
    }
    
}