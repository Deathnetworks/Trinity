using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.ScriptedRules
{
    internal class ScriptedRule
    {
        public string Name
        {
            get;
            set;
        }

        public Delegate LambdaExpression
        {
            get;
            set;
        }

        public string Expression
        {
            get;
            set;
        }

        public ScriptedRuleAction Action
        {
            get;
            set;
        }
    }
}
