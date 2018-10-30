using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ArcGisExportApi.Services;
using ArcGisExportApi.Models;

namespace ArcGisExportApi.Utilities
{
    public class QueryUtils
    {
        // TODO - delete:
        private static QueryRepo queryService = new QueryRepo();

        async public static Task<QueryResult> queryAll(string uri, List<string> mapPlanIds)
        {
            uri = createQuery(uri, mapPlanIds);
            Trace.WriteLine("query all: " + uri);

            QueryResult result = await queryService.getQuery(uri);
            return result;
        }

        public static string createQuery(string uri, List<string> mapPlanIds)
        {
            uri += "/query?f=json";

            // add this fields: *
            uri += "&outFields=" + encodeUrl("objectid, name, KARTA_SIFRA");
            uri += "&where=";

            string query = "";
            for (int i = 0; i < mapPlanIds.Count; i++)
            {
                query += "KARTA_SIFRA=" + "'" + mapPlanIds[i] + "'";
                if (i < mapPlanIds.Count - 1)
                    query += " OR ";
            }
            // change url signs:
            uri += encodeUrl(query);

            return uri;
        }

        // replace special characters from url:
        public static string encodeUrl(string str)
        {
            str = str.Replace(" ", "+");
            str = str.Replace("=", "%3D");
            // ouput fields:
            str = str.Replace(",", "%2C");

            str = str.Replace("\"", "%22");
            str = str.Replace("'", "%27");
            str = str.Replace(":", "%3A");
            return str;
        }
    }
}
