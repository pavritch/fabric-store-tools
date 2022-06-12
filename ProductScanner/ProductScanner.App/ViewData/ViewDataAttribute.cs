using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.App
{
    public class ViewDataAttribute : Attribute
    {
        /// <summary>
        /// Indicates which kind of commit batch this data model represents.
        /// </summary>
        public CommitBatchType BatchType { get; set; }

        /// <summary>
        /// The Type of user control to be used to display this data.
        /// </summary>
        public Type Viewer { get; set; }

        public bool IsFreezeColumnsSupported { get; set; }

        public ViewDataAttribute()
        {

        }

        public ViewDataAttribute(CommitBatchType BatchType, Type Viewer = null, bool IsFreezeColumnsSupported=false)
        {
            this.BatchType = BatchType;
            this.Viewer = Viewer;
            this.IsFreezeColumnsSupported = IsFreezeColumnsSupported;
        }

    }

}
