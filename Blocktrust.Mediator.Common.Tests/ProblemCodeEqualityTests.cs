namespace Blocktrust.Mediator.Common.Tests;

using Models.ProblemReport;

public class ProblemCodeEqualityTests
{
    [Fact]
    public void ProblemCodeEqualityTestSucceeds()
    {
        // Test for equality
        var problemCode1 = new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message);
        var problemCode2 = new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message);
        Assert.Equal(problemCode1, problemCode2);
    } 
    
    [Fact]
    public void ProblemCodeEqualityTestFails()
    {
        // Test for equality
        var problemCode1 = new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.StateName, "stateName");
        var problemCode2 = new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message);
        Assert.NotEqual(problemCode1,problemCode2);
    } 
}