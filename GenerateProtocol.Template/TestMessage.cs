using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerateProtocol.Core;

namespace GenerateProtocol.Template
{
    public class TestMessage : BaseMessage
    {
        [AnnotateAttribute("int64")]
        public Int64 ssss;

        [AnnotateAttribute("int32")]
        public Int32 b;
        public Int16 c;
        public Byte d;

        public UInt64 e;
        public UInt32 f;
        public UInt16 g;
        public SByte h;

        public string str;

        //循环结构，type是任意类型，包括结构体
        public List<TestMessage2> lt;

        //其他，结构体
        //public SubTestMessage other;

        //其他，消息
        public TestMessage1 other1;
    }
}
