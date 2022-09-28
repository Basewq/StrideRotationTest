using Stride.Core;
using Stride.UI.Controls;
using Stride.UI.Events;
using System;
using System.ComponentModel;

namespace StrideRotationTest.UI
{
    [DataContract]
    public class EditTextExt : EditText, INavigatableControl
    {
        public static readonly RoutedEvent<RoutedEventArgs> TextCommittedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "TextCommited",
            RoutingStrategy.Bubble,
            typeof(EditTextExt));

        public event EventHandler<RoutedEventArgs> TextCommitted
        {
            add { AddHandler(TextCommittedEvent, value); }
            remove { RemoveHandler(TextCommittedEvent, value); }
        }

        public static readonly RoutedEvent<RoutedEventArgs> NavigatedOutEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "NavigatedOut",
            RoutingStrategy.Bubble,
            typeof(EditTextExt));

        public event EventHandler<RoutedEventArgs> NavigatedOut
        {
            add { AddHandler(NavigatedOutEvent, value); }
            remove { RemoveHandler(NavigatedOutEvent, value); }
        }

        [DataMember]
        [DefaultValue(true)]
        [Display(null, "Behavior")]
        public bool IsSelectable { get; set; } = true;

        private bool _isSelected = false;
        [DataMemberIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                bool isNavigatedOut = !value && _isSelected != value;
                _isSelected = value;
                IsSelectionActive = value;
                if (!_isSelected)
                {
                    RaiseEvent(new RoutedEventArgs(NavigatedOutEvent));
                }
            }
        }

        bool INavigatableControl.HasFocusExternally { get; set; }

        bool INavigatableControl.CanNavigateToControl()
        {
            if (!IsEnabled || !IsVisible)
            {
                return false;
            }
            var parentElem = Parent;
            while (parentElem != null)
            {
                if (!parentElem.IsEnabled || !parentElem.IsVisible)
                {
                    return false;
                }
                parentElem = parentElem.Parent;
            }
            return true;
        }

        bool INavigatableControl.OnNavigationMovement(UINavigationInputMovement inputMovement)
        {
            if (inputMovement.InputType == UINavigationInputType.Input)
            {
                return true;    // Don't allow arrow movement, since the textbox uses it to move the caret
            }

            return false;
        }

        void INavigatableControl.OnNavigationCommitSelection()
        {
            RaiseEvent(new RoutedEventArgs(TextCommittedEvent));
        }

        protected override void OnTouchUp(Stride.UI.TouchEventArgs args)
        {
            base.OnTouchUp(args);
            if (IsSelectionActive)
            {
                ((INavigatableControl)this).HasFocusExternally = true;
            }
        }
    }
}
