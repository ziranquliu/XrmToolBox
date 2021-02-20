using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace DMM365.Helper
{

    public class GuidEqualityComparer : IEqualityComparer<Guid>
    {
        public bool Equals(Guid x, Guid y)
        {
            return x == y;
        }

        public int GetHashCode(Guid id)
        {
            return id.GetHashCode();
        }
    }


    public class sdkEntityEqualityComparers : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Entity obj)
        {
            return 0;
        }
    }


}
