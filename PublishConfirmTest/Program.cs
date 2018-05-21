using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishConfirmTest
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
                    // 事物机制
                    // channel.TxSelect
                    // channel.TxCommit;
                    // channel.TxRollback

                    //需要发送的消息
                    var body = Encoding.UTF8.GetBytes("test");
                    var properties = channel.CreateBasicProperties();

                    //设置消息持久化
                    properties.DeliveryMode = 2;

                    // Confirm模式
                    channel.ConfirmSelect();
                    channel.BasicPublish(exchange: "e.log",
                                       routingKey: "log.error",
                                       basicProperties: properties,
                                       body: body);
                    channel.WaitForConfirmsOrDie();
                }
            }
        }
    }
}
