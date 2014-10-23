using System.Diagnostics;
using System.IO;
using Arkitektum.GIS.Lib.SerializeUtil;
using NUnit.Framework;
using www.opengis.net;

namespace GeoNorgeAPI.Tests
{
    public class SerializeTest
    {
        [Test]
        public void ShouldSerializeWithDateTimeError()
        {
            string xml = File.ReadAllText("xml/datetime-error.xml");
            xml = xml.Replace(@"<gco:DateTime xmlns:gco=""http://www.isotc211.org/2005/gco"" />","");
            var getRecordsResponseType = SerializeUtil.DeserializeFromString<GetRecordsResponseType>(xml);
            Trace.WriteLine(SerializeUtil.SerializeToString(getRecordsResponseType));
        }
    }
}
