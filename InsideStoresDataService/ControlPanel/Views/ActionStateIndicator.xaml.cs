using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ControlPanel.Views
{
    public partial class ActionStateIndicator : UserControl
    {
        public ActionStateIndicator()
        {
            InitializeComponent();
            GotoState(ActionStates.Default);
        }

        #region ActionState (DependencyProperty)

        /// <summary>
        /// Operation action state.
        /// </summary>
        public ControlPanel.ActionStates ActionState
        {
            get { return (ControlPanel.ActionStates)GetValue(ActionStateProperty); }
            set { SetValue(ActionStateProperty, value); }
        }
        public static readonly DependencyProperty ActionStateProperty =
            DependencyProperty.Register("ActionState", typeof(ControlPanel.ActionStates), typeof(ActionStateIndicator),
            new PropertyMetadata(ControlPanel.ActionStates.Default, new PropertyChangedCallback(OnActionStateChanged)));

        private static void OnActionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ActionStateIndicator)d).OnActionStateChanged(e);
        }

        protected virtual void OnActionStateChanged(DependencyPropertyChangedEventArgs e)
        {
            GotoState((ControlPanel.ActionStates)e.NewValue);
        }

        #endregion

        private void GotoState(ControlPanel.ActionStates actionState)
        {
            switch (actionState)
            {
                case ActionStates.Executing:
                    ImageWarning.Visibility = Visibility.Collapsed;
                    ImageSuccess.Visibility = Visibility.Collapsed;
                    RadBusy.Visibility = Visibility.Visible;
                    RadBusy.IsBusy = true;
                    break;

                case ActionStates.Success:
                    ImageWarning.Visibility = Visibility.Collapsed;
                    ImageSuccess.Visibility = Visibility.Visible;
                    RadBusy.Visibility = Visibility.Collapsed;
                    RadBusy.IsBusy = false;
                    break;

                case ActionStates.Warning:
                    ImageWarning.Visibility = Visibility.Visible;
                    ImageSuccess.Visibility = Visibility.Collapsed;
                    RadBusy.Visibility = Visibility.Collapsed;
                    RadBusy.IsBusy = false;
                    break;

                default:
                    ImageWarning.Visibility = Visibility.Collapsed;
                    ImageSuccess.Visibility = Visibility.Collapsed;
                    RadBusy.Visibility = Visibility.Collapsed;
                    RadBusy.IsBusy = false;
                    break;
            }
        }

    }
}
