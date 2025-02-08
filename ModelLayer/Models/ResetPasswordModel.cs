using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Models
{
    public class ResetPasswordModel
    {

        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
