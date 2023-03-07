namespace Blocktrust.Mediator.Common.Models.ProblemReport;

// https://identity.foundation/didcomm-messaging/spec/#problem-codes
public enum EnumProblemCodeDescriptor
{
   Trust,
   TrustCrpyto,
   Transfer, // xfer
   Did,
   Message, // msg
   InternalError,
   InternalRessourceError,
   RequirementsNotSatified,
   TiminingRequirementsNotSatified,
   LegalReason,
   Other
}