namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text;
using FluentResults;

public class ProblemCode
{
    public EnumProblemCodeSorter Sorter { get; }
    public EnumProblemCodeScope Scope { get; }
    public string? StateNameForScope { get; }
    public EnumProblemCodeDescriptor Descriptor { get; }
    public string? OtherDescriptor { get; }
    public string? AdditionalDescriptor1 { get; }
    public string? AdditionalDescriptor2 { get; }
    
    // Equality comparison for Sorter, Scope, StateNameForScope, Descriptor, OtherDescriptor, AdditionalDescriptor1 and AdditionalDescriptor2
    public override bool Equals(object obj)
    {
        if (obj is ProblemCode)
        {
            return this.ToString().Equals(obj.ToString());
        }

        return false;
    }


    public ProblemCode(EnumProblemCodeSorter sorter, EnumProblemCodeScope scope, string? stateNameForScope = null)
    {
        Sorter = sorter;
        Scope = scope;
        if (scope == EnumProblemCodeScope.StateName)
        {
            StateNameForScope = stateNameForScope;
        }
    }

    public ProblemCode(EnumProblemCodeSorter sorter, EnumProblemCodeScope scope, string? stateNameForScope, EnumProblemCodeDescriptor descriptor, string? otherDescriptor = null)
    {
        Sorter = sorter;
        Scope = scope;
        if (scope == EnumProblemCodeScope.StateName)
        {
            StateNameForScope = stateNameForScope;
        }

        Descriptor = descriptor;
        if (descriptor == EnumProblemCodeDescriptor.Other)
        {
            OtherDescriptor = otherDescriptor;
        }
    }

    public ProblemCode(EnumProblemCodeSorter sorter, EnumProblemCodeScope scope, string? stateNameForScope, EnumProblemCodeDescriptor descriptor, string? otherDescriptor, string additionalDescriptor1, string? additionalDescriptor2 = null)
    {
        Sorter = sorter;
        Scope = scope;
        if (scope == EnumProblemCodeScope.StateName)
        {
            StateNameForScope = stateNameForScope;
        }

        Descriptor = descriptor;
        if (descriptor == EnumProblemCodeDescriptor.Other)
        {
            OtherDescriptor = otherDescriptor;
        }

        AdditionalDescriptor1 = additionalDescriptor1;
        AdditionalDescriptor2 = additionalDescriptor2;
    }


    public override string ToString()
    {
        // Sorter
        var sb = new StringBuilder();
        if (Sorter == EnumProblemCodeSorter.Warning)
        {
            sb.Append("w.");
        }
        else if (Sorter == EnumProblemCodeSorter.Error)
        {
            sb.Append("e.");
        }
        else
        {
            throw new NotImplementedException();
        }

        // Scope
        if (Scope == EnumProblemCodeScope.Message)
        {
            sb.Append("m.");
        }
        else if (Scope == EnumProblemCodeScope.Protocol)
        {
            sb.Append("p.");
        }
        else if (Scope == EnumProblemCodeScope.StateName)
        {
            if (string.IsNullOrEmpty(StateNameForScope))
            {
                throw new Exception("Invalid state-name");
            }

            if (StateNameForScope.EndsWith("."))
            {
                throw new Exception("State-names should not use dots '.' at the end");
            }

            sb.Append(StateNameForScope);
            sb.Append('.');
        }
        else
        {
            throw new NotImplementedException();
        }

        //Descriptor
        if (Descriptor == EnumProblemCodeDescriptor.Trust)
        {
            sb.Append("trust");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.TrustCrpyto)
        {
            sb.Append("trust.crypto");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.Transfer)
        {
            sb.Append("xfer");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.Did)
        {
            sb.Append("did");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.Message)
        {
            sb.Append("msg");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.InternalError)
        {
            sb.Append("me");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.InternalRessourceError)
        {
            sb.Append("me.res");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.RequirementsNotSatified)
        {
            sb.Append("req");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.TiminingRequirementsNotSatified)
        {
            sb.Append("req.time");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.LegalReason)
        {
            sb.Append("legal");
        }
        else if (Descriptor == EnumProblemCodeDescriptor.Other)
        {
            if (string.IsNullOrEmpty(OtherDescriptor))
            {
                throw new Exception("Invalid descriptor");
            }

            if (OtherDescriptor.EndsWith("."))
            {
                throw new Exception("Descriptors should not use dots '.' at the end");
            }

            sb.Append(OtherDescriptor);
        }

        if (!string.IsNullOrEmpty(AdditionalDescriptor1))
        {
            if (AdditionalDescriptor1.EndsWith("."))
            {
                throw new Exception("Additional descriptors should not use dots '.' at the end");
            }

            sb.Append('.');
            sb.Append(AdditionalDescriptor1);
        }
        else
        {
            return sb.ToString();
        }

        if (!string.IsNullOrEmpty(AdditionalDescriptor2))
        {
            if (AdditionalDescriptor2.EndsWith("."))
            {
                throw new Exception("Additional descriptors should not use dots '.' at the end");
            }

            sb.Append('.');
            sb.Append(AdditionalDescriptor2);
        }
        else
        {
            return sb.ToString();
        }

        return sb.ToString();
    }

    public static Result<ProblemCode> Parse(string problemCode)
    {
        var splitted = problemCode.Split(".");
        if (splitted.Length < 3)
        {
            return Result.Fail("Invalid problem code");
        }

        var sorter = ParseSorter(splitted[0]);
        if (sorter.IsFailed)
        {
            return Result.Fail($"Invalid problem code: sorter '{splitted[0]}' not supported");
        }

        var scope = ParseScope(splitted[1]);
        var descriptor0 = ParseDescriptor(splitted[2]);

        string? descriptor1 = null;
        string? descriptor2 = null;


        if (splitted.Length == 4)
        {
            descriptor1 = splitted[3];
        }

        if (splitted.Length == 5)
        {
            descriptor2 = splitted[4];
        }

        if (splitted.Length > 5)
        {
            return Result.Fail("Problem code format not supported: too long");
        }

        return Result.Ok(new ProblemCode(sorter.Value, scope.Item1, scope.Item2, descriptor0.Item1, descriptor0.Item2, descriptor1, descriptor2));
    }

    private static Result<EnumProblemCodeSorter> ParseSorter(string sorter)
    {
        return sorter switch
        {
            "w" => EnumProblemCodeSorter.Warning,
            "e" => EnumProblemCodeSorter.Error,
            _ => Result.Fail("Invalid sorter")
        };
    }

    private static (EnumProblemCodeScope, string) ParseScope(string scope)
    {
        return scope switch
        {
            "m" => (EnumProblemCodeScope.Message, string.Empty),
            "p" => (EnumProblemCodeScope.Protocol, string.Empty),
            _ => (EnumProblemCodeScope.StateName, scope),
        };
    }

    private static (EnumProblemCodeDescriptor, string) ParseDescriptor(string descriptor)
    {
        return descriptor switch
        {
            "trust" => (EnumProblemCodeDescriptor.Trust, string.Empty),
            "trust.crypto" => (EnumProblemCodeDescriptor.TrustCrpyto, string.Empty),
            "xfer" => (EnumProblemCodeDescriptor.Transfer, string.Empty),
            "did" => (EnumProblemCodeDescriptor.Did, string.Empty),
            "msg" => (EnumProblemCodeDescriptor.Message, string.Empty),
            "me" => (EnumProblemCodeDescriptor.InternalError, string.Empty),
            "me.res" => (EnumProblemCodeDescriptor.InternalRessourceError, string.Empty),
            "req" => (EnumProblemCodeDescriptor.RequirementsNotSatified, string.Empty),
            "req.time" => (EnumProblemCodeDescriptor.TiminingRequirementsNotSatified, string.Empty),
            "legal" => (EnumProblemCodeDescriptor.LegalReason, string.Empty),
            _ => (EnumProblemCodeDescriptor.Other, descriptor),
        };
    }
}