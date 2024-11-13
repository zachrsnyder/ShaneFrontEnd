using UnityEngine;

namespace FarmhandStuff
{
    public class PrintableObject
    {
        private string filename;
        
        public string Filename
        {
            get => filename;
        }
        
        private GameObject preview_model = null;
        
        public GameObject PreviewModel
        {
            get => preview_model;
        }

        public PrintableObject(string filename)
        {
            //TODO: This will search for a model that matches the filename
            //When a model is found, a ref will be stored here. Then, modelbtn just needs one of these objects to be initialized
            this.filename = filename;
            
            //returns null if there is no model found, model prefabs should be named the same as the gcode file including extension
            this.preview_model = Resources.Load<GameObject>("ObjectPreviews/" + this.filename);
        }
    }
}