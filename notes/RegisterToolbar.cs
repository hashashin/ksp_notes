using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using KSP.IO;
using UnityEngine;
using File = System.IO.File;

using KSP.UI.Screens;
using ToolbarControl_NS;

namespace notes
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(Notes.MODID, Notes.MODNAME);
        }
    }
}