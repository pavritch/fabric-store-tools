using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ControlPanel
{

    /// <summary>
    /// Extends <see cref="INotifyPropertyChanged"/> such that the change event can be raised by external parties.
    /// </summary>
    public interface INotifyPropertyChangedEx : INotifyPropertyChanged
    {
        /// <summary>
        /// Enables/Disables property change notification.
        /// </summary>
        bool IsNotifying { get; set; }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        void RaisePropertyChanged(string propertyName);

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        void Refresh();
    }
    
    /// <summary>
    /// A base class that implements the infrastructure for property change notification and automatically performs UI thread marshalling.
    /// </summary>
    public class PropertyChangedBase : INotifyPropertyChangedEx
    {
        /// <summary>
        /// Creates an instance of <see cref="PropertyChangedBase"/>.
        /// </summary>
        public PropertyChangedBase() {
            IsNotifying = true;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Enables/Disables property change notification.
        /// </summary>
        public bool IsNotifying { get; set; }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public void Refresh() {
            RaisePropertyChanged(string.Empty);
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            if(IsNotifying)
                Execute.OnUIThread(() => RaisePropertyChangedEventCore(propertyName));
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property expression.</param>
        public virtual void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> property) {
            RaisePropertyChanged(property.GetMemberInfo().Name);
        }

        /// <summary>
        /// Raises the property changed event immediately.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void RaisePropertyChangedEventImmediately(string propertyName) {
            if(IsNotifying)
                RaisePropertyChangedEventCore(propertyName);
        }

        private void RaisePropertyChangedEventCore(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            var myType = this.GetType();
            if (myType.GetProperty(propertyName) == null)
            {
                throw new ArgumentException("Property not found", propertyName);
            }
        }
    }
}