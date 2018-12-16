using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Translit.Entity
{
    public class DatabaseInfo
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public int NumberOfSymbols { get; set; }
        public int NumberOfExceptions { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
