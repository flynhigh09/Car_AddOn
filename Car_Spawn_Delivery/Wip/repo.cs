using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Globalization;

using GTA;
using GTA.Math;
using GTA.Native;

namespace Car_Spawn_Delivery
{
    public class repo : Script
    {
        private readonly RepoSettings _settings = RepoSettings.RepoSets;
        XmlElement docroot;

        public bool wipbool;

        string Vehicle = null;

        public Blip RepoBlip;
        public Ped driver;
        public static Vehicle veh = null;


        public repo()
        {
            Tick += RepoTick;
            Interval += _settings.repoTick;
            KeyDown += RepoKeyDown;
            KeyUp += OnKeyUp;

        }

        public void RepoKeyDown(object sender, KeyEventArgs key)
        {
            if (!wipbool)
            {
                if (key.KeyCode == _settings.keyEnable)
                {
                    UI.Notify("- Repo -\nEnabled!");
                    wipbool = true;
                }
                else if (key.KeyCode == _settings.keyEnable && wipbool)
                {
                    wipbool = false;
                    UI.Notify("- Repo -\nDisabled!");
                }
            }
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && Game.Player.Character.IsStopped)
            {
                List<string> text = new List<string>();
                Vehicle veh = Game.Player.Character.CurrentVehicle;
                text.Add("<Driver DriverName='" + veh.FriendlyName + "'>");
                text.Add("<PedModel>random</PedModel>");
                text.Add("<Vehicle>");
                text.Add("<Model>" + veh.DisplayName + "</Model>");

                text.Add("<Colors>");
                text.Add("<PrimaryColor>" + ((int)veh.PrimaryColor).ToString() + "</PrimaryColor>");
                text.Add("<SecondaryColor>" + ((int)veh.SecondaryColor).ToString() + "</SecondaryColor>");
                text.Add("<PearlescentColor>" + ((int)veh.PearlescentColor).ToString() + "</PearlescentColor>");
                text.Add("<RimColor>" + ((int)veh.RimColor).ToString() + "</RimColor>");
                text.Add("<NeonColor>");
                text.Add("<Color>");
                text.Add("<R>" + veh.NeonLightsColor.R + "</R>");
                text.Add("<G>" + veh.NeonLightsColor.G + "</G>");
                text.Add("<B>" + veh.NeonLightsColor.B + "</B>");
                text.Add("</Color>");
                text.Add("</NeonColor>");
                text.Add("</Colors>");

                text.Add("<LicensePlate>" + Function.Call<int>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, veh) + "</LicensePlate>");
                text.Add("<LicensePlateText>" + veh.NumberPlate + "</LicensePlateText>");

                text.Add("<Components>");
                for (int i = 0; i <= 25; i++)
                {
                    if (Function.Call<bool>(Hash.IS_VEHICLE_EXTRA_TURNED_ON, veh, i))
                        text.Add("<Component ComponentIndex='" + i + "'>" + "0" + "</Component>");
                    else
                        text.Add("<Component ComponentIndex='" + i + "'>" + "-1" + "</Component>");
                }
                text.Add("</Components>");

                text.Add("<ModToggles>");
                for (int i = 0; i <= 25; i++)
                {
                    if (Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, veh, i))
                        text.Add("<Toggle ToggleIndex='" + i + "'>" + "true" + "</Toggle>");
                }
                text.Add("</ModToggles>");

                text.Add("<WheelType>" + ((int)veh.WheelType).ToString() + "</WheelType>");
                text.Add("<Mods>");
                for (int i = 0; i <= 500; i++)
                {
                    if (Function.Call<int>(Hash.GET_VEHICLE_MOD, veh, i) != -1)
                        text.Add("<Mod ModIndex='" + i + "'>" + Function.Call<int>(Hash.GET_VEHICLE_MOD, veh, i).ToString() + "</Mod>");
                }
                text.Add("</Mods>");
                text.Add("<CustomTires>false</CustomTires>");

                text.Add("<WindowsTint>" + (int)veh.WindowTint + "</WindowsTint>");
                text.Add("<Neons>");
                text.Add("<Left>" + Function.Call<bool>(Hash._IS_VEHICLE_NEON_LIGHT_ENABLED, veh, 0).ToString() + "</Left>");
                text.Add("<Right>" + Function.Call<bool>(Hash._IS_VEHICLE_NEON_LIGHT_ENABLED, veh, 1).ToString() + "</Right>");
                text.Add("<Front>" + Function.Call<bool>(Hash._IS_VEHICLE_NEON_LIGHT_ENABLED, veh, 2).ToString() + "</Front>");
                text.Add("<Back>" + Function.Call<bool>(Hash._IS_VEHICLE_NEON_LIGHT_ENABLED, veh, 3).ToString() + "</Back>");
                text.Add("</Neons>");

                text.Add("</Vehicle>");

                text.Add("</Driver>");

                File.WriteAllLines(@"scripts\\repo\vehicleinfo.txt", text);
                UI.Notify(veh.DisplayName + " info saved to /scripts/repo/vehicleinfo.txt");
            }
        }

        public void RepoTick(object sender, EventArgs b)
        {
            if (wipbool && RandomInt(1) <= _settings.Chance)
            {
                UI.Notify("- UrWip_UrWay!");
                
                XmlDocument document = new XmlDocument();
                document.Load(@"scripts\\repo\Cars.xml");

                docroot = document.DocumentElement;
                XmlNodeList nodelist = docroot.SelectNodes("//Drivers/*");
                XmlNode carsxml = nodelist.Item(RandomInt(nodelist.Count));

                string Vehicle = carsxml.SelectSingleNode("Vehicle/Model").InnerText;
                Model modeladdon = new Model(Vehicle);//works
                
                Vector3 sfepos = Game.Player.Character.GetOffsetInWorldCoords(Game.Player.Character.Position.Around(_settings.Addpos));

                veh = World.CreateVehicle(modeladdon, sfepos, Game.Player.Character.Heading);

                veh.PlaceOnGround();
                RepoBlip = veh.AddBlip();
                RepoBlip.Sprite = BlipSprite.PersonalVehicleCar;
                RepoBlip.IsFlashing = true;
                RepoBlip.Color = BlipColor.Green;

                if (carsxml.SelectSingleNode("PedModel").InnerText == "random")
                    driver = World.CreateRandomPed(veh.Position.Around(2f));
                else
                {
                    string Addped = carsxml.SelectSingleNode("PedModel").InnerText;
                    Model modelAddped = new Model(Addped);//worked?
                    driver = World.CreatePed(modelAddped, veh.Position.Around(2f));
                }

                driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
                
                #region mods
                if (carsxml.SelectSingleNode("Vehicle/WindowsTint") != null)
                {
                    Function.Call(Hash.SET_VEHICLE_MOD_KIT, veh, 0);
                    veh.WheelType = (VehicleWheelType)int.Parse(carsxml.SelectSingleNode("Vehicle/WheelType").InnerText, CultureInfo.InvariantCulture);
                    veh.PrimaryColor = (VehicleColor)int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/PrimaryColor").InnerText, CultureInfo.InvariantCulture);
                    veh.SecondaryColor = (VehicleColor)int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/SecondaryColor").InnerText, CultureInfo.InvariantCulture);
                    veh.PearlescentColor = (VehicleColor)int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/PearlescentColor").InnerText, CultureInfo.InvariantCulture);
                    //Function.Call((Hash)0xF40DD601A65F7F19, veh.Handle, int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/TrimColor").InnerText, CultureInfo.InvariantCulture));
                   // Function.Call((Hash)0x6089CDF6A57F326C, veh.Handle, int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/DashColor").InnerText, CultureInfo.InvariantCulture));
                    veh.RimColor = (VehicleColor)int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/RimColor").InnerText, CultureInfo.InvariantCulture);
                    veh.NumberPlate = carsxml.SelectSingleNode("Vehicle/LicensePlateText").InnerText;
                    Function.Call(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, veh, int.Parse(carsxml.SelectSingleNode("Vehicle/LicensePlate").InnerText, CultureInfo.InvariantCulture));
                    veh.WindowTint = (VehicleWindowTint)int.Parse(carsxml.SelectSingleNode("Vehicle/WindowsTint").InnerText, CultureInfo.InvariantCulture);
                    if (carsxml.SelectSingleNode("Vehicle/Colors/SmokeColor") != null)
                    {
                        Color color = Color.FromArgb(255, int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/SmokeColor/Color/R").InnerText), int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/SmokeColor/Color/G").InnerText), int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/SmokeColor/Color/B").InnerText));
                        veh.TireSmokeColor = color;
                    }
                    if (carsxml.SelectSingleNode("Vehicle/Colors/NeonColor") != null)
                    {
                        Color color = Color.FromArgb(255, int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/NeonColor/Color/R").InnerText), int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/NeonColor/Color/G").InnerText), int.Parse(carsxml.SelectSingleNode("Vehicle/Colors/NeonColor/Color/B").InnerText));
                        veh.NeonLightsColor = color;
                    }
                    veh.SetNeonLightsOn(VehicleNeonLight.Back, bool.Parse(carsxml.SelectSingleNode("Vehicle/Neons/Back").InnerText));
                    veh.SetNeonLightsOn(VehicleNeonLight.Front, bool.Parse(carsxml.SelectSingleNode("Vehicle/Neons/Front").InnerText));
                    veh.SetNeonLightsOn(VehicleNeonLight.Left, bool.Parse(carsxml.SelectSingleNode("Vehicle/Neons/Left").InnerText));
                    veh.SetNeonLightsOn(VehicleNeonLight.Right, bool.Parse(carsxml.SelectSingleNode("Vehicle/Neons/Right").InnerText));
                    foreach (XmlElement component in carsxml.SelectNodes("Vehicle/Components/*"))
                    {
                        Function.Call(Hash.SET_VEHICLE_EXTRA, veh, int.Parse(component.GetAttribute("ComponentIndex")), int.Parse(component.InnerText, CultureInfo.InvariantCulture));
                    }
                    foreach (XmlElement component in carsxml.SelectNodes("Vehicle/ModToggles/*"))
                    {
                        Function.Call(Hash.TOGGLE_VEHICLE_MOD, veh, int.Parse(component.GetAttribute("ToggleIndex")), bool.Parse(component.InnerText));
                    }
                    foreach (XmlElement component in carsxml.SelectNodes("Vehicle/Mods/*"))
                    {
                        veh.SetMod((VehicleMod)int.Parse(component.GetAttribute("ModIndex")), int.Parse(component.InnerText, CultureInfo.InvariantCulture), bool.Parse(carsxml.SelectSingleNode("Vehicle/CustomTires").InnerText));
                    }
                }
                #endregion
                
                driver.Task.DriveTo(veh, Game.Player.Character.Position, 1f, 20f, 262199);    
                        
                if ((driver.Position.DistanceTo(Game.Player.Character.Position) <= 2.5f))
                {
                    if(Game.Player.Character.Position.DistanceTo(veh.Position) <= 2.5f)
                    driver.MarkAsNoLongerNeeded();
                    RepoBlip.Remove();
                    driver.Delete();
                }
            }
        }

        public static int RandomInt(int max)
        {
            int min = 0;
            max++;
            return Function.Call<int>(Hash.GET_RANDOM_INT_IN_RANGE, min, max);
        }

    }
 }