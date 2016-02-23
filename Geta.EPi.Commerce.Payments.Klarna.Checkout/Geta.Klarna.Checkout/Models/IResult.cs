using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Klarna.Checkout.Models
{
    public interface IResult
    {
        bool IsSuccess { get; set; }
        string ErrorMessage { get; set; }
    }
}
