using Irbis.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irbis.AppData
{
    internal class Connect
    {
        public static Entities1 c;
        public static Entities1 context
        {
            get
            {
                if (c == null)
                    c = new Entities1();
                return c;

            }
        }
    }
}
