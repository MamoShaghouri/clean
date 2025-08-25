using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Shaghouri.Models;

namespace Shaghouri.Services
{
    /// <summary>
    /// خدمة إدارة عمليات الرسم والأشكال
    /// </summary>
    public class DrawingService
    {
        private DrawingState _drawingState;
        
        public DrawingService()
        {
            _drawingState = new DrawingState();
        }
        
        /// <summary>
        /// بدء رسم مستطيل جديد
        /// </summary>
        public void StartRectangleDrawing()
        {
            _drawingState.IsDrawingRectangle = true;
            _drawingState.CurrentRectangleShape = null;
        }
        
        /// <summary>
        /// إلغاء عملية الرسم الحالية
        /// </summary>
        public void CancelDrawing()
        {
            _drawingState.IsDrawingRectangle = false;
            _drawingState.CurrentRectangleShape = null;
        }
        
        /// <summary>
        /// إنشاء مستطيل جديد
        /// </summary>
        public RectangleShape CreateRectangle(Point startPoint, Point endPoint)
        {
            var rect = new Rect(startPoint, endPoint);
            return new RectangleShape(rect);
        }
        
        /// <summary>
        /// تحديث معاينة المستطيل أثناء الرسم
        /// </summary>
        public void UpdateRectanglePreview(RectangleShape shape, Point currentPoint)
        {
            if (shape != null)
            {
                var anchorPoints = CreateAnchorPoints(shape.Rect);
                shape.AnchorPoints = anchorPoints;
            }
        }
        
        /// <summary>
        /// إنشاء نقاط مرجعية للمستطيل
        /// </summary>
        private List<AnchorPoint> CreateAnchorPoints(Rect rect)
        {
            return new List<AnchorPoint>
            {
                new AnchorPoint(new Point(rect.Left, rect.Top)),
                new AnchorPoint(new Point(rect.Right, rect.Top)),
                new AnchorPoint(new Point(rect.Right, rect.Bottom)),
                new AnchorPoint(new Point(rect.Left, rect.Bottom))
            };
        }
        
        /// <summary>
        /// الحصول على حالة الرسم الحالية
        /// </summary>
        public DrawingState GetDrawingState()
        {
            return _drawingState;
        }
        
        /// <summary>
        /// تحديث نقطة بداية الرسم
        /// </summary>
        public void SetStartPoint(Point point)
        {
            _drawingState.RectangleStartPoint = point;
        }
        
        /// <summary>
        /// تحديث نقطة نهاية الرسم
        /// </summary>
        public void SetEndPoint(Point point)
        {
            _drawingState.RectangleEndPoint = point;
        }
        
        /// <summary>
        /// تعيين الشكل الحالي
        /// </summary>
        public void SetCurrentShape(RectangleShape shape)
        {
            _drawingState.CurrentRectangleShape = shape;
        }
    }
} 