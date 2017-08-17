using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.SaveData
{
    class ProgressSaver
    {
        public bool existingSave;

        public int gold;
        public int level;

        public void ReadSave()
        {

            StreamReader reader = new StreamReader("Progress.json");


        }
        public void WriteSave() {



        }

    }
}
