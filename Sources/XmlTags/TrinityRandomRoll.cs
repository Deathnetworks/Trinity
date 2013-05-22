﻿using Trinity.Technicals;
using System;
using System.Globalization;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // TrinityRandom assigns a random value between min and max to a specified id
    [XmlElement("TrinityRandomRoll")]
    public class TrinityRandomRoll : ProfileBehavior
    {
        private bool m_IsDone = false;
        private int iID;
        private int iMin;
        private int iMax;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {

                // Generate a random value between the selected min-max range, and assign it to our dictionary of random values
                int iOldValue;
                Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
                int iNewRandomValue = (rndNum.Next((Max - Min) + 1)) + Min;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.ProfileTag, "Generating RNG for profile between {0} and {1}, result={2}", Min, Max, iNewRandomValue);
                if (!Trinity.dictRandomID.TryGetValue(ID, out iOldValue))
                {
                    Trinity.dictRandomID.Add(ID, iNewRandomValue);
                }
                else
                {
                    Trinity.dictRandomID[ID] = iNewRandomValue;
                }
                m_IsDone = true;
            });
        }

        [XmlAttribute("id")]
        public int ID
        {
            get
            {
                return iID;
            }
            set
            {
                iID = value;
            }
        }

        [XmlAttribute("min")]
        public int Min
        {
            get
            {
                return iMin;
            }
            set
            {
                iMin = value;
            }
        }

        [XmlAttribute("max")]
        public int Max
        {
            get
            {
                return iMax;
            }
            set
            {
                iMax = value;
            }
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
