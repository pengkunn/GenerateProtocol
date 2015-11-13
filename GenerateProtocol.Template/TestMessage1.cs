using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerateProtocol.Core;

namespace GenerateProtocol.Template
{
    public class TestMessage1 : BaseMessage
    {
        [AnnotateAttribute("int64")]
        public Int64 ssss;

        public Int32 b;
        public Int16 c;
        public Byte d;

        //public TestMessage2 t2;
    }
}
