using System;
using System.Windows.Forms;
using GTA;

namespace Car_Spawn_Delivery
{
     internal class RepoSettings
    {
        #region Fields
        private static RepoSettings myinstance;
        private readonly ScriptSettings _settings;

        public static RepoSettings RepoSets => myinstance ?? (myinstance = new RepoSettings());

        public float TempGaugePosX { get; private set; }
        public float TempGaugePosY { get; private set; }
        public float TempGaugeWidth { get; private set; }
        public float TempGaugeHeight { get; private set; }
        
        public int repoTick { get; private set; }
        public float Chance { get; private set; }
        public float Addpos { get; private set; }
        public Keys keyEnable { get; private set; }

        #endregion


        private RepoSettings()
        {
            string  settinglocation = "scripts\\Car_Spawn_Delivery.ini";
            _settings = ScriptSettings.Load(settinglocation);

            Load();
        }

        private void Load()
        {
            repoTick = _settings.GetValue("Values", "Tick", 1);
            Chance = _settings.GetValue("Values", "Chance", 1f);
            Addpos = _settings.GetValue("Values", "PosFromPed", .5f);

            keyEnable = _settings.GetValue<Keys>("Settings", "KeyEnable", Keys.F10);           
        }



    }
}
