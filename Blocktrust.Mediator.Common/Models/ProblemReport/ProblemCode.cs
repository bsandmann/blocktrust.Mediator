namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text;

public class ProblemCode
{
    public EnumProblemCodeSorter Sorter { get; }
    public EnumProblemCodeScope Scope { get; }
    public string? StateNameForScope { get; }
    public EnumProblemCodeDescriptor Descriptor { get; }
    public string? OtherDescriptor { get; }
    public string? AdditionalDescriptor1 { get; }
    public string? AdditionalDescriptor2 { get; }


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

            sb.Append(StateNameForScope);
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
}