using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static ArcGisExportApi.Models.UrbanisticPlansResults;
using ArcGisExportApi.Tests;
using ArcGisExportApi.Services;

namespace ArcGisExportApi.TestUtils
{
    public class QueryUtils
    {
        // TODO - delete:
        private static QueryService queryService = new QueryService();

        async public static Task<QueryResult> queryAll(string uri, List<PlanMap> plans)
        {
            uri = createQuery(uri, plans);
            Trace.WriteLine("query all: " + uri);

            QueryResult result = await queryService.getQuery(uri);
            return result;
        }

        public static string createQuery(string uri, List<PlanMap> plans)
        {
            uri += "/query?f=json";
            uri += "&where=";

            string query = "";
            for (int i = 0; i < plans.Count; i++)
            {
                query += "objectid=" + plans[i].Id;
                if (i < plans.Count - 1)
                    query += " OR ";
            }

            // change url signs:
            uri += encodeUrl(query);

            return uri;
        }

        // replace special characters from url:
        private static string encodeUrl(string str)
        {
            str = str.Replace(" ", "+");
            str = str.Replace("=", "%3D");

            str = str.Replace("\"", "%22");
            str = str.Replace("'", "%27");
            str = str.Replace(":", "%3A");
            return str;
        }

    }
}
