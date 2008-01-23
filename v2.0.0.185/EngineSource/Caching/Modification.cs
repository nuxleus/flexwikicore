using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public abstract class Modification
    {
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false; 
            }

            return true; 
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
