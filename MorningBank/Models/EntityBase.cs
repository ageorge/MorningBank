using MorningBank.DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MorningBank.Models
{
    [Serializable]
    public class EntityBase : IEntity
    {
        public void SetFields(DataRow dr)
        {
            Type tp = this.GetType();
            foreach (PropertyInfo pi in tp.GetProperties())
            {
                if (null != pi && pi.CanWrite)
                {
                    string nm = pi.PropertyType.Name.ToUpper();
                    string nmfull = pi.PropertyType.FullName.ToUpper();
                    if (nm.IndexOf("ENTITY") >= 0)
                    {
                        break;
                    }
                    if (nmfull.IndexOf("SYSTEM") < 0)
                    {
                        break;
                    }
                    if (pi.PropertyType.Name.ToUpper() != "BINARY")
                    {
                        Object value = dr[pi.Name];
                        if (value == DBNull.Value)
                            value = null;
                        pi.SetValue(this, value, null);
                    }
                }
            }
        }
    }
}