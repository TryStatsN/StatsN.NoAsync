## StatsN.NoAsync



This is a completely sync version of the core [StatsN Client](https://github.com/TryStatsN/StatsN.NoAsync). This was created for people whom wanted to use .net 4.0 but cannot use Microsoft.Bcl.Async package. Both projects inherit from a [common contracts package](https://github.com/TryStatsN/StatsN.Contracts). This may be extended to .net 3.5 in the future.

## Getting started

```
Install-Package StatsN.NoAsync
```

In short the api is easy. You can get a new IStatsd with a few different ways, and then you can log metrics with an IStatsd implementation. Here are some examples.

Note, You will want to store your IStatsd as a singleton (most likely inside a DI container). This type persists a tcp or udp connection. The client's functions are thread safe.
```csharp
IStatsdSync statsd = Statsd.New<Udp>(a=>a.HostOrIp = "10.22.2.1", Port = 8125, Prefix = "MyMicroserviceName");
IStatsdSync statsd = Statsd.New<Tcp>(a=>a.HostOrIp = "10.22.2.1"); //use tcp
IStatsdSync statsd = Statsd.New<NullChannel>(a=>a.HostOrIp = "10.22.2.1", Port = 8125); //pipes your metrics to nowhere...which can scale infinately btw
IStatsdSync statsd = Statsd.New(a=>a.HostOrIp = "10.22.2.1"); //defaults to udp
IStatsdSync statsd = Statsd.New(new StatsdOptions(){ HostOrIp = "127.0.0.1"}); //defaults to udp
IStatsdSync statsd = new Stastd(new StatsdOptions(){ HostOrIp = "127.0.0.1"});  //defaults to udp
IStatsdSync statsd = new Stastd(new StatsdOptions(){ HostOrIp = "127.0.0.1"}, new Tcp()); //pass a new udp client. You could in theory make your own transport if you inherit from BaseCommunicationProvider


statsd.Count("myapp.counterstat"); //default to 1 aka increment
statsd.Count("myapp.counterstat", 6);
statsd.Count("myapp.counterstat", -6);
statsd.Count("myapp.timeMyFunction", ()=>{
 //code to instrument
});
statsd.Count("myapp.timeData", 400); //400ms
statsd.Count("autotest.gaugeyo", 422);
statsd.Count("autotest.gaugeyo", -10);
statsd.Count("autotest.setyo", 888);

```

## Logging

Like most statsd clients, this client **avoids throwing exceptions at all costs**. Any errors/exceptions created will be logged as a Systems.Diagnostics.Trace messages.

You can pass lambda into the `StatsdOptions` class to be passed exceptions and log messages, instead of getting them through the Trace system.


```csharp

            var opt = new StatsdOptions
            {
                OnExceptionGenerated = (exception) => { /* handle exception */ },
				OnLogEventGenerated = (log) => { /* handle log msg */ }
            };
			var stats = Statsd.New(opt);

```

or

```csharp

var stats = Statsd.New(a=>a.OnExceptionGenerated = (exception) => { /* handle exception */ });
```

## Buffering metrics

By setting the `BufferMetrics` property in the options object to true, the metrics will be buffered thus sending less packets. The Buffer size defaults to 512, which is [documented by statsd](https://github.com/etsy/statsd/blob/master/docs/metric_types.md#multi-metric-packets). You may change its size using the BufferSize property of `StastdOptions`. This uses a Concurrent Queue to Queue up the metrics and a `BackgroundWorker` to peal metrics off the Queue and send them along aggregated.

```csharp

var opt = new StatsdOptions(){

    BufferMetrics = true,
    BufferSize = 512
};

```