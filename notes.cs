// -------------------------------------------------------------------------------------------------
// notes.cs 0.8.2
//
// Simple KSP plugin to take notes ingame.
// Copyright (C) 2014 Iván Atienza
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// 
// Email: mecagoenbush at gmail dot com
// Freenode: hashashin
//
// -------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace notes
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class notes : MonoBehaviour
    {
        private Vector2 _scrollViewVector = Vector2.zero;
        private Vector2 _scrollViewVector2 = Vector2.zero;
        private Rect _windowRect;
        private Rect _windowRect2;

        private static string _configfile = "notes.dat";
        private static string _notesdir = "notes/Plugins/PluginData/";
        private static string _file = KSP.IO.File.ReadAllText<notes>(_notesdir + _configfile);
        private string _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file + ".txt");

        private string _keybind;
        private int _fontsize;

        private bool _popup = false;
        private bool _visible = false;
        private bool _toggledel = false;
        private bool _toggleinput = false;

        private List<string> _filenames;
        private int _selectiongridint = 0;
        private static string _notes = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/" + _notesdir + "notes/";

        private IButton _button;
        private string _tooltipon = "Hide Notepad";
        private string _tooltipoff = "Show Notepad";
        private string _btexture_on = "notes/Textures/icon_on";
        private string _btexture_off = "notes/Textures/icon_off";

        private static string _showbuttondeltext = "Show del button";
        private static string _hidebuttondeltext = "Hide del button";
        private string _currentdeltext;

        private static string _reloadiconurl = "file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") + "/GameData/notes/Textures/reload.png";
        private WWW _reloadicontex = new WWW(_reloadiconurl);

        private string _version;
        private string _versionlastrun;

        private string _vesselinfo;
        private string _vesselname;
        private const ControlTypes BLOCK_ALL_CONTROLS = ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.ACTIONS_ALL | ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP | ControlTypes.MISC | ControlTypes.GROUPS_ALL | ControlTypes.CUSTOM_ACTION_GROUPS;


        void Awake()
        {
            LoadVersion();
            VersionCheck();
            LoadSettings();
            CheckDefaults();
        }

        void Start()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                _button = ToolbarManager.Instance.add("notes", "toggle");
                _button.TexturePath = _btexture_off;
                _button.ToolTip = _tooltipoff;
                _button.OnClick += ((e) =>
                {
                    Toggle();
                });
            }
        }

        void OnGUI()
        {
            if (_visible)
            {
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect, notesWindow, "Notepad");
            }
            if (_popup)
            {
                _windowRect2 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect2, listWindow, "Notes list");
                UpdateDelButtonText();
            }
        }

        private void notesWindow(int windowID)
        {
            GUI.SetNextControlName("notes");
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 15f, 420f, 380f), _scrollViewVector, new Rect(0f, 0f, 400f, 5300f));
            GUIStyle myStyle = new GUIStyle(GUI.skin.textArea);
            myStyle.fontSize = _fontsize;
            _text = GUI.TextArea(new Rect(3f, 0f, 400f, 5300f), _text, myStyle);
            GUI.EndScrollView();

            _file = GUI.TextField(new Rect(5f, 400f, 150f, 20f), _file);

            if (GUI.Button(new Rect(155f, 400f, 80f, 30f), "Load"))
            {
                Load();
            }
            if (GUI.Button(new Rect(235f, 400f, 80f, 30f), "Save"))
            {
                Save();
            }
            if (GUI.Button(new Rect(315f, 400f, 80f, 30f), "List Notes"))
            {
                if (_filenames == null)
                {
                    GetNotes();
                    _popup = true;
                }
                else
                {
                    _filenames = null;
                    _popup = false;
                }
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            if (HighLogic.LoadedSceneIsFlight && HighLogic.LoadedSceneHasPlanetarium)
            {
                _vesselname = FlightGlobals.ActiveVessel.GetName();
                if (GUI.Button(new Rect(5f, 422f, 100f, 20f), "Open ship log"))
                {
                    OpenLog();
                }
                if ("log_" + _vesselname == _file)
                {
                    if (GUI.Button(new Rect(5f, 442f, 100f, 20f), "New log entry"))
                    {
                        GetLogInfo();
                        _text = _text + _vesselinfo;
                    }
                }
            }
            if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                if (GUI.Toggle(new Rect(200f, 432f, 150f, 20f), _toggleinput, "Toggle input lock") != _toggleinput)
                {
                    _toggleinput = !_toggleinput;
                    if (_toggleinput)
                    {
                        InputLockManager.SetControlLock(BLOCK_ALL_CONTROLS, "notes");
                        //ScreenMessages.PostScreenMessage("LOCKED", 3f, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else
                    {
                        InputLockManager.RemoveControlLock("notes");
                        //ScreenMessages.PostScreenMessage("UNLOCKED", 3f, ScreenMessageStyle.UPPER_CENTER);

                    }
                }

            }
            GUI.DragWindow();
        }

        private void listWindow(int windowID)
        {
            _scrollViewVector2 = GUI.BeginScrollView(new Rect(3f, 15f, 295f, 300f), _scrollViewVector2, new Rect(0f, 0f, 0f, 4360f));
            _selectiongridint = GUILayout.SelectionGrid(_selectiongridint, _filenames.ToArray(), 1);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(5f, 320f, 100f, 30f), "Select & Load"))
            {
                _file = _filenames[_selectiongridint];
                Load();
                _filenames = null;
                _popup = false;
            }
            GUI.DrawTexture(new Rect(115f, 320f, 30f, 30f), _reloadicontex.texture, ScaleMode.ScaleToFit, true, 0f);
            if (GUI.Button(new Rect(115f, 320f, 30f, 30f), ""))
            {
                _filenames = null;
                _popup = false;
                GetNotes();
                _popup = true;
            }
            if (_toggledel = GUI.Toggle(new Rect(75f, 350.5f, 115f, 20f), _toggledel, _currentdeltext))
            {
                GUI.contentColor = Color.red;
                if (GUI.Button(new Rect(155f, 320f, 100f, 30f), "Delete"))
                {
                    Delete();
                    _filenames = null;
                    _popup = false;
                    GetNotes();
                    _popup = true;
                }
                GUI.contentColor = Color.white;
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                if (_popup)
                {
                    _filenames = null;
                    _popup = false;
                }
            }
            GUI.DragWindow();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
            }
        }

        void OnDestroy()
        {
            Save();
            SaveSettings();
            if (_button != null)
            {
                _button.Destroy();
            }
        }

        private void Save()
        {
            KSP.IO.File.WriteAllText<notes>(_text, _notesdir + _file + ".txt");
            KSP.IO.File.WriteAllText<notes>(_file, _notesdir + _configfile);
            if ((HighLogic.LoadedScene != GameScenes.LOADING) && (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
            {
                ScreenMessages.PostScreenMessage("File saved: " + _file + ".txt", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void Load()
        {
            if (KSP.IO.File.Exists<notes>(_file + ".txt"))
            {
                _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file + ".txt");
            }
            else if ((HighLogic.LoadedScene != GameScenes.LOADING) && (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
            {
                ScreenMessages.PostScreenMessage("File dont exist: " + _file + ".txt", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void OpenLog()
        {
            if (KSP.IO.File.Exists<notes>("log_" + _vesselname + ".txt"))
            {
                GetLogInfo();
                _text = KSP.IO.File.ReadAllText<notes>(_notesdir + "log_" + _vesselname + ".txt");
                _file = "log_" + _vesselname;
            }
            else
            {
                GetLogInfo();
                ScreenMessages.PostScreenMessage("Log for " + _vesselname + " dont exist, creating new: " + "log_" + _vesselname + ".txt", 3f, ScreenMessageStyle.UPPER_CENTER);
                _file = "log_" + _vesselname;
                _text = _vesselinfo;
                Save();
            }
        }

        private void Delete()
        {
            KSP.IO.File.Delete<notes>(_filenames[_selectiongridint] + ".txt");
            if ((HighLogic.LoadedScene != GameScenes.LOADING) && (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
            {
                ScreenMessages.PostScreenMessage(_filenames[_selectiongridint] + ".txt DELETED!", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void LoadSettings()
        {
            KSPLog.print("[notes.dll] Loading Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();
            configfile.load();

            _windowRect = configfile.GetValue<Rect>("windowpos");
            _windowRect2 = configfile.GetValue<Rect>("listwindowpos");
            _keybind = configfile.GetValue<string>("keybind");
            _versionlastrun = configfile.GetValue<string>("version");
            _fontsize = configfile.GetValue<int>("fontsize");
            KSPLog.print("[notes.dll] Config Loaded Successfully");
        }

        private void SaveSettings()
        {
            KSPLog.print("[notes.dll] Saving Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();

            configfile.SetValue("windowpos", _windowRect);
            configfile.SetValue("listwindowpos", _windowRect2);
            configfile.SetValue("keybind", _keybind);
            configfile.SetValue("version", _version);
            configfile.SetValue("fontsize", _fontsize);

            configfile.save();
            KSPLog.print("[notes.dll] Config Saved ");
        }

        private void Toggle()
        {
            if (_visible == true)
            {
                _visible = false;
                _filenames = null;
                _popup = false;
                _button.TexturePath = _btexture_off;
                _button.ToolTip = _tooltipoff;
            }
            else
            {
                _visible = true;
                _button.TexturePath = _btexture_on;
                _button.ToolTip = _tooltipon;
            }
        }

        private void GetNotes()
        {
            this._filenames = new List<string>(Directory.GetFiles(_notes, "*.txt"));

            for (int i = 0; i < this._filenames.Count; i++)
            {
                this._filenames[i] = Path.GetFileNameWithoutExtension(this._filenames[i]);
            }
        }

        private void CheckDefaults()
        {
            if (_windowRect == new Rect(0, 0, 0, 0))
            {
                _windowRect = new Rect(50f, 25f, 425f, 467f);
            }
            if (_windowRect2 == new Rect(0, 0, 0, 0))
            {
                _windowRect2 = new Rect((Screen.width / 2) - 150f, (Screen.height / 2) - 75f, 260f, 370f);
            }
            if (_keybind == null)
            {
                _keybind = "n";
            }
            if (_fontsize == 0)
            {
                _fontsize = 13;
            }
        }

        private void VersionCheck()
        {
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            KSPLog.print("notes.dll version: " + _version);
            if ((_version != _versionlastrun) && (KSP.IO.File.Exists<notes>("config.xml")))
            {
                KSP.IO.File.Delete<notes>("config.xml");
            }
        }

        private void LoadVersion()
        {
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();
            configfile.load();
            _versionlastrun = configfile.GetValue<string>("version");
        }

        private void UpdateDelButtonText()
        {
            if (_toggledel)
            {
                _currentdeltext = _hidebuttondeltext;
            }
            else
            {
                _currentdeltext = _showbuttondeltext;
            }
        }

        private void GetLogInfo()
        {
            if (HighLogic.LoadedSceneIsFlight && HighLogic.LoadedSceneHasPlanetarium)
            {
                double _seconds = Planetarium.GetUniversalTime();
                _seconds = Math.Abs(_seconds);

                const int _minuteL = 60;
                const int _hourL = 60 * _minuteL;
                int _dayL = 24 * _hourL;
                int _yearL = 365 * _dayL;
                if (GameSettings.KERBIN_TIME)
                {
                    _dayL = 6 * _hourL;
                    _yearL = 426 * _dayL;
                }
                int _years = (int)Math.Floor(_seconds / _yearL);
                int _ryears = _years + 1;
                int _Tseconds = (int)Math.Floor(_seconds);
                _seconds = _Tseconds - _years * _yearL;
                int _days = (int)Math.Floor(_seconds / _dayL);
                int _rdays = _days + 1;
                _seconds -= _days * _dayL;
                int _hours = (int)Math.Floor(_seconds / _hourL);
                _seconds -= _hours * _hourL;
                int _minutes = (int)Math.Floor(_seconds / _minuteL);
                _seconds -= _minutes * _minuteL;

                string _separator = "--------------------------------------------------------------------------------------------------";
                string _metY = FlightLogger.met_years.ToString();
                string _metD = FlightLogger.met_days.ToString();
                string _metH = FlightLogger.met_hours.ToString();
                string _metM = FlightLogger.met_mins.ToString("00");
                string _metS = FlightLogger.met_secs.ToString("00");
                string _situation = Vessel.GetSituationString(FlightGlobals.ActiveVessel);
                _vesselinfo =
                    "\n" +
                    _separator + "\n" +
                    _vesselname + " --- Year: " + _ryears + " Day: " + _rdays + " Time: "
                    + _hours + ":" + _minutes.ToString("00") + ":" + _seconds.ToString("00") + "\n" +

                    "MET: " + _metY + "y, " + _metD + "d, " + _metH + ":" + _metM + ":" + _metS +
                    " --- Status: " + _situation + "\n" +
                    _separator + "\n";
            }
        }
    }
}
