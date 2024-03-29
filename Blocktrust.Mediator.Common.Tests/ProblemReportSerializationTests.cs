﻿namespace Blocktrust.Mediator.Common.Tests;

using System.Text.Json;
using DIDComm.Utils;
using Models.ProblemReport;

public class ProblemReportSerializationTests
{
    [Fact]
    public void ProblemReportSerializationTestSucceds()
    {
        var problemReport1 = new ProblemReport(new ProblemCode(EnumProblemCodeSorter.Error, EnumProblemCodeScope.Message), "comment");
        var json = JsonSerializer.Serialize(problemReport1, SerializationOptions.UnsafeRelaxedEscaping);
        var problemReport2 = JsonSerializer.Deserialize<ProblemReport>(json);
        Assert.Equal(problemReport1, problemReport2);
    }
}