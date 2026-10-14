using System;
using System.Collections.Generic;

namespace RoyalCapital_3.Data
{
    public interface ISP_CALL : IDisposable
    {
        List<T> ListByRawQuery<T>(string sql);

        List<T> ListByRawQueryBySis<T>(string sql);
    }
}
