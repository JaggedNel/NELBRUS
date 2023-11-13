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

public partial class Program : MyGridProgram
{
    //======-SCRIPT BEGINNING-======

    /// <summary> Subprogram initilizer class base </summary>
    class InitSubP: SubP {
        /// <summary> Started subprogram instance </summary>
        SdSubP I;

        InitSubP(string n, string i, MyVersion v) : base(n, v, i) { }
        public InitSubP(string name, MyVersion v = null, string info = CONST.NA) : base(name, v, info) {
            OS.ISP(this);
        }
        public InitSubP(string name, string info) : this(name, null, info: info) { }

        /// <summary> Get subprogram </summary>
        /// <param name="id"> Identificator of new subprogram </param>
        /// <returns> Started subprogram </returns>
        public SdSubP Run(ushort id) => I ?? (I = Init(id));
        /// <summary> Stop started subprogram </summary>
        public void Stop() {
            OS.SSP(I);
            I = null;
        }
        /// <summary> Initiate new subprogram </summary>
        /// <param name="id"> Identificator of new subprogram </param>
        /// <returns> Started subprogram </returns>
        protected virtual SdSubP Init(ushort id) => null;

        public static InitSubP GetPlug(string name, MyVersion v = null, string info = CONST.NA) => new InitSubP(name, info, v);
    }

    //======-SCRIPT ENDING-======
}