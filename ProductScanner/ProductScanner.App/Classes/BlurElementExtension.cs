using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Media.Effects;
using Expression = System.Linq.Expressions.Expression;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;

namespace ProductScanner.App
{
    // http://www.codeproject.com/Tips/754397/Blur-Effect-on-Control
    // this.BlurApply(BlurRadius, new TimeSpan(0, 0, 1), TimeSpan.Zero);
    // this.BlurDisable(new TimeSpan(0, 0, 5), TimeSpan.Zero);
           
    /// <summary>
    /// Blur and unblur UIElement with animation.
    /// </summary>
    public static class BlurElementExtension
    {

        /// <summary>
        /// Turning blur on
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="blurRadius">blur radius</param>
        /// <param name="duration">blur animation duration</param>
        /// <param name="beginTime">blur animation delay</param>
        public static void BlurApply(this UIElement element,
            double blurRadius, TimeSpan duration, TimeSpan beginTime)
        {
            BlurEffect blur = new BlurEffect() { Radius = 0 };
            DoubleAnimation blurEnable = new DoubleAnimation(0, blurRadius, duration) { BeginTime = beginTime };
            element.Effect = blur;
            blur.BeginAnimation(BlurEffect.RadiusProperty, blurEnable);
        }
        /// <summary>
        /// Turning blur off
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="duration">blur animation duration</param>
        /// <param name="beginTime">blur animation delay</param>
        public static void BlurDisable(this UIElement element, TimeSpan duration, TimeSpan beginTime)
        {
            BlurEffect blur = element.Effect as BlurEffect;
            if (blur == null || blur.Radius == 0)
            {
                return;
            }
            DoubleAnimation blurDisable = new DoubleAnimation(blur.Radius, 0, duration) { BeginTime = beginTime };
            blur.BeginAnimation(BlurEffect.RadiusProperty, blurDisable);
        }
    } 

}