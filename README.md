GeoNorgeAPI
===========

C# API for communicating with GeoNorge.no through the CSW Service


Getting Started
===============

To install GeoNorgeAPI, run the following command in the Package Manager Console:

    PM> Install-Package GeoNorgeAPI
    

Example code
============
    
    using www.opengis.net;
    using GeoNorgeAPI;
    
    public class MyClass {
        public void MyMethod() {
            GeoNorgeAPI.GeoNorge api = new GeoNorgeAPI.GeoNorge();
            SearchResultsType result = api.Search("flomsone");
            Console.WriteLine(result.numberOfRecordsMatched + " metadata found");
        }
    }


Security notice
===============

GeoNorge.no is currently only running on HTTP. This means that your username &amp; password is sent over the wire in _plain text_!

We will fix this as soon as possible.
