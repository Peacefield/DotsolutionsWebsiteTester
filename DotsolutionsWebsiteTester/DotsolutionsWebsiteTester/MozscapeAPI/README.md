MozscapeAPI
===========

Some classes and functions for using the Mozscape API in C#


Examples

Examples of getting and parsing the results. You can review the Mozscape documentation for the various response fields by reading the request-response formats page

  Get URL Metrics Data
  
  //instantiate a new mozscapeAPI object
  
    Mozscape mozAPI = new MozscapeAPI();  
  
  //build our API URL 
  
    string strAPIURL = mozAPI.CreateAPIURL(strAccessID, strPrivateKey, 1, "url metrics", "uk.queryclick.com",  ""); 
  
  //get the results string 
  
    string strResults = mozAPI.FetchResults(strAPIURL); 
  
  //parse the results string. The ParseURLMetrics function returns a MozscapeURLLinkMetrics objects 
  
    MozscapeURLMetric msURLMetrics = mozAPI.ParseURLMetrics(strResults);
  
  //access the object values
  
    string strExternalLinks = msURLMetrics.ueid
  
  Get Links Data
  
  //instantiate a new MozscapeAPI object
  
    Mozscape mozAPI = new MozscapeAPI(); 
  
  //build our API URL
  
    string strAPIURL = mozAPI.CreateAPIURL(strAccessID, strPrivateKey, 1, "links", "uk.queryclick.com",  "&Scope=page_to_domain");
  
  //get the results string
  
    string strResults = mozAPI.FetchResults(strAPIURL);
  
  //parse the results string. The ParseLinkMetrics function returns a list of MozscapeLinkMetrics objects
  
    List<MozscapeLinkMetric> listResults = mozAPI.ParseLinkMetrics(strResults);
  
  //get the link url
  
    string strLink = listResults[0].upl