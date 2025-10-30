﻿namespace BuildingBlocks.Exceptions
{
    public class InternalServerException : Exception
    {
        public string Details { get; } = default!;
        public InternalServerException(string message) : base(message) { }

        public InternalServerException(string message, string details) : base(message)
        {
            Details = details;
        }
    }
}