using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Shaghouri.Services;
using Shaghouri.Models;

namespace Shaghouri
{
    /// <summary>
    /// معالجات عمليات الرسم
    /// </summary>
    public partial class Create
    {
        #region Mouse Event Handlers
        
        private void HandleMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var mouseWorld = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
            
            if (_drawingState.IsScalingGrainLine)
            {
                HandleGrainLineScalingStart(mouseWorld);
                return;
            }
            
            if (_drawingState.IsChangingGrainLineDirection)
            {
                HandleGrainLineDirectionChange(mouseWorld);
                return;
            }
            
            if (_drawingState.IsDrawingRectangle)
            {
                HandleRectangleDrawingStart(mouseWorld);
            }
        }
        
        private void HandleMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (_drawingState.IsScalingGrainLine && _drawingState.ScalingShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                HandleGrainLineScaling(e);
                return;
            }
            
            if (_drawingState.IsDrawingRectangle && _drawingState.CurrentRectangleShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                HandleRectangleDrawing(e);
            }
        }
        
        private void HandleMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_drawingState.IsScalingGrainLine && _drawingState.ScalingShape != null)
            {
                LeftCanvas.ReleaseMouseCapture();
                return;
            }
            
            if (_drawingState.IsDrawingRectangle && _drawingState.CurrentRectangleShape != null)
            {
                HandleRectangleDrawingComplete(e);
            }
        }
        
        #endregion
        
        #region Drawing Handlers
        
        private void HandleRectangleDrawingStart(Point worldStart)
        {
            _drawingService.SetStartPoint(worldStart);
            var shape = _drawingService.CreateRectangle(worldStart, worldStart);
            _drawingService.SetCurrentShape(shape);
            LeftCanvas.ShapeToDraw = shape;
            LeftCanvas.CaptureMouse();
        }
        
        private void HandleRectangleDrawing(System.Windows.Input.MouseEventArgs e)
        {
            var worldCurrent = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
            var startPoint = _drawingService.GetDrawingState().RectangleStartPoint;
            
            var anchorPoints = new List<AnchorPoint>
            {
                new AnchorPoint(new Point(startPoint.X, startPoint.Y)),
                new AnchorPoint(new Point(worldCurrent.X, startPoint.Y)),
                new AnchorPoint(new Point(worldCurrent.X, worldCurrent.Y)),
                new AnchorPoint(new Point(startPoint.X, worldCurrent.Y))
            };
            
            _drawingState.CurrentRectangleShape.AnchorPoints = anchorPoints;
            _drawingState.CurrentRectangleShape.Rect = new Rect(startPoint, worldCurrent);
            
            LeftCanvas.ShapeToDraw = _drawingState.CurrentRectangleShape;
            LeftCanvas.InvalidateVisual();
        }
        
        private void HandleRectangleDrawingComplete(MouseButtonEventArgs e)
        {
            var worldEnd = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
            _drawingService.SetEndPoint(worldEnd);
            
            // اسم مؤقت
            string tempName = $"Rect{rectangles.Count + 1}";
            _drawingState.CurrentRectangleShape.Name = tempName;
            
            // تأكد أن العرض والارتفاع لا يساويان صفر
            double initialWidth = _drawingState.CurrentRectangleShape.Rect.Width == 0 ? 10 : _drawingState.CurrentRectangleShape.Rect.Width;
            double initialHeight = _drawingState.CurrentRectangleShape.Rect.Height == 0 ? 10 : _drawingState.CurrentRectangleShape.Rect.Height;
            
            // افتح نافذة Draw_Rectangle
            string unitStr = SelectedUnit == UnitType.MM ? "mm" : SelectedUnit == UnitType.INCH ? "inch" : "cm";
            double widthForDialog = ConvertToDialogUnits(initialWidth);
            double heightForDialog = ConvertToDialogUnits(initialHeight);
            
            var dlg = new Draw_Rectangle(_drawingState.CurrentRectangleShape.Name, widthForDialog, heightForDialog, unitStr);
            dlg.Owner = this;
            dlg.ShowDialog();
            
            if (dlg.IsConfirmed)
            {
                CompleteRectangleCreation(dlg);
            }
            
            // تنظيف
            LeftCanvas.ShapesToDraw = rectangles;
            _drawingState.CurrentRectangleShape = null;
            LeftCanvas.ShapeToDraw = null;
            LeftCanvas.ReleaseMouseCapture();
            LeftCanvas.InvalidateVisual();
            
            // أوقف وضع الرسم
            _drawingService.CancelDrawing();
            UpdateCanvasDrawingState();
        }
        
        private void CompleteRectangleCreation(Draw_Rectangle dlg)
        {
            // تحويل الأبعاد إلى مم
            double widthInMM = ConvertFromDialogUnits(dlg.RectangleWidth);
            double heightInMM = ConvertFromDialogUnits(dlg.RectangleHeight);
            
            // تحديث المستطيل
            var startPoint = _drawingService.GetDrawingState().RectangleStartPoint;
            var endPoint = _drawingService.GetDrawingState().RectangleEndPoint;
            
            var finalRect = CalculateFinalRectangle(startPoint, endPoint, widthInMM, heightInMM);
            var finalAnchorPoints = CreateFinalAnchorPoints(finalRect);
            
            _drawingState.CurrentRectangleShape.AnchorPoints = finalAnchorPoints;
            _drawingState.CurrentRectangleShape.Rect = finalRect;
            _drawingState.CurrentRectangleShape.Name = dlg.RectangleName;
            _drawingState.CurrentRectangleShape.ArrowLength = _grainLineService.CalculateDefaultArrowLength(widthInMM, heightInMM);
            
            rectangles.Add(_drawingState.CurrentRectangleShape);
            ShapePreviewsList.ItemsSource = null;
            ShapePreviewsList.ItemsSource = rectangles;
        }
        
        #endregion
        
        #region Grain Line Handlers
        
        private void HandleGrainLineScalingStart(Point mouseWorld)
        {
            foreach (var shape in LeftCanvas.ShapesToDraw)
            {
                if (shape.GetGeometry().FillContains(mouseWorld))
                {
                    _drawingState.ScalingShape = shape;
                    _drawingState.ScalingStartLength = shape.ArrowLength;
                    _drawingState.ScalingStartPoint = mouseWorld;
                    _grainLineService.StartScalingGrainLine(shape, mouseWorld);
                    LeftCanvas.CaptureMouse();
                    break;
                }
            }
        }
        
        private void HandleGrainLineScaling(System.Windows.Input.MouseEventArgs e)
        {
            var mouseWorld = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
            _grainLineService.UpdateGrainLineScaling(
                _drawingState.ScalingShape, 
                mouseWorld, 
                _drawingState.ScalingStartPoint, 
                _drawingState.ScalingStartLength
            );
            LeftCanvas.InvalidateVisual();
        }
        
        private void HandleGrainLineDirectionChange(Point mouseWorld)
        {
            foreach (var shape in LeftCanvas.ShapesToDraw)
            {
                if (shape.GetGeometry().FillContains(mouseWorld))
                {
                    _grainLineService.ChangeGrainLineDirection(shape);
                    LeftCanvas.InvalidateVisual();
                    break;
                }
            }
            _drawingState.IsChangingGrainLineDirection = false;
            Mouse.OverrideCursor = null;
        }
        
        #endregion
        
        #region Utility Methods
        
        private void UpdateCanvasDrawingState()
        {
            var state = _drawingService.GetDrawingState();
            LeftCanvas.IsDrawingShape = state.IsDrawingRectangle;
            LeftCanvas.ShapeToDraw = state.CurrentRectangleShape;
            LeftCanvas.InvalidateVisual();
        }
        
        private double ConvertToDialogUnits(double pixels)
        {
            switch (SelectedUnit)
            {
                case UnitType.CM:
                    return pixels / 10.0;
                case UnitType.INCH:
                    return pixels / 25.4;
                default: // MM
                    return pixels;
            }
        }
        
        private double ConvertFromDialogUnits(double dialogValue)
        {
            switch (SelectedUnit)
            {
                case UnitType.CM:
                    return dialogValue * 10.0;
                case UnitType.INCH:
                    return dialogValue * 25.4;
                default: // MM
                    return dialogValue;
            }
        }
        
        private Rect CalculateFinalRectangle(Point startPoint, Point endPoint, double width, double height)
        {
            var dx = endPoint.X - startPoint.X;
            var dy = endPoint.Y - startPoint.Y;
            
            double x1, y1, x2, y2;
            
            if (dx >= 0 && dy >= 0) // سحب لليمين والأسفل
            {
                x1 = startPoint.X;
                y1 = startPoint.Y;
                x2 = x1 + width;
                y2 = y1 + height;
            }
            else if (dx < 0 && dy < 0) // سحب لليسار وللأعلى
            {
                x2 = startPoint.X;
                y2 = startPoint.Y;
                x1 = x2 - width;
                y1 = y2 - height;
            }
            else if (dx >= 0 && dy < 0) // سحب لليمين وللأعلى
            {
                x1 = startPoint.X;
                y2 = startPoint.Y;
                x2 = x1 + width;
                y1 = y2 - height;
            }
            else // dx < 0 && dy >= 0 // سحب لليسار وللأسفل
            {
                x2 = startPoint.X;
                y1 = startPoint.Y;
                x1 = x2 - width;
                y2 = y1 + height;
            }
            
            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }
        
        private List<AnchorPoint> CreateFinalAnchorPoints(Rect rect)
        {
            return new List<AnchorPoint>
            {
                new AnchorPoint(new Point(rect.Left, rect.Top)),
                new AnchorPoint(new Point(rect.Right, rect.Top)),
                new AnchorPoint(new Point(rect.Right, rect.Bottom)),
                new AnchorPoint(new Point(rect.Left, rect.Bottom))
            };
        }
        
        private void UpdateShapeDimensionDisplay()
        {
            foreach (var rect in rectangles)
            {
                double width = rect.Rect.Width;
                double height = rect.Rect.Height;
                string unit = "";
                
                switch (SelectedUnit)
                {
                    case UnitType.MM:
                        width = width * 1.0;
                        height = height * 1.0;
                        unit = "mm";
                        break;
                    case UnitType.CM:
                        width = width / 10.0;
                        height = height / 10.0;
                        unit = "cm";
                        break;
                    case UnitType.INCH:
                        width = width / 25.4;
                        height = height / 25.4;
                        unit = "inch";
                        break;
                }
            }
            
            LeftCanvas.InvalidateVisual();
        }
        
        #endregion
    }
} 