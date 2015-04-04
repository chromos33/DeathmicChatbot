using System;
using System.Diagnostics;

namespace DeathmicChatbot.Exceptions
{
    public class InvalidCommandParametersException : Exception
    {
        public InvalidCommandParametersException(int minParameters, int? maxParameters = null)
            : base()
        {
            Debug.Assert(minParameters >= 0,
                "minParameters must be at least zero.");
            Debug.Assert(maxParameters == null || maxParameters >= minParameters,
                "maxParameters must be at least minParameters.");

            this.MinParameters = minParameters;
            this.MaxParameters = maxParameters ?? minParameters;
        }

        public int MinParameters
        {
            get;
            private set;
        }

        public int MaxParameters
        {
            get;
            private set;
        }

        public override string Message
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public string GetMessage(string command)
        {
            if (this.MinParameters == 0 && this.MaxParameters == 0)
                return "This Command takes no parameters";
            else if (this.MinParameters == this.MaxParameters)
                return "This Command takes " + this.MaxParameters + " parameters";
            else
                return "This Command takes " + this.MinParameters + " to " + this.MaxParameters + " parameters";
        }
    }
}
