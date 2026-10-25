using ClassBook.IntegrationTests.Infrastructure;
using Xunit.Sdk;
using Xunit.v3;

// One SQL Server container for the whole test run; tests share it and reset it with Respawn, so they run sequentially.
[assembly: AssemblyFixture(typeof(ApiFixture))]
[assembly: Parallelization(Mode = ParallelMode.None)]
