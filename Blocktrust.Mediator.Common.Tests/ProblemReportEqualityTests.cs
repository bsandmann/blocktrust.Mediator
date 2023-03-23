namespace Blocktrust.Mediator.Common.Tests;

using Models.ProblemReport;

public class ProblemReportEqualityTests
{
    [Fact]
    public void ProblemReportEqualityTestSucceds()
    {
        var problemReport1 = new ProblemReport(new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message), "comment");
        var problemReport2 = new ProblemReport(new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message), "comment");
        Assert.Equal(problemReport1, problemReport2);
    }
    
    [Fact]
    public void ProblemReportEqualityTestFails()
    {
        var problemReport1 = new ProblemReport(new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message), "comment");
        var problemReport2 = new ProblemReport(new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message), "otherComment");
        Assert.NotEqual(problemReport1, problemReport2);
    }
}