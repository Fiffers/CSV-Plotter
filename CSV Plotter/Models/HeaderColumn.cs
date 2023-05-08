using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV_Plotter.Models
{
    public class HeaderColumn
    {
        private string _header = string.Empty;
        public string Header
        {
            get { return _header;}
            set { _header = value; }
        }

        private string[] _column = Array.Empty<string>();
        public string[] Column
        {
            get { return _column; }
            set { _column = value; }
        }

        private bool _hasBeenCasted = false;
        public bool HasBeenCasted
        {
            get { return _hasBeenCasted; }
            set { _hasBeenCasted = value; }
        }
    }
}
