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
            public LocMem() {
                CabinName = GetMC("CabinName", "Cockpit");
                StopLampsGroupName = GetMC("StopLampsGroupName", "Lamps (red)");
            }

            public MemCell<string> CabinName, StopLampsGroupName;
        }

        class TP: SdSubP {

            ActI MA;
            LocMem Mem = new LocMem();
            ActI SLA;

            JNSController Cabin;
            List<IMyLightingBlock> StopLamps = new List<IMyLightingBlock>();
            bool IsStopLampsOn = false;
            bool IsGasUp = false;

            public TP(ushort id, InitSubP p) : base(id, p) {
                ///Mem = p.Mem as LocMem;
                Cabin = new JNSController(Mem.CabinName);
                OS.GTS.GetBlockGroupWithName(Mem.StopLampsGroupName)
                    .GetBlocksOfType(StopLamps);

                SLA = AddAct(SignalStop);
            }

            void SignalStop() {
                if (IsGasUp = Cabin.Move.Z < 0) {
                    if (IsStopLampsOn) {
                        StopLamps.ForEach(l => l.Color = Color.Black);
                        IsStopLampsOn = false;
                    }
                } else {
                    if (!IsStopLampsOn) {
                        StopLamps.ForEach(l => l.Color = Color.Red);
                        IsStopLampsOn = true;
                    }
                }
            }
        }
    }
    JBolid iBolid = new JBolid();

    /// <summary>
    /// Ship controller
    /// </summary>
    class JNSController {
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
        public Vector2 Slide => Controller.RotationIndicator;
        public float Roll => Controller.RollIndicator;
        public Vector3 Move => Controller.MoveIndicator;

        public JNSController(string name) {
            Controller = OS.GTS.GetBlockWithName(name) as IMyShipController;
        }

        #region Imputs detecting
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
        #endregion Imputs detecting

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
- стоп-сигнал
- таймер время
- скоростное ограничение питзоны
- поворот колес: про акерман или антиакерман?
- гироскопы: 
 -- стабилизация в плоскости (roll pitch)
 -- поворот (yaw)
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