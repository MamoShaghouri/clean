using System.Windows;

namespace Shaghouri.Models
{
    /// <summary>
    /// نموذج حالة الرسم الحالية
    /// </summary>
    public class DrawingState
    {
        /// <summary>
        /// الشكل الحالي المرسوم
        /// </summary>
        public RectangleShape CurrentRectangleShape { get; set; }
        
        /// <summary>
        /// هل في وضع رسم مستطيل
        /// </summary>
        public bool IsDrawingRectangle { get; set; }
        
        /// <summary>
        /// نقطة بداية الرسم
        /// </summary>
        public Point RectangleStartPoint { get; set; }
        
        /// <summary>
        /// نقطة نهاية الرسم
        /// </summary>
        public Point RectangleEndPoint { get; set; }
        
        /// <summary>
        /// هل في وضع تغيير اتجاه خط الحبوب
        /// </summary>
        public bool IsChangingGrainLineDirection { get; set; }
        
        /// <summary>
        /// هل في وضع تغيير حجم خط الحبوب
        /// </summary>
        public bool IsScalingGrainLine { get; set; }
        
        /// <summary>
        /// الشكل الذي يتم تغيير حجم خط الحبوب له
        /// </summary>
        public Shape ScalingShape { get; set; }
        
        /// <summary>
        /// الطول الأولي لخط الحبوب قبل التغيير
        /// </summary>
        public double ScalingStartLength { get; set; }
        
        /// <summary>
        /// نقطة بداية تغيير الحجم
        /// </summary>
        public Point ScalingStartPoint { get; set; }
        
        /// <summary>
        /// إعادة تعيين جميع الحالات
        /// </summary>
        public void Reset()
        {
            CurrentRectangleShape = null;
            IsDrawingRectangle = false;
            IsChangingGrainLineDirection = false;
            IsScalingGrainLine = false;
            ScalingShape = null;
            ScalingStartLength = 0;
        }
        
        /// <summary>
        /// التحقق من أن الرسم نشط
        /// </summary>
        public bool IsDrawingActive => IsDrawingRectangle || IsChangingGrainLineDirection || IsScalingGrainLine;
    }
} 