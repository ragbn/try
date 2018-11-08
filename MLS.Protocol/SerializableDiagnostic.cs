﻿
using Newtonsoft.Json;

namespace MLS.Protocol
{
    public class SerializableDiagnostic
    {
        [JsonConstructor]
        public SerializableDiagnostic(int start, int end, string message, DiagnosticSeverity severity, string id)
        {
            Start = start;
            End = end;
            Message = message;
            Severity = severity;
            Id = id;
        }

        public int Start { get; }
        public int End { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }
        public string Id { get; }
    }
}
