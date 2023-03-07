namespace Blocktrust.Mediator.Common.Models.ProblemReport;
//https://identity.foundation/didcomm-messaging/spec/#problem-codes

public enum EnumProblemCodeScope
{
    Protocol, //The protocol within which the error occurs is abandoned or reset.
    Message, // The error was triggered by the previous message on the thread; the scope is one message. The outcome is that the problematic message is rejected (has no effect).
    StateName // A formal state name from the sender’s state machine in the active protocol. This means the error represented a partial failure of the protocol, but the protocol as a whole is not abandoned. Instead, the sender uses the scope to indicate what state it reverts to
    // When the Scope is StateName a separate string is used to describe the state name
}