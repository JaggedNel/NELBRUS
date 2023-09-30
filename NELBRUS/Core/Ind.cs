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

public partial class Program : MyGridProgram
{
    //======-SCRIPT BEGINNING-======

    /// <summary> Indicator automat </summary>
    class Ind {
        /// <summary> Current variant </summary>
        int CV;
        /// <summary> Indicator variants </summary>
        string[] Vrnts;
        /// <summary> Direction iterator </summary>
        int i;

        /// <summary> Method to update indicator </summary>
        /// <param name="cv"> Current variant </param>
        /// <param name="v"> Indicator variants </param>
        /// <param name="i"> Direction iterator </param>
        public delegate void UpdMethod(ref int cv, string[] v, ref int i);
        UpdMethod UM;

        /// <param name="um"> Method to update indicator </param>
        /// <param name="v"> Variants </param>
        public Ind(UpdMethod um, params string[] v) {
            CV = 0;
            UM = um;
            Vrnts = v;
            i = 1;
        }

        /// <summary> Update the indicator </summary>
        public void Upd() {
            UM(ref CV, Vrnts, ref i);
        }
        /// <summary> Get current indicator </summary>
        public string Get() => Vrnts[CV];

        #region UpdateMethods

        /// <summary> Update in order then reverse </summary>
        public static void UpdTurn(ref int cv, string[] v, ref int i) {
            cv += i;
            if (cv >= v.Length - 1 || cv <= 0)
                i *= -1;
        }
        /// <summary> Update in order then repeat </summary>
        public static void UpdRepeat(ref int cv, string[] v, ref int i) {
            cv = cv >= v.Length - 1 ? 0 : cv + i;
        }

        #endregion UpdateMethods
    }

    //======-SCRIPT ENDING-======
}