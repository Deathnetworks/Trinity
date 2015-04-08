using System;
using Zeta.Game;

namespace Trinity
{
    public class Avoidance : IEquatable<Avoidance>
    {
        public SNOActor SNOActor { get; set; }
        public AvoidType Type { get; set; }
        public Element Element { get; set; }
        public bool GroundEffect { get; set; }

        public int Id
        {
            get { return (int)SNOActor; }
            set { Id = value; }
        }
        public string Name
        {
            get { return SNOActor.ToString(); }
            set { Name = value; }
        }

        public float CustomRadius { get; set; }
        public float Radius
        {
            get { return CustomRadius; }
            set { if (CustomRadius <= 0f) CustomRadius = value; }
        }

        public Avoidance(SNOActor snoActor, AvoidType type, Element element, bool groundEffect = false, float customRadius = 0f)
        {
            SNOActor = snoActor;
            Type = type;
            Element = element;
            GroundEffect = groundEffect;
            CustomRadius = customRadius;
        }

        public bool Equals(Avoidance other)
        {
            return SNOActor == other.SNOActor;
        }
    }
}
