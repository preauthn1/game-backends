using System;

namespace FW.Core.Helper
{
    public interface ICloneable<TObject>
    {
        TObject Clone();
    }
}
