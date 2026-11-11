using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
