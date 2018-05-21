using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLetterTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "admin", VirtualHost = "/" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //声明一个direct类型的交换机，用来当做某个队列的死信交换机(DLX)
                    channel.ExchangeDeclare("e.log", "direct"); //交换机类型
                    channel.ExchangeDeclare("e.log.dead", "direct"); //交换机类型

                    //声明一个队列，用来存放死信消息
                    channel.QueueDeclare(queue: "q.log.dead",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    //声明一个队列，并指定该队列的DLX和死信路由key，且还需要设置TTL(消息存活时间)
                    channel.QueueDeclare(queue: "q.log.error",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: new Dictionary<string, object> {
                                         { "x-dead-letter-exchange","e.log.dead"}, //设置当前队列的DLX
                                         { "x-dead-letter-routing-key","dead"}, //设置DLX的路由key，DLX会根据该值去找到死信消息存放的队列
                                         { "x-message-ttl",10000} //设置消息的存活时间，即过期时间
                                         });

                    //将DLX和死信存放队列绑定，并产生一个路由key
                    channel.QueueBind("q.log.dead", "e.log.dead", "dead");

                    //绑定消息队列
                    channel.QueueBind("q.log.error", //队列名称
                                      "e.log",      //交换机名称
                                      "log.error");  //自定义的RoutingKey
                    //需要发送的消息
                    var body = Encoding.UTF8.GetBytes("test");

                    var properties = channel.CreateBasicProperties();

                    //设置消息持久化
                    properties.DeliveryMode = 2;

                    //发布消息
                    channel.BasicPublish(exchange: "e.log",
                                         routingKey: "log.error",
                                         basicProperties: properties,
                                         body: body);
                }
            }
        }
    }
}
