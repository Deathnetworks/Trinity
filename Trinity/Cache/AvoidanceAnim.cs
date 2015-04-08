using System;
using Zeta.Game;

namespace Trinity
{
    public class Anim : IEquatable<Anim>
    {
        public SNOAnim SNOAnim { get; set; }
        public AvoidType Type { get; set; }
        public Element Element { get; set; }
        public bool GroundEffect { get; set; }

        public int Id
        {
            get { return SNOAnim != SNOAnim.Invalid ? (int)SNOAnim : Id; }
            set { Id = value; }
        }
        public string Name
        {
            get { return SNOAnim != SNOAnim.Invalid ? SNOAnim.ToString() : Name; }
            set { Name = value; }
        }

        public float CustomRadius { get; set; }
        public float Radius
        {
            get { return CustomRadius; }
            set { if (CustomRadius <= 0f) CustomRadius = value; }
        }

        public Anim(SNOAnim snoAnim, AvoidType type, Element element, bool groundEffect = false, float customRadius = 0f)
        {
            SNOAnim = snoAnim;
            Type = type;
            Element = element;
            GroundEffect = groundEffect;
            CustomRadius = customRadius;
        }

        public bool Equals(Anim other)
        {
            return SNOAnim == other.SNOAnim;
        }
    }
}
