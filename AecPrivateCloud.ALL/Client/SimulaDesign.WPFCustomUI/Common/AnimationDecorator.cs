using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SimulaDesign.WPFCustomUI.Common
{
    public class AnimationDecorator : Decorator
    {
        static AnimationDecorator()
        {
          
        }

        public AnimationDecorator()
            : base()
        {
            ClipToBounds = true;
        }

        

        public bool OpacityAnimation
        {
            get { return (bool)GetValue(OpacityAnimationProperty); }
            set { SetValue(OpacityAnimationProperty, value); }
        }

        public static readonly DependencyProperty OpacityAnimationProperty =
            DependencyProperty.Register("OpacityAnimation", 
            typeof(bool), 
            typeof(AnimationDecorator),
            new UIPropertyMetadata(true));



        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", 
            typeof(bool), 
            typeof(AnimationDecorator),
            new PropertyMetadata(true,IsExpandedChanged));


        public static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimationDecorator expander = d as AnimationDecorator;
            bool expanded = (bool)e.NewValue;
            expander.DoAnimate(expanded);
        }



        public DoubleAnimation HeightAnimation
        {
            get { return (DoubleAnimation)GetValue(HeightAnimationProperty); }
            set { SetValue(HeightAnimationProperty, value); }
        }

        public static readonly DependencyProperty HeightAnimationProperty =
            DependencyProperty.Register("HeightAnimation", 
            typeof(DoubleAnimation), 
            typeof(AnimationDecorator), 
            new UIPropertyMetadata(null));




        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(AnimationDecorator), new UIPropertyMetadata(new Duration(new TimeSpan(0,0,0,400))));



        private void DoAnimate(bool expanded)
        {
            if (Child != null)
            {
                if (YOffset > 0) YOffset = 0;
                if (-YOffset > Child.DesiredSize.Height) YOffset = -Child.DesiredSize.Height;
                DoubleAnimation animation = HeightAnimation;
                if (animation == null)
                {
                    animation = new DoubleAnimation();
                    animation.DecelerationRatio = 0.9;
                    animation.Duration = Duration;
                }
                animation.From = null;
                animation.To = expanded ? 0 : -Child.DesiredSize.Height;
                this.BeginAnimation(AnimationDecorator.YOffsetProperty, animation);

                if (OpacityAnimation)
                {
                    animation.From = null;
                    animation.To = expanded ? 1 : 0;
                    this.BeginAnimation(Control.OpacityProperty, animation);
                }
            }
            else
            {
                YOffset = int.MinValue;
            }
        }


        protected void SetYOffset(bool expanded)
        {
            YOffset = expanded ? 0 : -Child.DesiredSize.Height;
        }

        public Double YOffset
        {
            get { return (Double)GetValue(YOffsetProperty); }
            set { SetValue(YOffsetProperty, value); }
        }

        public static readonly DependencyProperty YOffsetProperty =
            DependencyProperty.Register("YOffset", typeof(Double), typeof(AnimationDecorator),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsMeasure));

        protected override Size MeasureOverride(Size constraint)
        {
            if (Child == null) return new Size(0, 0);

            Child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size size = new Size();
            size.Width = DesiredSize.Width;
            size.Height = Child.DesiredSize.Height;
            Double h = size.Height + YOffset;
            if (h < 0) h = 0;
            size.Height = h;
            if (Child != null) Child.IsEnabled = h > 0;
            return size;
        }


        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child == null) return arrangeSize;
            Size size = new Size();
            size.Width = arrangeSize.Width;
            size.Height = Child.DesiredSize.Height;

            Point p = new Point(0, YOffset);

            Child.Arrange(new Rect(p, size));

            Double h = Child.DesiredSize.Height + YOffset;
            if (h < 0) h = 0;
            size.Height = h;
            return size;
        }
    }
}
