using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoNorgeAPI
{
    public class MetadataTransaction
    {
        public string TotalInserted { get; set; }
        public string TotalUpdated { get; set; }
        public string TotalDeleted { get; set; }

        public List<string> Identifiers { get; set; }
    }
}
