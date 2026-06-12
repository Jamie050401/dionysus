Console.WriteLine("Hello World!");

var a = new Dionysus.Client.GrpcService<string>("http://127.0.0.1:0001", _ => "Dummy Client");
Console.WriteLine(a.Client);
