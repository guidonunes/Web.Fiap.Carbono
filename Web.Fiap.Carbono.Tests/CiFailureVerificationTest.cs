namespace Web.Fiap.Carbono.Tests.Failure;


public sealed class CiFailureVerificationTest
{
    [Fact]
    public void ControlledFailureProducesDiagnostics()
    {
        throw new InvalidOperationException(
            "CI-FAILURE-VERIFICATION: expected temporary failure.");
    }
}