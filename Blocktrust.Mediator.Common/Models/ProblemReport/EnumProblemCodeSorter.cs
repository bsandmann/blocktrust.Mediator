namespace Blocktrust.Mediator.Common.Models.ProblemReport;

//https://identity.foundation/didcomm-messaging/spec/#problem-codes
public enum EnumProblemCodeSorter
{
    Warning, // The consequences of this problem are not obvious to the reporter; evaluating its effects requires judgment from a human or from some other party or system. 
    Error // This problem clearly defeats the intentions of at least one of the parties. It is therefore an error. A situation with error semantics might be that a protocol requires payment, but a payment attempt was rejected.
}