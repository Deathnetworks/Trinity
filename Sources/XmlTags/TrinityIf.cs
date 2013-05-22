using Trinity.Technicals;
using System;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // Trinity If perfectly mimics DB If - this is just for experimenting
    [XmlElement("TrinityIf")]
    public class TrinityIf : BaseComplexNodeTag, IPythonExecutable
    {
        private Func<bool> funcConditionalProcess;
        private string sConditionString;

        public virtual void CompilePython()
        {
            ScriptManager.GetCondition(Condition);
        }

        protected override Composite CreateBehavior()
        {
            PrioritySelector decorated = new PrioritySelector(new Composite[0]);
            foreach (ProfileBehavior behavior in base.GetNodes())
            {
                decorated.AddChild(behavior.Behavior);
            }
            return new Zeta.TreeSharp.Decorator(new CanRunDecoratorDelegate(CheckNotAlreadyDone), decorated);
        }

        public override bool GetConditionExec()
        {
            bool flag;
            try
            {
                if (Conditional == null)
                {
                    Conditional = ScriptManager.GetCondition(Condition);
                }
                flag = Conditional();
            }
            catch (Exception exception)
            {
                DbHelper.Log(TrinityLogLevel.Error, LogCategory.ProfileTag, ScriptManager.FormatSyntaxErrorException(exception)); 
                BotMain.Stop(false, "");
                throw;
            }
            return flag;
        }

        private bool CheckNotAlreadyDone(object obj)
        {
            return !IsDone;
        }

        [XmlAttribute("condition", true)]
        public string Condition
        {
            get
            {
                return sConditionString;
            }
            set
            {
                sConditionString = value;
            }
        }

        public Func<bool> Conditional
        {
            get
            {
                return funcConditionalProcess;
            }
            set
            {
                funcConditionalProcess = value;
            }
        }
    }
}
