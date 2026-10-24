using System;
using System.Collections.Generic;
using example.invalid;

namespace target.Models.ViewModels
{

    public class RemoteFileDownloadModel
    {
        public Stream DownloadStream { get; set; }

        public string FileName { get; set; }

    }


}
