using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Web;

namespace Scoop
{
    class MozscapeAPI
    {
        /// <summary>
        /// Wrapper for the Mozscape API
        /// </summary>

        public MozscapeAPI()
        {

        }

        /// <summary>
        /// Generate a valid signature string from a message(e.g. "?AccessID=member-cf180f7081&Expires=1225138899") and your Mozscape secret
        /// key (e.g. "045hasfdj339010103")
        /// </summary>
        /// <param name="intExpiryHours"></param>
        public string CreateEncryptedSig(string message, string keyString)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            // Convert our message and keystring to bytes
            byte[] keyByte = encoding.GetBytes(keyString);
            byte[] messageBytes = encoding.GetBytes(message);

            // Compute the hash
            HMACSHA1 algorithm = new HMACSHA1(keyByte);
            byte[] hash = algorithm.ComputeHash(messageBytes);

            // Base64 encode the hash
            string strSignature = Convert.ToBase64String(hash);

            // Return a url encoded version of the encoded hash string
            return HttpUtility.UrlEncode(strSignature);
        }

        /// <summary>
        /// Generate a valid unix timestamp string representing the current time + intExpiryHours
        /// </summary>
        /// <param name="intExpiryHours"></param>
        public string CreateExpiryTimestamp(int intExpiryHours)
        {

            // Get timespan object that can be used to generate timestamp
            TimeSpan tsDuration = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

            // Add intExpiryHours to the duration
            TimeSpan tsEnd = TimeSpan.FromHours(intExpiryHours);
            tsDuration = tsDuration.Add(tsEnd);

            // Generate our unix timestamp
            double dblUnixTime = tsDuration.TotalSeconds;

            // Remove the decimal parts, we don't need them
            int intUnixTime = (int)dblUnixTime;

            // Convert to string for the api request
            string strExpiry = intUnixTime.ToString();

            return strExpiry;
        }


        /// <summary>
        /// Builds the Mozscape API URL
        /// </summary>
        /// <param name="strAccessID"></param>
        /// <param name="strPrivateKey"></param>
        /// <param name="intExpiryHours"></param>
        /// <param name="strType"></param>
        /// <param name="strURL"></param>
        public string CreateAPIURL(string strAccessID, string strPrivateKey, int intExpiryHours, string strType, string strURL, string strOptions)
        {
            // Build the API URL string using api credentials, expiry timestamp, signed signature, URL and the type of data we want

            string strAPIUrl = "";

            // Get the unix timestamp
            string strExpiry = CreateExpiryTimestamp(intExpiryHours);

            // Genereate the message string that we'll encrypt using the private key0
            string strMessage = strAccessID + "\n" + strExpiry;

            // Create hashed signature using the message string and private key
            string strSignature = CreateEncryptedSig(strMessage, strPrivateKey);

            switch (strType)
            {
                case "url metrics":
                    strAPIUrl = "http://lsapi.seomoz.com/linkscape/url-metrics/" + strURL + "?AccessID=" + strAccessID + "&Expires=" + strExpiry + "&Signature=" + strSignature;
                    break;
                case "links":
                    if (strOptions != "")
                    {
                        strAPIUrl = "http://lsapi.seomoz.com/linkscape/links/" + strURL + "?AccessID=" + strAccessID + "&Expires=" + strExpiry + "&Signature=" + strSignature + strOptions;
                    }
                    else
                    {
                        strAPIUrl = "http://lsapi.seomoz.com/linkscape/links/" + strURL + "?AccessID=" + strAccessID + "&Expires=" + strExpiry + "&Signature=" + strSignature;
                    }
                    break;
            }

            return strAPIUrl;
        }



        /// <summary>
        /// Fetches Mozscape API results, returns a string
        /// </summary>
        /// <param name="strAPIURL"></param>
        public string FetchResults(string strAPIURL)
        {
            string strResults = "";
            // Used to build output
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];

            // Create web request using the Mozscape API URL
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(strAPIURL);

            // Execute the request
            try
            {
                // Get the response from the API
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();

                // Read data via the response stream
                System.IO.Stream resStream = response.GetResponseStream();

                string tempString = null;
                int count = 0;

                // Read the stream into a buffer and build our results string 
                do
                {
                    // Fill the buffer with data
                    count = resStream.Read(buf, 0, buf.Length);

                    // Make sure we read some data
                    if (count != 0)
                    {
                        // Translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buf, 0, count);

                        // Continue building the string
                        sb.Append(tempString);
                    }
                }
                while (count > 0); // Any more data to read?
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(error.Message);
            }

            strResults = sb.ToString();
            return strResults;
        }

        /// <summary>
        /// Parses a mozscape URL metrics API results string and returns a MozscapeURLMetric object
        /// </summary>
        /// <param name="strResults"></param>
        public MozscapeURLMetric ParseURLMetrics(string strResults)
        {
            JavaScriptSerializer jSerializer = new JavaScriptSerializer();
            MozscapeURLMetric msURLMetrics = jSerializer.Deserialize<MozscapeURLMetric>(strResults);
            return msURLMetrics;
        }

        /// <summary>
        /// Parses a mozscape links API results string and returns a list containing MozscapeLinkMetric objects
        /// </summary>
        /// <param name="strResults"></param>
        public List<MozscapeLinkMetric> ParseLinkMetrics(string strResults)
        {
            JavaScriptSerializer jSerializer = new JavaScriptSerializer();
            List<MozscapeLinkMetric> msLinkMetrics = jSerializer.Deserialize<List<MozscapeLinkMetric>>(strResults);
            return msLinkMetrics;
        }
    }
}
