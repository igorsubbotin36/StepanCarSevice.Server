using StepanCarSevice.VisitService.API;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.VisitService.Tests;

public class VisitServiceFactory : ServiceFactory<Program, VisitDBContext>
{
    protected override string DatabasePrefix => "visit";
}

// БД Visit без HTTP; контекст создаётся в нужном тенанте (FakeTenantAccessor)
public class VisitDatabase : ServiceDatabase<VisitDBContext>
{
    protected override string Prefix => "visit";

    public override VisitDBContext CreateContext(FakeTenantAccessor tenantAccessor) => new(CreateOptions(), tenantAccessor);
}
