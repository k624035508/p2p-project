using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace Agp2p.Common
{
    /*
     * sample:
     * var query = from ....;
     * DataTable dt = query.ToDataTable(rec => new object[] { query }); 
     * 
    */

    public static class Linq2DataTableHandler
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> varlist, CreateRowDelegate<T> fn)
        {

            DataTable dtReturn = new DataTable();

            // column names

            PropertyInfo[] oProps = null;

            // Could add a check to verify that there is an element 0

            foreach (T rec in varlist)
            {

                // Use reflection to get property names, to create table, Only first time, others will follow

                if (oProps == null)
                {

                    oProps = ((Type)rec.GetType()).GetProperties();

                    foreach (PropertyInfo pi in oProps)
                    {

                        Type colType = pi.PropertyType; if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {

                            colType = colType.GetGenericArguments()[0];

                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));

                    }

                }

                DataRow dr = dtReturn.NewRow(); foreach (PropertyInfo pi in oProps)
                {

                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);

                }

                dtReturn.Rows.Add(dr);

            }

            return (dtReturn);

        }

        public delegate object[] CreateRowDelegate<T>(T t);
    }
}

