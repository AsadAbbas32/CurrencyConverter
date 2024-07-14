using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Models.Common
{
    public class RequestResponseDto<T>
    {
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }
    public class PaginatedApiResponseDto<T> : RequestResponseDto<T>
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }
}
