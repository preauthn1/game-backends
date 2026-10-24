using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using Newtonsoft.Json;

namespace game.AdminSystem
{
    public class CKEditorUploadResultViewModel
    {

        public bool uploaded { get; set; }

        public string url { get; set; }

    }
}