using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace GilesTrinity.Cache
{
    internal abstract class CacheObject
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheObject" /> class.
        /// </summary>
        /// <param name="rActorGuid">The Actor GUID.</param>
        public CacheObject(int rActorGuid)
        {
            RActorGuid = rActorGuid;
            Type = GObjectType.Unknown;
            LastAccessDate = DateTime.UtcNow;
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the RActorGUID.
        /// </summary>
        /// <value>The RActorGUID.</value>
        public int RActorGuid
        {
            get;
            private set;
        }

        public string InternalName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the last access date.
        /// </summary>
        /// <value>The last access date.</value>
        public DateTime LastAccessDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last refresh date.
        /// </summary>
        /// <value>The last refresh date.</value>
        public DateTime LastRefreshDate 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        /// <value>The object type.</value>
        public GObjectType Type 
        { 
            get; 
            set; 
        }
        #endregion Properties

        protected readonly static Regex NameNumberRemover = new Regex(@"-\d+$", RegexOptions.Compiled);

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Cloned instance of <see cref="CacheObject"/>.</returns>
        public abstract CacheObject Clone();
        #endregion Methods
    }
}
