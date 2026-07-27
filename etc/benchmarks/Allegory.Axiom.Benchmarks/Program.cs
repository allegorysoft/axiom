using Allegory.Axiom.Benchmarks.Integration.RabbitMQ;
using Allegory.Axiom.Benchmarks.Modularity.DependencyInjection;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<RabbitMqConnectionFactoryBenchmark>();