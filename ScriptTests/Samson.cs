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

namespace ScriptTests.Samson {

    public class Program: MyGridProgram {
        //======-SCRIPT BEGINNING-======


        //// НАЧАЛО СКРИПТА
        ////Initialisation
        //PrBase thisPr;//[0]
        //static Program refPr; //ссылка для работы некоторых функций//[0]
        //NetNode thisNd;
        //Program() {
        //    thisPr = new PrBase("IVN - Inter Vessel Network", "[IVN]LCD log", "JaggedNel", "Dev", "[IVN]");//[0]
        //    refPr = this;//[0]
        //    thisNd = new NetNode();
        //    Runtime.UpdateFrequency = UpdateFrequency.Update1;
        //    if (thisPr.logAvail = PrBase.GetLCD(ref thisPr.LCDlog, thisPr.LCDlog_Name)) thisPr.ToLog("New session initialized: Program: " + PrBase.prName + " Version: " + PrBase.version);//[0]
        //    thisNd.MakeIP();

        //    Me.CustomData = "IP: " + thisNd.MyIP + "\nPos: " + Me.GetPosition();
        //}
        //void Save() {
        //    if (thisPr.logAvail) thisPr.ToLog("Status has been saved");
        //}
        //void Main(string inArg, UpdateType uType) {
        //    if (thisPr.logAvail)
        //        if (inArg != "") thisPr.ToLog("Run by [" + uType + "] with arg.: " + inArg);
        //        else if (PrBase.CurTick() % 600 == 0) thisPr.ToLog("ChPoint.");

        //    //UpdateType.A:
        //    thisPr.CloseTick();//[0]
        //}//main

        ////Блок базового функционала программы JNel v1.0
        //class PrBase {
        //    static public string prName;
        //    public readonly string LCDlog_Name;
        //    static public string author, version, abbreviation;

        //    static Indicator ind; //индикатор работы программы
        //    private static ushort curTick { get; set; }

        //    public PrBase(string name, string logName, string aut, string ver, string abbr) {
        //        prName = name; LCDlog_Name = logName; author = aut; version = ver; abbreviation = abbr;
        //        ind = new Indicator();
        //        curTick = 0;
        //    }
        //    public void CloseTick() {
        //        if (CurTick() % ind.freq == 0) ind.Refresh();
        //        curTick++;
        //        if (CurTick() == 3600) curTick = 0;
        //    }
        //    static public ushort CurTick() { return curTick; }

        //    public IMyTextPanel LCDlog = null;
        //    public bool logAvail = false; //имеется LCD для хранения лога

        //    //Возратно-поступательный индикатор JNel v2.0
        //    public class Indicator {
        //        public sbyte curVarInd = 0, freq = 30; sbyte indMod = 1;
        //        public readonly string[] variantsInd = { "(._.)", "   ( l: )", "      (.–.)", "         ( :l )", "            (._.)" };
        //        public void Refresh() {
        //            curVarInd += indMod;
        //            if (curVarInd == variantsInd.Length - 1 || curVarInd == 0) indMod = (sbyte)-indMod;
        //            RefreshEcho();
        //        }
        //    }

        //    static public void RefreshEcho() {
        //        string outputText = prName + '\n';
        //        outputText += "By " + author + "\nVersion: " + version + '\n';
        //        outputText += "I`m working   " + ind.variantsInd[ind.curVarInd] + "\n";
        //        refPr.Echo(outputText);
        //    }
        //    static public string CurTime() { return DateTime.Now.ToString("[HH:mm:ss]"); }
        //    static public bool GetLCD(ref IMyTextPanel thisLCD, string thisLCDname) {
        //        thisLCD = refPr.GridTerminalSystem.GetBlockWithName(thisLCDname) as IMyTextPanel;
        //        return thisLCD != null;
        //    }
        //    public void ToLog(string logMes) {
        //        string savedLog = LCDlog.GetPublicText();
        //        LCDlog.WritePublicText(CurTime() + ' ' + logMes + '\n' + savedLog, false);
        //    }
        //}//class PrBase
        ////Блок функционала узла сети
        //class NetNode {
        //    public string MyIP;
        //    public string MakeIP() {
        //        string[] ip = new string[3];
        //        ip[0] = ConvertDegree((int)(refPr.Me.GetPosition().X * 100));
        //        ip[1] = ConvertDegree((int)(refPr.Me.GetPosition().Y * 100));
        //        ip[2] = ConvertDegree((int)(refPr.Me.GetPosition().Z * 100));
        //        for (int i = 0; i < 3; i++)
        //            if (ip[i][0] == '-') ip[i] = ip[i].Replace('-', ':');
        //            else ip[i] = ip[i] = ip[i].Replace('+', '.');
        //        ip[0] += ip[1] + ip[2];
        //        return MyIP = ConvertDegree(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second).Remove(0, 1) + ip[0];
        //    }
        //}
        ////Кодирование числа в 176 систему счисления
        //static string ConvertDegree(int number) {
        //    const string alf = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюяÄäßÇ¢çĎďĚěєƒĞğĦħĪīĴĵЌќ£ĿŀŇňÕõøØþÞŘŗšŠ§$Ţţ†ŦÛûŴŵ×";
        //    string res = "";
        //    int ost, razr = alf.Length;
        //    char neg = '+';
        //    if (number < 0) { number = -number;  neg = '-'; }
        //    while (number > razr) {
        //        ost = number % razr;
        //        number /= razr;
        //        res = alf[ost] + res;
        //    }
        //    res = alf[number] + res;
        //    return neg + res;
        //}


        //// КОНЕЦ СКРИПТА
        ///

        // Special content for SAMSON by JaggedNel
        // ------------------------------------------------------------------------------------------
        // The script is fully configured and does not require your intervention
        // ------------------------------------------------------------------------------------------

        bool automaticStandingMode = true, manualStandingMode = true, currentModeIsStanding = false, underControl = false, gyroEnable = true, needGyro, lowFriction = true, notPil = true, rotNorm = false, tempB;
        //aSM is a flag that responsible for autoswitch driving mode on high speed regardless of the driver
        //mSM is a flag that responsible for autoswitch aiming mode on low speed
        //gE is a flag that responsible for enable all gyroscopes when turn and disable when no because when gyroscopes enable all time tank is not react to relief
        //nG this flag indicates that we need to control with the gyros. When this is false gyroscopes will not be turned on regardless of the gE
        //nP is flag of no control
        //rN is turn flag

        double currentShipSpeed, azimuthToTarget, elevationToTarget, changedAzimuth, changedElevation;
        char lastDriving = ' '; //Flag of last movement direction

        IMyShipController CurrentController;
        List<IMyShipController> ShipControllers = new List<IMyShipController>();

        List<IMySmallMissileLauncher> MissileLaunchers = new List<IMySmallMissileLauncher>();

        List<IMyMotorSuspension> Suspensions = new List<IMyMotorSuspension>(), LeftSuspensions = new List<IMyMotorSuspension>(), RightSuspensions = new List<IMyMotorSuspension>(), BasisSuspensions = new List<IMyMotorSuspension>(), OutsideRollers = new List<IMyMotorSuspension>(), InsideRollers = new List<IMyMotorSuspension>();

        List<IMyGyro> Gyros = new List<IMyGyro>();

        Vector2 MouseInput, MouseVec, GuidanceDirection;
        //MV is a mouse movement vector
        //GD is a specify to change the target vector after all mouse movement transformations
        //X vector is elevation, Y vector is azimuth
        Vector3 WASDinputVec;
        //Z vector is forward/backward, X vector is left/right, Z vector is up/down
        Vector3D TargetVec, TargetVecLoc;
        //TVL is a vector to target in the coordinate system of the tank

        byte currentTick = 0;
        int dividerOperationFrequency, dividerIndFrequency; //Every dOF run main functional and every dIF update indicator. Definition in Initiation function

        //Block of setting constants
        public class SettingsSystem {
            public byte operationFrequency = 20, indFrequency = 2; //Frequency of run main functional and indicator updates
            public float standingSpeedLimit = 10, mouseMult = 0.0003f, dividerMouseMult = 3, radMult = (float)Math.PI / 180 / 2, elevGyroLimit = 0.1f;
            public bool start = true; //Flag of necessity initiation, after initiation will be false all time
        }
        public class SettingsBasisSusp {
            public int R = 3;
            public float maxHeightSt = 0.21f, minHeightSt = -0.32f, HeightMarch = 0.12f, midHeight, locHeight;
            public List<float> luftUp, luftDown;
        }
        public class SettingsOutSusp {
            public float HeightSt = 0.26f, HeightMarch = 0.07f;
        }
        public class SettingsInSusp {
            public int R = 5;
            public float maxHeightSt = 0.06f, minHeightSt = -0.26f, HeightMarch = -0.32f, midHeight, locHeight;
            public List<float> luftUp, luftDown;
        }
        SettingsSystem MySystem = new SettingsSystem();
        SettingsBasisSusp MyBasisSusp = new SettingsBasisSusp();
        SettingsOutSusp MyOutSusp = new SettingsOutSusp();
        SettingsInSusp MyInSusp = new SettingsInSusp();

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string inputArgument) {
            if (MySystem.start)
                Initiation(); //Initiation when program start working
            if (inputArgument != "")
                GetInputArgument(inputArgument); //Receiving command
            if (currentTick == 60)
                currentTick = 0; //Opetating cycle is second
            if (currentTick % dividerIndFrequency == 0)
                RefreshWorkingIndicator();
            underControl = underControl ? CurrentController.IsUnderControl : FindCurrentControl();

            //Summation of mouse movements
            if (underControl) {
                MouseVec = GetMouseMovement();
                GuidanceDirection += MouseVec;
            }

            if (currentTick % dividerOperationFrequency == 0) //Run every dOF ticks
            {
                if (underControl) {
                    notPil = false;

                    WASDinputVec = CurrentController.MoveIndicator; //WASDCF input
                    if (WASDinputVec.X == 0) //If A or D button is not pressed (mouse aiming)
                    {
                        if (lastDriving != 'f') //Forward or backward driving
                        {
                            foreach (IMyMotorSuspension thisSusp in Suspensions) {
                                thisSusp.SetValue("Propulsion override", 0f);
                                thisSusp.Friction = 100f;
                            }
                            foreach (IMyGyro thisGyro in Gyros)
                                thisGyro.Yaw = 0;
                            TargetVec = CurrentController.WorldMatrix.Forward;
                            lastDriving = 'f';
                            lowFriction = true;
                            needGyro = false;
                        }

                        if (manualStandingMode) {
                            currentShipSpeed = CurrentController.GetShipSpeed();
                            automaticStandingMode = currentShipSpeed > MySystem.standingSpeedLimit ? false : true;
                            if (automaticStandingMode)
                            //Aim mode block
                            {
                                if (!currentModeIsStanding) {
                                    //Switch aim mode
                                    SetSuspHeightGr(OutsideRollers, MyOutSusp.HeightSt);
                                    currentModeIsStanding = true;
                                }

                                // Special content for SAMSON by JaggedNel

                                bool vecChanged = GuidanceDirection.X != 0 || GuidanceDirection.Y != 0; //Did the mouse move?

                                if (vecChanged || currentShipSpeed > 0 || Math.Abs(azimuthToTarget) >= 0.015) //Turning in the direction of the target vector
                                {
                                    MatrixD Orientation = CurrentController.WorldMatrix.GetOrientation();
                                    TargetVecLoc = VectorTransform(TargetVec, Orientation);

                                    //Azimuth and elevation in radians
                                    azimuthToTarget = Math.Atan2(-TargetVecLoc.X, TargetVecLoc.Z);
                                    elevationToTarget = Math.Asin(-TargetVecLoc.Y / TargetVecLoc.Length());

                                    //Azimuth and elevation based on mouse 
                                    changedAzimuth = azimuthToTarget - GuidanceDirection.Y;
                                    if (!(elevationToTarget < -0.1753 && GuidanceDirection.X < 0 || elevationToTarget > 0.1753 && GuidanceDirection.X > 0))
                                        //Elevation does not change so as not to run out of the available slope range
                                        changedElevation = elevationToTarget + GuidanceDirection.X;

                                    //Recovery extreme values of the range
                                    if (changedElevation > 0.53)
                                        changedElevation = 0.53;
                                    else if (changedElevation < -0.53)
                                        changedElevation = -0.53;

                                    float tangElevation = (float)Math.Tan(changedElevation);

                                    //Adjust suspension height for height for elevation guidance
                                    SetModSuspHeightGr(BasisSuspensions, MyBasisSusp.luftUp, MyBasisSusp.luftDown, MyBasisSusp.midHeight, tangElevation, 1);
                                    SetModSuspHeightGr(InsideRollers, MyInSusp.luftUp, MyInSusp.luftDown, MyInSusp.midHeight, tangElevation, 0.42f);

                                    if (vecChanged)
                                        TargetVec = VectorUnTransofrm(Math.PI / 2 - changedElevation, changedAzimuth, TransposeMatr(Orientation)); //Transferring a changed vector from a local coordinate system to a global
                                }

                                if (Math.Abs(azimuthToTarget) >= 0.015) //Turn to target
                                {
                                    rotNorm = false;
                                    float force = -(float)azimuthToTarget * 2;
                                    foreach (IMyMotorSuspension thisSusp in Suspensions)
                                        thisSusp.SetValue("Propulsion override", force);
                                    foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                                        if (thisSusp.Position.Z != 1)
                                            thisSusp.Friction = 40f; //A stong friction of not central whells stopping the rotation so make it less

                                    if (WASDinputVec.Z != 0) {
                                        if (WASDinputVec.Z < 0) {
                                            if (azimuthToTarget > 0)
                                                foreach (IMyMotorSuspension thisSusp in LeftSuspensions)
                                                    thisSusp.SetValue("Propulsion override", 0.2f);
                                            else if (azimuthToTarget < 0)
                                                foreach (IMyMotorSuspension thisSusp in RightSuspensions)
                                                    thisSusp.SetValue("Propulsion override", -0.2f);
                                        } else {
                                            if (azimuthToTarget > 0)
                                                foreach (IMyMotorSuspension thisSusp in LeftSuspensions)
                                                    thisSusp.SetValue("Propulsion override", -0.2f);
                                            else if (azimuthToTarget < 0)
                                                foreach (IMyMotorSuspension thisSusp in RightSuspensions)
                                                    thisSusp.SetValue("Propulsion override", 0.2f);
                                        }
                                    }
                                } else
                                if (!rotNorm) //Stop the turn
                                {
                                    rotNorm = true;
                                    foreach (IMyMotorSuspension thisSusp in Suspensions)
                                        thisSusp.SetValue("Propulsion override", 0f);
                                    foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                                        thisSusp.Friction = 100;
                                }
                                //End of aim mode block
                            } else {
                                if (currentModeIsStanding)
                                    SetMarchMode();
                            }
                        } else {
                            if (currentModeIsStanding)
                                SetMarchMode();
                        }
                    } else //Turn when A or D button is pressed
                      {
                        needGyro = true;
                        float tempF;
                        if (lowFriction)
                            tempF = 5;
                        else
                            tempF = 50;
                        foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                            thisSusp.Friction = tempF;
                        lowFriction = !lowFriction;

                        //Does needed to invert a propulsion direction
                        tempB = WASDinputVec.X < 0 && WASDinputVec.Z <= 0 || WASDinputVec.X > 0 && WASDinputVec.Z > 0;

                        if (tempB) //Left turn
                        {
                            if (lastDriving != 'l') //Set the settings for left turn (forward or backward it does not matter thanks to tempB)
                            {
                                foreach (IMyMotorSuspension thisSusp in Suspensions)
                                    thisSusp.SetValue("Propulsion override", -1f);
                                foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                                    if (thisSusp.Position.Z != 1)
                                        thisSusp.Friction = 5;
                                foreach (IMyMotorSuspension thisSusp in InsideRollers)
                                    thisSusp.Friction = 1;
                                foreach (IMyMotorSuspension thisSusp in OutsideRollers)
                                    thisSusp.Friction = 1;
                                lastDriving = 'l';
                            }
                            foreach (IMyGyro thisGyro in Gyros)
                                thisGyro.Yaw = -1.047f * 2;

                        } else //Right turn
                          {
                            if (lastDriving != 'r') //Set the settings for right turn (forward or backward it does not matter thanks to tempB)
                            {
                                foreach (IMyMotorSuspension thisSusp in Suspensions)
                                    thisSusp.SetValue("Propulsion override", 1f);
                                foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                                    if (thisSusp.Position.Z != 1)
                                        thisSusp.Friction = 5;
                                foreach (IMyMotorSuspension thisSusp in InsideRollers)
                                    thisSusp.Friction = 1;
                                foreach (IMyMotorSuspension thisSusp in OutsideRollers)
                                    thisSusp.Friction = 1;
                                lastDriving = 'r';
                            }
                            foreach (IMyGyro thisGyro in Gyros)
                                thisGyro.Yaw = 1.047f * 2;
                        }
                    }
                } else {
                    if (!notPil) {
                        foreach (IMyMotorSuspension thisSusp in Suspensions)
                            thisSusp.SetValue("Propulsion override", 0f); //Discharge gas
                        foreach (IMyMotorSuspension thisSusp in BasisSuspensions)
                            thisSusp.Friction = 100;
                        notPil = true;
                    }
                }
                GuidanceDirection.X = 0;
                GuidanceDirection.Y = 0; //Discharge guidance
            }

            //In cycle: gyros is enabled - disabled - enabled ...
            tempB = gyroEnable && needGyro;
            foreach (IMyGyro thisGyro in Gyros)
                thisGyro.Enabled = tempB;
            gyroEnable = !gyroEnable;

            currentTick++;
        }

        void Initiation() {
            MySystem.start = false; //Change of flag

            //Receiving function blocks
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(ShipControllers);
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(MissileLaunchers);
            GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(Suspensions);
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyros);

            //Determining blocks belongs to which group
            foreach (IMyMotorSuspension thisSuspension in Suspensions) {
                //Determing left or right side
                //This is for tank steering. Method of determing is not safe for blueprints which have ship controllers with not in a direct direction or not between whill suspension blocks
                if (ShipControllers[0].WorldMatrix.Left.Dot(thisSuspension.WorldMatrix.Up) > 0.9)
                    LeftSuspensions.Add(thisSuspension);
                else if (-(ShipControllers[0].WorldMatrix.Left.Dot(thisSuspension.WorldMatrix.Up)) > 0.9)
                    RightSuspensions.Add(thisSuspension);

                //Determing of group to choose mаnagement method
                //Description of groups in the application
                if (thisSuspension.Position.Y == 4)
                    BasisSuspensions.Add(thisSuspension);
                else
                    switch (Math.Abs(thisSuspension.Position.Z)) {
                        case 5:
                            InsideRollers.Add(thisSuspension);
                            break;
                        case 7:
                            OutsideRollers.Add(thisSuspension);
                            break;
                    }
            }

            //Getting constant in order not to calculate each time
            dividerOperationFrequency = 60 / MySystem.operationFrequency;
            dividerIndFrequency = 60 / MySystem.indFrequency;

            //Setting the settings
            SuspensionInitiation(BasisSuspensions, MyBasisSusp.maxHeightSt, MyBasisSusp.minHeightSt, MyBasisSusp.R, ref MyBasisSusp.midHeight, ref MyBasisSusp.locHeight, ref MyBasisSusp.luftUp, ref MyBasisSusp.luftDown);
            SuspensionInitiation(InsideRollers, MyInSusp.maxHeightSt, MyInSusp.minHeightSt, MyInSusp.R, ref MyInSusp.midHeight, ref MyInSusp.locHeight, ref MyInSusp.luftUp, ref MyInSusp.luftDown);
            SetSuspHeightGr(BasisSuspensions, MyBasisSusp.midHeight);
            SetSuspHeightGr(OutsideRollers, MyOutSusp.HeightSt);
            SetSuspHeightGr(InsideRollers, MyInSusp.midHeight);

            GuidanceDirection.X = 0;
            GuidanceDirection.Y = 0;
        }

        // Special content for SAMSON by JaggedNel

        /// <summary>
        /// Determination of coefficients for suspensions settings depending on the position and common group constants
        /// </summary>
        void SuspensionInitiation(List<IMyMotorSuspension> thisGroup, float maxHeight, float minHeight, int r, ref float midHeight, ref float locHeight, ref List<float> luftMax, ref List<float> luftMin) {
            luftMax = new List<float>();
            luftMin = new List<float>();
            int tempI;
            float luft;
            midHeight = (minHeight + maxHeight) / 2;
            locHeight = maxHeight - midHeight;
            for (int i = 0; i < thisGroup.Count; i++) {
                tempI = Math.Abs(thisGroup[i].Position.Z);
                luft = locHeight * tempI / r;
                luftMax.Add(midHeight + luft);
                luftMin.Add(midHeight - luft);
            }
        }
        //Indicators block
        sbyte indVariant = 0;
        sbyte indModifer = 1;
        string currentIndicator;
        string[] indicators = { "(._.)", "   ( I: )", "      (.–.)", "         ( :I )", "            (._.)" };
        void RefreshWorkingIndicator() {
            currentIndicator = indicators[indVariant];
            indVariant += indModifer;
            if (indVariant == indicators.Length - 1 || indVariant == 0)
                indModifer = (sbyte)-indModifer;
            RefreshEcho();
        }
        void RefreshEcho() {
            string outputText = "";
            outputText += "- Special Script for SAMSON" + "\n";
            outputText += "- Made by JaggedNel" + "\n";
            outputText += "- I am working   " + currentIndicator;
            Echo(outputText);
        }
        //End of indicators block
        void GetInputArgument(string input) {
            switch (input) {
                case "Switch manual standing mode":
                    manualStandingMode = !manualStandingMode;
                    break;
            }
        }
        void SetMarchMode() {
            foreach (IMyMotorSuspension thisSusp in Suspensions)
                thisSusp.SetValue("Propulsion override", 0f);
            SetSuspHeightGr(BasisSuspensions, MyBasisSusp.HeightMarch);
            SetSuspHeightGr(OutsideRollers, MyOutSusp.HeightMarch);
            SetSuspHeightGr(InsideRollers, MyInSusp.HeightMarch);
            currentModeIsStanding = false;
        }
        Vector2 GetMouseMovement() {
            MouseInput = CurrentController.RotationIndicator;
            Vector2 tempV2 = MouseInput;
            return tempV2 *= MySystem.mouseMult;
        }
        bool FindCurrentControl() {
            foreach (IMyShipController Contr in ShipControllers)
                if (Contr.IsUnderControl) {
                    CurrentController = Contr;
                    TargetVec = CurrentController.WorldMatrix.Forward;
                    return true;
                }
            return false;
        }
        void SetSuspHeightGr(List<IMyMotorSuspension> thisGroup, float height) {
            foreach (IMyMotorSuspension thisSuspension in thisGroup)
                thisSuspension.Height = height;
        }

        /// <summary>
        /// Soft reduction of the suspension height to the required value for a certain time
        /// </summary>
        void SetModSuspHeightGr(List<IMyMotorSuspension> thisGroup, List<float> luftMax, List<float> luftMin, float midHeight, float tang, float mod) {
            float tempF;
            int tempI;
            for (int i = 0; i < thisGroup.Count; i++) {
                tempI = thisGroup[i].Position.Z;
                tempF = midHeight + 0.5f * tempI * tang * mod;
                if (tempF > luftMax[i])
                    thisGroup[i].Height = luftMax[i];
                else if (tempF < luftMin[i])
                    thisGroup[i].Height = luftMin[i];
                else
                    thisGroup[i].Height = tempF;
            }
        }

        public Vector3D VectorUnTransofrm(double angEl, double angAz, MatrixD TOrientation) {
            //Fckng mathematic magic and i dont remember why this is so
            double X, Y, Z;
            X = Math.Sin(angEl) * Math.Sin(angAz);
            Y = Math.Cos(angEl);
            Z = Math.Sin(angEl) * Math.Cos(angAz);
            Vector3D ChangedVecLoc = new Vector3D(X, Y, Z);
            return VectorTransform(ChangedVecLoc, TOrientation);
        }
        /// <summary>
        /// Transfer of coordinates of Vec to Orientation coordinate system
        /// </summary>
        public Vector3D VectorTransform(Vector3D Vec, MatrixD Orientation) {
            return new Vector3D(Vec.Dot(Orientation.Right), Vec.Dot(Orientation.Up), Vec.Dot(Orientation.Forward));
        }
        public MatrixD TransposeMatr(MatrixD O) //(Pseudotranspose)
        {
            return new MatrixD(-O.M11, -O.M21, -O.M31, -O.M12, -O.M22, -O.M32, O.M13, O.M23, O.M33);
        }

        //======-SCRIPT ENDING-======
    }

}