using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sub_Missions.ManWindows
{
    public interface IGUIFormat
    {
        GUIPopupDisplay Display { get; set; }

        void RunGUI(int ID);

        void DelayedUpdate();
        void FastUpdate();
        void OnRemoval();

        void OnOpen();
    }
}
