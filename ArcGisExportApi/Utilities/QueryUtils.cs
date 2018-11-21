using System.Threading.Tasks;
using PGZ.UI.PrintService.Services;
using PGZ.UI.PrintService.Models;

namespace PGZ.UI.PrintService.Utilities
{
    public class QueryUtils
    {
        async public static Task<QueryResult> queryAll(UrbanPlan urbanPlan, string layerUrl)
        {
            string uri = createQuery(layerUrl, urbanPlan);

            QueryResult result = await QueryRepo.getQuery(uri);
            return result;
        }

        public static string createQuery(string uri, UrbanPlan urbanPlan)
        {
            uri += "/query?f=json";

            // add this fields: *
            uri += "&outFields=" + encodeUrl("objectid,name," + urbanPlan.RasterIdAttribute);
            uri += "&where=";

            string query = "";
            for (int i = 0; i < urbanPlan.Maps.Count; i++)
            {
                query += urbanPlan.RasterIdAttribute + "=" + "'" + urbanPlan.Maps[i] + "'";
                if (i < urbanPlan.Maps.Count - 1)
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
