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

using VRage.Game.GUI.TextPanel;
using VRage;

namespace ScriptTests.Yasen {

    public class Program: MyGridProgram {
        //======-SCRIPT BEGINNING-======

        #region Properties

        const string _debugLCDTag = "Отладка";
        const int ReInitTime = 360;
        Color _interfaceColor = new Color(179, 237, 255, 255);
        Color _targetColor = new Color(0, 255, 0);
        Color _ballisticColor = new Color(0, 150, 0);
        Color _weaponColor = new Color(255, 0, 0);
        Color _powerColor = new Color(255, 255, 0);
        Color _propulsionColor = new Color(0, 0, 255);
        static int _unlockTime = 180;
        const int timeToUpdateSettings = 5;
        string _updateInfo, _statusInfo, _debuginfo = "";
        static MyIni languageIni = new MyIni();
        bool b = languageIni.TryParse(Languages.storage);
        List<IMyTerminalBlock> _allBlocks = new List<IMyTerminalBlock>();
        List<IMyUserControllableGun> _allGuns = new List<IMyUserControllableGun>();
        List<IMyUserControllableGun> _myGuns = new List<IMyUserControllableGun>();
        List<IMySmallGatlingGun> _gatlings = new List<IMySmallGatlingGun>();
        List<IMySmallMissileLauncher> _mLaunchers = new List<IMySmallMissileLauncher>();
        List<IMyMotorStator> _allRotors = new List<IMyMotorStator>();
        List<IMyMotorStator> _rotorsE = new List<IMyMotorStator>();
        List<IMyCameraBlock> _myCameras = new List<IMyCameraBlock>();
        List<IMyCameraBlock> _radarCameras = new List<IMyCameraBlock>();
        List<IMyCameraBlock> _allCameras = new List<IMyCameraBlock>();
        List<IMyTextPanel> _textPanels = new List<IMyTextPanel>();
        List<IMyShipController> _shipControllers = new List<IMyShipController>();
        List<IMyGyro> _myGyro = new List<IMyGyro>();
        List<IMyLargeTurretBase> _turrets = new List<IMyLargeTurretBase>();
        List<IMyTurretControlBlock> _TCs = new List<IMyTurretControlBlock>();
        IMyBroadcastListener _missilesListener;
        IMyTextPanel _debugLCD;
        IMyBlockGroup _FCSGroup;
        IMyMotorStator _rotorA;
        IMyShipController _myShipController;
        IMyShipController _activeShipController;
        Turret _turret = new Turret();
        HullGuidance Hull = new HullGuidance();
        Radar _radar;
        TurretRadar _turretRadar = new TurretRadar();
        Dictionary<int, MyTuple<string, float, float>> _observerInSC = 
            new Dictionary<int, MyTuple<string, float, float>>{
                {0,new MyTuple<string,float,float>("SCOCKPIT",0.46f,0.28f)},
                {1,new MyTuple<string,float,float>("FCOCKPIT",0.46f,0.28f)},
                {2,new MyTuple<string,float,float>("SCSEAT",0.46f,0.28f)},
                {3,new MyTuple<string,float,float>("LCOCKPIT",0.5f,0.12f)},
                {4,new MyTuple<string,float,float>("CSEAT",0.5f,0.12f)},
                {5,new MyTuple<string,float,float>("CUSTOM",0.5f,0.12f)},
            };
        Dictionary<int, MyTuple<string, float, float, float>> _weaponDict = 
            new Dictionary<int, MyTuple<string, float,float, float>>{
                {0,new MyTuple<string,float,float,float>("GUTLING",400,800,1/11.67f)},
                {1,new MyTuple<string,float,float,float>("AUTOCANON",400,800,1/2.5f)},
                {2,new MyTuple<string,float,float,float>("ASSAULT",500,1400,6)},
                {3,new MyTuple<string,float,float,float>("ARTY",500,2000,12)},
                {4,new MyTuple<string,float,float,float>("SRAILGUN",1000,1400,20)},
                {5,new MyTuple<string,float,float,float>("LRAILGUN",2000,2000,60)},
                {6,new MyTuple<string,float,float,float>("CUSTOM",0,0,0)},
            };
        MyIni _myIni = new MyIni();
        const string
            INI_SECTION_NAMES = "Names",
            INI_LANGUAGE = "Language",
            INI_GROUP_NAME_TAG = "Group name tag",
            INI_AZ_ROTOR_NAME_TAG = "Azimuth Rotor name tag",
            INI_EL_ROTOR_NAME_TAG = "Elevation Rotor name tag",
            INI_MAIN_COCKPIT_NAME_TAG = "Main Cockpit name tag",
            INI_SECTION_RADAR = "Radar",
            INI_INITIAL_RANGE = "Initial Range",
            INI_SECTION_CONTROLS = "Controls",
            INI_EL_MULT = "Elevation Rotor Multiplier",
            INI_AZ_MULT = "Azimuth Rotor Multiplier",
            INI_YAW_MULT = "Yaw Gyro Multiplier",
            INI_PITCH_MULT = "Pitch Gyro Multiplier",
            INI_SECTION_WEAPON = "Weapon",
            INI_NUMBER_OF_WEAPON = "Number of weapon",
            INI_WEAPON_SHOOT_VELOCITY = "Projectile velocity",
            INI_WEAPON_FIRE_RANGE = "Shot range",
            INI_WEAPON_RELOAD_TIME = "Reload Time",
            INI_SECTION_COCKPIT = "Cockpit",
            INI_NUMBER_OF_COCKPIT = "Number of cockpit",
            INI_COEF_UP = "Observer position - up",
            INI_COEF_BACK = "Observer position - back",
            INI_SECTION_TARGETS = "Targets",
            INI_ENEMY = "Enemy",
            INI_NEUTRAL = "Neutral",
            INI_ALLIE = "Allie",
            INI_DISPLAYED_TARGET = "Displayed Target";
        string
            _language = "English",
            _FCSTag = "Ash",
            _azimuthRotorTag = "Azimuth",
            _elevationRotorTag = "Elevation",
            _mainCockpitTag = "Main";
        float
            elevationSpeedMult = 0.005f,
            azimuthSpeedMult = 0.005f,
            yawMult = 0.001f,
            pitchMult = 0.001f,
            _myWeaponShotVelocity = 400,
            _myWeaponRangeToFire = 800,
            _myWeaponReloadTime = 1 / 2.5f,
            _obsCoefUp = 0,
            _obsCoefBack = 0,
            _initialRange = 2000;
        bool
            isTurret = false,
            canAutoTarget = false,
            stabilization = true,
            settingsMode = false,
            autotarget = false,
            aimAssist = false,
            isVehicle = false,
            getTarget = false,
            drawTank = true;
        long Tick = 0;
        bool 
            allie = false,
            enemy = true,
            neutral = true;
        IMyMotorStator _mainElRotor;
        float 
            horizont = 0,
            vertical = 0,
            xMove = 0,
            yMove = 0,
            menuMove = 0;
        int 
            menuTimer = 0,
            menuTab = 0,
            menuline = 0,
            myCockpit = 0,
            myWeapon = 0,
            _targetingPoint = 1;

        #endregion Properties

        #region Common

        Program() {
            _missilesListener = IGC.RegisterBroadcastListener(Me.EntityId.ToString());
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }
        void Main(string argument, UpdateType updateSource) {
            if (menuTimer > 0)
                menuTimer--;
            _statusInfo = "\nSystem status:\n";

            // Обновление блоков
            if ((Tick % ReInitTime) == 0) {
                UpdateBlocks(ref _updateInfo);
            }

            _activeShipController = null;
            switch (argument) {
                case "settings":
                    if (settingsMode) {
                        menuTab = 0;
                        menuline = 0;
                        settingsMode = false;
                        SC();
                    } else
                        settingsMode = true;
                    break;
                case "action":
                    getTarget = true;
                    break;
                case "switch_lock":
                    if (_radar.Searching) { 
                        _radar.DropLock();
                    } else
                        _radar.Searching = true;
                    break;
                case "switch_aimAssist":
                    aimAssist = !aimAssist;
                    autotarget = false;
                    break;
                case "use_gatling":
                    myWeapon = 0;
                    SC();
                    break;
                case "use_autoCanon":
                    myWeapon = 1;
                    SC();
                    break;
                case "use_assaultCanon":
                    myWeapon = 2;
                    SC();
                    break;
                case "use_artillery":
                    myWeapon = 3;
                    SC();
                    break;
                case "use_smallRail":
                    myWeapon = 4;
                    SC();
                    break;
                case "use_largeRail":
                    myWeapon = 5;
                    SC();
                    break;
                case "switch_aiMode":
                    if (aimAssist && autotarget) {
                        aimAssist = false;
                        autotarget = false;
                    } else {
                        if (!aimAssist)
                            aimAssist = true;
                        else
                            autotarget = !autotarget;
                    }
                    break;
                case "switch_stab":
                    stabilization = !stabilization;
                    break;
                default:
                    break;
            }

            // Радар
            _radar.Update(ref _debuginfo, Tick, _unlockTime, _initialRange);
            _statusInfo += $"{Languages.Translate(_language, "SEARCHING")}: {_radar.Searching}\n";
            if (_radar.lockedtarget != null) {
                EnemyTargetedInfo newTarget;
                newTarget = _turretRadar.Update(ref _debuginfo, Tick, _radar.lockedtarget);
                _radar.UpdateTarget(newTarget);
                _statusInfo += $"{Languages.Translate(_language, "LOCKED")}: {_radar.lockedtarget.Type} \n";
            } else { 
                _turretRadar.Update(Tick);
            }

            // Контроллер
            if (_myShipController != null)
                _activeShipController = _myShipController;
            else
                foreach (var cocpit in _shipControllers) { 
                    if (cocpit.IsUnderControl) { 
                        _activeShipController = cocpit;
                        break; 
                    } 
                }
            if (_activeShipController != null) {
                horizont = _activeShipController.RotationIndicator.Y;
                vertical = -_activeShipController.RotationIndicator.X;
                xMove = _activeShipController.MoveIndicator.X;
                yMove = _activeShipController.MoveIndicator.Z;
                menuMove = _activeShipController.RollIndicator;
            }

            if ((xMove != 0 || yMove != 0 || menuMove != 0) && settingsMode) {
                if (menuTimer == 0) {
                    if (menuMove != 0) {
                        menuline = 0;
                        menuTab += (int)Math.Round(menuMove);
                        if (menuTab > 2) { 
                            menuTab = 0; 
                        }
                        if (menuTab < 0) {
                            menuTab = 2; 
                        }
                    }
                    switch (menuTab) {
                        case 0:
                            if (yMove != 0) {
                                menuline += (int)Math.Round(yMove);
                                if (menuline > 2) { 
                                    menuline = 0; 
                                } else if (menuline < 0) { 
                                    menuline = 2;
                                }
                            }
                            if (xMove != 0) {
                                switch (menuline) {
                                    case 0:
                                        if (_language == "Russian")
                                            _language =  "English";
                                        else
                                            _language = "Russian";
                                        break;
                                    case 1:
                                        myCockpit += (int)Math.Round(xMove);
                                        if (myCockpit > (_observerInSC.Count() - 1))
                                            myCockpit = 0;
                                        else if (myCockpit < 0)
                                            myCockpit = _observerInSC.Count() - 1;
                                        break;
                                    case 2:
                                        myWeapon += (int)Math.Round(xMove);
                                        if (myWeapon > (_weaponDict.Count() - 1))
                                            myWeapon = 0;
                                        else if (myWeapon < 0)
                                            myWeapon = _weaponDict.Count() - 1;
                                        break;
                                }
                            }
                            break;
                        case 1:
                            if (yMove != 0) {
                                menuline += (int)Math.Round(yMove);
                                if (menuline > 3) {
                                    menuline = 0;
                                } else if (menuline < 0) {
                                    menuline = 3;
                                }
                            }
                            if (xMove != 0) {
                                switch (menuline) {
                                    case 0:
                                        myCockpit = 5;
                                        _obsCoefUp += 0.01f * (float)Math.Round(xMove);
                                        break;
                                    case 1:
                                        myCockpit = 5;
                                        _obsCoefBack += 0.01f * (float)Math.Round(xMove);
                                        break;
                                    case 2:
                                        myWeapon = 6;
                                        _myWeaponShotVelocity += 50 * (float)Math.Round(xMove);
                                        break;
                                    case 3:
                                        myWeapon = 6;
                                        _myWeaponRangeToFire += 100 *
                                        (float)Math.Round(xMove);
                                        break;
                                }
                            }
                            break;
                        case 2:
                            if (yMove != 0) {
                                menuline += (int)Math.Round(yMove);
                                if (menuline > 4) {
                                    menuline = 0;
                                } else if (menuline < 0) {
                                    menuline = 4;
                                }
                            }
                            if (xMove != 0) {
                                switch (menuline) {
                                    case 0:
                                        _targetingPoint += (int)Math.Round(xMove);
                                        if (_targetingPoint > 2)
                                            _targetingPoint = 0;
                                        else if (_targetingPoint < 0)
                                            _targetingPoint = 2;
                                        break;
                                    case 1:
                                        _initialRange += 500f * (float)Math.Round(xMove);
                                        if (_initialRange < 500)
                                            _initialRange = 500;
                                        break;
                                    case 2:
                                        enemy = !enemy;
                                        break;
                                    case 3:
                                        neutral = !neutral;
                                        break;
                                    case 4:
                                        allie = !allie;
                                        break;
                                }
                                _radar.SetTargets(allie, neutral, enemy);
                            }
                            break;
                    }
                }
                menuTimer = timeToUpdateSettings;
                SC();
            }
            if (myCockpit != 5) {
                MyTuple<string, float, float> obsInfo;
                _observerInSC.TryGetValue(myCockpit, out obsInfo);
                _obsCoefUp = obsInfo.Item2;
                _obsCoefBack = obsInfo.Item3;
            }
            if (menuTab == 0) {
                if (myWeapon != 6) {
                    MyTuple<string, float, float, float> weaponInfo;
                    _weaponDict.TryGetValue(myWeapon, out weaponInfo);
                    _myWeaponShotVelocity = weaponInfo.Item2;
                    _myWeaponRangeToFire = weaponInfo.Item3;
                    _myWeaponReloadTime = weaponInfo.Item4;
                }
            }
            Vector3D? obs = null;
            Vector3D? obsForward = null;
            Vector3D? Intersept = null;
            Vector3D? BallicticPoint = null;
            Vector3D? ShootDirection = null;
            Vector3D MyPos;
            Drawing.GetObserverPos(ref obs, ref obsForward, _obsCoefUp, _obsCoefBack, _activeShipController, _myCameras);
            if (isTurret) { 
                ShootDirection = _turret.referenceGun.WorldMatrix.Forward;
                MyPos = _turret.referenceGun.GetPosition();
            } else if (isVehicle) {
                if (_activeShipController != null)
                    MyPos = _activeShipController.GetPosition();
                else
                    MyPos = Me.GetPosition();
            } else {
                if (obs != null)
                    MyPos = obs.GetValueOrDefault();
                else
                    MyPos = Me.GetPosition();
            }
            if (getTarget)
                if (obs != null) {
                    GetClosedTarget(_turretRadar.GetTargets(), obs.GetValueOrDefault(), obsForward.GetValueOrDefault(), ref _radar, Tick);
                    getTarget = false;
                }
            if (ShootDirection == null) {
                if (_myShipController != null) { 
                    ShootDirection = _myShipController.WorldMatrix.Forward;
                } else if (_shipControllers.Count > 0)
                    ShootDirection = _shipControllers[0].WorldMatrix.Forward;
                else
                    ShootDirection = Me.WorldMatrix.Forward;
            }
            if (_radar.lockedtarget != null) {
                if (_shipControllers.Count > 0) {
                    EnemyTargetedInfo Target = _radar.lockedtarget;
                    Vector3D MySpeed = _shipControllers[0].GetShipVelocities().LinearVelocity;
                    Vector3D gravity = _shipControllers[0].GetNaturalGravity();
                    Intersept = MyMath.FindInterceptGVector(MyPos, MySpeed, Target, gravity, _myWeaponShotVelocity, _targetingPoint, false);
                    Vector3D prSpeed = ShootDirection.GetValueOrDefault() * _myWeaponShotVelocity;
                    BallicticPoint = MyMath.FindBallisticPoint(MyPos, MySpeed, Target, gravity, prSpeed, _targetingPoint);
                }
            } else if (_radar.pointOfLock != null)
                if (_shipControllers.Count > 0) {
                    Vector3D MySpeed = _shipControllers[0].GetShipVelocities().LinearVelocity;
                    Vector3D gravity = _shipControllers[0].GetNaturalGravity();
                    Vector3D prSpeed = ShootDirection.GetValueOrDefault() * _myWeaponShotVelocity;
                    BallicticPoint = MyMath.FindBallisticPoint(MyPos, MySpeed, _radar.pointOfLock.GetValueOrDefault(), gravity, prSpeed);
                }
            if (canAutoTarget) {
                if (isTurret) {
                    if (aimAssist) {
                        if (Intersept != null) {
                            if (autotarget) {
                                _turret.Status(ref _statusInfo, _language, _azimuthRotorTag, _elevationRotorTag);
                                _turret.Update(Intersept.GetValueOrDefault(), true, 0, 0, false);
                            } else {
                                _turret.Status(ref _statusInfo, _language, _azimuthRotorTag, _elevationRotorTag);
                                _turret.Update(Intersept.GetValueOrDefault(), false, azimuthSpeedMult * horizont, elevationSpeedMult * vertical, false);
                            }
                        } else {
                            _turret.Status(ref _statusInfo, _language, _azimuthRotorTag, _elevationRotorTag);
                            _turret.Update(azimuthSpeedMult * horizont, elevationSpeedMult * vertical, stabilization);
                        };
                    } else {
                        _turret.Status(ref _statusInfo, _language, _azimuthRotorTag, _elevationRotorTag);
                        _turret.Update(azimuthSpeedMult * horizont, elevationSpeedMult * vertical, stabilization);
                    }
                } else if (isVehicle) {
                    if (aimAssist) {
                        if (Intersept == null) { 
                            Hull.Drop(_myGyro);
                        } else {
                            if (_myShipController != null) {
                                if (autotarget)
                                    Hull.Control(ref _debuginfo, _myShipController, Intersept.Value, _myShipController.RollIndicator, _myGyro, false);
                                else {
                                    Hull.Control(ref _debuginfo, _myShipController, Intersept.Value, _myShipController.RollIndicator, _myGyro, false, false, yawMult, pitchMult);
                                }
                            } else {
                                IMyShipController activeSC = null;
                                foreach (var sc in _shipControllers) { 
                                    if (sc.IsUnderControl) { 
                                        activeSC = sc; break;
                                    }
                                }
                                if (activeSC != null) {
                                    if (autotarget)
                                        Hull.Control(ref _debuginfo, activeSC, Intersept.Value, activeSC.RollIndicator, _myGyro, false);
                                    else {
                                        Hull.Control(ref _debuginfo, activeSC, Intersept.Value, activeSC.RollIndicator, _myGyro, false, false, yawMult, pitchMult);
                                    }
                                } else
                                    Hull.Drop(_myGyro);
                            }
                        }
                    } else
                        Hull.Drop(_myGyro);
                }
            }
            if (obs != null) {
                foreach (var lcd in _textPanels) {
                    Drawing.SetupDrawSurface(lcd);
                    var frame = lcd.DrawFrame();
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.ContentType = ContentType.SCRIPT;
                    DrawingInfo defaultI = new DrawingInfo(obsForward.GetValueOrDefault(), frame, lcd, obs.GetValueOrDefault(), _interfaceColor);
                    defaultI.Target = _radar.lockedtarget;
                    DrawingInfo DI = defaultI;
                    if (settingsMode) {
                        switch (menuTab) {
                            case 0:
                                Drawing.SettingsInterface(DI, ref _observerInSC, ref _weaponDict, menuline, myCockpit, myWeapon, _language, 1.0f, isTurret, isVehicle);
                                break;
                            case 1:
                                Drawing.SettingsInterface(DI, menuline, _obsCoefUp, _obsCoefBack, _myWeaponShotVelocity, _myWeaponRangeToFire, _language, 1.0f, isTurret, isVehicle);
                                break;
                            case 2:
                                Drawing.SettingsInterface(DI, menuline, _targetingPoint, _initialRange, enemy, neutral, allie, _language, 1.0f, isTurret, isVehicle);
                                break;
                        }
                    } else {
                        foreach (var target in _turretRadar.GetTargets()) {
                            DI.Target = target;
                            DI.color = _targetColor;
                            if (_radar.lockedtarget != null) {
                                if (target.EntityId != _radar.lockedtarget.EntityId)
                                    Drawing.DrawTurretTarget(DI);
                            } else
                                Drawing.DrawTurretTarget(DI);
                        }
                        DI = defaultI;
                        if (_radar.lockedtarget != null) {
                            if (_radar.lockedtarget.TargetSubsystems.Count == 0) { 
                                DI.color = _targetColor; 
                                Drawing.DrawTarget(ref _debuginfo, DI, _targetingPoint); 
                                DI = defaultI;
                            } else {
                                Drawing.DrawSubsystemType(ref _debuginfo, DI, _weaponColor, _propulsionColor, _powerColor);
                                foreach (var subsystem in _radar.lockedtarget.TargetSubsystems) {
                                    Drawing.DrawSubsystem(ref _debuginfo, DI, subsystem, _weaponColor, _propulsionColor, _powerColor);
                                }
                            }
                        }
                        if (Intersept != null) {
                            DI.point = Intersept.Value;
                            DI.color = _targetColor;
                            Drawing.DrawInterceptVector(DI);
                            DI = defaultI;
                        }
                        if (BallicticPoint != null) {
                            DI.point = BallicticPoint.Value;
                            DI.color = _ballisticColor;
                            Drawing.DrawBallisticPoint(DI, _interfaceColor, Intersept == null);
                            DI = defaultI;
                        }
                        double distance = 0;
                        float losing = 1f;
                        bool searching = _radar.Searching;
                        if (_radar.lockedtarget != null) {
                            distance = (obs - _radar.lockedtarget.HitPosition).GetValueOrDefault().Length();
                            losing = (float)(_unlockTime - _radar.counter) / _unlockTime;
                            searching = false;
                        }
                        DI.point = obsForward.GetValueOrDefault();
                        DI.color = _interfaceColor;
                        TankInfo tankInfo = new TankInfo(0, false, 0);
                        if (isTurret) {
                            if (drawTank) {
                                tankInfo.turretRotation = (float)MyMath.CalculateRotorDeviationAngle(obsForward.Value, _turret.turretMatrix);
                                tankInfo.hullRotation = (float)MyMath.CalculateRotorDeviationAngle(obsForward.Value, _activeShipController.WorldMatrix);
                                tankInfo.drawTank = true;
                            }
                        }
                        Drawing.BattleInterface(DI, _language, searching, tankInfo, 1.0f, distance, losing, isTurret, isVehicle, autotarget, aimAssist);
                        _debuginfo = "";
                    }
                    frame.Dispose();
                }
            }
            Echo($"Before next update {(ReInitTime - (Tick % ReInitTime)) / 60} seconds");
            Echo(_updateInfo + _statusInfo);
            Tick++;
        }

        #endregion Common

        #region Methods

        void GetClosedTarget(List<EnemyTargetedInfo> targets, Vector3D mypos, Vector3D obsDir, ref Radar radar, long tick) {
            if (targets.Count > 0) {
                double minAngle = MathHelper.Pi;
                foreach (EnemyTargetedInfo target in targets) {
                    Vector3D dir = target.Position - mypos;
                    if (minAngle > MyMath.VectorAngleBetween(dir, obsDir)) {
                        minAngle = MyMath.VectorAngleBetween(dir, obsDir);
                        radar.GetTarget(target, tick);
                    }
                }
            }
        }
        bool UpdateBlocks(ref string updateInfo) {
            LoadIniConfig();
            updateInfo = "";
            updateInfo += $"Language: {_language}\n";
            canAutoTarget = false;
            _allBlocks.Clear();
            _allGuns.Clear();
            _myGuns.Clear();
            _gatlings.Clear();
            _mLaunchers.Clear();
            _myCameras.Clear();
            _textPanels.Clear();
            _radarCameras.Clear();
            _turrets.Clear();
            _TCs.Clear();
            _shipControllers.Clear();
            _allRotors.Clear();
            _rotorsE.Clear();
            _myShipController = null;
            _mainElRotor = null;
            _rotorA = null;
            _myGyro.Clear();
            _debugLCD = GridTerminalSystem.GetBlockWithName(_debugLCDTag) as IMyTextPanel;
            GridTerminalSystem.GetBlocksOfType(_shipControllers);
            GridTerminalSystem.GetBlocksOfType(_allRotors);
            GridTerminalSystem.GetBlocksOfType(_gatlings);
            GridTerminalSystem.GetBlocksOfType(_mLaunchers);
            foreach (var weapon in _gatlings) {
                _allGuns.Add(weapon as IMyUserControllableGun);
            }
            foreach (var weapon in _mLaunchers) {
                _allGuns.Add(weapon as IMyUserControllableGun);
            }
            isTurret = false;
            isVehicle = false;
            GridTerminalSystem.GetBlocksOfType(_allCameras);
            foreach (var camera in _allCameras) {
                if (camera.IsSameConstructAs(Me)) {
                    _myCameras.Add(camera);
                }
            }
            _FCSGroup = GridTerminalSystem.GetBlockGroupWithName(_FCSTag);
            if (_FCSGroup == null) {
                updateInfo += $"\n{languageIni.Get(_language, "GnF").ToString("Не найдена группа блоков!")}\n{languageIni.Get(_language, "NAME").ToString("Имя группы:")} \"{_FCSTag}\"\n";
            } else {
                _FCSGroup.GetBlocks(_allBlocks);
                foreach (var block in _allBlocks) {
                    if (SystemHelper.AddToListIfType(block, _textPanels))
                        continue;
                    if (block.IsSameConstructAs(Me)) {
                        if (SystemHelper.AddToListIfType(block, _TCs))
                            continue;
                        if (SystemHelper.AddToListIfType(block, _turrets))
                            continue;
                        if (SystemHelper.AddToListIfType(block, _radarCameras))
                            continue;
                    }
                    if (SystemHelper.AddToListIfType(block, _myGyro))
                        continue;
                }
            }
            if (_turrets.Count == 0 && _TCs.Count == 0) {
                GridTerminalSystem.GetBlocksOfType(_TCs);
                GridTerminalSystem.GetBlocksOfType(_turrets);
                _turretRadar.UpdateBlocks(_turrets, _TCs, false);
            } else
                _turretRadar.UpdateBlocks(_turrets, _TCs);
            if (_radar == null) { _radar = new Radar(_radarCameras); _radar.SetTargets(allie, neutral, enemy); } else {
                _radar.radarCameras = _radarCameras;
                _radar.countOfCameras = _radarCameras.Count;
                _radar.SetTargets(allie, neutral, enemy);
            }
            if (_radarCameras.Count < 2) {
                updateInfo += $"\n{Languages.Translate(_language, "CANTLOCK")}\n";
            }
            updateInfo += $"\n{Languages.Translate(_language, "LASTUPDATE")}\n" + 
                $"{Languages.Translate(_language, "RADARCAMERAS")} " + _radarCameras.Count + 
                $"\n{Languages.Translate(_language, "TEXTPANELS")} " + _textPanels.Count;
            if (_shipControllers.Count == 1) { 
                _myShipController = _shipControllers[0];
            } else {
                foreach (var block in _allBlocks) {
                    SystemHelper.AddBlockIfType(block, out _myShipController);
                }
            }
            if (_myShipController != null)
                updateInfo += $"\n{Languages.Translate(_language, "MAINCOCKPIT")} {_myShipController.CustomName}\n";
            isTurret = false;
            bool added = false;
            foreach (var block in _allBlocks) {
                if (block.CustomName.Contains(_azimuthRotorTag))
                    if (SystemHelper.AddBlockIfType(block, out _rotorA))
                        continue;
                if (block.CustomName.Contains(_elevationRotorTag))
                    if (SystemHelper.AddToListIfType(block, _rotorsE))
                        continue;
            }
            if (_rotorA != null && _rotorsE.Count != 0) {
                foreach (var rotor in _rotorsE) {
                    foreach (var gun in _allGuns) {
                        if (rotor.TopGrid == gun.CubeGrid) {
                            if (rotor.CustomName.Contains(_mainCockpitTag))
                                _mainElRotor = rotor;
                            if (_mainElRotor == null)
                                _mainElRotor = rotor;
                            if (!_myGuns.Contains(gun))
                                _myGuns.Add(gun);
                        }
                    }
                }
                if (_myGuns.Count == 0) {
                    updateInfo += $"{Languages.Translate(_language, "TURRETGROUPBLOCKS")}\n" +
                        $"{Languages.Translate(_language, "FAIL")}\n" + 
                        $"{Languages.Translate(_language, "NOGUNS")} \"{_elevationRotorTag}\"\n";
                    _rotorsE.Clear();
                    _rotorA = null;
                    _myGuns.Clear();
                } else {
                    _turret.UpdateBlocks(_rotorA, _rotorsE, _mainElRotor, _myGuns, _radarCameras, _myGyro);
                    updateInfo += $"{Languages.Translate(_language, "TURRETGROUPBLOCKS")}\n" + 
                        $"{Languages.Translate(_language, "SUCCESS")}\n";
                    isTurret = true;
                    canAutoTarget = true;
                }
            } else {
                updateInfo += $"{Languages.Translate(_language, "TURRETGROUPBLOCKS")}\n" +
                    $"{Languages.Translate(_language, "FAIL")}\n" + $"{Languages.Translate(_language, "NOROTORS")}\n";
            }
            if (!isTurret) {
                foreach (var rotor in _allRotors) {
                    added = false;
                    if (rotor.TopGrid == Me.CubeGrid) {
                        _rotorA = rotor;
                        continue;
                    } else if (rotor.CubeGrid == Me.CubeGrid) {
                        foreach (var gun in _allGuns) {
                            if (rotor.TopGrid == gun.CubeGrid) {
                                if (rotor.CustomName.Contains(_mainCockpitTag)) {
                                    _rotorsE.Add(rotor);
                                    _mainElRotor = rotor;
                                    added = true;
                                }
                                if (_mainElRotor == null && !added) {
                                    _rotorsE.Add(rotor);
                                    _mainElRotor = rotor;
                                    added = true;
                                } else if (!added) {
                                    added = true;
                                    _rotorsE.Add(rotor);
                                }
                                if (!_myGuns.Contains(gun))
                                    _myGuns.Add(gun);
                            }
                        }
                        if (!added) {
                            foreach (var camera in _radarCameras) {
                                if (rotor.TopGrid == camera.CubeGrid) {
                                    if (_mainElRotor == null && !added) {
                                        _rotorsE.Add(rotor);
                                        _mainElRotor = rotor;
                                        added = true;
                                        break;
                                    } else if (!added) {
                                        added = true;
                                        _rotorsE.Add(rotor);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (_rotorA != null && _mainElRotor != null && _myGuns.Count != 0) {
                    _turret.UpdateBlocks(_rotorA, _rotorsE, _mainElRotor, _myGuns, _radarCameras, _myGyro);
                    updateInfo += $"{Languages.Translate(_language, "AUTOTURRETSUCCESS")}\n";
                    isTurret = true;
                    canAutoTarget = true;
                } else
                    updateInfo += $"{Languages.Translate(_language, "AUTOTURRETFAIL")}\n";
            }
            if (!isTurret) {
                if (_myGyro.Count == 0)
                    GridTerminalSystem.GetBlocksOfType(_myGyro);
                if (_shipControllers.Count == 0) {
                    updateInfo += $"{Languages.Translate(_language, "AUTOHULLFAIL")}: " + 
                        $"{Languages.Translate(_language, "NOCOCKPITS")}\n";
                } else {
                    if (_myGyro.Count == 0)
                        updateInfo += $"{Languages.Translate(_language, "AUTOHULLFAIL")}: " + 
                            $"{Languages.Translate(_language, "NOGYRO")}\n";
                    else {
                        canAutoTarget = true;
                        isVehicle = true;
                        updateInfo += $"{Languages.Translate(_language, "AUTOHULLSUCCESS")}: \n" + 
                            $"{Languages.Translate(_language, "GYROS")}: {_myGyro.Count}\n";
                    }
                }
            }
            return true;
        }
        void LoadIniConfig() {
            _myIni.Clear();
            bool parsed = _myIni.TryParse(Me.CustomData);
            if (!parsed) { SC(); return; }

            _FCSTag = _myIni.Get(INI_SECTION_NAMES, INI_GROUP_NAME_TAG).ToString(_FCSTag);
            _language = _myIni.Get(INI_SECTION_NAMES, INI_LANGUAGE).ToString(_language);
            _azimuthRotorTag = _myIni.Get(INI_SECTION_NAMES, INI_AZ_ROTOR_NAME_TAG).ToString(_azimuthRotorTag);
            _elevationRotorTag = _myIni.Get(INI_SECTION_NAMES, INI_EL_ROTOR_NAME_TAG).ToString(_elevationRotorTag);
            _mainCockpitTag = _myIni.Get(INI_SECTION_NAMES, INI_MAIN_COCKPIT_NAME_TAG).ToString(_mainCockpitTag);
            _initialRange = (float)_myIni.Get(INI_SECTION_RADAR, INI_INITIAL_RANGE).ToDouble(_initialRange);
            elevationSpeedMult = (float)_myIni.Get(INI_SECTION_CONTROLS, INI_EL_MULT).ToDouble(elevationSpeedMult);
            azimuthSpeedMult = (float)_myIni.Get(INI_SECTION_CONTROLS, INI_AZ_MULT).ToDouble(azimuthSpeedMult);
            yawMult = (float)_myIni.Get(INI_SECTION_CONTROLS, INI_YAW_MULT).ToDouble(yawMult);
            pitchMult = (float)_myIni.Get(INI_SECTION_CONTROLS, INI_YAW_MULT).ToDouble(pitchMult);
            myWeapon = _myIni.Get(INI_SECTION_WEAPON, INI_NUMBER_OF_WEAPON).ToInt32(myWeapon);
            _myWeaponRangeToFire = (float)_myIni.Get(INI_SECTION_WEAPON, INI_WEAPON_FIRE_RANGE).ToDouble(_myWeaponRangeToFire);
            _myWeaponShotVelocity = (float)_myIni.Get(INI_SECTION_WEAPON, INI_WEAPON_SHOOT_VELOCITY).ToDouble(_myWeaponShotVelocity);
            _myWeaponReloadTime = (float)_myIni.Get(INI_SECTION_WEAPON, INI_WEAPON_RELOAD_TIME).ToDouble(_myWeaponShotVelocity);
            myCockpit = _myIni.Get(INI_SECTION_COCKPIT, INI_NUMBER_OF_COCKPIT).ToInt32(myCockpit);
            _obsCoefUp = (float)_myIni.Get(INI_SECTION_COCKPIT, INI_COEF_UP).ToDouble(_myWeaponShotVelocity);
            _obsCoefBack = (float)_myIni.Get(INI_SECTION_COCKPIT, INI_COEF_BACK).ToDouble(_myWeaponShotVelocity);
            allie = _myIni.Get(INI_SECTION_TARGETS, INI_ALLIE).ToBoolean(allie);
            neutral = _myIni.Get(INI_SECTION_TARGETS, INI_NEUTRAL).ToBoolean(neutral);
            enemy = _myIni.Get(INI_SECTION_TARGETS, INI_ENEMY).ToBoolean(enemy);
            _targetingPoint = _myIni.Get(INI_SECTION_TARGETS, INI_DISPLAYED_TARGET).ToInt32(_targetingPoint);

            SC();
        }
        void SC() {
            _myIni.Clear();
            _myIni.Set(INI_SECTION_NAMES, INI_GROUP_NAME_TAG, _FCSTag);
            _myIni.Set(INI_SECTION_NAMES, INI_LANGUAGE, _language);
            _myIni.Set(INI_SECTION_NAMES, INI_AZ_ROTOR_NAME_TAG, _azimuthRotorTag);
            _myIni.Set(INI_SECTION_NAMES, INI_EL_ROTOR_NAME_TAG, _elevationRotorTag);
            _myIni.Set(INI_SECTION_NAMES, INI_MAIN_COCKPIT_NAME_TAG, _mainCockpitTag);
            _myIni.Set(INI_SECTION_RADAR, INI_INITIAL_RANGE, _initialRange);
            _myIni.Set(INI_SECTION_CONTROLS, INI_EL_MULT, elevationSpeedMult);
            _myIni.Set(INI_SECTION_CONTROLS, INI_AZ_MULT, azimuthSpeedMult);
            _myIni.Set(INI_SECTION_CONTROLS, INI_YAW_MULT, yawMult);
            _myIni.Set(INI_SECTION_CONTROLS, INI_PITCH_MULT, pitchMult);
            _myIni.Set(INI_SECTION_WEAPON, INI_NUMBER_OF_WEAPON, myWeapon);
            _myIni.Set(INI_SECTION_WEAPON, INI_WEAPON_FIRE_RANGE, _myWeaponRangeToFire);
            _myIni.Set(INI_SECTION_WEAPON, INI_WEAPON_SHOOT_VELOCITY, _myWeaponShotVelocity);
            _myIni.Set(INI_SECTION_WEAPON, INI_WEAPON_RELOAD_TIME, _myWeaponReloadTime);
            _myIni.Set(INI_SECTION_COCKPIT, INI_NUMBER_OF_COCKPIT, myCockpit);
            _myIni.Set(INI_SECTION_COCKPIT, INI_COEF_UP, _obsCoefUp);
            _myIni.Set(INI_SECTION_COCKPIT, INI_COEF_BACK, _obsCoefBack);
            _myIni.Set(INI_SECTION_TARGETS, INI_ALLIE, allie);
            _myIni.Set(INI_SECTION_TARGETS, INI_NEUTRAL, neutral);
            _myIni.Set(INI_SECTION_TARGETS, INI_ENEMY, enemy);
            _myIni.Set(INI_SECTION_TARGETS, INI_DISPLAYED_TARGET, _targetingPoint);
            Me.CustomData = _myIni.ToString();
        }

        #endregion Methods

    }

    static class Drawing {
        static Color _BLACK = new Color(0, 0, 0, 255);
        static Color _ChosenComponent = new Color(10, 20, 30, 255);
        static float 
            _largeLcd = 240f,
            _smallLsd = 1080f;
        static float dz = 0.25f - 0.005f;
        static bool large = false;
        static Vector3D cord_lcd;
        static PlaneD plane;
        static Vector3D point_on_lcd;
        static Vector3D delta;
        static MatrixD m;
        static MatrixD mTrans;
        static Vector3D vectorHudLocal;
        static Vector2 marker;
        static RectangleF rect;
        static float mult;

        #region Methods

        static bool PrepeareCoords(IMyTextPanel surface, Vector3D obspos, Vector3D viewvector) {
            mult = _smallLsd;
            if (large)
                mult = _largeLcd;
            large = false;
            if (surface.CubeGrid.GridSizeEnum == MyCubeSize.Large) {
                dz = 1.25f;
                large = true;
            } else {
                dz = 0.25f - 0.005f;
                large = false;
            }
            cord_lcd = surface.GetPosition() + surface.WorldMatrix.Forward * dz;
            plane = new PlaneD(cord_lcd, surface.WorldMatrix.Forward);
            point_on_lcd = plane.Intersection(ref obspos, ref viewvector);
            delta = point_on_lcd - cord_lcd;
            m = surface.WorldMatrix;
            mTrans = MatrixD.Transpose(m);
            vectorHudLocal = Vector3D.TransformNormal(delta, mTrans);
            marker = new Vector2((float)vectorHudLocal.X, -(float)vectorHudLocal.Y);
            rect = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
            if (Math.Abs(marker.X * mult) > (surface.TextureSize.X / 2) || 
                Math.Abs(marker.Y * mult) > (surface.TextureSize.Y / 2))
                return false;
            return true;
        }
        public static void SetupDrawSurface(IMyTextSurface surface) {
            surface.ScriptBackgroundColor = Color.Black;
            surface.ContentType = ContentType.SCRIPT;
            surface.Script = "";
        }
        public static void DrawTarget(ref string debugInfo, DrawingInfo dI, int targetingPoint = 0) {
            if (dI.Target != null) {
                Vector3D target = dI.Target.Position;
                if (targetingPoint == 0)
                    if (dI.Target.TargetedPoint != null)
                        target = dI.Target.TargetedPoint.GetValueOrDefault();
                if (targetingPoint == 2)
                    if (dI.Target.HitPosition != null)
                        target = dI.Target.HitPosition.GetValueOrDefault();
                Vector3D viewvector = target - dI.obspos;
                if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                    return;
                Vector3D lcdToTarget = target - cord_lcd;
                if (viewvector.Length() > lcdToTarget.Length()) {
                    float size = _smallLsd;
                    DrawBox(dI.color, dI.frame, rect.Center + (marker * mult));
                }
            }
        }
        public static void DrawSubsystemType(ref string debugInfo, DrawingInfo dI, Color w, Color p, Color e) {
            Vector3D viewvector = dI.point;
            if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                return;
            string weapon = "Weapons: " + dI.Target.WeaponSubsystems.Count;
            string prop = "Propulsion: " + dI.Target.PropSubsystems.Count;
            string power = "PowerSystems: " + dI.Target.PowerSubsystems.Count;
            float step = 20f;
            dI.frame.Add(new MySprite(SpriteType.TEXT, weapon, new Vector2(-220f, 150f) + rect.Center, null, w, "DEBUG", TextAlignment.LEFT, 0.7f));
            dI.frame.Add(new MySprite(SpriteType.TEXT, prop, new Vector2(-220f, 150f + step) + rect.Center, null, p, "DEBUG", TextAlignment.LEFT, 0.7f));
            dI.frame.Add(new MySprite(SpriteType.TEXT, power, new Vector2(-220f, 150f + step * 2) + rect.Center, null, e, "DEBUG", TextAlignment.LEFT, 0.7f));
        }
        public static void DrawSubsystem(ref string debugInfo, DrawingInfo dI, TargetSubsystem subsystem, Color w, Color p, Color e) {
            Vector3D target = subsystem.GetPosition(dI.Target);
            Vector3D viewvector = target - dI.obspos;
            if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                return;
            Vector3D lcdToTarget = target - cord_lcd;
            Color color;
            switch (subsystem.subsystemType) {
                case "Weapons":
                    color = w;
                    break;
                case "Propulsion":
                    color = p;
                    break;
                default:
                    color = e;
                    break;
            }
            if (viewvector.Length() > lcdToTarget.Length()) {
                DrawPoint(color, dI.frame, rect.Center + (marker * mult));
            }
        }
        public static void DrawTurretTarget(DrawingInfo dI) {
            Vector3D targetpos = dI.Target.Position;
            Vector3D viewvector = targetpos - dI.obspos;
            if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                return;
            Vector3D lcdToTarget = targetpos - cord_lcd;
            if (viewvector.Length() > lcdToTarget.Length()) { 
                DrawPassiveTarget(dI.color, dI.frame, rect.Center + (marker * mult));
            }
        }
        public static void DrawInterceptVector(DrawingInfo dI) {
            Vector3D viewvector = dI.point - dI.obspos;
            if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                return;
            Vector3D lcdToTarget = dI.point - cord_lcd;
            if (viewvector.Length() > lcdToTarget.Length()) {
                if (large) {
                    DrawCircle(dI.color, dI.frame, rect.Center + (marker * _largeLcd));
                } else {
                    DrawCircle(dI.color, dI.frame, rect.Center + (marker * _smallLsd));
                }
            }
        }
        public static void DrawBallisticPoint(DrawingInfo dI, Color interfaceColor, bool distanceB = false) {
            Vector3D viewvector = dI.point - dI.obspos;
            if (!PrepeareCoords(dI.surface, dI.obspos, viewvector))
                return;
            Vector3D lcdToTarget = dI.point - cord_lcd;
            if (viewvector.Length() > lcdToTarget.Length()) {
                DrawSpriteX(dI.color, dI.frame, rect.Center + (marker * mult));
                if (distanceB) {
                    double distance = (dI.point - dI.obspos).Length();
                    DrawDistance(interfaceColor, dI.frame, rect.Center + (marker * mult), distance, 0.7f);
                }
            }
        }
        public static void BattleInterface(DrawingInfo dI, string Language, bool searching, TankInfo tankInfo,
            float scale = 1f, double distance = 0, float locked = 1.0f, bool isTurret = false,
            bool isVeachle = false, bool autoAim = false, bool aimAssist = false) {
            Vector3D viewvector = dI.point;
            PrepeareCoords(dI.surface, dI.obspos, viewvector);
            DrawInterface(dI, Language, isTurret, isVeachle);
            Vector2 statusPos = new Vector2(-40, 60);
            DrawSight(dI.color, dI.frame, rect.Center + (marker * mult));
            if (searching)
                DrawXSight(dI.color, dI.frame, rect.Center + (marker * mult));
            if (distance != 0)
                DrawDistance(dI.color, dI.frame, rect.Center + (marker * mult), distance, 0.7f);
            if (locked < 0.9f)
                LosingTarget(dI.color, dI.frame, rect.Center + (marker * mult), locked, 0.7f);
            if (aimAssist)
                DrawLockedMode(dI.color, Language, dI.frame, rect.Center + statusPos + (marker * mult), autoAim, 0.7f);
            if (tankInfo.drawTank) {
                Vector2 tankPos = new Vector2(150, 150);
                DrawHull(dI.color, dI.frame, tankPos + rect.Center + (marker * mult), tankInfo.hullRotation);
                DrawTurret(dI.color, dI.frame, tankPos + rect.Center + (marker * mult), tankInfo.turretRotation);
            }
        }
        public static void SettingsInterface(DrawingInfo dI, ref Dictionary<int, MyTuple<string, float, float>> obsDict,
            ref Dictionary<int, MyTuple<string, float, float,float>> weaponDict, int nubmerOfLine, int numberOfCockpit,
            int numberOfWeapon, string Language, float scale = 1f, bool isTurret = false, bool isVeachle = false) {
            Vector3D viewvector = dI.point;
            PrepeareCoords(dI.surface, dI.obspos, viewvector);
            DrawInterface(dI, Language, isTurret, isVeachle);
            Vector2 settingsPos = new Vector2(0, -160);
            DrawSettings(dI.color, Language, dI.frame, rect.Center + settingsPos, 0, 0.7f);
            float step = 100f;
            Vector2 moduleCoord = new Vector2(0f, -50f);
            MyTuple<string, float, float> obsInfo;
            obsDict.TryGetValue(numberOfCockpit, out obsInfo);
            MyTuple<string, float, float, float> weaponInfo;
            weaponDict.TryGetValue(numberOfWeapon, out weaponInfo);
            string cocpitType = Languages.Translate(Language, obsInfo.Item1);
            string weaponType = Languages.Translate(Language, weaponInfo.Item1);
            DrawModule0(dI.color, Language, dI.frame, rect.Center + moduleCoord, nubmerOfLine, cocpitType, weaponType, step, 0.7f);
        }
        public static void SettingsInterface(DrawingInfo dI, int nubmerOfLine, float obsUp, float obsBack, float weaponVel,
            float weaponRange, string Language, float scale = 1f, bool isTurret = false, bool isVeachle = false) {
            Vector3D viewvector = dI.point;
            PrepeareCoords(dI.surface, dI.obspos, viewvector);
            DrawInterface(dI, Language, isTurret, isVeachle);
            Vector2 settingsPos = new Vector2(0, -160);
            DrawSettings(dI.color, Language, dI.frame, rect.Center + settingsPos + (marker * mult), 1, 0.7f);
            float step = 70f;
            Vector2 moduleCoord = new Vector2(0f, -50f);
            DrawModule1(dI.color, Language, dI.frame, rect.Center + moduleCoord, nubmerOfLine, obsUp, obsBack, weaponVel, weaponRange, step, 0.7f);
        }
        public static void SettingsInterface(DrawingInfo dI, int nubmerOfLine, int targetingPoint, float lockRange, bool
            enemy, bool neutral, bool allie, string Language, float scale = 1f, bool isTurret = false, bool isVeachle = false) {
            Vector3D viewvector = dI.point;
            PrepeareCoords(dI.surface, dI.obspos, viewvector);
            DrawInterface(dI, Language, isTurret, isVeachle);
            Vector2 settingsPos = new Vector2(0, -160);
            DrawSettings(dI.color, Language, dI.frame, rect.Center + settingsPos + (marker * mult), 2, 0.7f);
            float step = 70f;
            Vector2 moduleCoord = new Vector2(0f, -50f);
            DrawModule2(dI.color, Language, dI.frame, rect.Center + moduleCoord, nubmerOfLine, targetingPoint, lockRange, enemy, neutral, allie, step, 0.7f);
        }
        static void DrawInterface(DrawingInfo dI, string Language, bool isTurret, bool isVeachle) {
            string aiMode = "";
            Vector2 logoPos = new Vector2(-200, -200);
            Vector2 modPos = new Vector2(100, -200);
            Vector2 centerPos = rect.Center + logoPos + (marker * mult);
            DrawLogo(dI.color, Language, dI.frame, centerPos, 0.7f);
            if (isTurret) {
                aiMode = Languages.Translate(Language, "TURRET");
            } else if (isVeachle) {
                aiMode = Languages.Translate(Language, "HULL");
            }
            DrawAiMode(dI.color, dI.frame, aiMode, rect.Center + modPos + (marker * mult), 0.7f);
        }
        static void LosingTarget(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float percent, float scale = 1f) {
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(120f, 0f) * scale + centerPos, new Vector2(16f, 100f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(120f, 0f) * scale + centerPos, new Vector2(14f, 98f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(120f, 0f + (48 * (1 - percent))) * scale + centerPos, new Vector2(12f, 96f * percent) * scale, color, null, TextAlignment.CENTER, 0f));
        }
        static void DrawDistance(Color color, MySpriteDrawFrame frame, Vector2 centerPos, double distance, float scale = 1f) {
            if (distance < 1000)
                frame.Add(new MySprite(SpriteType.TEXT, $"{Math.Round(distance, 0)} м", new Vector2(-20f, 40f) + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            else
                frame.Add(new MySprite(SpriteType.TEXT, $"{Math.Round(distance / 1000, 2)} км", new Vector2(-20f, 40f) + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawSight(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(1f, 1f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 10f) * scale + centerPos,
                Size = new Vector2(2f, 5f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(10f, 0f) * scale + centerPos,
                Size = new Vector2(5f, 2f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(-10f, 0f) * scale + centerPos,
                Size = new Vector2(5f, 2f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
        }
        static void DrawLogo(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            string scriptName = Languages.Translate(Language, "SCRIPT_NAME");
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(145f, 17.5f) * scale + centerPos, new Vector2(50f, 42f) * scale, color, null, TextAlignment.CENTER, 3.1416f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos, new Vector2(156f, 36f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(145f, 17.5f) * scale + centerPos, new Vector2(46f, 39f) * scale, _BLACK, null, TextAlignment.CENTER, 3.1416f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos, new Vector2(154f, 34f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, scriptName, new Vector2(5f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawAiMode(Color color, MySpriteDrawFrame frame, string text, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-10f, 17.5f) * scale + centerPos, new Vector2(50f, 42f) * scale, color, null, TextAlignment.CENTER, 3.1416f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos, new Vector2(156f, 36f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-10f, 17.5f) * scale + centerPos, new Vector2(46f, 39f) * scale, _BLACK, null, TextAlignment.CENTER, 3.1416f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos, new Vector2(153f, 34f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(5f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawSettings(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, int module = 0, float scale = 1f) {
            Vector2 englD = new Vector2(0, 0);
            if (Language == "English")
                englD = new Vector2(20, 0);
            string settings = Languages.Translate(Language, "SETTINGS");
            Vector2 delta = new Vector2(-65, -27);
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(145f, 12f) * scale + centerPos + delta * scale, new Vector2(50f, 42f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-10f, 12f) * scale + centerPos + delta * scale, new Vector2(50f, 42f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos + delta * scale, new Vector2(156f, 36f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(145f, 12f) * scale + centerPos + delta * scale, new Vector2(46f, 39f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-10f, 12f) * scale + centerPos + delta * scale, new Vector2(46f, 39f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(68f, 15f) * scale + centerPos + delta * scale, new Vector2(154f, 34f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, settings, new Vector2(5f, 0f) * scale + centerPos + delta * scale, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 38f) * scale + centerPos, new Vector2(400f, 30f) * scale, new Color(255, 255, 255, 255), null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 38f) * scale + centerPos, new Vector2(398f, 28f) * scale, _BLACK, null, TextAlignment.CENTER, 0f));
            string simple = Languages.Translate(Language, "SIMPLE");
            string advanced = Languages.Translate(Language, "ADVANCED");
            string radar = Languages.Translate(Language, "RADAR");
            switch (module) {
                case 0:
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-142f, 38f) * scale + centerPos, new Vector2(114f, 28f) * scale, _ChosenComponent, null, TextAlignment.CENTER, 0f));
                    break;
                case 1:
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(3, 38f) * scale + centerPos, new Vector2(176, 28f) * scale, _ChosenComponent, null, TextAlignment.CENTER, 0f));
                    break;
                case 2:
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(144, 38f) * scale + centerPos, new Vector2(110f, 28f) * scale, _ChosenComponent, null, TextAlignment.CENTER, 0f));
                    break;
            }
            frame.Add(new MySprite(SpriteType.TEXT, simple, new Vector2(-190f, 21f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, advanced, (new Vector2(-77f, 21f) + englD) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, radar, new Vector2(100f, 21f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawModule0(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, int component, string cocpitType, string weaponType, float step, float scale = 1f) {
            string cockpit = Languages.Translate(Language, "COCKPIT");
            string weapon = Languages.Translate(Language, "WEAPON");
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(24f, 16f + (step * component)) * scale + centerPos, new Vector2(475f, 60f) * scale, _ChosenComponent, null,TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, "Language", new Vector2(-180f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, Language, new Vector2(100f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, cockpit, new Vector2(-180f, 0f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, cocpitType, new Vector2(100f, -15f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, weapon, new Vector2(-180f, 0f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, weaponType, new Vector2(100f, 0f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawModule1(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, int component, float obsUp, float obsBack, float weaponVel, float weaponRange, float step, float scale = 1f) {
            string projvel = Languages.Translate(Language, "PROJVEL"), 
                shootrange = Languages.Translate(Language, "SHOTRANGE");
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(24f, 16f + (step * component)) * scale + centerPos, new Vector2(475f, 60f) * scale, _ChosenComponent, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, "Cockpit - Up", new Vector2(-180f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{obsUp}", new Vector2(100f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, "Cockpit - Back", new Vector2(-180f, 0f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{obsBack}", new Vector2(100f, 0f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, projvel, new Vector2(-180f, -15f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{weaponVel}", new Vector2(100f, 0f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, shootrange, new Vector2(-180f, -15f + 3 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{weaponRange}", new Vector2(100f, 0f + 3 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawModule2(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, int component, int targetingPoint, float lockRange, bool enemyB, bool neutralB, bool allieB, float step, float scale = 1f) {
            string target = Languages.Translate(Language, "TARGETINGPOINT");
            string point = Languages.Translate(Language, "POINT" + targetingPoint);
            string range = Languages.Translate(Language, "INITIALRANGE");
            string enemy = Languages.Translate(Language, "ENEMY");
            string neutral = Languages.Translate(Language, "NEUTRAL");
            string allie = Languages.Translate(Language, "ALLIE");
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(24f, 16f + (step * component)) * scale + centerPos, new Vector2(475f, 60f) * scale, _ChosenComponent, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXT, target, new Vector2(-180f, -15f + 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, point, new Vector2(100f, -15f + 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, range, new Vector2(-180f, -15f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{lockRange}m", new Vector2(100f, 0f + step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, enemy, new Vector2(-180f, 0f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{enemyB}", new Vector2(100f, 0f + 2 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, neutral, new Vector2(-180f, 0f + 3 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{neutralB}", new Vector2(100f, 0f + 3 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, allie, new Vector2(-180f, 0f + 4 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
            frame.Add(new MySprite(SpriteType.TEXT, $"{allieB}", new Vector2(100f, 0f + 4 * step) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
        }
        static void DrawLockedMode(Color color, string Language, MySpriteDrawFrame frame, Vector2 centerPos, bool autotarget, float scale = 1f) {
            string auto;
            string aim;
            if (autotarget) {
                aim = Languages.Translate(Language, "AIM");
                auto = Languages.Translate(Language, "AUTO");
                frame.Add(new MySprite(SpriteType.TEXT, aim, new Vector2(0f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
                frame.Add(new MySprite(SpriteType.TEXT, auto, new Vector2(-35f, 25f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-11f, 15f) * scale + centerPos, new Vector2(7f, 7f) * scale, color, null, TextAlignment.CENTER, 0f));
            } else {
                aim = Languages.Translate(Language, "TRACKING");
                auto = Languages.Translate(Language, "ASSIST");
                frame.Add(new MySprite(SpriteType.TEXT, aim, new Vector2(0f, 0f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
                frame.Add(new MySprite(SpriteType.TEXT, auto, new Vector2(-5f, 25f) * scale + centerPos, null, color, "DEBUG", TextAlignment.LEFT, 1f * scale));
                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-11f, 15f) * scale + centerPos, new Vector2(7f, 7f) * scale, color, null, TextAlignment.CENTER, 0f));
            }
        }
        static void DrawXSight(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(20f, 20f) * scale + centerPos, new Vector2(1f, 8f) * scale, color, null, TextAlignment.CENTER, -0.7854f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(20f, -20f) * scale + centerPos, new Vector2(1f, 8f) * scale, color, null, TextAlignment.CENTER, 0.7854f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-20f, -20f) * scale + centerPos, new Vector2(1f, 8f) * scale, color, null, TextAlignment.CENTER, -0.7854f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-20f, 20f) * scale + centerPos, new Vector2(1f, 8f) * scale, color, null, TextAlignment.CENTER, 0.7854f));
        }
        static void DrawBox(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(20f, 20f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(18f, 18f) * scale,
                Color = _BLACK,
                RotationOrScale = 0f
            });
        }
        static void DrawPoint(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0f, 0f) * scale + centerPos, new Vector2(5f, 5f) * scale, color, null, TextAlignment.CENTER, 0f));
        }
        static void DrawPassiveTarget(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(20f, 20f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(18f, 18f) * scale,
                Color = _BLACK,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "SquareSimple",
                    Position = new Vector2(0f, 0f) * scale + centerPos,
                    Size = new Vector2(12f, 20f) * scale,
                    Color = new Color(0, 0, 0, 255),
                    RotationOrScale = 0f
                });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(20f, 12f) * scale,
                Color = new Color(0, 0, 0, 255),
                RotationOrScale = 0f
            });
        }
        static void DrawCircle(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "Circle",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(20f, 20f) * scale,
                Color = color,
                RotationOrScale = 0f
            });
            frame.Add(new MySprite() {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "Circle",
                Position = new Vector2(0f, 0f) * scale + centerPos,
                Size = new Vector2(18f, 18f) * scale,
                Color = _BLACK,
                RotationOrScale = 0f
            });
        }
        public static bool GetObserverPos(ref Vector3D? k, ref Vector3D? forwardDirection, float up, float backward, IMyShipController cockpit = null, List<IMyCameraBlock> all_cams = null) {
            IMyCameraBlock cam = null;
            foreach (var viewCam in all_cams) {
                if (viewCam.IsActive) {
                    cam = viewCam;
                    break; 
                }
            }
            if (cam != null) {
                k = cam.WorldMatrix.Translation + cam.WorldMatrix.Forward * (cam.CubeGrid.GridSize / 2f - 0.005f);
                forwardDirection = cam.WorldMatrix.Forward;
                return true;
            } else if (cockpit != null && cockpit.IsUnderControl) {
                k = cockpit.GetPosition() + cockpit.WorldMatrix.Up * (up) + cockpit.WorldMatrix.Backward * (backward);
                forwardDirection = cockpit.WorldMatrix.Forward;
                return true;
            } else
                return false;
        }
        public static void DrawSpriteX(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f, float rotation = 0f) {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) * scale + centerPos, new Vector2(2f, 10f) * scale, color, null, TextAlignment.CENTER, 0.7854f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) * scale + centerPos, new Vector2(10f, 2f) * scale, color, null, TextAlignment.CENTER, 0.7854f + rotation));
        }
        public static void DrawSpritesRectangle(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(4f, 0f) * scale + centerPos, new Vector2(2f, 10f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-4f, 0f) * scale + centerPos, new Vector2(2f, 10f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, -4f) * scale + centerPos, new Vector2(10f, 2f) * scale, color, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 4f) * scale + centerPos, new Vector2(10f, 2f) * scale, color, null, TextAlignment.CENTER, 0f));
        }
        public static void DrawSpritesRectangleRotateAndScale(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f, float rotation = 0f) {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 4f - sin * 0f, sin * 4f + cos * 0f) * scale + centerPos, new Vector2(2f, 10f) * scale, color, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * -4f - sin * 0f, sin * -4f + cos * 0f) * scale + centerPos, new Vector2(2f, 10f) * scale, color, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * -4f, sin * 0f + cos * -4f) * scale + centerPos, new Vector2(10f, 2f) * scale, color, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 4f, sin * 0f + cos * 4f) * scale + centerPos, new Vector2(10f, 2f) * scale, color, null, TextAlignment.CENTER, 0f + rotation));
        }
        public static void DrawTurret(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float rotation = 0f) {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) + centerPos, new Vector2(25f, 25f), color, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) + centerPos, new Vector2(23f, 23f), _BLACK, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * -39f, sin * 0f + cos * - 39f) + centerPos, new Vector2(5f, 51f), _BLACK, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * -39f, sin * 0f + cos * -39f) + centerPos, new Vector2(3f, 49f), color, null, TextAlignment.CENTER, 0f + rotation));
        }
        public static void DrawHull(Color color, MySpriteDrawFrame frame, Vector2 centerPos, float rotation = 0f) {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) + centerPos, new Vector2(40f, 60f), color, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 0f, sin * 0f + cos * 0f) + centerPos, new Vector2(38f, 58f), _BLACK, null, TextAlignment.CENTER, 0f + rotation));
            frame.Add(new MySprite(SpriteType.TEXTURE, "AH_BoreSight", new Vector2(cos * 0f - sin * -40f, sin * 0f + cos * -40f) + centerPos, new Vector2(40f, 40f), color, null, TextAlignment.CENTER, -1.5708f + rotation));
        }

        #endregion Methods
    }

    public class DrawingInfo {
        public EnemyTargetedInfo Target;
        public Vector3D point;
        public MySpriteDrawFrame frame;
        public IMyTextPanel surface;
        public Vector3D obspos;
        public Color color;
        public DrawingInfo(Vector3D point, MySpriteDrawFrame frame, IMyTextPanel surface, Vector3D obspos, Color color) {
            this.point = point;
            this.frame = frame;
            this.surface = surface;
            this.obspos = obspos;
            this.color = color;
        }
        public DrawingInfo(EnemyTargetedInfo target, MySpriteDrawFrame frame, IMyTextPanel surface, Vector3D obspos, Color color) {
            this.Target = target;
            this.frame = frame;
            this.surface = surface;
            this.obspos = obspos;
            this.color = color;
        }
    }

    public class TankInfo {
        public float turretRotation;
        public bool drawTank;
        public float hullRotation;
        public TankInfo(float turretRotation, bool drawTank, float hullRotation) {
            this.turretRotation = turretRotation;
            this.drawTank = drawTank;
            this.hullRotation = hullRotation;
        }
    }

    public class EnemyTargetedInfo {
        public long EntityId { private set; get; }
        public MyDetectedEntityType Type;
        public Vector3D? TargetedPoint;
        public Vector3D? HitPosition;
        public Vector3D Position;
        public Vector3D? inBodyPointPosition;
        public Vector3D? DeltaPosition;
        public Vector3D Velocity;
        public Vector3D? Acceleration;
        public MatrixD Orientation;
        public long LastLockTick;
        public List<TargetSubsystem> TargetSubsystems = new List<TargetSubsystem>();
        public List<TargetSubsystem> PowerSubsystems = new List<TargetSubsystem>();
        public List<TargetSubsystem> PropSubsystems = new List<TargetSubsystem>();
        public List<TargetSubsystem> WeaponSubsystems = new List<TargetSubsystem>();
        public EnemyTargetedInfo(long tick, MyDetectedEntityInfo newentityInfo, Vector3D? dir = null) {
            EntityId = newentityInfo.EntityId;
            Type = newentityInfo.Type;
            HitPosition = newentityInfo.HitPosition;
            Position = newentityInfo.Position;
            if (HitPosition != null) {
                TargetedPoint = HitPosition;
                DeltaPosition = HitPosition.GetValueOrDefault() - Position;
            }
            Orientation = newentityInfo.Orientation;
            if (TargetedPoint != null) {
                Vector3D worldDirection = TargetedPoint.GetValueOrDefault() - Position;
                inBodyPointPosition = Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(Orientation));
                if (dir != null)
                    TargetedPoint += dir / dir.GetValueOrDefault().Length() * 0.5;
            }
            Velocity = newentityInfo.Velocity;
            Acceleration = null;
            LastLockTick = tick;
        }
        public void UpdateTargetInfo(long tick, MyDetectedEntityInfo newEntityInfo, Vector3D? dir = null) {
            EntityId = newEntityInfo.EntityId;
            Type = newEntityInfo.Type;
            HitPosition = newEntityInfo.HitPosition;
            Position = newEntityInfo.Position;
            Orientation = newEntityInfo.Orientation;
            if (HitPosition != null) {
                DeltaPosition = HitPosition.GetValueOrDefault() - Position;
                if (TargetedPoint == null) {
                    TargetedPoint = HitPosition;
                    if (dir != null)
                        TargetedPoint += dir / dir.GetValueOrDefault().Length() * 0.5;
                    Vector3D worldDirection = TargetedPoint.GetValueOrDefault() - Position;
                    inBodyPointPosition = Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(Orientation));
                }
            } else
                DeltaPosition = null;
            if (inBodyPointPosition != null) {
                Vector3D worldDirection = Vector3D.TransformNormal(inBodyPointPosition.GetValueOrDefault(), Orientation);
                TargetedPoint = worldDirection + Position;
            } else
                TargetedPoint = null;
            if (LastLockTick - tick > 60)
                Acceleration = null;
            else
                Acceleration = (newEntityInfo.Velocity - Velocity) / (LastLockTick - tick);
            Velocity = newEntityInfo.Velocity;
            LastLockTick = tick;
        }
        public bool AddSubsystem(Vector3D worldPosition, string type) {
            TargetSubsystem newSubsystem = new TargetSubsystem(worldPosition, this, type);
            float size;
            if (Type == MyDetectedEntityType.LargeGrid)
                size = 2.5f;
            else if (Type == MyDetectedEntityType.SmallGrid)
                size = 0.5f;
            else
                return false;
            foreach (var subsystem in TargetSubsystems) {
                Vector3D delta = subsystem.gridPosition - newSubsystem.gridPosition;
                if (Math.Round(delta.Length() / size) == 0)
                    return false;
            }
            TargetSubsystems.Add(newSubsystem);
            if (type == "Weapons")
                WeaponSubsystems.Add(newSubsystem);
            if (type == "Propulsion")
                PropSubsystems.Add(newSubsystem);
            if (type == "PowerSystems")
                PowerSubsystems.Add(newSubsystem);
            return true;
        }
        public bool AddSubsystem(MyDetectedEntityInfo newEntityInfo, string type) {
            TargetSubsystem newSubsystem = new TargetSubsystem(newEntityInfo.HitPosition.GetValueOrDefault(), this, type);
            float size;
            if (Type == MyDetectedEntityType.LargeGrid)
                size = 2.5f;
            else if (Type == MyDetectedEntityType.SmallGrid)
                size = 0.5f;
            else
                return false;
            foreach (var subsystem in TargetSubsystems) {
                Vector3D
            delta = subsystem.gridPosition - newSubsystem.gridPosition;
                if (delta.Length() / size < 1)
                    return false;
            }
            TargetSubsystems.Add(newSubsystem);
            if (type == "Weapons")
                WeaponSubsystems.Add(newSubsystem);
            if (type == "Propulsion")
                PropSubsystems.Add(newSubsystem);
            if (type == "PowerSystems")
                PowerSubsystems.Add(newSubsystem);
            return true;
        }
    }

    public class TargetSubsystem {
        public string subsystemType { get; }
        public Vector3D gridPosition { get; }
        public TargetSubsystem(Vector3D worldPosition, EnemyTargetedInfo Target, string type) {
            subsystemType = type;
            Vector3D worldDirection = worldPosition - Target.Position;
            gridPosition = Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(Target.Orientation));
        }
        public Vector3D GetPosition(EnemyTargetedInfo Target) {
            Vector3D worldDirection = Vector3D.TransformNormal(gridPosition, Target.Orientation);
            Vector3D worldPosition = worldDirection + Target.Position;
            return worldPosition;
        }
    }

    public class HullGuidance {
        bool _firstrun = true;
        const double TURNGYROCONST = 0.0035;
        const float SOFTNAVCONST = 0.8f;
        Vector3D lastTargetDir = Vector3D.Zero;
        MatrixD lastShipMatrix = MatrixD.Identity;
        MatrixD shipMatrix;
        double lastPitch = 0;
        double lastYaw = 0;
        double maneuvrabilityYaw = 0;
        double maneuvrabilityPitch = 0;
        bool fullDriveYaw = false;
        bool fullDrivePitch = false;
        bool b;
        double yawDeltaInput = 0;
        double yawInputSpeed = 0;
        double pitchDeltaInput = 0;
        double pitchInputSpeed = 0;

        public void Control(ref string debuginfo, IMyShipController shipController, Vector3D targetDir, float rollIndicator,
            List<IMyGyro> gyros, bool dir = true, bool autoAim = true, float yawMult = 1, float pitchMult = 1) {
            if (!dir)
                targetDir -= shipController.WorldMatrix.Translation;
            shipMatrix = shipController.WorldMatrix;
            Vector3D WorldAngularVelocity = shipController.GetShipVelocities().AngularVelocity;
            Vector3D LocalAngularVelocity = Vector3D.TransformNormal(WorldAngularVelocity, MatrixD.Transpose(shipMatrix));
            double ownYaw = -LocalAngularVelocity.Y / 60;
            double ownPitch = -LocalAngularVelocity.X / 60;
            double wantedYaw, wantedPitch;
            double yawSpeed, pitchSpeed;
            double targetAngularVelYaw, targetAngularVelPitch;
            Vector3D targetVecLoc = Vector3D.TransformNormal(targetDir, MatrixD.Transpose(shipMatrix));
            wantedYaw = Math.Atan2(targetVecLoc.X, -targetVecLoc.Z);
            double xyLenght = new Vector2D(targetVecLoc.X, targetVecLoc.Z).Length();
            if (targetVecLoc.Z > 0) {
                xyLenght *= -1;
            }
            wantedPitch = Math.Atan2(-targetVecLoc.Y, xyLenght);
            if (_firstrun) {
                lastYaw = 0;
                lastPitch = 0;
                lastShipMatrix = shipMatrix;
                _firstrun = false;
                lastTargetDir = targetDir;
                yawDeltaInput = 0;
                pitchDeltaInput = 0;
                yawInputSpeed = 0;
                pitchInputSpeed = 0;
                if (wantedYaw > 0) {
                    fullDriveYaw = true;
                    yawSpeed = ownYaw + TURNGYROCONST;
                } else
                    yawSpeed = ownYaw - TURNGYROCONST;
                if (wantedPitch > 0) {
                    fullDrivePitch = true;
                    pitchSpeed = ownPitch + TURNGYROCONST;
                } else
                    pitchSpeed = ownPitch - TURNGYROCONST;
            } else {
                Vector3D lastTargetVecLoc = Vector3D.TransformNormal(lastTargetDir, MatrixD.Transpose(shipMatrix));
                double lastTargetDirYaw = Math.Atan2(lastTargetVecLoc.X, -lastTargetVecLoc.Z);
                xyLenght = new Vector2D(lastTargetVecLoc.X, lastTargetVecLoc.Z).Length();
                if (lastTargetVecLoc.Z > 0) {
                    xyLenght *= -1;
                }
                double lastTargetDirPitch = Math.Atan2(-lastTargetVecLoc.Y, xyLenght);
                targetAngularVelYaw = wantedYaw - lastTargetDirYaw;
                targetAngularVelPitch = wantedPitch - lastTargetDirPitch;
                Vector3D myLastDir = lastShipMatrix.Forward;
                Vector3D myLastDirLoc = Vector3D.TransformNormal(myLastDir, MatrixD.Transpose(shipMatrix));
                if (fullDriveYaw)
                    maneuvrabilityYaw = Math.Abs(lastYaw - ownYaw);
                if (fullDrivePitch)
                    maneuvrabilityPitch = Math.Abs(lastPitch - ownPitch);
                fullDriveYaw = false;
                fullDrivePitch = false;
                double yawRotationInput = yawMult * shipController.RotationIndicator.Y;
                double pitchRotationInput = pitchMult * shipController.RotationIndicator.X;
                if (!autoAim) {
                    if (Math.Abs(yawInputSpeed - yawRotationInput) > maneuvrabilityYaw) {
                        if (yawInputSpeed - yawRotationInput > 0)
                            yawInputSpeed -= maneuvrabilityYaw;
                        else
                            yawInputSpeed += maneuvrabilityYaw;
                    } else {
                        yawInputSpeed = yawRotationInput;
                    }
                    if (Math.Abs(pitchInputSpeed - pitchRotationInput) > maneuvrabilityPitch) {
                        if (pitchInputSpeed - pitchRotationInput > 0)
                            pitchInputSpeed -= maneuvrabilityPitch;
                        else
                            pitchInputSpeed += maneuvrabilityPitch;
                    } else {
                        pitchInputSpeed = pitchRotationInput;
                    }
                    yawDeltaInput += yawInputSpeed;
                    pitchDeltaInput += pitchInputSpeed;
                } else {
                    yawRotationInput = 0;
                    pitchRotationInput = 0;
                    yawDeltaInput = 0;
                    pitchDeltaInput = 0;
                }
                double timeToStopYaw = Math.Abs((ownYaw - targetAngularVelYaw - yawRotationInput) / maneuvrabilityYaw);
                double avaibleDistanceYaw = wantedYaw + yawDeltaInput - ownYaw + (targetAngularVelYaw + yawRotationInput) * timeToStopYaw;
                double optimalAngularVelYaw = SOFTNAVCONST * Math.Sqrt(Math.Abs(2 * maneuvrabilityYaw * avaibleDistanceYaw));
                if (wantedYaw + yawDeltaInput < 0)
                    optimalAngularVelYaw *= -1;
                optimalAngularVelYaw += targetAngularVelYaw;
                b = Math.Abs(wantedYaw + yawDeltaInput) > maneuvrabilityYaw;
                if (b) {
                    if (ownYaw < optimalAngularVelYaw) {
                        yawSpeed = ownYaw + TURNGYROCONST;
                        fullDriveYaw = true;
                    } else {
                        yawSpeed = ownYaw - TURNGYROCONST;
                        fullDriveYaw = true;
                    }
                } else {
                    yawSpeed = wantedYaw + yawDeltaInput + targetAngularVelYaw;
                    fullDriveYaw = false;
                }
                double timeToStopPitch = Math.Abs((ownPitch - targetAngularVelPitch - pitchRotationInput) / maneuvrabilityPitch);
                double avaibleDistancePitch = wantedPitch + pitchDeltaInput - ownPitch + (targetAngularVelPitch + pitchRotationInput) * timeToStopPitch;
                double optimalAngularVelPitch = SOFTNAVCONST * Math.Sqrt(Math.Abs(2 * maneuvrabilityPitch * avaibleDistancePitch));
                if (wantedPitch + pitchDeltaInput < 0)
                    optimalAngularVelPitch *= -1;
                optimalAngularVelPitch += targetAngularVelPitch;
                b = Math.Abs(wantedPitch + pitchDeltaInput) > maneuvrabilityPitch;
                if (b) {
                    if (ownPitch < optimalAngularVelPitch) {
                        pitchSpeed = ownPitch + TURNGYROCONST;
                        fullDrivePitch = true;
                    } else {
                        pitchSpeed = ownPitch - TURNGYROCONST;
                        fullDrivePitch = true;
                    }
                } else {
                    pitchSpeed = wantedPitch + pitchDeltaInput + targetAngularVelPitch;
                    fullDrivePitch = false;
                }
            }
            double rollSpeed = rollIndicator;
            lastShipMatrix = shipMatrix;
            lastTargetDir = targetDir;
            lastYaw = ownYaw;
            lastPitch = ownPitch;
            pitchSpeed *= 60;
            yawSpeed *= 60;
            ApplyGyroOverride(pitchSpeed, yawSpeed, rollSpeed, gyros, shipMatrix);
        }
        public void Drop(List<IMyGyro> gyroList) {
            _firstrun = true;
            DropGyro(gyroList);
            yawDeltaInput = 0;
            pitchDeltaInput = 0;
        }
        public static void DropGyro(List<IMyGyro> gyroList) {
            foreach (var thisGyro in gyroList) {
                thisGyro.GyroOverride = false;
            }
        }
        public static void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList, MatrixD worldMatrix) {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);
            foreach (var thisGyro in gyroList) {
                var transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(thisGyro.WorldMatrix));
                thisGyro.Pitch = (float)transformedRotationVec.X;
                thisGyro.Yaw = (float)transformedRotationVec.Y;
                thisGyro.Roll = (float)transformedRotationVec.Z;
                thisGyro.GyroOverride = true;
            }
        }
    }

    public static class Languages {

        #region StorageString
        public static string storage = 
            "[Russian]\n" +
                "SCRIPT_NAME=СУО \"Ясень\"\n" + "TURRET=Башня\n" + "HULL=Корпус\n" +
                "GnF=Не найдена группа блоков!\n" + "NAME=Имя группы:\n" +
                "CANTLOCK=Недостаточно камер в радаре\n" + "LASTUPDATE=Последнее обновление блоков:\n" + "RADARCAMERAS=Камер в радаре -\n" +
                "TEXTPANELS=Панелей индикации -\n" + "MAINCOCKPIT=Главный кокпит -\n" + "SUCCESS=Успех\n" + "FAIL=Провал\n" +
                "TURRETGROUPBLOCKS=Попытка создать турель из блоков группы...\n" + "NOGUNS=Нет пушек на роторах\n" +
                "NOROTORS=Не достаточно роторов в группе\n" + "AUTOTURRETSUCCESS=Автоматический переход в режим турели - успех, все составляющие найдены\n" +
                "AUTOTURRETFAIL=Автоматический переход в режим турели не удался\n" + "AUTOHULLFAIL=Попытка перейти в режим наведения корпусом неуспешна\n" +
                "AUTOHULLSUCCESS=Активировано наведение корпусом\n" + "NOCOCKPITS=Нет кокпитов\n" + "NOGYRO=Нет гироскопов\n" + "GYROS=Гироскопов\n" +
                "SEARCHING=Радар - поиск целей:\n" + "LOCKED=Цель захвачена\n" + 
                "ROTOR=Ротор\n" + "MAINEROTOR=Ведущий подъемный ротор\n" + "ALLROTORS=Подъемных роторов всего\n" + "ALLWEAPONS=Всего орудий\n" +
                "SETTINGS=Настройки\n" + "SIMPLE=Базовые\n" + "ADVANCED=Расширенные\n" + "RADAR=Радар\n" + "COCKPIT=Кокпит\n" + "WEAPON=Орудие\n" + 
                "SCOCKPIT=Малый@кокпит\n" + "FCOCKPIT=Кокпит@Истребителя\n" + "SCSEAT=Малое кресло@пилота\n" + "LCOCKPIT=Большой@кокпит\n" +
                "CSEAT=Кресло@пилота\n" + "CUSTOM=Кастомный\n" + "GUTLING=Пулемет\n" + "AUTOCANON=Автопушка\n" + "ASSAULT=Штурмовая\n" + "ARTY=Артиллерия\n" +
                "SRAILGUN=РельсаМ\n" + "LRAILGUN=РельсаБ\n" + "ENEMY=Враждебные\n" + "NEUTRAL=Нейтральные\n" + "ALLIE=Союзные\n" + "AIM=Наведение\n" +
                "AUTO=Автоматическое\n" + "TRACKING=Помощь в\n" + "ASSIST=наведении\n" + "PROJVEL=Cкорость@снаряда\n" + "SHOTRANGE=Дальность@выстрела\n" +
                "TARGETINGPOINT=Точка@прицеливания\n" + "INITIALRANGE=Дальность@поиска\n" + "POINT0=Точка@захвата\n" + "POINT1=Центр@Цели\n" + "POINT2=Реальный@захват\n" +
            "[English]\n" + 
                "SCRIPT_NAME=FCS \"Ash\"\n" + "TURRET=Turret\n" + "HULL=Hull\n" + 
                "GnF=Group not found!\n" + "NAME=Name of Group:\n" +
                "CANTLOCK=Not enought cameras in the radar\n" + "LASTUPDATE=Last update:\n" + "RADARCAMERAS=Cameras in radar -\n" + 
                "TEXTPANELS=Text panels -\n" + "MAINCOCKPIT=Main cockpit -\n" + "SUCCESS=Success\n" + "FAIL=Failure\n" + 
                "TURRETGROUPBLOCKS=Trying to create a turret from blocks in a group...\n" + "NOGUNS=Not found weapons on rotors\n" +
                "NOROTORS=Not enought rotors in the group\n" + "AUTOTURRETSUCCESS=Successful auto-transition to turret mode, all components found\n" +
                "AUTOTURRETFAIL=Auto-transition to turret mode failed\n" + "AUTOHULLFAIL=Transition to hull-guided mode failed\n" +
                "AUTOHULLSUCCESS=Hull aiming activated\n" + "NOCOCKPITS=No cockpits\n" + "NOGYRO=No gyros\n" + "GYROS=Gyroscopes\n" +
                "SEARCHING=Radar - searching\n" + "LOCKED=Target locked\n" +
                "ROTOR=Rotor\n" + "MAINEROTOR=Main elevation rotor\n" + "ALLROTORS=Total elevation rotors\n" + "ALLWEAPONS=Total weapons\n" +
                "SETTINGS=Settings\n" + "SIMPLE=Simple\n" + "ADVANCED=Advanced\n" + "RADAR=Radar\n" + "COCKPIT=Cockpit\n" + "WEAPON=Weapon\n" +
                "SCOCKPIT=Small@cockpit\n" + "FCOCKPIT=Fighter@cockpit\n" + "SCSEAT=Small control@seat\n" + "LCOCKPIT=Large@cockpit\n" +
                "CSEAT=Control@seat\n" + "CUSTOM=Custom\n" + "GUTLING=Gutling\n" + "AUTOCANON=Autocanon\n" + "ASSAULT=Assault\n" + "ARTY=Artillery\n" +
                "SRAILGUN=S Railgun\n" + "LRAILGUN=L RAILGUN\n" + "ENEMY=Enemy\n" + "NEUTRAL=Neutral\n" + "ALLIE=Allie\n" + "AIM=Auto Aim\n" +
                "AUTO= \n" + "TRACKING=Aim Assist\n" + "ASSIST=\n" + "PROJVEL=Projectile@velocity\n" + "SHOTRANGE=Shoot@range\n" +
                "TARGETINGPOINT=Aiming@point\n" + "INITIALRANGE=Lock@Range\n" + "POINT0=First@Lock\n" + "POINT1=Center of@Target\n" + "POINT2=Real@lock\n";

        #endregion StorageString

        static MyIni languageIni = new MyIni();
        static bool b = languageIni.TryParse(storage);

        public static string Translate(string language, string name) {
            string s = languageIni.Get(language, name).ToString("Translation error");
            return s.Replace("@", "\n");
        }
    }

    public static class MyMath {
        public static Vector3D FindInterceptGVector(Vector3D myPos, Vector3D MySpeed, EnemyTargetedInfo Target, Vector3D gravity, double projectileSpeed, int targetingPoint = 0, bool dir = true) {
            Vector3D target = Target.Position;
            if (targetingPoint == 0)
                if (Target.TargetedPoint != null)
                    target = Target.TargetedPoint.GetValueOrDefault();
            if (targetingPoint == 2)
                if (Target.HitPosition != null)
                    target = Target.HitPosition.GetValueOrDefault();
            Vector3D targetDirection = target - myPos;
            double speed = projectileSpeed;
            double correctedSpeed = speed;
            Vector3D sumspeed = Target.Velocity - MySpeed;
            Vector3D InterceptVector = FindInterceptVector(myPos, correctedSpeed, target, sumspeed);
            for (int i = 0; i < 10; i++) {
                FindGravityCorrection_DirectFire(speed, ref correctedSpeed, InterceptVector, gravity);
                InterceptVector = FindInterceptVector(myPos, correctedSpeed, target, sumspeed);
            }
            double timeToHit = InterceptVector.Length() / correctedSpeed;
            Vector3D yVector = -gravity * timeToHit * timeToHit / 2;
            InterceptVector = InterceptVector + yVector;
            if (dir)
                return InterceptVector;
            else
                return InterceptVector + myPos;
        }
        public static Vector3D FindBallisticPoint(Vector3D myPos, Vector3D mySpeed, EnemyTargetedInfo Target, Vector3D grav, Vector3D projectileSpeed, int targetingPoint = 0) {
            Vector3D target = Target.Position;
            if (targetingPoint == 0)
                if (Target.TargetedPoint != null)
                    target = Target.TargetedPoint.GetValueOrDefault();
            if (targetingPoint == 2)
                if (Target.HitPosition != null)
                    target = Target.HitPosition.GetValueOrDefault();
            Vector3D sumSpeed = mySpeed + projectileSpeed - Target.Velocity;
            Vector3D dirToTarget = target - myPos;
            double distanceToTarget = dirToTarget.Length();
            double projectileSumSpeed = sumSpeed.Length();
            double timeToImpact = distanceToTarget / projectileSumSpeed;
            Vector3D BallisticPoint = myPos + sumSpeed * timeToImpact + grav * timeToImpact * timeToImpact / 2;
            return BallisticPoint;
        }
        public static Vector3D FindBallisticPoint(Vector3D myPos, Vector3D mySpeed, Vector3D Target, Vector3D grav, Vector3D projectileSpeed) {
            Vector3D sumSpeed = mySpeed + projectileSpeed;
            Vector3D dirToTarget = Target - myPos;
            double distanceToTarget = dirToTarget.Length();
            double projectileSumSpeed = sumSpeed.Length();
            double timeToImpact = distanceToTarget / projectileSumSpeed;
            Vector3D BallisticPoint = myPos + sumSpeed * timeToImpact + grav * timeToImpact * timeToImpact / 2;
            return BallisticPoint;
        }
        static void FindGravityCorrection_DirectFire(double speed, ref double x, Vector3D targetDir, Vector3D grav) {
            double distanceToTarget = targetDir.Length();
            double angleCos = grav.Dot(targetDir) / (grav.Length() * targetDir.Length());
            double l = targetDir.Length();
            double g = grav.Length();
            double a = 1;
            double b = (2 * g * l * angleCos) - (speed * speed);
            double c = Math.Pow((g * l / 2), 2);
            double? resoult1;
            double? resoult2;
            double? x1 = null;
            double? x2 = null;
            QuadraticEquation(a, b, c, out resoult1, out resoult2);
            if (resoult1 != null)
                if (resoult1 > 0) {
                    x1 = Math.Sqrt(resoult1.GetValueOrDefault());
                    x = x1.GetValueOrDefault();
                }
            if (resoult2 != null)
                if (resoult2 > 0) {
                    x2 = Math.Sqrt(resoult2.GetValueOrDefault());
                    if (x1 != null)
                        if (x2 > x1)
                            x = x2.GetValueOrDefault();
                }
        }
        public static MatrixD CreateLookAtForwardDir(Vector3D cameraPosition, Vector3D cameraForwardVector, Vector3D suggestedUp) {
            Vector3D up = Vector3D.Cross(Vector3D.Cross(cameraForwardVector, suggestedUp), cameraForwardVector);
            Vector3D vector3D = Vector3D.Normalize(-cameraForwardVector);
            Vector3D vector3D2 = Vector3D.Normalize(Vector3D.Cross(up, vector3D));
            Vector3D vector = Vector3D.Cross(vector3D, vector3D2);
            MatrixD result = default(MatrixD);
            result.M11 = vector3D2.X;
            result.M12 = vector3D2.Y;
            result.M13 = vector3D2.Z;
            result.M14 = 0.0;
            result.M21 = vector.X;
            result.M22 = vector.Y;
            result.M23 = vector.Z;
            result.M24 = 0.0;
            result.M31 = vector3D.X;
            result.M32 = vector3D.Y;
            result.M33 = vector3D.Z;
            result.M34 = 0.0;
            result.M41 = cameraPosition.X;
            result.M42 = cameraPosition.Y;
            result.M43 = cameraPosition.Z;
            result.M44 = 1.0;
            return result;
        }
        public static MatrixD CreateLookAtUpDir(Vector3D cameraPosition, Vector3D suggestedForward, Vector3D cameraUpVector) {
            Vector3D cameraForwardVector = Vector3D.Cross(Vector3D.Cross(cameraUpVector, suggestedForward), cameraUpVector);
            Vector3D vector3D = Vector3D.Normalize(-cameraForwardVector);
            Vector3D vector3D2 = Vector3D.Normalize(Vector3D.Cross(cameraUpVector, vector3D));
            Vector3D vector = Vector3D.Cross(vector3D, vector3D2);
            MatrixD result = default(MatrixD);
            result.M11 = vector3D2.X;
            result.M12 = vector3D2.Y;
            result.M13 = vector3D2.Z;
            result.M14 = 0.0;
            result.M21 = vector.X;
            result.M22 = vector.Y;
            result.M23 = vector.Z;
            result.M24 = 0.0;
            result.M31 = vector3D.X;
            result.M32 = vector3D.Y;
            result.M33 = vector3D.Z;
            result.M34 = 0.0;
            result.M41 = cameraPosition.X;
            result.M42 = cameraPosition.Y;
            result.M43 = cameraPosition.Z;
            result.M44 = 1.0;
            return result;
        }
        public static Vector3D FindInterceptVector(Vector3D shotOrigin, double shotVel, Vector3D targetOrigin, Vector3D targetVel) {
            Vector3D toTarget = targetOrigin - shotOrigin;
            Vector3D dirToTarget = Vector3D.Normalize(toTarget);
            Vector3D targetVelOrth = Vector3D.Dot(targetVel, dirToTarget) * dirToTarget;
            Vector3D targetVelTang = targetVel - targetVelOrth;
            Vector3D shotVelTang = targetVelTang;
            double shotVelSpeed = shotVelTang.Length();
            if (shotVelSpeed > shotVel) {
                return Vector3D.Normalize(targetVel) * shotVel;
            } else {
                double shotSpeedOrth = Math.Sqrt(shotVel * shotVel - shotVelSpeed * shotVelSpeed);
                Vector3D shotVelOrth = dirToTarget * shotSpeedOrth;
                double timeToHit = toTarget.Length() / (targetVelOrth - shotVelOrth).Length();
                return (shotVelOrth + shotVelTang).Normalized() * (timeToHit * shotVel);
            }
        }
        public static void QuadraticEquation(double a, double b, double c, out double? x1, out double? x2) {
            var discriminant = Math.Pow(b, 2) - 4 * a * c;
            if (discriminant < 0) {
                x1 = null; x2 = null;
            } else {
                if (discriminant == 0) {
                    x1 = -b / (2 * a);
                    x2 = x1;
                } else {
                    x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                    x2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                }
            }
        }
        public static Vector3D VectorTransform(Vector3D Vec, MatrixD Orientation) {
            return new Vector3D(Vec.Dot(Orientation.Right), Vec.Dot(Orientation.Up), Vec.Dot(Orientation.Backward));
        }
        public static double CosBetween(Vector3D a, Vector3D b) {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return 0;
            else
                return MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1);
        }
        public static double CalculateRotorDeviationAngle(Vector3D forwardVector, MatrixD lastOrientation) {
            var flattenedForwardVector = VectorRejection(forwardVector, lastOrientation.Up);
            return VectorAngleBetween(flattenedForwardVector, lastOrientation.Forward) * Math.Sign(flattenedForwardVector.Dot(lastOrientation.Left));
        }
        public static void CalculateYawVelocity(MatrixD turretMatrix, MatrixD turretLastMatrix, out double speed) {
            Vector3D now = turretMatrix.Forward;
            var flattenedForwardVector = VectorRejection(now, turretLastMatrix.Up);
            speed = -VectorAngleBetween(flattenedForwardVector, turretLastMatrix.Forward) * Math.Sign(flattenedForwardVector.Dot(turretLastMatrix.Left));
        }
        public static void CalculatePitchVelocity(MatrixD weaponMatrix, MatrixD weaponLastMatrix, out double speed) {
            Vector3D now = weaponMatrix.Forward;
            var flattenedForwardVector = VectorRejection(now, weaponLastMatrix.Right);
            speed = -VectorAngleBetween(flattenedForwardVector, weaponLastMatrix.Forward) * Math.Sign(flattenedForwardVector.Dot(weaponLastMatrix.Down));
        }
        public static Vector3D VectorProjection(Vector3D a, Vector3D b) {
            return a.Dot(b) / b.LengthSquared() * b;
        }
        public static Vector3D VectorRejection(Vector3D a, Vector3D b) {
            if (Vector3D.IsZero(b)                                      )
                return Vector3D.Zero;
            return a - a.Dot(b) / b.LengthSquared() * b;
        }
        public static double VectorAngleBetween(Vector3D a, Vector3D b) {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return 0;
            else
                return Math.Acos(MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
        }
        public static double Vector2AngleBetween(Vector2D a, Vector2D b) {
            if (a.Length() == 0 || b.Length() == 0)
                return 0;
            else
                return Math.Acos(MathHelper.Clamp(Vector2D.Dot(a, b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
        }
        public static double Vector2DeviationFromZero(Vector2D a) {
            if (a.Length() == 0)
                return 0;
            Vector2D b = new Vector2D(1, 0);
            return Math.Acos(MathHelper.Clamp(Vector2D.Dot(a, b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
        }
    }

    class PID {
        public double Kp { get; set; } = 0;
        public double Ki { get; set; } = 0;
        public double Kd { get; set; } = 0;
        public double Value { get; private set; }
        double _timeStep = 0;
        double _inverseTimeStep = 0;
        double _errorSum = 0;
        double _lastError = 0;
        bool _firstRun = true;

        public PID(double kp, double ki, double kd, double timeStep) {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            _timeStep = timeStep;
            _inverseTimeStep = 1 / _timeStep;
        }
        protected virtual double GetIntegral(double currentError, double errorSum, double timeStep) {
            return errorSum + currentError * timeStep;
        }
        public double Control(double error) {
            double errorDerivative = (error - _lastError) * _inverseTimeStep;
            if (_firstRun) {
                errorDerivative = 0;
                _firstRun = false;
            }
            _errorSum = GetIntegral(error, _errorSum, _timeStep);
            _lastError = error;
            Value = Kp * error + Ki * _errorSum + Kd * errorDerivative;
            return Value;
        }
        public double Control(double error, double timeStep) {
            if (timeStep != _timeStep) {
                _timeStep = timeStep;
                _inverseTimeStep = 1 / _timeStep;
            }
            return Control(error);
        }
        public void Reset() {
            _errorSum = 0;
            _lastError = 0;
            _firstRun = true;
        }
    }

    class Radar {
        public List<IMyCameraBlock> radarCameras = new List<IMyCameraBlock>();
        public EnemyTargetedInfo lockedtarget { private set; get; }
        long lastRadarLockTick = 0;
        public Vector3D? pointOfLock;
        public int countOfCameras;
        public bool Searching = false;
        public int counter = 0;
        const float STABLELOCK = 1.1f;
        const float NEWTARGET = 0.25f;
        bool enemy = true;
        bool neutral = false;
        bool allie = false;

        public Radar(List<IMyCameraBlock> radar) {
            radarCameras = radar;
            foreach (var camera in radarCameras)
                camera.EnableRaycast = true;
            lockedtarget = null;
            countOfCameras = radarCameras.Count;
        }

        #region Methods

        public void SetTargets(bool allieIn, bool neutralIn, bool enemyIn) { enemy = enemyIn; neutral = neutralIn; allie = allieIn; }
        public bool UpdateTarget(EnemyTargetedInfo newInfo) {
            if (lockedtarget != null)
                if (lockedtarget.EntityId == newInfo.EntityId) {
                    lockedtarget = newInfo;
                    return true;
                }
            return false;
        }
        public bool TryLock(long tick, double InitialRange = 2000) {
            MyDetectedEntityInfo newDetectedInfo;
            long TickPassed = tick - lastRadarLockTick;
            if (TickPassed > InitialRange * 0.03 / countOfCameras) {
                var lockcam = GetCameraWithMaxRange(radarCameras);
                if (lockcam == null)
                    return false;
                if (lockcam.CanScan(InitialRange)) {
                    newDetectedInfo = lockcam.Raycast(InitialRange, 0, 0);
                    if (!newDetectedInfo.IsEmpty()) {
                        if (newDetectedInfo.Type == MyDetectedEntityType.SmallGrid || newDetectedInfo.Type == MyDetectedEntityType.LargeGrid) {
                            if ((newDetectedInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) && enemy ||
                                (newDetectedInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Neutral) && neutral ||
                                (newDetectedInfo.Relationship == MyRelationsBetweenPlayerAndBlock.NoOwnership) && neutral ||
                                (newDetectedInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Friends) && allie ||
                                (newDetectedInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Owner) && allie) {
                                lockedtarget = new EnemyTargetedInfo(tick, newDetectedInfo, lockcam.WorldMatrix.Forward);
                                lastRadarLockTick = tick;
                                counter = 0;
                                pointOfLock = null;
                            } else
                                pointOfLock = newDetectedInfo.HitPosition;
                        } else
                            pointOfLock = newDetectedInfo.HitPosition;
                    }
                }
            }
            return true;
        }
        public bool Update(ref string debuginfo, long tick, int unlockTime, double initialRange = 2000) {
            counter++;
            if (countOfCameras < 2)
                return false;
            if (lockedtarget != null) {
                long TickPassed = tick - lastRadarLockTick;
                Vector3D shift = lockedtarget.Velocity * TickPassed / 60;
                if (shift.Length() < 0.002)
                    shift = Vector3D.Zero;
                if (TickPassed > (lockedtarget.Position + shift - radarCameras[0].GetPosition()).Length() * 0.03 / radarCameras.Count * STABLELOCK) {
                    if (radarCameras == null)
                        return false;
                    IMyCameraBlock c = GetCameraWithMaxRange(radarCameras);
                    if (c == null)
                        return false;
                    double TargetDistance = (lockedtarget.Position + shift - c.GetPosition()).Length() + 10d;
                    MyDetectedEntityInfo DetectedEntity;
                    if (c.AvailableScanRange >= TargetDistance) {
                        Vector3D point;
                        Vector3D dir;
                        Vector3D locDir;
                        Vector3D camPos = c.GetPosition();
                        if (lockedtarget.DeltaPosition != null) {
                            point = lockedtarget.Position + shift + lockedtarget.DeltaPosition.GetValueOrDefault();
                            dir = point - camPos;
                            locDir = Vector3D.TransformNormal(dir, MatrixD.Transpose(c.WorldMatrix));
                            DetectedEntity = c.Raycast(dir.Length() + 10, locDir);
                            if (DetectedEntity.EntityId == lockedtarget.EntityId)
                                goto UpdateInfo;
                            if (counter > unlockTime * NEWTARGET)
                                if (CheckForNewTarget(DetectedEntity, tick, c.WorldMatrix.Forward))
                                    goto UpdateInfo;
                        }
                        if (lockedtarget.TargetedPoint != null) {
                            point = lockedtarget.inBodyPointPosition.GetValueOrDefault() + shift;
                            dir = point - camPos;
                            locDir = Vector3D.TransformNormal(dir, MatrixD.Transpose(c.WorldMatrix));
                            DetectedEntity = c.Raycast(dir.Length() + 10, locDir);
                            if (DetectedEntity.EntityId == lockedtarget.EntityId) {
                                goto UpdateInfo;
                            }
                            if (counter > unlockTime * NEWTARGET)
                                if (CheckForNewTarget(DetectedEntity, tick, c.WorldMatrix.Forward))
                                    goto UpdateInfo;
                        }
                        point = lockedtarget.Position + shift;
                        dir = point - camPos;
                        locDir = Vector3D.TransformNormal(dir, MatrixD.Transpose(c.WorldMatrix));
                        DetectedEntity = c.Raycast(dir.Length() + 10, locDir);
                        if (DetectedEntity.EntityId == lockedtarget.EntityId)
                            goto UpdateInfo;
                        if (counter > unlockTime * NEWTARGET)
                            if (CheckForNewTarget(DetectedEntity, tick, c.WorldMatrix.Forward))
                                goto UpdateInfo;
                    } else
                        return false;
                UpdateInfo:
                    if (DetectedEntity.EntityId == lockedtarget.EntityId) {
                        lastRadarLockTick = tick;
                        lockedtarget.UpdateTargetInfo(tick, DetectedEntity, c.WorldMatrix.Forward);
                        counter = 0;
                    }
                }
                if (counter >= unlockTime) {
                    counter = 0;
                    lockedtarget = null;
                }
            } else if (Searching) {
                TryLock(tick, initialRange);
            }
            return true;
        }
        public void GetTarget(EnemyTargetedInfo target, long tick) {
            Searching = true;
            lockedtarget = target;
            lastRadarLockTick = tick;
        }
        bool CheckForNewTarget(MyDetectedEntityInfo newEntity, long tick, Vector3D viewvec) {
            if (newEntity.Type == MyDetectedEntityType.SmallGrid || newEntity.Type == MyDetectedEntityType.LargeGrid)
                if ((newEntity.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) && enemy ||
                    (newEntity.Relationship == MyRelationsBetweenPlayerAndBlock.Neutral) && neutral ||
                    (newEntity.Relationship == MyRelationsBetweenPlayerAndBlock.Friends) && allie ||
                    (newEntity.Relationship == MyRelationsBetweenPlayerAndBlock.Owner) && allie) {
                    lockedtarget = new EnemyTargetedInfo(tick, newEntity, viewvec);
                    lastRadarLockTick = tick;
                    counter = 0;
                    return true;
                }
            return false;
        }
        public void DropLock() {
            pointOfLock = null;
            lockedtarget = null;
            Searching = false;
        }
        IMyCameraBlock GetCameraWithMaxRange(List<IMyCameraBlock> cameras) {
            double maxRange = 0;
            IMyCameraBlock maxRangeCamera = null;
            foreach (var c in cameras) {
                if (c.AvailableScanRange > maxRange) {
                    maxRangeCamera = c;
                    maxRange = maxRangeCamera.AvailableScanRange;
                }
            }
            return maxRangeCamera;
        }

        #endregion Methods
    }

    static class SystemHelper {
        public static bool AddBlockIfType<T>(IMyTerminalBlock block, out T orig) where T : class, IMyTerminalBlock {
            T typedBlock = block as T;
            orig = typedBlock;
            if (typedBlock == null)
                return false;
            return true;
        }
        public static bool AddToListIfType<T>(IMyTerminalBlock block, List<T> list) where T : class, IMyTerminalBlock {
            T typedBlock;
            return AddToListIfType(block, list, out typedBlock);
        }
        public static bool AddToListIfType<T>(IMyTerminalBlock block, List<T> list, out T typedBlock) where T : class, IMyTerminalBlock {
            typedBlock = block as T;
            if (typedBlock != null) {
                list.Add(typedBlock);
                return true;
            }
            return false;
        }
    }
    
    class Turret {
        const double TURNGYROCONST = 0.0035;
        const float SOFTNAVCONST = 0.8f;
        IMyMotorStator rotorA;
        List<IMyMotorStator> rotorsE;
        IMyMotorStator MainElRotor;
        List<IMyUserControllableGun> weapons;
        List<IMyCameraBlock> radarCameras;
        List<IMyGyro> _turretGyros = new List<IMyGyro>();
        List<IMyGyro> _weaponGyros = new List<IMyGyro>();
        Vector3D turretFrontVec;
        Vector3D lastInterceptVector;
        float MultiplierElevation;
        float deltaAzimuth;
        public IMyUserControllableGun referenceGun;
        public MatrixD turretMatrix { get; private set; }
        MatrixD lastWeaponMatrix;
        MatrixD lastTurretMatrix;
        MatrixD lastRotorAMatrix;
        MatrixD lastRotorEMatrix;
        static MyIni languageIni = new MyIni();
        bool b = languageIni.TryParse(Languages.storage);
        bool firstRunAim = true;
        double yawDeltaInput = 0;
        double yawInputSpeed = 0;
        double pitchDeltaInput = 0;
        double pitchInputSpeed = 0;
        double? lastSpeedYaw = null, lastSpeedPitch = null;
        double? maneuvrabilityYaw = null;
        double? maneuvrabilityPitch = null;
        bool _firstUpdate = true;
        bool fullDriveYaw = false, fullDrivePitch = false;
        public Turret() {
            _turretGyros = new List<IMyGyro>();
            radarCameras = new List<IMyCameraBlock>();
            rotorsE = new List<IMyMotorStator>();
            weapons = new List<IMyUserControllableGun>();
        }

        #region Methods

        public bool UpdateBlocks(IMyMotorStator newRotorA, List<IMyMotorStator> newRotorsE,
        IMyMotorStator mainElRotor, List<IMyUserControllableGun> newWeapons, List<IMyCameraBlock> cameras, List<IMyGyro> gyros) {
            radarCameras = cameras;
            rotorA = newRotorA;
            MainElRotor = mainElRotor;
            rotorsE = newRotorsE;
            rotorsE.Remove(mainElRotor);
            weapons = newWeapons;
            referenceGun = null;
            foreach (var weapon in weapons) {
                if (weapon.CubeGrid == MainElRotor.TopGrid) {
                    referenceGun = weapon;
                    break;
                }
            }
            if (referenceGun == null)
                return false;
            _turretGyros.Clear();
            _weaponGyros.Clear();
            foreach (var g in gyros) {
                if (g.CubeGrid == rotorA.TopGrid) {
                    _turretGyros.Add(g);
                }
                if (g.CubeGrid == referenceGun.CubeGrid)
                    _weaponGyros.Add(g);
            }
            turretFrontVec = referenceGun.WorldMatrix.Forward;
            if (_firstUpdate) {
                _firstUpdate = false;
                lastRotorAMatrix = newRotorA.WorldMatrix;
                lastRotorEMatrix = MainElRotor.WorldMatrix;
                lastWeaponMatrix = MyMath.CreateLookAtForwardDir(referenceGun.GetPosition(), turretFrontVec, rotorA.WorldMatrix.Up);
                lastTurretMatrix = MyMath.CreateLookAtUpDir(rotorA.Top.WorldMatrix.Translation, turretFrontVec, rotorA.WorldMatrix.Up);
            }
            MultiplierElevation = 1;
            float deltaAzimuthCos = (float)rotorA.Top.WorldMatrix.Right.Dot(MainElRotor.WorldMatrix.Up);
            Vector3D absUpVec = rotorA.WorldMatrix.Up;
            Vector3D turretSideVec = MainElRotor.WorldMatrix.Up;
            Vector3D turretFrontCrossSide = turretFrontVec.Cross(turretSideVec);
            if (turretFrontCrossSide.Dot(absUpVec) < 0) {
                deltaAzimuthCos += MathHelper.Pi;
                MultiplierElevation = -1;
            }
            if (deltaAzimuthCos > 1)
                deltaAzimuthCos = 1;
            if (deltaAzimuthCos < -1)
                deltaAzimuthCos = -1;
            deltaAzimuth = (float)Math.Acos(deltaAzimuthCos);
            return true;
        }
        public void Update(Vector3D interceptVector, bool autoAim = true, float az = 0, float el = 0, bool dir = true) {
            if (!dir)
                interceptVector -= referenceGun.WorldMatrix.Translation;
            turretFrontVec = referenceGun.WorldMatrix.Forward;
            MatrixD weaponMatrix = MyMath.CreateLookAtForwardDir(referenceGun.GetPosition(), turretFrontVec, rotorA.WorldMatrix.Up);
            turretMatrix = MyMath.CreateLookAtUpDir(referenceGun.GetPosition(), turretFrontVec, rotorA.WorldMatrix.Up);
            float azError = (float)MyMath.CalculateRotorDeviationAngle(rotorA.WorldMatrix.Forward, lastRotorAMatrix);
            float elError = (float)MyMath.CalculateRotorDeviationAngle(MainElRotor.WorldMatrix.Forward, lastRotorEMatrix);
            double ownYaw, ownPitch;
            MyMath.CalculateYawVelocity(turretMatrix, lastTurretMatrix, out ownYaw);
            MyMath.CalculatePitchVelocity(weaponMatrix, lastWeaponMatrix, out ownPitch);
            double yawRotationInput = az;
            double pitchRotationInput = el;
            if (autoAim) {
                yawRotationInput = 0;
                pitchRotationInput = 0;
                yawDeltaInput = 0;
                pitchDeltaInput = 0;
            } else {
                if (maneuvrabilityYaw != null && Math.Abs(yawInputSpeed - yawRotationInput) > maneuvrabilityYaw) {
                    if (yawInputSpeed - yawRotationInput > 0)
                        yawInputSpeed -= maneuvrabilityYaw.Value;
                    else
                        yawInputSpeed += maneuvrabilityYaw.Value;
                } else {
                    yawInputSpeed = yawRotationInput; }
                if (maneuvrabilityPitch != null && Math.Abs(pitchInputSpeed - pitchRotationInput) > maneuvrabilityPitch) {
                    if (pitchInputSpeed - pitchRotationInput > 0)
                        pitchInputSpeed -= maneuvrabilityPitch.Value;
                    else
                        pitchInputSpeed += maneuvrabilityPitch.Value;
                } else {
                    pitchInputSpeed = pitchRotationInput;
                }
                yawDeltaInput += yawInputSpeed;
                pitchDeltaInput += pitchInputSpeed;
            }
            Vector3D targetVecLocTurret = Vector3D.TransformNormal(interceptVector, MatrixD.Transpose(turretMatrix));
            double wantedYaw = Math.Atan2(targetVecLocTurret.X, -targetVecLocTurret.Z);
            Vector3D targetVecLocWeapon = Vector3D.TransformNormal(interceptVector, MatrixD.Transpose(weaponMatrix));
            double xyLenght = new Vector2D(targetVecLocWeapon.X, targetVecLocWeapon.Z).Length();
            double wantedPitch = Math.Atan2(targetVecLocWeapon.Y, xyLenght);
            if (firstRunAim) {
                firstRunAim = false;
                lastInterceptVector = interceptVector;
            }
            double targetAngularVelYaw, targetAngularVelPitch;
            Vector3D lastTargetVecLocTurret = Vector3D.TransformNormal(lastInterceptVector, MatrixD.Transpose(turretMatrix));
            double lastTargetDirYaw = Math.Atan2(lastTargetVecLocTurret.X, -lastTargetVecLocTurret.Z);
            Vector3D lastTargetVecLocWeapon = Vector3D.TransformNormal(lastInterceptVector, MatrixD.Transpose(weaponMatrix));
            xyLenght = new Vector2D(lastTargetVecLocWeapon.X, lastTargetVecLocWeapon.Z).Length();
            double lastTargetDirPitch = Math.Atan2(lastTargetVecLocWeapon.Y, xyLenght);
            targetAngularVelYaw = wantedYaw - lastTargetDirYaw;
            targetAngularVelPitch = wantedPitch - lastTargetDirPitch;
            double yawRotorSpeed, pitchRotorSpeed, yawGyroSpeed, pitchGyroSpeed;
            if (fullDriveYaw) {
                fullDriveYaw = false;
                maneuvrabilityYaw = Math.Abs(lastSpeedYaw.GetValueOrDefault() - ownYaw);
            }
            if (maneuvrabilityYaw == null) {
                if (wantedYaw > 0) {
                    yawRotorSpeed = MathHelper.Pi / 60;
                    yawGyroSpeed = ownYaw + TURNGYROCONST;
                } else {
                    yawRotorSpeed = -MathHelper.Pi / 60;
                    yawGyroSpeed = ownYaw - TURNGYROCONST;
                }
                fullDriveYaw = true;
            } else {
                double timeToStopYaw = Math.Abs((ownYaw - targetAngularVelYaw - yawRotationInput - azError) / maneuvrabilityYaw.Value);
                double avaibleDistanceYaw = wantedYaw + yawDeltaInput - ownYaw + (targetAngularVelYaw + yawRotationInput + azError) * timeToStopYaw;
                double optimalAngularVelYaw = SOFTNAVCONST * Math.Sqrt(Math.Abs(2 * maneuvrabilityYaw.Value * avaibleDistanceYaw));
                if (wantedYaw + yawDeltaInput < 0)
                    optimalAngularVelYaw *= -1;
                optimalAngularVelYaw += targetAngularVelYaw + azError;
                if (Math.Abs(wantedYaw + yawDeltaInput) > maneuvrabilityYaw) {
                    yawRotorSpeed = optimalAngularVelYaw;
                    yawGyroSpeed = ownYaw < optimalAngularVelYaw ? ownYaw + TURNGYROCONST : ownYaw - TURNGYROCONST;
                    fullDriveYaw = true;
                } else {
                    yawRotorSpeed = wantedYaw + yawDeltaInput + targetAngularVelYaw + azError;
                    yawGyroSpeed = wantedYaw + yawDeltaInput + targetAngularVelYaw;
                    fullDriveYaw = false;
                }
            }
            if (fullDrivePitch) {
                fullDrivePitch = false;
                maneuvrabilityPitch = Math.Abs(lastSpeedPitch.GetValueOrDefault() - ownPitch);
            }
            if (maneuvrabilityPitch == null) {
                if (wantedPitch > 0) {
                    pitchRotorSpeed = MathHelper.Pi / 60;
                    pitchGyroSpeed = ownPitch + TURNGYROCONST;
                } else {
                    pitchRotorSpeed = -MathHelper.Pi / 60;
                    pitchGyroSpeed = ownPitch - TURNGYROCONST;
                }
                fullDrivePitch = true;
            } else {
                double timeToStopPitch = Math.Abs((ownPitch - targetAngularVelPitch - pitchRotationInput - elError) / maneuvrabilityPitch.Value);
                double avaibleDistancePitch = wantedPitch + pitchDeltaInput - ownPitch + (targetAngularVelPitch + pitchRotationInput + elError) * timeToStopPitch;
                double optimalAngularVelPitch = SOFTNAVCONST * Math.Sqrt(Math.Abs(2 * maneuvrabilityPitch.Value * avaibleDistancePitch));
                if (wantedPitch + pitchDeltaInput < 0)
                    optimalAngularVelPitch *= -1;
                optimalAngularVelPitch += targetAngularVelPitch + elError;
                if (Math.Abs(wantedPitch + pitchDeltaInput) > maneuvrabilityPitch) {
                    pitchRotorSpeed = optimalAngularVelPitch;
                    pitchGyroSpeed = ownPitch < optimalAngularVelPitch ? ownPitch + TURNGYROCONST : ownPitch - TURNGYROCONST;
                    fullDrivePitch = true;
                } else {
                    pitchRotorSpeed = wantedPitch + pitchDeltaInput + targetAngularVelPitch + elError;
                    pitchGyroSpeed = wantedPitch + pitchDeltaInput + targetAngularVelPitch;
                    fullDrivePitch = false;
                }
            }
            rotorA.TargetVelocityRad = (float)yawRotorSpeed * 60;
            MainElRotor.TargetVelocityRad = (float)pitchRotorSpeed * 60;
            foreach (var rotor in rotorsE) {
                if (!rotor.Closed)
                    SetSupprotRotor(rotor, turretFrontVec, (float)pitchRotorSpeed);
            }
            HullGuidance.ApplyGyroOverride(0, yawGyroSpeed * 60, 0, _turretGyros, turretMatrix);
            HullGuidance.ApplyGyroOverride(-pitchGyroSpeed * 60, yawGyroSpeed * 60, 0, _weaponGyros, turretMatrix);
            lastInterceptVector = interceptVector;
            lastSpeedYaw = ownYaw;
            lastSpeedPitch = ownPitch;
            lastTurretMatrix = turretMatrix;
            lastWeaponMatrix = weaponMatrix;
            lastRotorAMatrix = rotorA.WorldMatrix;
            lastRotorEMatrix = MainElRotor.WorldMatrix;
        }
        public void Update(float az, float el, bool stab = true) {
            yawDeltaInput = 0;
            pitchDeltaInput = 0;
            firstRunAim = false;
            float azError = 0;
            float elError = 0;
            if (stab) {
                azError = (float)MyMath.CalculateRotorDeviationAngle(rotorA.WorldMatrix.Forward, lastRotorAMatrix);
                elError = (float)MyMath.CalculateRotorDeviationAngle(MainElRotor.WorldMatrix.Forward, lastRotorEMatrix);
            }
            MatrixD weaponMatrix = MyMath.CreateLookAtForwardDir(referenceGun.GetPosition(), turretFrontVec, rotorA.WorldMatrix.Up);
            turretMatrix = MyMath.CreateLookAtUpDir(rotorA.Top.WorldMatrix.Translation, turretFrontVec, rotorA.WorldMatrix.Up);
            double ownYaw, ownPitch;
            MyMath.CalculateYawVelocity(turretMatrix, lastTurretMatrix, out ownYaw);
            MyMath.CalculatePitchVelocity(weaponMatrix, lastWeaponMatrix, out ownPitch);
            double yawRotationInput = az;
            double pitchRotationInput = el;
            turretFrontVec = referenceGun.WorldMatrix.Forward;
            float elevation = (float)(elError + pitchRotationInput);
            float azimuth = (float)(azError + yawRotationInput);
            MainElRotor.TargetVelocityRad = MultiplierElevation * elevation * 60;
            rotorA.TargetVelocityRad = azimuth * 60;
            foreach (var rotor in rotorsE) {
                if (!rotor.Closed)
                    SetSupprotRotor(rotor, turretFrontVec, elevation);
            }
            double yawSpeed = 0, pitchSpeed = 0;
            if (fullDriveYaw) {
                fullDriveYaw = false;
                maneuvrabilityYaw = Math.Abs(lastSpeedYaw.GetValueOrDefault() - ownYaw);
            }
            if (maneuvrabilityYaw == null) {
                if (azimuth != 0) {
                    if (azimuth > 0) {
                        fullDriveYaw = true;
                        yawSpeed = ownYaw + TURNGYROCONST;
                    } else
                        yawSpeed = ownYaw - TURNGYROCONST;
                    fullDriveYaw = true;
                }
            } else {
                if (Math.Abs(ownYaw - azimuth) > maneuvrabilityYaw) {
                    if (ownYaw - azimuth > 0)
                        yawSpeed = ownYaw - TURNGYROCONST;
                    else
                        yawSpeed = ownYaw + TURNGYROCONST;
                    fullDriveYaw = true;
                } else {
                    yawSpeed = azimuth;
                }
            }
            if (fullDrivePitch) {
                fullDrivePitch = false;
                maneuvrabilityPitch = Math.Abs(lastSpeedPitch.GetValueOrDefault() - ownPitch);
            }
            if (maneuvrabilityPitch == null) {
                if (elevation != 0) {
                    if (elevation > 0) {
                        pitchSpeed = ownPitch + TURNGYROCONST;
                    } else
                        pitchSpeed = ownPitch - TURNGYROCONST;
                    fullDrivePitch = true;
                }
            } else {
                if (Math.Abs(ownPitch - elevation) > maneuvrabilityPitch) {
                    if (ownPitch - elevation > 0)
                        pitchSpeed = ownPitch - TURNGYROCONST;
                    else
                        pitchSpeed = ownPitch + TURNGYROCONST;
                    fullDrivePitch = true;
                } else {
                    pitchSpeed = elevation;
                }
            }
            HullGuidance.ApplyGyroOverride(0, yawSpeed * 60, 0, _turretGyros, turretMatrix);
            HullGuidance.ApplyGyroOverride(-pitchSpeed * 60, yawSpeed * 60, 0, _weaponGyros, turretMatrix);
            lastSpeedYaw = ownYaw;
            lastSpeedPitch = ownPitch;
            lastTurretMatrix = turretMatrix;
            lastWeaponMatrix = weaponMatrix;
            lastRotorAMatrix = rotorA.WorldMatrix;
            lastRotorEMatrix = MainElRotor.WorldMatrix;
        }
        public void Status(ref string statusInfo, string language, string azimuthTag, string elevationTag) {
            statusInfo += $"\n{Languages.Translate(language, "ROTOR")} \"{azimuthTag}\": {rotorA.CustomName}\n " + 
                $"{Languages.Translate(language, "MAINEROTOR")} \"{elevationTag}\": {MainElRotor.CustomName}\n" +
                $"{Languages.Translate(language, "ALLROTORS")}: {rotorsE.Count + 1}\n" + 
                $"{Languages.Translate(language, "ALLWEAPONS")}: {weapons.Count}\n";
        }
        bool SetSupprotRotor(IMyMotorStator rotor, Vector3D direction, float mainRotTurnSpeed) {
            float localMultiplierElevation = MultiplierElevation;
            if (rotor.WorldMatrix.Up.Dot(MainElRotor.WorldMatrix.Up) < 0) {
                localMultiplierElevation = -localMultiplierElevation;
            }
            Vector3D? frontVec = null;
            foreach (var gun in weapons)
                if (gun.CubeGrid == rotor.TopGrid) {
                    frontVec = gun.WorldMatrix.Forward;
                    break;
                }
            if (frontVec == null) {
                foreach (var camera in radarCameras)
                    if (camera.CubeGrid == rotor.TopGrid) {
                        frontVec = camera.WorldMatrix.Forward;
                        break;
                    }
            }
            if (frontVec == null)
                return false;
            Vector3D TargetVectorLoc = MyMath.VectorTransform(direction, rotor.WorldMatrix.GetOrientation());
            Vector3D GunVectorLoc = MyMath.VectorTransform(frontVec.GetValueOrDefault(), rotor.WorldMatrix.GetOrientation());
            double targetAngleLoc = Math.Atan2(-TargetVectorLoc.X, TargetVectorLoc.Z);
            double myAngleLoc = Math.Atan2(-GunVectorLoc.X, GunVectorLoc.Z);
            float Elevation = (float)(0.1 * (targetAngleLoc - myAngleLoc) + localMultiplierElevation * mainRotTurnSpeed);
            rotor.TargetVelocityRad = Elevation * 60;
            return true;
        }

        #endregion Methods
    }

    class TurretRadar {
        public List<IMyLargeTurretBase> _turrets = new List<IMyLargeTurretBase>();
        public List<IMyTurretControlBlock> _TCs = new List<IMyTurretControlBlock>();
        List<EnemyTargetedInfo> _enemyTargetedInfos = new List<EnemyTargetedInfo>();
        public long? _lastChangeTick = null;
        bool _change = false;
        int cycle = 0;

        public void UpdateBlocks(List<IMyLargeTurretBase> turrets, List<IMyTurretControlBlock> turretControlBlocks, bool change = true) {
            _change = change;
            _turrets = turrets;
            if (_change) {
                foreach (var t in _turrets) {
                    switch (cycle) {
                        case 0:
                            t.SetTargetingGroup("Weapons");
                            break;
                        case 1:
                            t.SetTargetingGroup("Propulsion");
                            break;
                        default:
                            t.SetTargetingGroup("PowerSystems");
                            break;
                    }
                }
                foreach (var t in _TCs) {
                    switch (cycle) {
                        case 0:
                            t.SetTargetingGroup("Weapons");
                            break;
                        case 1:
                            t.SetTargetingGroup("Propulsion");
                            break;
                        default:
                            t.SetTargetingGroup("PowerSystems");
                            break;
                    }
                }
            }
            _TCs = turretControlBlocks;
        }
        public void Update(long tick, bool b = true) {
            if (b) _lastChangeTick = null;
            foreach (var t in _turrets) {
                bool weHaveThisTarget = false;
                if (t.HasTarget && !t.GetTargetedEntity().IsEmpty()) {
                    MyDetectedEntityInfo NewTarget = t.GetTargetedEntity();
                    for (int i = 0; i < _enemyTargetedInfos.Count; i++) {
                        if (NewTarget.EntityId == _enemyTargetedInfos[i].EntityId) {
                            _enemyTargetedInfos[i].UpdateTargetInfo(tick, NewTarget);
                            weHaveThisTarget = true;
                        }
                    }
                    if (!weHaveThisTarget) {
                        EnemyTargetedInfo target = new EnemyTargetedInfo(tick, NewTarget);
                        _enemyTargetedInfos.Add(target);
                    }
                }
            }
            foreach (var t in _TCs) {
                bool weHaveThisTarget = false;
                if (t.HasTarget && !t.GetTargetedEntity().IsEmpty()) {
                    MyDetectedEntityInfo NewTarget = t.GetTargetedEntity();
                    for (int i = 0; i < _enemyTargetedInfos.Count; i++) {
                        if (NewTarget.EntityId == _enemyTargetedInfos[i].EntityId) {
                            _enemyTargetedInfos[i].UpdateTargetInfo(tick, NewTarget);
                            weHaveThisTarget = true;
                        }
                    }
                    if (!weHaveThisTarget) {
                        EnemyTargetedInfo target = new EnemyTargetedInfo(tick, NewTarget);
                        _enemyTargetedInfos.Add(target);
                    }
                }
            }
            DeleteOldTargets(tick);
        }
        public EnemyTargetedInfo Update(ref string _debuginfo, long tick, EnemyTargetedInfo target) {
            Update(tick, false);
            if (_change) {
                foreach (var t in _turrets) {
                    if (t.HasTarget && !t.GetTargetedEntity().IsEmpty()) {
                        MyDetectedEntityInfo NewSubsystem = t.GetTargetedEntity();
                        if (target.EntityId == NewSubsystem.EntityId) {
                            _debuginfo += target.AddSubsystem(NewSubsystem, t.GetTargetingGroup()) + "\n";
                        }
                    }
                }
                foreach (var t in _TCs) {
                    if (t.HasTarget && !t.GetTargetedEntity().IsEmpty()) {
                        MyDetectedEntityInfo NewSubsystem = t.GetTargetedEntity();
                        if (target.EntityId == NewSubsystem.EntityId) {
                            _debuginfo += target.AddSubsystem(NewSubsystem, t.GetTargetingGroup()) + "\n";
                        }
                    }
                }
                if (_lastChangeTick == null) {
                    _lastChangeTick = tick;
                    foreach (var t in _turrets) {
                        switch (cycle) {
                            case 0:
                                t.SetTargetingGroup("Weapons");
                                break;
                            case 1:
                                t.SetTargetingGroup("Propulsion");
                                break;
                            default:
                                t.SetTargetingGroup("PowerSystems");
                                break;
                        }
                    }
                    foreach (var t in _TCs) {
                        switch (cycle) {
                            case 0:
                                t.SetTargetingGroup("Weapons");
                                break;
                            case 1:
                                t.SetTargetingGroup("Propulsion");
                                break;
                            default:
                                t.SetTargetingGroup("PowerSystems");
                                break;
                        }
                    }
                } else {
                    if ((_lastChangeTick - tick) % 120 == 0) {
                        switch (cycle) {
                            case 2:
                                cycle = 0;
                                break;
                            default:
                                cycle++;
                                break;
                        }
                        if (_change) {
                            foreach (var t in _turrets) {
                                switch (cycle) {
                                    case 0:
                                        t.SetTargetingGroup("Weapons");
                                        break;
                                    case 1:
                                        t.SetTargetingGroup("Propulsion");
                                        break;
                                    default:
                                        t.SetTargetingGroup("PowerSystems");
                                        break;
                                }
                            }
                            foreach (var t in _TCs) {
                                switch (cycle) {
                                    case 0:
                                        t.SetTargetingGroup("Weapons");
                                        break;
                                    case 1:
                                        t.SetTargetingGroup("Propulsion");
                                        break;
                                    default:
                                        t.SetTargetingGroup("PowerSystems");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return target;
        }
        void DeleteOldTargets(long tick) { 
            foreach (var t in _enemyTargetedInfos) 
                if (tick - t.LastLockTick > 300) {
                    _enemyTargetedInfos.Remove(t);
                    return; 
                }
        }
        public List<EnemyTargetedInfo> GetTargets() {
            return _enemyTargetedInfos;
        }

        //======-SCRIPT ENDING-======
    }

}