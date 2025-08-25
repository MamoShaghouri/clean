using System.Collections.Generic;

namespace Shaghouri.DTOs
{
    /// <summary>
    /// نموذج نقل بيانات مشروع HCAD
    /// </summary>
    public class HcadProjectDto
    {
        /// <summary>
        /// قائمة المستطيلات في المشروع
        /// </summary>
        public List<RectangleShapeDto> Rectangles { get; set; } = new List<RectangleShapeDto>();
        
        /// <summary>
        /// اسم المشروع
        /// </summary>
        public string ProjectName { get; set; } = "Unnamed Project";
        
        /// <summary>
        /// تاريخ إنشاء المشروع
        /// </summary>
        public string CreatedDate { get; set; } = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        /// <summary>
        /// إصدار المشروع
        /// </summary>
        public string Version { get; set; } = "1.0.0";
        
        /// <summary>
        /// وحدات القياس المستخدمة
        /// </summary>
        public string Units { get; set; } = "MM";
    }
} 