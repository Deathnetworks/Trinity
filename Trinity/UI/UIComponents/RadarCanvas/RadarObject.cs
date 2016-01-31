using System;
using System.ComponentModel;
using System.Windows;
using Trinity.LazyCache;
using Zeta.Common;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.UIComponents
{
    /// <summary>
    /// RadarObject wraps a TrinityObject to add a canvas plot location.
    /// </summary>
    public class RadarObject : INotifyPropertyChanged
    {
        private TrinityObject _actor;

        /// <summary>
        /// Contains the actors position and other useful information.
        /// </summary>
        public PointMorph Morph = new PointMorph();

        /// <summary>
        /// Position in game world space for a point at radius distance 
        /// from actor's center and in the direction the actor is facing.
        /// </summary>
        public Vector3 HeadingVectorAtRadius { get; set; }

        /// <summary>
        /// Position on canvas (in pixels) for a point at radius distance 
        /// from actor's center and in the direction the actor is facing.
        /// </summary>
        public Point HeadingPointAtRadius { get; set; }

        /// <summary>
        /// Actors current position on canvas (in pixels).
        /// </summary>
        public Point Point
        {
            get { return Morph.Point; }
        }

        /// <summary>
        /// TrinityNode wraps a TrinityObject to add a canvas plot location.
        /// </summary>
        public RadarObject(TrinityObject obj, CanvasData canvasData)
        {
            Actor = obj;
            obj.PropertyChanged += ItemOnPropertyChanged;
            Morph.CanvasData = canvasData;
            Update();
        }

        /// <summary>
        /// Updates the plot location on canvas based on Item's current position.
        /// </summary>
        public void Update()
        {
            try
            {
                Morph.Update(Actor.Position);

                if (Actor.IsUnit || Actor.IsProjectile || Actor.AvoidanceType == AvoidanceType.Arcane)
                {
                    HeadingVectorAtRadius = MathEx.GetPointAt(new Vector3(Actor.Position.X, Actor.Position.Y, 0), Actor.Radius, Actor.Movement.GetHeadingRadians());
                    HeadingPointAtRadius = new PointMorph(HeadingVectorAtRadius, Morph.CanvasData).Point;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in RadarUI.TrinityItemPoint.Update(). {0} {1}", ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// The game object
        /// </summary>
        public TrinityObject Actor
        {
            set
            {
                if (!Equals(value, _actor))
                {
                    _actor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Actor"));
                }
            }
            get { return _actor; }
        }

        #region PropertyChanged Handling

        public event PropertyChangedEventHandler PropertyChanged;

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        #endregion

        public override int GetHashCode()
        {
            return Actor.GetHashCode();
        }

    }
}
