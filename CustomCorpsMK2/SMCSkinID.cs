using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Sub_Missions
{
    [Serializable]
    public class SMCSkinID
    {
        public string Name = "Is Determined by the folder('s name) that this is in.";
        public int UniqueID = 0;
        public string Albedo = "Albedo.png";
        public string Emissive = "Emissive.png";
        public bool AlwaysEmissive = false;
        public string Metal = "Metal.png";

        // UI
        public string Preview = "Preview.png";
        public string Button = "Button.png";
    }
}
