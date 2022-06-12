using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Web.Mvc;
using System.Linq;

namespace Website
{

    /// <summary>
    /// Resolves types using the Managed Extensibility Framework.
    /// </summary>
    public class MEFDependencyResolver : IDependencyResolver
    {
        #region Fields
        private readonly CompositionContainer container;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="MEFDependencyResolver"/>.
        /// </summary>
        /// <param name="container">The current container.</param>
        public MEFDependencyResolver(CompositionContainer container)
        {
            this.container = container;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an instance of the service of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An instance of the service of the specified type.</returns>
        public object GetService(Type type)
        {

            try
            {
                var exports = this.container.GetExports(type, null, null);
                return exports.Any() ? exports.First().Value : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all instances of the services of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An enumerable of all instances of the services of the specified type.</returns>
        public IEnumerable<object> GetServices(Type type)
        {
            string name = AttributedModelServices.GetContractName(type);

            try
            {
                var exports = this.container.GetExports(type, null, null);
                return exports.Any() ? exports.Select(e => e.Value).AsEnumerable() : Enumerable.Empty<object>();
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}