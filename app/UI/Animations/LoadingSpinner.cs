using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Flarial.Launcher.UI.Theme;

namespace Flarial.Launcher.UI.Animations;

/// <summary>
/// Spotify-style loading animation with bouncing dots.
/// </summary>
class LoadingSpinner : Canvas
{
    readonly Ellipse[] _dots;
    readonly Storyboard _storyboard;

    public LoadingSpinner()
    {
        Width = 50;
        Height = 20;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        _dots = new Ellipse[3];
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        CreateDots();
        CreateAnimation();

        Loaded += (s, e) => Start();
        Unloaded += (s, e) => Stop();
    }

    private void CreateDots()
    {
        for (int i = 0; i < 3; i++)
        {
            _dots[i] = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = FlarialTheme.AccentBrush,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };

            SetLeft(_dots[i], i * 17);
            SetTop(_dots[i], 5);
            Children.Add(_dots[i]);
        }
    }

    private void CreateAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            var beginTime = TimeSpan.FromMilliseconds(i * 150);

            // Scale Y animation (bounce effect)
            var scaleY = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = beginTime
            };

            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1.5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            });
            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(900))));

            Storyboard.SetTarget(scaleY, _dots[i]);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            _storyboard.Children.Add(scaleY);

            // Translate Y animation (move up when scaling)
            var translateY = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = beginTime
            };

            translateY.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            translateY.KeyFrames.Add(new EasingDoubleKeyFrame(-5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            translateY.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            });
            translateY.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(900))));

            // Need to add TranslateTransform for this
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new TranslateTransform(0, 0));
            _dots[i].RenderTransform = transformGroup;

            Storyboard.SetTarget(translateY, _dots[i]);
            Storyboard.SetTargetProperty(translateY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
            _storyboard.Children.Add(translateY);

            // Update scale animation path for TransformGroup
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
        }
    }

    public void Start()
    {
        _storyboard.Begin(this, true);
    }

    public void Stop()
    {
        _storyboard.Stop(this);
    }
}

/// <summary>
/// Circular loading spinner (alternative style).
/// </summary>
class CircularSpinner : Canvas
{
    readonly Storyboard _storyboard;

    public CircularSpinner()
    {
        Width = Height = 24;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        // Create arc
        var arc = new Path
        {
            Stroke = FlarialTheme.AccentBrush,
            StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Data = CreateArcGeometry(12, 12, 10, 0, 270),
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform()
        };
        Children.Add(arc);

        // Rotation animation
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var rotation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromMilliseconds(1000)
        };

        Storyboard.SetTarget(rotation, arc);
        Storyboard.SetTargetProperty(rotation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
        _storyboard.Children.Add(rotation);

        Loaded += (s, e) => _storyboard.Begin(this, true);
        Unloaded += (s, e) => _storyboard.Stop(this);
    }

    private static Geometry CreateArcGeometry(double centerX, double centerY, double radius, double startAngle, double endAngle)
    {
        var startRad = startAngle * Math.PI / 180;
        var endRad = endAngle * Math.PI / 180;

        var startX = centerX + radius * Math.Cos(startRad);
        var startY = centerY + radius * Math.Sin(startRad);
        var endX = centerX + radius * Math.Cos(endRad);
        var endY = centerY + radius * Math.Sin(endRad);

        var largeArc = Math.Abs(endAngle - startAngle) > 180;

        var pathFigure = new PathFigure
        {
            StartPoint = new Point(startX, startY),
            IsClosed = false
        };

        pathFigure.Segments.Add(new ArcSegment(
            new Point(endX, endY),
            new Size(radius, radius),
            0,
            largeArc,
            SweepDirection.Clockwise,
            true
        ));

        var geometry = new PathGeometry();
        geometry.Figures.Add(pathFigure);
        return geometry;
    }
}
