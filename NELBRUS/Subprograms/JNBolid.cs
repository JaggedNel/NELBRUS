using System;
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

    class JBolid: InitSubP {
        public JBolid() : base("Bolid", new MyVersion(1, 0)) { }

        protected override SdSubP Init(ushort id) => new TP(id, this);

        class LocMem: MemReg {
            public LocMem(SdSubP p) : base(p) {
                CabinName = GetMC("CabinName", "Cockpit");
                StopLampsGroupName = GetMC("StopLampsGroupName", "Lamps (red)");
            }
            
            public MemCell<string> CabinName, StopLampsGroupName;
        }

        class TP: SdSubPCmd {

            ActI MA;
            LocMem Mem;
            ActI SLA;

            JNSCtrl Cabin;
            List<IMyLightingBlock> StopLamps = new List<IMyLightingBlock>();
            bool IsStopLampsOn = false;

            public TP(ushort id, InitSubP p) : base(id, p) {
                SetCmd(new Dictionary<string, Cmd>
                {
                    { "cabin", new Cmd(CmdCabin, "Change cabin name.") },
                });

                Mem = new LocMem(this);

                SLA = AddAct(SignalStop);
            }
            public override bool Init() {
                Cabin = JNSCtrl.Init(Mem.CabinName);
                OS.GTS.GetBlockGroupWithName(Mem.StopLampsGroupName)
                    .GetBlocksOfType(StopLamps);
                if (Cabin == null || !StopLamps.Any()) {
                    Terminate("Blocks initialize error.");
                    return false;
                }
                return true;
            }
            void SignalStop() {
                if (Cabin.Move.Z >= 0 || Cabin.Move.Y > 0 || Cabin.Controller.HandBrake) {
                    if (!IsStopLampsOn) {
                        StopLamps.ForEach(l => l.Color = Color.Red);
                        IsStopLampsOn = true;
                    }
                } else {
                    if (IsStopLampsOn) {
                        StopLamps.ForEach(l => l.Color = Color.Black);
                        IsStopLampsOn = false;
                    }
                }
            }

            string CmdCabin(List<string> a) => Mem.CabinName.V = a[0];
        }
    }
    JBolid iBolid = new JBolid();

    /// <summary> Ship controller </summary>
    class JNSCtrl {
        /// <summary> Slide buffer </summary>
        Vector2 SB;
        /// <summary> Roll buffer </summary>
        float RB;
        /// <summary> Move buffer </summary>
        Vector3 MB;

        IMyShipController _controller;
        public IMyShipController Controller {
            get { return _controller; }
            set {
                _controller = value;
                CS();
                CR();
                CM();
            }
        }
        public bool IsControl => Controller.IsUnderControl;

        // ⇓     => Y?
        // ⇨     => X?
        public Vector2 Slide => Controller.RotationIndicator;
        // Q     => -?
        // E     => +?
        public float Roll => Controller.RollIndicator;
        // W     => -Z
        // S     => +Z
        // A     => -X
        // D     => +X
        // Space => +Y
        // C     => -Y
        public Vector3 Move => Controller.MoveIndicator;

        JNSCtrl(IMyShipController c) {
            Controller = c;
        }

        /// <summary> Initialize new ship controller by block </summary>
        /// <param name="c"> Ship controller </param>
        public static JNSCtrl Init(IMyShipController c) => c == null ? null : new JNSCtrl(c);
        /// <summary> Initialize new ship controller by block </summary>
        /// <param name="n"> Name of ship controller block </param>
        public static JNSCtrl Init(string n) => Init(OS.GTS.GetBlockWithName(n) as IMyShipController);

        #region Inputs detecting

        public void UpdSlide() {
            SB += Slide;
        }
        public Vector2 GetSlide() => SB;
        public Vector2 PopSlide() {
            var t = SB;
            CS();
            return t;
        }
        public void UpdRoll() {
            RB += Roll;
        }
        public float GetRoll() => RB;
        public float PopRoll() {
            var t = RB;
            CR();
            return t;
        }
        public void UpdMove() {
            MB += Controller.MoveIndicator;
        }
        public Vector3 GetMove() => MB;
        public Vector3 PopMove() {
            var t = MB;
            CM();
            return t;
        }

        #endregion Inputs detecting

        #region Buffers clearing

        void CS() {
            SB = new Vector2();
        }
        void CR() {
            RB = 0;
        }
        void CM() {
            MB = new Vector3();
        }

        #endregion Buffers clearing
    }

    //======-SCRIPT ENDING-======
}

/*
- стоп-сигнал +
- таймер время
- скоростное ограничение питзоны
- поворот колес:
 -- про акерман или антиакерман?
- гироскопы: 
 -- стабилизация в плоскости (roll pitch)
 -- поворот (yaw)
- Крабик для питстопа
- АБС?
- активная настройка подвески
- управление стиком
 */

/*
Gyroscopes	Yaw <-> Roll
Hydrogen generators
Hydrogen tanks
Lamps (red)
Wheels
Wheels (front)
Wheels (rear)
Cockpit
 */