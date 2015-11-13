using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateProtocol.Template
{
    public enum MessageSection
    {
	    MT_HEAD = 0,					// 空消息头
	    MT_LINK = 1,					// 连接
	    MT_UNLINK = 2,					// 断连	  6
	    MT_DEFTRANS = 3,				// 缺省的包格式 default transmit packet
	    MT_HEARTBEAT = 4,				// 心跳监测消息
	    MT_PUBLICKEY = 5,				// 验证公钥

	    GATE_MESSAGE_BEGIN = 100000,	//gate服务器消息起始
	    GAME_MESSAGE_BEGIN = 200000,	//游戏服务器消息起始
    };

    public enum GameSection
    {

    }

    //门消息号
    public enum GateMessageId
    {
	    GATE_VALID_SUCCESS		= 1 + MessageSection.GATE_MESSAGE_BEGIN,	//校验成功消息	gate->server
        GATE_CONNECT_INIT = 2 + MessageSection.GATE_MESSAGE_BEGIN,	//发送校验信息	server->gate
        GATE_CONNECT_INIT_RET = 3 + MessageSection.GATE_MESSAGE_BEGIN,	//返回处理		gate->server
        TestMessage = 4 + MessageSection.GATE_MESSAGE_BEGIN,	//返回处理		gate->server
    };

    public enum GameMessageId
    {
        GATE_VALID_SUCCESS = 1 + MessageSection.GATE_MESSAGE_BEGIN,	//校验成功消息	gate->server
        GATE_CONNECT_INIT = 2 + MessageSection.GATE_MESSAGE_BEGIN,	//发送校验信息	server->gate
        GATE_CONNECT_INIT_RET = 2 + MessageSection.GATE_MESSAGE_BEGIN,	//返回处理		gate->server

    };  


}
