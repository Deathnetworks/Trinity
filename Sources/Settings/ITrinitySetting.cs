using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Settings
{
    public interface ITrinitySetting<T> where T : ITrinitySetting<T>
    {
        void Reset();
        void CopyTo(T setting);
        T Clone();
    }
}
