﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Text.RegularExpressions;

public partial class Program: MyGridProgram {
    //======-SCRIPT BEGINNING-======

    /// <summary>
    /// Solar Tracking system
    /// Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=2232417828
    /// </summary>
    class JNSolarTracker: InitSubP {
        public JNSolarTracker() : base("Solar Tracking", new MyVersion(1, 0)) { }

        protected override SdSubP Init(ushort id) {
            return new TP(id, this);
        }

        class TP: SdSubP {
            static float SolarAlign = 0.2f;
            string ignoreTag = "ST-X";
            List<IMyMotorStator> Rotors = new List<IMyMotorStator>();
            List<IMySolarPanel> Panels = new List<IMySolarPanel>();
            List<IMyOxygenFarm> Farms = new List<IMyOxygenFarm>();
            List<SolarArray> SolarArrays = new List<SolarArray>();
            ActI BSA, MA;

            public TP(ushort id, InitSubP p) : base(id, p) {
                BSA = AddAct(BuildSolarArrays, 600);
                MA = AddAct(Main, 120, 60 * 3);
            }

            void Main() {
                for (int i = 0; i < SolarArrays.Count; i++) {
                    if (SolarArrays[i].Update()) {
                        BuildSolarArrays();
                        break;
                    }
                }
            }
            void BuildSolarArrays() {
                SolarArrays.Clear();
                OS.GTS.GetBlocksOfType(Rotors);
                foreach (var v in Rotors) {
                    if (!v.IsSameConstructAs(OS.P.Me) || v.CustomName.EndsWith(ignoreTag) || v.CustomData.Contains(ignoreTag))
                        continue;
                    OS.GTS.GetBlocksOfType(Panels, b => b.CubeGrid == v.TopGrid);
                    OS.GTS.GetBlocksOfType(Farms, b => b.CubeGrid == v.TopGrid);
                    if (Panels.Count > 0 || Farms.Count > 0) {
                        SolarArrays.Add(new SolarArray(v, Panels, Farms));
                    }
                }
            }

            class SolarArray {
                public IMyMotorStator Rotor { get; set; }
                public List<IMySolarPanel> Panels { get; set; }
                public List<IMyOxygenFarm> Farms { get; set; }
                public float Power { get; set; }
                public float Oxygen { get; set; }
                public float PowerOld { get; set; }
                public float OxygenOld { get; set; }

                public SolarArray(IMyMotorStator r, List<IMySolarPanel> p, List<IMyOxygenFarm> f) {
                    Rotor = r;
                    Panels = new List<IMySolarPanel>(p);
                    Power = 0f;
                    PowerOld = 0f;
                    Farms = new List<IMyOxygenFarm>(f);
                    Oxygen = 0f;
                    OxygenOld = 0f;
                }

                public bool Closed(IMyTerminalBlock b) {
                    return b == null || b.WorldMatrix == MatrixD.Identity;
                }
                public bool Update() {
                    if (Closed(Rotor))
                        return true;
                    PowerOld = Power;
                    Power = 0f;
                    OxygenOld = Oxygen;
                    Oxygen = 0f;
                    foreach (var v in Panels) {
                        if (Closed(v))
                            return true;
                        Power += v.MaxOutput;
                    }
                    foreach (var v in Farms) {
                        if (Closed(v))
                            return true;
                        Oxygen += v.GetOutput();
                    }

                    float current = Power;
                    float old = PowerOld;
                    if (Panels.Count == 0 && Farms.Count > 0) {
                        current = Oxygen;
                        old = OxygenOld;
                    }

                    if (current == old) {
                        Rotor.TargetVelocityRPM = 0f;
                    } else if (Rotor.TargetVelocityRPM != 0 && current < old) {
                        Rotor.TargetVelocityRPM = -Rotor.TargetVelocityRPM; // Moving, power < old = reverse direction
                    } else if (Rotor.TargetVelocityRPM == 0) {
                        Rotor.TargetVelocityRPM = SolarAlign;
                    }
                    return false;
                }
            }
        }
    }
    JNSolarTracker iJNSolarTracker = new JNSolarTracker();

    //======-SCRIPT ENDING-======
}