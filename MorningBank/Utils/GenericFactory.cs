using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MorningBank.Utils
{
    public class GenericFactory<T,I> where T:I, new()
    {
        GenericFactory() { } //private constructor
        public static I GetInstance(params object[] args)
        {
            return (I)Activator.CreateInstance(typeof(T), args);
        }
    }
}