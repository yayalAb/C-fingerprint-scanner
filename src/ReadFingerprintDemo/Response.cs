using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFingerprintDemo
{
   public class Response

    {
        public int statusCode { get; set; } = 404;
        public bool success { get; set; } = false;

        public string message { get; set; } = "";
        public string FpImage { get; set; }

    }
}
