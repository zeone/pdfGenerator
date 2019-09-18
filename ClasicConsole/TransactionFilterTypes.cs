using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole
{
    public enum TransFilterType
    {
        Payment = 1,
        Bill = 2,
        Family = 3,
        Member = 4
    }

    public enum TransFilterView
    {
        Details = 0,
        Total = 1
    }
}
