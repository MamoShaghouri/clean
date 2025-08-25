using System;
using System.Windows;
using Shaghouri.Models;

namespace Shaghouri.Services
{
    /// <summary>
    /// خدمة إدارة خطوط الحبوب والأسهم
    /// </summary>
    public class GrainLineService
    {
        /// <summary>
        /// تغيير اتجاه خط الحبوب
        /// </summary>
        public void ChangeGrainLineDirection(Shape shape)
        {
            if (shape != null)
            {
                shape.ArrowAngle = (shape.ArrowAngle == 0) ? 90 : 0;
            }
        }
        
        /// <summary>
        /// بدء تغيير حجم خط الحبوب
        /// </summary>
        public void StartScalingGrainLine(Shape shape, Point startPoint)
        {
            if (shape != null)
            {
                shape.ArrowLengthPreview = shape.ArrowLength;
            }
        }
        
        /// <summary>
        /// تحديث تغيير حجم خط الحبوب
        /// </summary>
        public void UpdateGrainLineScaling(Shape shape, Point currentPoint, Point startPoint, double startLength)
        {
            if (shape != null)
            {
                double delta;
                if (shape.ArrowAngle == 90)
                    delta = currentPoint.Y - startPoint.Y; // سحب عمودي للسهم العمودي
                else
                    delta = currentPoint.X - startPoint.X; // سحب أفقي للسهم الأفقي
                    
                double newLength = startLength + delta;
                if (newLength < 10) newLength = 10; // الحد الأدنى للطول
                shape.ArrowLengthPreview = newLength;
            }
        }
        
        /// <summary>
        /// تأكيد تغيير حجم خط الحبوب
        /// </summary>
        public void ConfirmGrainLineScaling(Shape shape)
        {
            if (shape != null && shape.ArrowLengthPreview.HasValue)
            {
                shape.ArrowLength = shape.ArrowLengthPreview.Value;
                shape.ArrowLengthPreview = null;
            }
        }
        
        /// <summary>
        /// حساب طول خط الحبوب بناءً على أبعاد الشكل
        /// </summary>
        public double CalculateDefaultArrowLength(double width, double height)
        {
            // الطول الافتراضي = 30% من العرض
            return width * 0.3;
        }
        
        /// <summary>
        /// تحديث اتجاه خط الحبوب بناءً على اتجاه السحب
        /// </summary>
        public void UpdateArrowDirection(Shape shape, Point startPoint, Point endPoint)
        {
            if (shape != null)
            {
                double deltaX = endPoint.X - startPoint.X;
                double deltaY = endPoint.Y - startPoint.Y;
                
                // تحديد الاتجاه بناءً على اتجاه السحب
                if (Math.Abs(deltaX) > Math.Abs(deltaY))
                {
                    shape.ArrowAngle = 0; // أفقي
                }
                else
                {
                    shape.ArrowAngle = 90; // عمودي
                }
            }
        }
    }
} 