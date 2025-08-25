using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Shaghouri.DTOs;

namespace Shaghouri.Services
{
    /// <summary>
    /// خدمة إدارة حفظ وتحميل الملفات
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// حفظ المشروع إلى ملف
        /// </summary>
        public void SaveProject(List<RectangleShape> rectangles, string filePath)
        {
            try
            {
                var projectDto = new HcadProjectDto
                {
                    Rectangles = rectangles.Select(r => new RectangleShapeDto
                    {
                        X = r.Rect.X,
                        Y = r.Rect.Y,
                        Width = r.Rect.Width,
                        Height = r.Rect.Height,
                        Name = r.Name,
                        ArrowAngle = r.ArrowAngle,
                        ArrowLength = r.ArrowLength,
                        AnchorPoints = r.AnchorPoints.Select(a => new AnchorPointDto 
                        { 
                            X = a.Position.X, 
                            Y = a.Position.Y 
                        }).ToList()
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(projectDto, new JsonSerializerOptions { WriteIndented = true });
                string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                File.WriteAllText(filePath, encoded);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في حفظ المشروع: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// تحميل المشروع من ملف
        /// </summary>
        public List<RectangleShape> LoadProject(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"الملف غير موجود: {filePath}");
                }

                string encoded = File.ReadAllText(filePath);
                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var projectDto = JsonSerializer.Deserialize<HcadProjectDto>(json);

                var rectangles = new List<RectangleShape>();
                if (projectDto?.Rectangles != null)
                {
                    foreach (var dto in projectDto.Rectangles)
                    {
                        var rect = new Rect(dto.X, dto.Y, dto.Width, dto.Height);
                        var shape = new RectangleShape(rect)
                        {
                            Name = dto.Name,
                            ArrowAngle = dto.ArrowAngle,
                            ArrowLength = dto.ArrowLength,
                            AnchorPoints = dto.AnchorPoints.Select(a => new AnchorPoint(new Point(a.X, a.Y))).ToList()
                        };
                        rectangles.Add(shape);
                    }
                }
                
                return rectangles;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في تحميل المشروع: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// تصدير المشروع إلى DXF
        /// </summary>
        public void ExportToDXF(List<RectangleShape> rectangles, string filePath)
        {
            try
            {
                // هنا يمكن إضافة كود تصدير DXF
                // سيتم تنفيذه لاحقاً مع مكتبة OpenCascade
                throw new NotImplementedException("تصدير DXF سيتم تنفيذه لاحقاً");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في تصدير DXF: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// تصدير المشروع إلى HPGL
        /// </summary>
        public void ExportToHPGL(List<RectangleShape> rectangles, string filePath)
        {
            try
            {
                var hpgl = new System.Text.StringBuilder();
                hpgl.AppendLine("IN;"); // تهيئة
                hpgl.AppendLine("SP1;"); // اختيار قلم 1

                foreach (var rect in rectangles)
                {
                    // تحويل البكسل إلى سم (10 بكسل = 1 سم)
                    double x = rect.Rect.Left / 10.0;
                    double y = rect.Rect.Top / 10.0;
                    double w = rect.Rect.Width / 10.0;
                    double h = rect.Rect.Height / 10.0;

                    // HPGL: 1 سم = 400 وحدة
                    int x1 = (int)(x * 400);
                    int y1 = (int)(y * 400);
                    int x2 = (int)((x + w) * 400);
                    int y2 = (int)((y + h) * 400);
                    
                    // الانتقال إلى البداية
                    hpgl.AppendLine($"PU{x1},{y1};");
                    // رسم المستطيل
                    hpgl.AppendLine($"PD{x2},{y1},{x2},{y2},{x1},{y2},{x1},{y1};");
                    hpgl.AppendLine("PU;"); // رفع القلم
                }

                File.WriteAllText(filePath, hpgl.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في تصدير HPGL: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// التحقق من صحة الملف
        /// </summary>
        public bool ValidateFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;
                    
                string content = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(content))
                    return false;
                    
                // محاولة فك الترميز
                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content));
                var projectDto = JsonSerializer.Deserialize<HcadProjectDto>(json);
                
                return projectDto != null && projectDto.Rectangles != null;
            }
            catch
            {
                return false;
            }
        }
    }
} 